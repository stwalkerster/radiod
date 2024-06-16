namespace LizardNetRadio.Sidecar;

using System.Runtime.InteropServices;
using System.Text;
using Common;
using log4net;
using log4net.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

class Program
{
    private LiquidSoapClient liquidSoapClient;
    private IModel channel;

    private readonly ILog logger = LogManager.GetLogger(typeof(Program));

    static void Main(string[] args)
    {
        XmlConfigurator.Configure(new FileInfo("log4net.xml"));
        
        var configFileName = "config.yml";
        if (args.Length >= 1)
        {
            configFileName = args[0];
        }

        var config = ConfigurationReader.ReadConfiguration<GlobalConfiguration>(configFileName);
        
        new Program().Run(config);
    }

    private void Run(GlobalConfiguration config)
    {
        this.liquidSoapClient = new LiquidSoapClient(config.LiquidSoapHostname, config.LiquidSoapPort);
        
        var factory = new ConnectionFactory
        {
            HostName = config.RabbitMqConfiguration.Hostname,
            Port = config.RabbitMqConfiguration.Port,
            VirtualHost = config.RabbitMqConfiguration.VirtualHost,
            UserName = config.RabbitMqConfiguration.Username,
            Password = config.RabbitMqConfiguration.Password,
            ClientProvidedName = "Radio-LizardNetD/0.1 (sidecar)",
            ClientProperties = new Dictionary<string, object>
            {
                {"product", Encoding.UTF8.GetBytes("Radio LizardNet")},
                {"platform", Encoding.UTF8.GetBytes(RuntimeInformation.FrameworkDescription)},
                {"os", Encoding.UTF8.GetBytes(Environment.OSVersion.ToString())}
            },
            Ssl = new SslOption{Enabled = config.RabbitMqConfiguration.Tls, ServerName = config.RabbitMqConfiguration.Hostname}
        };

        var connection = factory.CreateConnection();
        this.channel = connection.CreateModel();
        
        var queue = config.RabbitMqConfiguration.ObjectPrefix + "request";
        this.channel.QueueDeclare(queue, true, false, false);
        this.channel.QueuePurge(queue);
        
        this.channel.ExchangeDeclare(queue, "direct", true);
        this.channel.QueueBind(queue, queue, "");
            
        var consumer = new EventingBasicConsumer(this.channel);
        consumer.Received += this.ConsumerOnReceived;
        
        this.logger.Info("Started up sidecar, listening for events");
        
        this.channel.BasicConsume(queue, false, consumer);

        new ManualResetEvent(false).WaitOne();
    }

    private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
    {
        try
        {
            var props = e.BasicProperties;
            var replyProps = this.channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;
            replyProps.Type = "Reply";
            var publicationAddress = PublicationAddress.Parse(props.ReplyTo);

            var content = Encoding.UTF8.GetString(e.Body.ToArray());
            
            string response;
            try
            {
                response = this.liquidSoapClient.DoRpcCall(content);
            }
            catch (Exception ex)
            {
                this.logger.Warn("Error doing RPC call.", ex);
                this.channel.BasicNack(e.DeliveryTag, false, false);

                replyProps.Type = "Error";


                this.channel.BasicPublish(
                    exchange: publicationAddress.ExchangeName,
                    routingKey: publicationAddress.RoutingKey,
                    basicProperties: replyProps,
                    body: Encoding.UTF8.GetBytes("Error communicating with radio daemon: " + ex.Message));

                return;
            }

            if (props.ReplyTo != null)
            {
                var responseBytes = Encoding.UTF8.GetBytes(response);
                
                this.channel.BasicPublish(
                    exchange: publicationAddress.ExchangeName,
                    routingKey: publicationAddress.RoutingKey,
                    basicProperties: replyProps,
                    body: responseBytes);
            }

            this.channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            this.logger.Error(ex);
        }
    }
}
namespace LizardNetRadio.Bot.Service;

using System.Text;
using System.Web;
using Castle.Core;
using Castle.Core.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Stwalkerster.IrcClient.Interfaces;

public interface IMetadataReceiverService : IStartable;

public class MetadataReceiverService : IMetadataReceiverService
{
    private readonly IIrcClient ircClient;
    private readonly ILogger logger;
    private readonly IModel channel;
    private readonly string metadataQueue;
    private readonly EventingBasicConsumer consumer;
    private readonly string metadataChannel;
    private readonly string streamName;

    public MetadataReceiverService(IIrcClient ircClient, IConnection rabbitConnection, GlobalConfiguration config, ILogger logger)
    {
        this.ircClient = ircClient;
        this.logger = logger;
        this.channel = rabbitConnection.CreateModel();
        this.metadataQueue = config.RabbitMqConfiguration.ObjectPrefix + config.MetadataQueue;
        this.metadataChannel = config.MetadataChannel;
        this.streamName = "/" + config.StreamName;
        
        this.consumer = new EventingBasicConsumer(this.channel);
        this.consumer.Received += this.ConsumerOnReceived;
        
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
    {
        try
        {
            var body = e.Body.ToArray();

            var bodyHex = BitConverter.ToString(body);
            bodyHex = bodyHex.Substring(bodyHex.LastIndexOf("7C", StringComparison.Ordinal) + 3);
            this.logger.Debug($"Received message: {bodyHex}");
            
            // var rawMessage = Encoding.UTF8.GetString(e.Body.ToArray());
            var isoEncoding = Encoding.GetEncoding("Windows-1252");
            var rawMessage = isoEncoding.GetString(body);
            // rawMessage = Encoding.UTF8.GetString(Encoding.Convert(isoEncoding, Encoding.UTF8, isoEncoding.GetBytes(rawMessage)));
            this.logger.Trace(rawMessage);
            
            var message = rawMessage.Split('|');
            if (message[1] != this.streamName)
            {
                this.channel.BasicAck(e.DeliveryTag, false);
                return;
            }
            
            this.ircClient.SendMessage(this.metadataChannel, "Now Playing: " + HttpUtility.HtmlDecode(message[3]));

            this.channel.BasicAck(e.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            this.logger.Error("Something went wrong", ex);
        }
    }

    public void Start()
    {
        this.channel.BasicConsume(this.metadataQueue, false, this.consumer);
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
}
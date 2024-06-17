namespace LizardNetRadio.Bot.Service;

using System.Runtime.InteropServices;
using System.Text;
using Castle.Core;
using Castle.Core.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class LiquidSoapClient : IStartable, ILiquidSoapClient
{
    private readonly ILogger logger;
    private readonly IModel channel;
    private readonly EventingBasicConsumer consumer;

    private readonly Dictionary<string, (SemaphoreSlim semaphore, string responseType, string data)> taskList = new();
    private readonly string objectPrefix;
    
    public LiquidSoapClient(GlobalConfiguration config, ILogger logger)
    {
        this.logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = config.RabbitMqConfiguration.Hostname,
            Port = config.RabbitMqConfiguration.Port,
            VirtualHost = config.RabbitMqConfiguration.VirtualHost,
            UserName = config.RabbitMqConfiguration.Username,
            Password = config.RabbitMqConfiguration.Password,
            ClientProvidedName = "Radio-LizardNetD/0.1 (bot)",
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
        
        this.objectPrefix = config.RabbitMqConfiguration.ObjectPrefix;
        var queue = this.objectPrefix + "reply";
        this.channel.QueueDeclare(queue, true, false, false);
        this.channel.QueuePurge(queue);
        
        this.channel.ExchangeDeclare(queue, "direct", true);
        this.channel.QueueBind(queue, queue, "");

        this.consumer = new EventingBasicConsumer(this.channel);
        this.channel.BasicConsume(queue, true, this.consumer);
    }

    private (string, SemaphoreSlim) RemoteProcedureCall(string command)
    {
        var guid = Guid.NewGuid().ToString();
        var semaphore = new SemaphoreSlim(0, 1);
        
        var sendProps = this.channel.CreateBasicProperties();
        sendProps.ReplyToAddress = new PublicationAddress("direct", this.objectPrefix + "reply", "");
        sendProps.CorrelationId = guid;
        sendProps.Timestamp = new AmqpTimestamp();
        sendProps.Type = "Request";

        lock (this)
        {
            this.taskList.Add(guid, (semaphore, "", ""));
        }
        
        this.logger.DebugFormat("Sending request with ID {0}", guid);
        
        this.channel.BasicPublish(
            exchange: this.objectPrefix + "request",
            routingKey: "",
            basicProperties: sendProps,
            body: Encoding.UTF8.GetBytes(command));

        return (guid, semaphore);
    }

    private void ConsumerOnReceived(object sender, BasicDeliverEventArgs e)
    {
        var correlationId = e.BasicProperties.CorrelationId;
        var type = e.BasicProperties.Type;
        var body = Encoding.UTF8.GetString(e.Body.ToArray());
        
        this.logger.DebugFormat("Received AMQP {1} for {0}", correlationId, type);

        lock (this)
        {
            if (!this.taskList.TryGetValue(correlationId, out var tuple))
            {
                return;
            }

            tuple.responseType = type;
            tuple.data = body;
            this.taskList[correlationId] = tuple;
            
            tuple.semaphore.Release();
        }
    }

    void IStartable.Start()
    {
        this.logger.Info("Started LiquidSoapClient");
        this.consumer.Received += this.ConsumerOnReceived;
    }

    void IStartable.Stop()
    {
        this.consumer.Received -= this.ConsumerOnReceived;
        this.logger.Info("Stopped LiquidSoapClient");
    }

    public async Task SkipTrack()
    {
        this.logger.DebugFormat("Skipping track...");
        
        var (guid, semaphore) = this.RemoteProcedureCall("radio.skip");
        
        await semaphore.WaitAsync();

        this.logger.DebugFormat("Track skip response received");
        
        string data, responseType;
        lock (this)
        {
            (var _, responseType, data) = this.taskList[guid];
            this.taskList.Remove(guid);
        }

        if (responseType == "Reply" && data == "Done")
        {
            // success
        }
        else
        {
            throw new Exception(data);
        }
    }

    public async Task<IEnumerable<string>> Inject(string command)
    {
        var (guid, semaphore) = this.RemoteProcedureCall(command);
        
        await semaphore.WaitAsync();
        
        string data, responseType;
        lock (this)
        {
            (var _, responseType, data) = this.taskList[guid];
            this.taskList.Remove(guid);
        }

        if (responseType == "Reply")
        {
            return data.Split("\n").ToList();
        }

        throw new Exception(data);
    }
    
    public async Task<int> Request(string command)
    {
        var (guid, semaphore) = this.RemoteProcedureCall("request.push " + command);
        
        await semaphore.WaitAsync();
        
        string data, responseType;
        lock (this)
        {
            (var _, responseType, data) = this.taskList[guid];
            this.taskList.Remove(guid);
        }

        if (responseType != "Reply")
        {
            throw new Exception(data);
        }

        return int.Parse(data);
    }

    public async Task<double> Remaining()
    {
        var (guid, semaphore) = this.RemoteProcedureCall("radio.remaining");
        
        await semaphore.WaitAsync();
        
        string data, responseType;
        lock (this)
        {
            (var _, responseType, data) = this.taskList[guid];
            this.taskList.Remove(guid);
        }

        if (responseType == "Reply")
        {
            return double.Parse(data);
        }

        throw new Exception(data);
    }
    
    public async Task<(string artist, string title)> NowPlaying()
    {
        var (guid, semaphore) = this.RemoteProcedureCall("radio.metadata");
        
        await semaphore.WaitAsync();
        
        string data, responseType;
        lock (this)
        {
            (_, responseType, data) = this.taskList[guid];
            this.taskList.Remove(guid);
        }

        if (responseType != "Reply")
        {
            throw new Exception(data);
        }

        var lines = data.Split("\n").ToList();
        var artist = "";
        var title = "";

        foreach (var l in lines)
        {
            if (l.StartsWith("--- ") && l.EndsWith(" ---"))
            {
                artist = title = string.Empty;
            }

            if (l.StartsWith("artist="))
            {
                artist = l.Substring("artist=".Length).Trim('"');
            }

            if (l.StartsWith("title="))
            {
                title = l.Substring("title=".Length).Trim('"');
            }
        }

        return (artist, title);
    }
}

public interface ILiquidSoapClient
{
    Task SkipTrack();

    Task<(string artist, string title)> NowPlaying();

    Task<IEnumerable<string>> Inject(string command);
    Task<int> Request(string file);

    Task<double> Remaining();
}
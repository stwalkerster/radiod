namespace LizardNetRadio.MetadataMonitor;

using System.Runtime.InteropServices;
using System.Text;
using LizardNetRadio.Common;
using log4net;
using log4net.Config;
using RabbitMQ.Client;

class Program
{
    private readonly ILog logger = LogManager.GetLogger(typeof(Program));  
    private IModel channel;
    private string queue;

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
        this.logger.InfoFormat("Initialising metadata monitor for {0}...", config.LogDirectory);
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        this.SetupMq(config);

        var watcher = new FileSystemWatcher(config.LogDirectory, config.LogFilter);
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        
        watcher.Changed += this.WatcherOnChanged;
        watcher.EnableRaisingEvents = true;
        this.logger.InfoFormat("Monitoring enabled on {0}", config.LogFilter);
        
        new ManualResetEvent(false).WaitOne();
    }

    private void SetupMq(GlobalConfiguration config)
    {
        this.logger.InfoFormat("Using broker at {0}:{1}...", config.RabbitMqConfiguration.Hostname, config.RabbitMqConfiguration.Port);
        
        var factory = new ConnectionFactory
        {
            HostName = config.RabbitMqConfiguration.Hostname,
            Port = config.RabbitMqConfiguration.Port,
            VirtualHost = config.RabbitMqConfiguration.VirtualHost,
            UserName = config.RabbitMqConfiguration.Username,
            Password = config.RabbitMqConfiguration.Password,
            ClientProvidedName = "Radio-LizardNetD/0.1 (metadata monitor)",
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
        
        this.queue = config.RabbitMqConfiguration.ObjectPrefix + config.AnnounceQueue;
        this.channel.QueueDeclare(this.queue, true, false, false);
        this.channel.QueuePurge(this.queue);
        
        this.channel.ExchangeDeclare(this.queue, "direct", true);
        this.channel.QueueBind(this.queue, this.queue, "");
    }

    private void WatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        try
        {
            var lastMetadata = Encoding.GetEncoding(1252).GetString(File.ReadAllBytes(e.FullPath)).Split('\n', StringSplitOptions.RemoveEmptyEntries).Last();
            
            this.logger.DebugFormat("Metadata detected: {0}", lastMetadata);

            var props = this.channel.CreateBasicProperties();
            
            var content = Encoding.UTF8.GetBytes(lastMetadata);
            this.channel.BasicPublish(this.queue, "", props, content);
        }
        catch (Exception exception)
        {
            this.logger.Error(exception);
        }
    }
}
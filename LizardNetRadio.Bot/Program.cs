namespace LizardNetRadio.Bot;

using System.Runtime.InteropServices;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Common;
using RabbitMQ.Client;
using Startup;
using Stwalkerster.Bot.CommandLib.Services.Interfaces;
using Stwalkerster.IrcClient;
using Stwalkerster.IrcClient.Interfaces;
using Stwalkerster.IrcClient.Messages;
using Stwalkerster.IrcClient.Network;

public class Program : IApplication
{
    public static void Main(string[] args)
    {
        var configFileName = "config.yml";
        if (args.Length >= 1)
        {
            configFileName = args[0];
        }

        var config = ConfigurationReader.ReadConfiguration<GlobalConfiguration>(configFileName);

        
        var container = new WindsorContainer();
        container.Register(Component.For<GlobalConfiguration>().Instance(config));
        container.Register(Component.For<RabbitMqConfiguration>().Instance(config.RabbitMqConfiguration));
        container.Register(
            Component.For<IConnection>().Instance(CreateRabbitMqConnection(config.RabbitMqConfiguration)));
        container.Register(
            Component.For<IIrcConfiguration>()
                .Instance(
                    new IrcConfiguration(
                        config.Hostname,
                        config.Port,
                        true,
                        config.Nickname,
                        config.Nickname,
                        config.RealName,
                        true,
                        "LizardNet",
                        servicesCertificate: config.ServicesCertificate,
                        serverPassword:config.ServerPassword
                    )));
        
        container.Install(new Installer());

        var app = container.Resolve<IApplication>();
    }

    private static IConnection CreateRabbitMqConnection(RabbitMqConfiguration config)
    {
        var factory = new ConnectionFactory
        {
            HostName = config.Hostname,
            Port = config.Port,
            VirtualHost = config.VirtualHost,
            UserName = config.Username,
            Password = config.Password,
            ClientProvidedName = "Radio-LizardNetD/0.1 (bot)",
            ClientProperties = new Dictionary<string, object>
            {
                {"product", Encoding.UTF8.GetBytes("Radio LizardNet")},
                {"platform", Encoding.UTF8.GetBytes(RuntimeInformation.FrameworkDescription)},
                {"os", Encoding.UTF8.GetBytes(Environment.OSVersion.ToString())}
            },
            Ssl = new SslOption{Enabled = config.Tls, ServerName = config.Hostname}
        };
        
        var connection = factory.CreateConnection();
        return connection;
    }

    public Program(IIrcClient client, ICommandHandler commandHandler,  GlobalConfiguration configuration)
    {
        client.ReceivedMessage += commandHandler.OnMessageReceived;
        
        client.WaitOnRegistration();

        if (!string.IsNullOrWhiteSpace(configuration.OperUser) && !string.IsNullOrWhiteSpace(configuration.OperPass))
        {
            client.Send(new Message("OPER", [configuration.OperUser, configuration.OperPass]));
            ((NetworkClient)((IrcClient)client).NetworkClient).FloodDelay = 0;
        }

        client.JoinChannel(configuration.DefaultChannel);
        
        if (configuration.DefaultChannel != configuration.MetadataChannel)
        {
            client.JoinChannel(configuration.MetadataChannel);
        }
    }

    public void Stop()
    {
    }

    public void Run()
    {
    }
}
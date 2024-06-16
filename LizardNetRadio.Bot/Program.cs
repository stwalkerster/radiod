namespace LizardNetRadio.Bot;

using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Common;
using Startup;
using Stwalkerster.Bot.CommandLib.Services.Interfaces;
using Stwalkerster.IrcClient;
using Stwalkerster.IrcClient.Interfaces;

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
                        servicesCertificate: config.ServicesCertificate
                    )));

        container.Install(new Installer());

        var app = container.Resolve<IApplication>();
        
        
    }

    public Program(IIrcClient client, ICommandHandler commandHandler,  GlobalConfiguration configuration)
    {
        client.ReceivedMessage += commandHandler.OnMessageReceived;

        client.JoinChannel(configuration.DefaultChannel);
    }

    public void Stop()
    {
    }

    public void Run()
    {
    }
}
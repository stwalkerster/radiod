namespace LizardNetRadio.Bot
{
    using Common;
    using Stwalkerster.IrcClient;

    public class GlobalConfiguration
    {
        public RabbitMqConfiguration RabbitMqConfiguration { get; set; }

        public string Hostname { get; set; }
        public ushort Port { get; set; }
    
        public string Nickname { get; set; }
        public string RealName { get; set; }
        public string ServicesCertificate { get; set; }
        
        public string CommandPrefix { get; set; }
        public string DefaultChannel { get; set; }
        
        public string AdminMask { get; set; }
    }
}
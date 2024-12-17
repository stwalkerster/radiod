namespace LizardNetRadio.Bot
{
    using Common;
    using Stwalkerster.IrcClient;

    public class GlobalConfiguration
    {
        public RabbitMqConfiguration RabbitMqConfiguration { get; set; }
        public AwsConfiguration AwsConfiguration { get; set; }

        public string Hostname { get; set; }
        public ushort Port { get; set; }
    
        public string Nickname { get; set; }
        public string RealName { get; set; }
        public string ServicesCertificate { get; set; }
        
        public string CommandPrefix { get; set; }
        public string DefaultChannel { get; set; }
        public string MetadataChannel { get; set; }
        public List<string> ForbiddenChannels { get; set; }
        
        public string AdminMask { get; set; }
        
        public string ServerPassword { get; set; }
        public string OperUser { get; set; }
        public string OperPass { get; set; }
        
        public string MyQueue { get; set; }
        public string RequestQueue { get; set; }
        public string MetadataQueue { get; set; }
        
        public string StreamName { get; set; }
    }
}
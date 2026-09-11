namespace LizardNetRadio.Bot
{
    using Common;
    using Stwalkerster.IrcClient;

    public class GlobalConfiguration
    {
        private string serverPassword;
        private string operUser;
        private string operPass;
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

        public string ServerPassword
        {
            get
            {
                if (string.IsNullOrEmpty(this.serverPassword) && Environment.GetEnvironmentVariable("IRC_SERVER_PASSWORD") != null)
                {
                    return Environment.GetEnvironmentVariable("IRC_SERVER_PASSWORD");
                }

                return this.serverPassword;
            }
            set => this.serverPassword = value;
        }

        public string OperUser
        {
            get
            {
                if (string.IsNullOrEmpty(this.operUser) && Environment.GetEnvironmentVariable("IRC_OPER_USER") != null)
                {
                    return Environment.GetEnvironmentVariable("IRC_OPER_USER");
                }

                return this.operUser;
            }
            set => this.operUser = value;
        }

        public string OperPass
        {
            get
            {
                if (string.IsNullOrEmpty(this.operPass) && Environment.GetEnvironmentVariable("IRC_OPER_PASSWORD") != null)
                {
                    return Environment.GetEnvironmentVariable("IRC_OPER_PASSWORD");
                }

                return this.operPass;
            }
            set => this.operPass = value;
        }

        public string MyQueue { get; set; }
        public string RequestQueue { get; set; }
        public string MetadataQueue { get; set; }
        
        public string StreamName { get; set; }
    }
}
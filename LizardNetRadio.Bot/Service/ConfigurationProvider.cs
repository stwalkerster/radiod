namespace LizardNetRadio.Bot.Service
{
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;

    public class ConfigurationProvider(GlobalConfiguration config) : IConfigurationProvider
    {
        public string CommandPrefix => config.CommandPrefix;
        public string DebugChannel => config.DefaultChannel;
        public bool AllowQuotedStrings => true;
        public bool IncludeBuiltins => true;
    }
}
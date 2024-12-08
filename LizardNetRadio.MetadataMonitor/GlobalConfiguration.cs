namespace LizardNetRadio.MetadataMonitor;

using LizardNetRadio.Common;

public class GlobalConfiguration
{
    public string LogDirectory { get; set; }
    public string LogFilter { get; set; }

    public string AnnounceQueue { get; set; } = "metadata";

    public RabbitMqConfiguration RabbitMqConfiguration { get; set; }
}
namespace LizardNetRadio.Sidecar;

using Common;

public class GlobalConfiguration
{
    public string LiquidSoapHostname { get; set; } = "localhost";
    public ushort LiquidSoapPort { get; set; } = 1234;

    public string RequestQueue { get; set; } = "request";
    
    public RabbitMqConfiguration RabbitMqConfiguration { get; set; }
}
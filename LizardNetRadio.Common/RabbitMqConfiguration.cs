namespace LizardNetRadio.Common;

public class RabbitMqConfiguration
{
    public bool Tls { get; set; } = false;
    public string ObjectPrefix { get; set; }
    public string Hostname { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}
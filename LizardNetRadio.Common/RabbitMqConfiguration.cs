namespace LizardNetRadio.Common;

public class RabbitMqConfiguration
{
    private string password;
    private string username;
    
    public bool Tls { get; set; } = false;
    public string ObjectPrefix { get; set; }
    public string Hostname { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";

    public string Username
    {
        get
        {
            if (string.IsNullOrEmpty(this.username) && Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") != null)
            {
                return Environment.GetEnvironmentVariable("RABBITMQ_USERNAME");
            }

            return this.username;
        }
        set => this.username = value;
    }

    public string Password
    {
        get
        {
            if (string.IsNullOrEmpty(this.password) && Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") != null)
            {
                return Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
            }

            return this.password;
        }
        set => this.password = value;
    }
}
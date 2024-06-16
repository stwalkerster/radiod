namespace LizardNetRadio.Sidecar;

using System.Net.Sockets;

public class LiquidSoapClient
{
    private readonly string hostname;
    private readonly ushort port;

    public LiquidSoapClient(string hostname, ushort port)
    {
        this.hostname = hostname;
        this.port = port;
    }

    private void Log(string message)
    {
        Console.WriteLine(message);
    }
    
    public string DoRpcCall(string command)
    {
        using var client = new TcpClient(this.hostname, this.port);
        using var sr = new StreamReader(client.GetStream());
        using var sw = new StreamWriter(client.GetStream());
        
        this.Log("SEND: " + command);
        sw.WriteLine(command);
        sw.Flush();

        var cache = new List<string>();
        
        while (!sr.EndOfStream)
        {
            var data = sr.ReadLine();
            this.Log("RECV: " + data);
            if (data == "END")
            {
                break;
            }

            cache.Add(data);
        }
        
        sw.WriteLine("exit");
        sw.Flush();
        sw.Close();
        sr.Close();
        client.Close();
        this.Log("RPC done.");

        return string.Join('\n', cache);
    }
}
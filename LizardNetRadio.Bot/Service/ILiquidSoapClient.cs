namespace LizardNetRadio.Bot.Service;

public interface ILiquidSoapClient
{
    Task SkipTrack();

    Task<(string artist, string title)> NowPlaying();

    Task<IEnumerable<string>> Inject(string command);
    Task<int> Request(string file, string queue);

    Task<double?> Remaining();
}
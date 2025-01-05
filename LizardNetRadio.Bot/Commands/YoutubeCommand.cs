namespace LizardNetRadio.Bot.Commands;

using Microsoft.Extensions.Logging;
using Service;
using Stwalkerster.Bot.CommandLib.Attributes;
using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
using Stwalkerster.Bot.CommandLib.Model;
using Stwalkerster.Bot.CommandLib.Services.Interfaces;
using Stwalkerster.IrcClient.Interfaces;
using Stwalkerster.IrcClient.Model.Interfaces;

[CommandInvocation("youtube")]
[CommandInvocation("yt")]
[CommandFlag(Flag.Standard)]
public class YoutubeCommand : CommandBase
{
    private readonly ILiquidSoapClient liquidSoapClient;

    public YoutubeCommand(
        string commandSource,
        IUser user,
        IList<string> arguments,
        ILogger logger,
        IFlagService flagService,
        IConfigurationProvider configurationProvider,
        IIrcClient client,
        ILiquidSoapClient liquidSoapClient) : base(commandSource, user, arguments, logger, flagService, configurationProvider, client)
    {
        this.liquidSoapClient = liquidSoapClient;
    }

    [Help("<URL>", "Adds a YouTube video (or other yt-dlp compatible source) to the request queue.")]
    protected override IEnumerable<CommandResponse> Execute()
    {
        try
        {
            var first = this.Arguments.First().Replace("\\.", ".");
            
            if (first.StartsWith('"') && first.EndsWith('"'))
            {
                first = first.Substring(1, first.Length - 2);
            }
            
            var task = this.liquidSoapClient.Request("youtube-dl:" + first, "request");
            task.Wait();

            return new[] { new CommandResponse { Message = "Queued requested video as ID " + task.Result } };
        }
        catch (Exception e)
        {
            return new CommandResponse[] { new() {Message = e.Message }};
        }
    }
}
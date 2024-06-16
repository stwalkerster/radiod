namespace LizardNetRadio.Bot.Commands;

using Castle.Core.Logging;
using Service;
using Stwalkerster.Bot.CommandLib.Attributes;
using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
using Stwalkerster.Bot.CommandLib.Model;
using Stwalkerster.Bot.CommandLib.Services.Interfaces;
using Stwalkerster.IrcClient.Interfaces;
using Stwalkerster.IrcClient.Model.Interfaces;

[CommandInvocation("playing")]
[CommandFlag(Flag.Standard)]
public class PlayingCommand : CommandBase
{
    private readonly ILiquidSoapClient liquidSoapClient;

    public PlayingCommand(
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

    protected override IEnumerable<CommandResponse> Execute()
    {
        try
        {
            var task = this.liquidSoapClient.NowPlaying();
            var remTask = this.liquidSoapClient.Remaining();

            Task.WaitAll(task, remTask);

            var remaining = new TimeSpan(0, 0, 0, (int)remTask.Result).ToString("mm\\:ss");
            
            return new CommandResponse[]
            {
                new()
                {
                    Message = $"Now Playing: {task.Result.artist} - {task.Result.title} ({remaining} remaining)"
                }
            };
        }
        catch (Exception e)
        {
            return new CommandResponse[] { new() {Message = e.Message }};
        }
    }
}
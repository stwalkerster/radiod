namespace LizardNetRadio.Bot.Commands;

using Microsoft.Extensions.Logging;
using Stwalkerster.Bot.CommandLib.Attributes;
using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
using Stwalkerster.Bot.CommandLib.Model;
using Stwalkerster.Bot.CommandLib.Services.Interfaces;
using Stwalkerster.IrcClient;
using Stwalkerster.IrcClient.Interfaces;
using Stwalkerster.IrcClient.Model.Interfaces;

[CommandFlag(Flag.Owner)]
[CommandInvocation("raw")]
public class RawCommand : CommandBase
{
    public RawCommand(
        string commandSource,
        IUser user,
        IList<string> arguments,
        ILogger logger,
        IFlagService flagService,
        IConfigurationProvider configurationProvider,
        IIrcClient client) : base(commandSource, user, arguments, logger, flagService, configurationProvider, client)
    {
    }

    protected override IEnumerable<CommandResponse> Execute()
    {
        ((IrcClient)this.Client).Inject(this.OriginalArguments);
        yield break;
    }
}
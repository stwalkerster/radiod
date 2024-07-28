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

[CommandInvocation("request")]
[CommandFlag(Flag.Standard)]
public class RequestCommand : CommandBase
{
    private readonly ILiquidSoapClient liquidSoapClient;

    public RequestCommand(
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

    [Help("<path>", "Adds a file to the song request queue. This file must be a local path or HTTP stream.")]
    protected override IEnumerable<CommandResponse> Execute()
    {
        try
        {
            var first = this.Arguments.First().Replace("\\.", ".");
            var task = this.liquidSoapClient.Request(first);
            task.Wait();

            return new[] { new CommandResponse { Message = "Queued requested track as ID " + task.Result } };
        }
        catch (Exception e)
        {
            return new CommandResponse[] { new() {Message = e.Message }};
        }
    }
}
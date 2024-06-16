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

    protected override IEnumerable<CommandResponse> Execute()
    {
        try
        {
            var task = this.liquidSoapClient.Request(this.Arguments.First());
            task.Wait();

            return new[] { new CommandResponse { Message = "Queued requested track as ID " + task.Result } };
        }
        catch (Exception e)
        {
            return new CommandResponse[] { new() {Message = e.Message }};
        }
    }
}
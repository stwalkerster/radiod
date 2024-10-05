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

[CommandInvocation("diggy")]
[CommandInvocation("diggydiggy")]
[CommandFlag(Flag.Standard)]
public class DiggyCommand : CommandBase
{
    private readonly ILiquidSoapClient liquidSoapClient;

    public DiggyCommand(
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
            var task = this.liquidSoapClient.Request("/music/finland/diggy.mp3");
            task.Wait();

            
            var task2 = this.liquidSoapClient.SkipTrack();
            task2.Wait();
            
            return new[] { new CommandResponse { Message = "Dwarf detected!" } };
        }
        catch (Exception e)
        {
            return new CommandResponse[] { new() {Message = e.Message }};
        }
    }
}
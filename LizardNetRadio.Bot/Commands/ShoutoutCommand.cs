namespace LizardNetRadio.Bot.Commands;

using LizardNetRadio.Bot.Service;
using Microsoft.Extensions.Logging;
using Stwalkerster.Bot.CommandLib.Attributes;
using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities;
using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
using Stwalkerster.Bot.CommandLib.ExtensionMethods;
using Stwalkerster.Bot.CommandLib.Model;
using Stwalkerster.Bot.CommandLib.Services.Interfaces;
using Stwalkerster.IrcClient.Interfaces;
using Stwalkerster.IrcClient.Model.Interfaces;

[CommandInvocation("shoutout")]
[CommandFlag(Flag.Standard)]
public class ShoutoutCommand : CommandBase
{
    private readonly ITextToSpeechService ttsService;
    private readonly ILiquidSoapClient liquidSoapClient;

    public ShoutoutCommand(
        string commandSource,
        IUser user,
        IList<string> arguments,
        ILogger logger,
        IFlagService flagService,
        IConfigurationProvider configurationProvider,
        IIrcClient client,
        ITextToSpeechService ttsService,
        ILiquidSoapClient liquidSoapClient) 
        : base(commandSource, user, arguments, logger, flagService, configurationProvider, client)
    {
        this.ttsService = ttsService;
        this.liquidSoapClient = liquidSoapClient;
    }

    [RequiredArguments(1)]
    [Help("<shoutout>", "Prints the currently playing track")]
    [CommandParameter("voice=", "Use the specified voice.", "voice", typeof(string))]
    protected override IEnumerable<CommandResponse> Execute()
    {
        var voice = this.Parameters.GetParameter("voice", string.Empty);
        
        var uriTask = this.ttsService.SpeakAsync(string.Join(' ', this.Arguments), voice);
        uriTask.Wait();

        var annotate = $"annotate:artist=\"{this.User.Nickname}\",title=\"Shoutout!\":";
        
        var task = this.liquidSoapClient.Request(annotate + uriTask.Result, "shoutouts");

        return new[] { new CommandResponse { Message = "Queued requested shoutout as ID " + task.Result } };
    }
}
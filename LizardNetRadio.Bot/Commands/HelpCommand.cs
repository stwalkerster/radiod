namespace LizardNetRadio.Bot.Commands
{
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Extensions.Logging;
    using Stwalkerster.Bot.CommandLib.Attributes;
    using Stwalkerster.Bot.CommandLib.Commands.CommandUtilities.Response;
    using Stwalkerster.Bot.CommandLib.ExtensionMethods;
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model.Interfaces;

    [CommandFlag(Flag.Standard)]
    [CommandInvocation("help")]
    public class HelpCommand : Stwalkerster.Bot.CommandLib.Commands.HelpCommand
    {
        private readonly IFlagService flagService;
        private readonly ICommandParser commandParser;
        
        public HelpCommand(
            string commandSource,
            IUser user,
            IList<string> arguments,
            ILogger logger,
            IFlagService flagService,
            IConfigurationProvider configurationProvider,
            ICommandParser commandParser,
            IIrcClient client)
            : base(commandSource, user, arguments, logger, flagService, configurationProvider, commandParser, client)
        {
            this.flagService = flagService;
            this.commandParser = commandParser;
        }

        [Help("[command]")]
        protected override IEnumerable<CommandResponse> Execute()
        {
            foreach (var x in base.Execute())
            {
                x.Type = CommandResponseType.Message;
                x.Destination = CommandResponseDestination.Default;
                yield return x;
            }
        }

        protected override IEnumerable<CommandResponse> OnNoArguments()
        {
            var commandRegistrations = this.commandParser.GetCommandRegistrations();

            var regSets = new Dictionary<Type, (List<string> aliases, string help)>();
            foreach (var reg in commandRegistrations)
            {
                var (_, type) = reg.Value.FirstOrDefault(kvp=> kvp.Key.Channel == null);

                if (!regSets.ContainsKey(type))
                {
                    var commandFlagAttribute = type.GetAttribute<CommandFlagAttribute>();
                    var requiredFlag = Flag.Owner;
                    if (commandFlagAttribute != null)
                    {
                        requiredFlag = commandFlagAttribute.Flag;
                    }

                    if (!this.flagService.UserHasFlag(this.User, requiredFlag, null))
                    {
                        continue;
                    }

                    var helpMessage = string.Empty;
                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);
                    foreach (var mi in methods)
                    {
                        if (mi.Name != nameof(this.Execute)) continue;

                        var helpAttribute = mi.GetAttribute<HelpAttribute>();
                        if (helpAttribute == null) continue;
                        
                        helpMessage = helpAttribute.HelpMessage.Text.FirstOrDefault(string.Empty);
                    }
                    
                    regSets.Add(type, ([], helpMessage));
                }
                
                regSets[type].aliases.Add(reg.Key);
            }

            foreach (var set in regSets)
            {
                if (set.Value.help == string.Empty)
                {
                    continue;
                }
                
                yield return new CommandResponse
                {
                    Message = string.Join(", ", set.Value.aliases) + ": " + set.Value.help
                };
            }
        }
    }
}
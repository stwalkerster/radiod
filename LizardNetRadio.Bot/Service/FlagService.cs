namespace LizardNetRadio.Bot.Service
{
    using Stwalkerster.Bot.CommandLib.Model;
    using Stwalkerster.Bot.CommandLib.Services.Interfaces;
    using Stwalkerster.IrcClient.Interfaces;
    using Stwalkerster.IrcClient.Model;
    using Stwalkerster.IrcClient.Model.Interfaces;

    public class FlagService : IFlagService
    {
        private readonly IrcUserMask staffMask;
        public FlagService(GlobalConfiguration configuration, IIrcClient client)
        {
            this.staffMask = new IrcUserMask(configuration.AdminMask, client);
        }
        
        public bool UserHasFlag(IUser user, string flag, string locality)
        {
            if (this.staffMask.Matches(user).GetValueOrDefault(false))
            {
                return true;
            }

            return flag == Flag.Standard;
        }
        
        public IEnumerable<string> GetFlagsForUser(IUser user, string locality)
        {
            if (this.staffMask.Matches(user).GetValueOrDefault(false))
            {
                return new[] {Flag.Owner, Flag.Standard};
            }

            return Array.Empty<string>();        }
    }
}
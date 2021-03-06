using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SimpleWarps.Provider;
using System;
using System.Threading.Tasks;

namespace SimpleWarps.Commands
{
    [Command("warps")]
    [CommandDescription("See all warps")]
    public class CommandWarps : Command
    {
        private readonly IWarpManager m_WarpManager;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandWarps(IServiceProvider serviceProvider, IWarpManager warpManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_WarpManager = warpManager;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;

            if (await m_WarpManager.GetAllWarps() == "")
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:no_warps"]);
            }
            else
            {
                await user.PrintMessageAsync(await m_WarpManager.GetAllWarps());
            }
        }
    }
}

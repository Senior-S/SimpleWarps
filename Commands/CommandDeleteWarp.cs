using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SimpleWarps.Provider;
using System;
using System.Threading.Tasks;

namespace SimpleWarps.Commands
{
    [Command("deletewarp")]
    [CommandSyntax("<warp name>")]
    [CommandDescription("Delete a warp.")]
    public class CommandDeleteWarp : Command
    {
        private readonly IWarpManager m_WarpManager;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandDeleteWarp(IServiceProvider serviceProvider, IWarpManager warpManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_WarpManager = warpManager;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            string name = await Context.Parameters.GetAsync<string>(0);
            if (await m_WarpManager.TryRemoveWarp(name))
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:warp_removed"]);
            }
            else
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:warp_dont_exist", new { name = name }]);
            }
        }
    }
}

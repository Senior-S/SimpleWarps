using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Users;
using SimpleWarps.Provider;
using System;
using System.Threading.Tasks;

namespace SimpleWarps.Commands
{
    [Command("warp")]
    [CommandSyntax("<warp name>")]
    [CommandDescription("Teleport to a designed warp.")]
    public class CommandWarp : Command
    {
        private readonly IWarpManager m_WarpManager;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandWarp(IServiceProvider serviceProvider, IWarpManager warpManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_WarpManager = warpManager;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            string warpName = await Context.Parameters.GetAsync<string>(0);
            if (await m_WarpManager.CheckPermission(warpName, user))
            {
                if (await m_WarpManager.CheckWarpCooldown(warpName, ulong.Parse(user.SteamId.ToString())))
                {
                    await m_WarpManager.UseWarp(warpName, ulong.Parse(user.SteamId.ToString()), user);
                    await user.PrintMessageAsync(m_StringLocalizer["translations:warp_used", new { name = warpName }]);
                }
                else
                {
                    await user.PrintMessageAsync(m_StringLocalizer["translations:warp_cooldown"]);
                }
            }
            else
            {
                await user.PrintMessageAsync(m_StringLocalizer["translations:warp_error"]);
            }
        }
    }
}

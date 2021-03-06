using Command = OpenMod.Core.Commands.Command;
using System;
using System.Threading.Tasks;
using OpenMod.Core.Commands;
using SimpleWarps.Provider;
using OpenMod.Unturned.Users;
using Microsoft.Extensions.Localization;

namespace SimpleWarps.Commands
{
    [Command("addwarp")] 
    [CommandSyntax("<warpName> [permission] [cooldown]")]
    [CommandDescription("Add a warp in your position.")]
    public class CommandAddWarp : Command
    {
        private readonly IWarpManager m_WarpManager;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandAddWarp(IServiceProvider serviceProvider, IWarpManager warpManager, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_WarpManager = warpManager;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            UnturnedUser user = (UnturnedUser)Context.Actor;
            string name = await Context.Parameters.GetAsync<string>(0);
            if (Context.Parameters.Count == 1)
            {
                if (await m_WarpManager.TryAddWarp(name, new System.Numerics.Vector3(user.Player.Transform.Position.X, user.Player.Transform.Position.Y, user.Player.Transform.Position.Z)))
                {
                    await user.PrintMessageAsync(m_StringLocalizer["translations:warp_added"]);
                }
                else
                {
                    await user.PrintMessageAsync(m_StringLocalizer["translations:warp_already_exist"]);
                }
            }
            else if (Context.Parameters.Count == 2)
            {
                string perm = await Context.Parameters.GetAsync<string>(1);
                if (await m_WarpManager.TryAddWarp(name, new System.Numerics.Vector3(user.Player.Transform.Position.X, user.Player.Transform.Position.Y, user.Player.Transform.Position.Z), perm))
                {
                    await user.PrintMessageAsync(m_StringLocalizer["translations:warp_added"]);
                }
                else
                {
                    await user.PrintMessageAsync(m_StringLocalizer["translations:warp_already_exist"]);
                }
            }
            else
            {
                string perm = await Context.Parameters.GetAsync<string>(1);
                int cooldown = await Context.Parameters.GetAsync<int>(2);
                if (await m_WarpManager.TryAddWarp(name, new System.Numerics.Vector3(user.Player.Transform.Position.X, user.Player.Transform.Position.Y, user.Player.Transform.Position.Z), perm, cooldown))
                {
                    await user.PrintMessageAsync(m_StringLocalizer["translations:warp_added"]);
                }
                else
                {
                    await user.PrintMessageAsync(m_StringLocalizer["translations:warp_already_exist"]);
                }
            }
        }
    }
}

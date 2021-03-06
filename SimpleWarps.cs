using System;
using Microsoft.Extensions.Logging;
using Cysharp.Threading.Tasks;
using OpenMod.Unturned.Plugins;
using OpenMod.API.Plugins;
using OpenMod.API.Persistence;
using SimpleWarps.Provider;
using System.Collections.Generic;
using SimpleWarps.Models;
using OpenMod.API.Permissions;
using OpenMod.Unturned.Users;
using System.Threading.Tasks;

[assembly: PluginMetadata("SS.SimpleWarps", DisplayName = "SimpleWarps", Author = "Senior S")]
namespace SimpleWarps
{
    public class SimpleWarps : OpenModUnturnedPlugin
    {
        private readonly ILogger<SimpleWarps> m_Logger;
        private readonly IDataStore m_DataStore;
        private readonly IPermissionRegistry m_PermissionRegistry;
        private readonly IPermissionChecker m_PermissionChecker;

        public SimpleWarps(
            ILogger<SimpleWarps> logger,
            IDataStore dataStore,
            IPermissionRegistry permissionRegistry,
            IPermissionChecker permissionChecker,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Logger = logger;
            m_DataStore = dataStore;
            m_PermissionRegistry = permissionRegistry;
            m_PermissionChecker = permissionChecker;
        }

        protected override async UniTask OnLoadAsync()
        {
            if (!await m_DataStore.ExistsAsync(WarpManager.CKEY))
            {
                await m_DataStore.SaveAsync(WarpManager.CKEY, new List<User>());
            }
            if (!await m_DataStore.ExistsAsync(WarpManager.WKEY))
            {
                await m_DataStore.SaveAsync(WarpManager.WKEY, new List<Warp>());
            }
            var warps = await m_DataStore.LoadAsync<List<Warp>>(WarpManager.WKEY);
            if (warps != null)
            {
                warps.ForEach(wp =>
                {
                    if (wp.Permission != "") m_PermissionRegistry.RegisterPermission(this, wp.Permission, $"Permission to warp to {wp.Name}");
                });
            }

            m_Logger.LogInformation(" Plugin loaded correctly!");
            m_Logger.LogInformation(" Made by Senior S");
        }

        public async Task<bool> CheckPermisison(string perm, UnturnedUser user)
        {
            if (await m_PermissionChecker.CheckPermissionAsync(user, perm) == PermissionGrantResult.Grant)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override async UniTask OnUnloadAsync()
        {
            m_Logger.LogInformation(" Plugin unloaded correctly!");
        }
    }
}

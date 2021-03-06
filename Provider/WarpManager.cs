using Cysharp.Threading.Tasks;
using OpenMod.API.Ioc;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Users;
using SDG.Unturned;
using SimpleWarps.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SimpleWarps.Provider
{
    [Service]
    public interface IWarpManager
    {
        public Task<bool> TryAddWarp(string name, Vector3 position, string perm = "", int cooldown = 0);

        public Task<bool> TryRemoveWarp(string name);

        public Task<bool> CheckWarpCooldown(string name, ulong ownerId);

        public Task<bool> UseWarp(string name, ulong ownerId, UnturnedUser user);

        public Task<bool> CheckPermission(string name, UnturnedUser user);

        public Task<string> GetAllWarps();
    }

    [ServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, Priority = OpenMod.API.Prioritization.Priority.Low)]
    public class WarpManager : IWarpManager, IDisposable
    {
        private List<Warp> m_WarpsCache = new List<Warp>();
        private List<User> m_CooldownsCache = new List<User>();
        
        private readonly IPluginAccessor<SimpleWarps> m_PluginAccessor;

        public WarpManager(IPluginAccessor<SimpleWarps> pluginAccessor)
        {
            m_PluginAccessor = pluginAccessor;
        }

        public async Task<bool> TryAddWarp(string name, Vector3 position, string perm = "", int cooldown = 0)
        {
            await ReadData();
            if (!m_WarpsCache.Any(w => w.Name.ToLower() == name.ToLower()))
            {
                m_WarpsCache.Add(new Warp
                {
                    Name = name,
                    Permission = perm,
                    Position = position,
                    Cooldown = cooldown
                });
 
                await SaveAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> TryRemoveWarp(string name)
        {
            await ReadData();
            if (m_WarpsCache.Any(w => w.Name.ToLower() == name.ToLower()))
            {
                var warp = m_WarpsCache.Where(w => w.Name.ToLower() == name.ToLower()).First();
                m_WarpsCache.Remove(warp);
                await SaveAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> GetAllWarps()
        {
            await ReadData();
            if (m_WarpsCache == null || m_WarpsCache.Count == 0)
            {
                return "";
            }
            string warps = "Warps: ";
            m_WarpsCache.ForEach(wc => 
            {
                if (m_WarpsCache.Last() == wc) 
                {
                    warps += $"{wc.Name}";
                }
                else
                {
                    warps += $"{wc.Name}, ";
                }
            });
            warps.Substring(0, warps.Length - 2);

            return warps;
            
        }

        public async Task<bool> UseWarp(string name, ulong ownerId, UnturnedUser user)
        {
            await ReadData();
            await UniTask.SwitchToMainThread();
            var player = PlayerTool.getPlayer(new Steamworks.CSteamID(ownerId));
            if (player != null) 
            {
                var warp = m_WarpsCache.Where(k => k.Name.ToLower() == name.ToLower()).FirstOrDefault();
                if (warp != null)
                {
                    await user.PrintMessageAsync("You will be teleport in 3 seconds.");
                    await UniTask.Delay(TimeSpan.FromSeconds(1));
                    await user.PrintMessageAsync("You will be teleport in 2 seconds.");
                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    var cooldown = m_CooldownsCache.Where(cc => cc.SteamId == ownerId).First().warpCooldowns;
                    var warpCooldown = cooldown.Where(wp => wp.WarpName.ToLower() == name.ToLower()).FirstOrDefault();
                    if (warpCooldown == null) cooldown.Add(new WarpCooldown { WarpName = name, LastUsed = DateTime.Now });
                    else warpCooldown.LastUsed = DateTime.Now;

                    player.teleportToLocationUnsafe(new UnityEngine.Vector3(warp.Position.X, warp.Position.Y, warp.Position.Z), player.look.yaw);

                    

                    await SaveAsync();

                    return true;
                }
            }
            return false;
        }

        public async Task<bool> CheckPermission(string name, UnturnedUser user)
        {
            await ReadData();
            var warp = m_WarpsCache.Where(wc => wc.Name.ToLower() == name.ToLower()).FirstOrDefault();
            if (warp != null)
            {
                if (warp.Permission == null || warp.Permission == "")
                {
                    return true;
                }
                else
                {
                    if(m_PluginAccessor.Instance != null)
                        return await m_PluginAccessor.Instance.CheckPermisison(warp.Permission, user);
                }
            }
            return false;
        }

        public async Task<bool> CheckWarpCooldown(string name, ulong ownerId)
        {
            await ReadData();
            var user = m_CooldownsCache.Where(k => k.SteamId == ownerId).Count();
            if (user == 0)
            {
                m_CooldownsCache.Add(new User { SteamId = ownerId, warpCooldowns = new List<WarpCooldown>() });
                await SaveAsync();
                return true;
            }
            else
            {
                var warp = m_CooldownsCache.Where(x => x.SteamId == ownerId).First().warpCooldowns.Where(wc => wc.WarpName.ToLower() == name.ToLower()).FirstOrDefault();
                if (warp == null)
                {
                    return true;
                }
                else
                {
                    var cooldown = m_WarpsCache.Where(wc => wc.Name.ToLower() == name.ToLower()).First();
                    if ((DateTime.Now - warp.LastUsed).TotalSeconds > cooldown.Cooldown)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private async Task SaveAsync()
        {
            if (m_PluginAccessor != null && m_PluginAccessor.Instance != null)
            {
                await m_PluginAccessor.Instance.DataStore.SaveAsync(WKEY, m_WarpsCache);
                await m_PluginAccessor.Instance.DataStore.SaveAsync(CKEY, m_CooldownsCache);
            }
        }

        private async Task ReadData()
        {
            if (m_PluginAccessor != null && m_PluginAccessor.Instance != null)
            {
                m_WarpsCache = await m_PluginAccessor.Instance.DataStore.LoadAsync<List<Warp>>(WKEY)
                            ?? new List<Warp>();
                m_CooldownsCache = await m_PluginAccessor.Instance.DataStore.LoadAsync<List<User>>(CKEY)
                            ?? new List<User>();
            }
            else
            {
                m_WarpsCache = new List<Warp>();
                m_CooldownsCache = new List<User>();
            }
        }

        public void Dispose()
        {
            m_WarpsCache = new List<Warp>();
            m_CooldownsCache = new List<User>();
        }

        public const string WKEY = "Warps";
        public const string CKEY = "UsersCooldowns";
    }
}

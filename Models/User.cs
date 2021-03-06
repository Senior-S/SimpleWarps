using System.Collections.Generic;

namespace SimpleWarps.Models
{
    public class User
    {
        public ulong SteamId { get; set; }

        public List<WarpCooldown> warpCooldowns { get; set; } = new List<WarpCooldown>();
    }
}

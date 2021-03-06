using System;

namespace SimpleWarps.Models
{
    public class WarpCooldown
    {
        public string WarpName { get; set; } = "";
        
        public DateTime LastUsed { get; set; }
    }
}

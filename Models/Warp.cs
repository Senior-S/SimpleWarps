using System.Numerics;

namespace SimpleWarps.Models
{
    public class Warp
    {
        public string Name { get; set; } = "";

        public Vector3 Position { get; set; }

        public string Permission { get; set; } = "";

        public int Cooldown { get; set; } = 0;
    }
}

using Godot;

namespace Safelight.Props
{
    public class SolarPanel : Buildable
    {
        public int EuTick { get; set; } = 1;

        public override void _Ready()
        {
            WorldState.I.Connect("WorldTick", this, "OnTick");
        }

        public void OnTick()
        {
            if (this.Alive) WorldState.I.AddPower(this.EuTick);
        }
    }
}

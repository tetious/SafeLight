using Godot;

namespace Safelight.Props
{
    public class Buildable : Area2D
    {
        public int MaxHp { get; set; } = 1000;

        public int Hp { get; set; }

        public bool Alive => this.Hp > 0;
    }
}

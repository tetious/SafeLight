using Godot;

namespace Safelight.Props
{
    public class Wall : Buildable
    {
        [Export]
        public int tileIndex { get; set; }
    }
}

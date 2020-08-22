using Godot;

namespace Safelight.Props
{
    public class SafeLight : Light2D
    {
        public WorldManager World { get; private set; }

        private float lightDistance;

        public override void _Ready()
        {
            this.World = (WorldManager)this.FindParent("WorldManager");
            this.lightDistance = this.TextureScale * 48;
        }

        public Rect2 LightRect => new Rect2(this.GlobalPosition - new Vector2(this.lightDistance, this.lightDistance) / 2,
            new Vector2(this.lightDistance, this.lightDistance));

        public override void _Draw()
        {
            return;
            if (this.World.DEBUG == false) return;
            this.DrawSetTransform(new Vector2() - GlobalPosition, 0, Vector2.One);
            this.DrawRect(this.LightRect, Colors.Yellow, false);
        }
    }
}

using Godot;

namespace Safelight.Actors
{
    public class Spawner : Area2D
    {
        private int msSince = 0;

        public WorldManager World { get; private set; }

        private RandomNumberGenerator rng = new RandomNumberGenerator();

        private PackedScene mobScene = GD.Load<PackedScene>("Actors/Mob.tscn");
        private Node2D mobGroup;

        public override void _Ready()
        {
            this.World = (WorldManager)this.FindParent("WorldManager");
            this.mobGroup = this.World.GetNode<Node2D>("Mobs");
        }

        public override void _PhysicsProcess(float delta)
        {
            this.msSince += (int)(delta * 1000);
            if (this.msSince > 100)
            {
                this.msSince = 0;
                var mobCount = this.GetOverlappingBodies().Count;
                if (mobCount < 20)
                {
                    GD.Print("SPAWNING");
                    var mob = this.mobScene.Instance() as Mob;
                    var area = mob.GetNode<Area2D>("Area2D");

                    var spot = new Vector2(this.rng.RandiRange(-400, 400), this.rng.RandiRange(-400, 400)) + this.GlobalPosition;
                    mob.GlobalPosition = spot;
                    var overlappingCount = area.GetOverlappingBodies().Count + area.GetOverlappingAreas().Count;
                    if (overlappingCount > 0)
                    {
                        while (this.World.HasLineOfSightToLight(spot) && overlappingCount > 0)
                        {
                            overlappingCount = area.GetOverlappingBodies().Count + area.GetOverlappingAreas().Count;
                            mob.GlobalPosition = spot;
                            spot = new Vector2(this.rng.RandiRange(-400, 400), this.rng.RandiRange(-400, 400)) + this.GlobalPosition;
                        }
                    }
                    area.QueueFree();
                    this.mobGroup.AddChild(mob);
                }
            }
        }
    }
}

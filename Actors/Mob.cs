using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Safelight.Props;


namespace Safelight.Actors
{
    public class Mob : KinematicBody2D, IPathed
    {
        public WorldManager World { get; private set; }

        public int WalkSpeed { get; } = 100;

        public Vector2 TargetLight = Vector2.Zero;

        public List<Vector2> Path { get; set; } = new List<Vector2>();

        public Vector2 PathGoal { get; set; }

        [Export]
        public int SightDistance { get; set; } = 400;

        public List<(Vector2 source, Vector2 dest, Dictionary obstacle)> LightTargets { get; } = new List<(Vector2, Vector2, Dictionary)>();

        public Rect2 SightRect => new Rect2(this.GlobalPosition - new Vector2(this.SightDistance, this.SightDistance) / 2,
            new Vector2(this.SightDistance, this.SightDistance));

        private readonly BehaviorTreeTask root;

        private float distanceMoved = 0;

        private bool HasMovedOneTile
        {
            get
            {
                if (this.distanceMoved > 32)
                {
                    this.distanceMoved = 0;
                    return true;
                }

                return false;
            }
        }

        public bool IsPathing => this.Path.Any() || this.PathGoal != Vector2.Zero;

        public Mob()
        {
            // move toward by default, build path if we get stuck
            this.root = new ParallelSequence(null, new FindLight(this, () => !this.IsPathing || this.HasMovedOneTile), new MoveAlongPathTask<Mob>(this));
        }

        public override void _Ready()
        {
            this.World = (WorldManager)this.FindParent("WorldManager");
//            WorldState.I.Connect("WorldTick", this, "WorldTick");
        }

        public override void _Process(float delta)
        {
            //if (this.Path.Count != 0) this.MoveAlongPath(this.WalkSpeed * delta);
        }

        public override void _Draw()
        {
            if (this.World.DEBUG == false) return;

            this.DrawSetTransform(new Vector2() - this.GlobalPosition, 0, Vector2.One);
            if (this.Path.Any()) this.DrawCircle(this.Path.Last(), 2, Colors.Blue);
            this.DrawRect(this.SightRect, Colors.YellowGreen, false);

            foreach (var target in this.LightTargets)
            {
                if (target.obstacle.Count == 0)
                {
                    this.DrawLine(target.source, target.dest, Colors.Green);
                }
                else
                {
                    this.DrawLine(target.source, (Vector2)target.obstacle["position"], Colors.Red);
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            if (this.PathGoal != Vector2.Zero)
            {
                var start = this.Position;
                var toNext = start.DistanceTo(this.PathGoal);
                if (toNext < 1)
                {
                    this.Position = this.PathGoal;
                }
                else
                {
                    this.Position = this.Position.LinearInterpolate(this.PathGoal, this.WalkSpeed * delta / toNext);
                }

                if (this.Position == this.PathGoal)
                {
                    GD.Print("Hit dest! ", this.PathGoal);
                    this.PathGoal = Vector2.Zero;
                }

                this.distanceMoved += this.Position.DistanceTo(start);
            }

            this.root.Run();
            this.Update();
        }
    }
}

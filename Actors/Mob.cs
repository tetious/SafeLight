using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;


namespace Safelight.Actors
{
    public class Mob : KinematicBody2D, IPathed, ITargeted
    {
        public WorldManager World { get; private set; }

        public int WalkSpeed { get; } = 100;

        public Vector2 Target { get; set; } = Vector2.Zero;

        public List<Vector2> Path { get; set; } = new List<Vector2>();

        public Vector2 PathSegmentGoal { get; set; }

        [Export]
        public int SightDistance { get; set; } = 400;

        [Signal]
        public delegate void Hit(int damage);

        public Rect2 SightRect => new Rect2(this.GlobalPosition - new Vector2(this.SightDistance, this.SightDistance) / 2,
            new Vector2(this.SightDistance, this.SightDistance));

        public List<(Vector2 source, Vector2 dest, Dictionary obstacle)> LightTargets { get; } = new List<(Vector2, Vector2, Dictionary)>();

        private readonly BehaviorTreeTask root;

        public Mob()
        {
            var path = new Selector(new MoveTowardPathSegmentGoalTask<Mob>(this), new NextPathSegmentTask<Mob>(this));

            var move = new Selector(new MoveTowardTarget<Mob>(this), path);
            this.root = new Sequence(move, new FindLight(this));
        }

        public override void _Ready()
        {
            this.World = (WorldManager)this.FindParent("WorldManager");
            this.Connect("Hit", this, "OnHit");
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
            this.root.Run(delta);
            this.Update();
        }

        public void OnHit(int damage) => GD.Print("HIT!!! ", damage);

    }
}

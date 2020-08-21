using System;
using System.Collections.Generic;
using Godot;
using System.Linq;

public enum BotType { None, Gatherer }

namespace Safelight.Actors
{
    public class StandingOnResourceTask : BehaviorTreeTask
    {
        public StandingOnResourceTask(Node node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run()
        {
            var bot = this.Node as Bot;
            var overlappingAreas = bot.GetOverlappingAreas().Cast<Area2D>();
            if (overlappingAreas.Any(a => a is WorldResource))
            {
                GD.Print("StandingOnResourceTask: Standing on a resource!");
                this.Status = TaskStatus.Succeeded;
            }
            else
            {
                GD.Print("StandingOnResourceTask: NOT Standing on a resource!");
                this.Status = TaskStatus.Failed;
            }
        }
    }

    public class MoveAlongPathTask : BehaviorTreeTask
    {
        public MoveAlongPathTask(Node node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run()
        {
            GD.Print("MoveAlongPathTask");
            var bot = this.Node as Bot;
            this.Status = bot.Path.Any() ? TaskStatus.Running : TaskStatus.Succeeded;
        }
    }

    public class GatherResourceTask : BehaviorTreeTask
    {
        public GatherResourceTask(Node node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run()
        {
            GD.Print("GatherResourceTask");
            this.Status = TaskStatus.Succeeded;
        }
    }

    public class FindNearestResource : BehaviorTreeTask
    {
        public FindNearestResource(Node node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run()
        {
            var bot = this.Node as Bot;
            var resources = bot.World.GetNode("Map/Resources").GetChildren().Cast<WorldResource>().ToArray();
            if (resources.Length > 0)
            {
                var closest = resources.First();
                foreach (var resource in resources)
                {
                    if (resource.GlobalPosition.DistanceTo(bot.GlobalPosition) < closest.GlobalPosition.DistanceTo(bot.GlobalPosition))
                    {
                        closest = resource;
                    }
                }

                bot.Path = bot.World.GetNode<Map>("Map").GetPath(bot.Position, closest.Position).ToList();
                GD.Print("FindNearestResource: Found a resource!");

                this.Status = TaskStatus.Succeeded;
            }
            else
            {
                GD.Print("FindNearestResource: Didn't find a resource!");
                this.Status = TaskStatus.Failed;
            }
        }
    }

    public class Bot : Area2D
    {
        public WorldManager World { get; private set; }

        private readonly Selector root;

        public Bot()
        {
            this.root = new Selector();
            var gatherer = new Selector(() => this.Type == BotType.Gatherer,
                new Sequence(null, new StandingOnResourceTask(this), new GatherResourceTask(this)),
                new FindNearestResource(this)
            );
            this.root.AddChildren(new MoveAlongPathTask(this, () => this.Path.Any()), gatherer);
        }

        [Export]
        public int WalkSpeed { get; set; } = 400;

        [Export]
        public BotType Type { get; set; }

        public List<Vector2> Path { get; set; } = new List<Vector2>();

        public override void _Ready()
        {
            this.World = (WorldManager)this.FindParent("WorldManager");
            WorldState.I.Connect("WorldTick", this, "WorldTick");
        }

        public override void _Draw() { }

        public override void _Process(float delta)
        {
            if (this.Path.Count != 0) this.MoveAlongPath(this.WalkSpeed * delta);
        }

        private void WorldTick()
        {
            this.root.Run();
        }

        private void MoveAlongPath(float distance)
        {
            var start = this.Position;
            for (var i = 0; i < this.Path.Count; i++)
            {
                var tip = this.Path.First();
                var toNext = start.DistanceTo(tip);
                if (distance <= toNext && distance >= 0)
                {
                    this.Position = start.LinearInterpolate(tip, distance / toNext);
                    break;
                }

                if (distance < 0)
                {
                    this.Position = tip;
                }

                distance -= toNext;
                start = tip;
                this.Path.RemoveAt(0);
            }
        }
    }
}

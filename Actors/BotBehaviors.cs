using System;
using System.Linq;
using Godot;

namespace Safelight.Actors
{
    public class DelayTask : BehaviorTreeTask
    {
        private readonly Func<int> delayMs;

        public DelayTask(Func<int> delayMs, Func<bool> guard = null) : base(guard)
        {
            this.delayMs = delayMs;
        }

        private float delay = 0;

        public override void Run(float delta)
        {
            this.delay += delta * 1000;

            this.Status = TaskStatus.Running;
            if (this.delay >= this.delayMs()) this.Status = TaskStatus.Succeeded;
        }

        public override void Reset()
        {
            base.Reset();
            this.delay = 0;
        }
    }

    public class ShootNearestBaddie : BehaviorTreeTask<Bot>
    {
        public ShootNearestBaddie(Bot node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run(float delta)
        {
            var me = this.Node;

            if (me.VisibleMobs.Any() == false) return;

            var nearest = me.VisibleMobs.First();
            foreach (var mob in me.VisibleMobs.Skip(1))
            {
                if (mob.GlobalPosition.DistanceTo(me.GlobalPosition) < nearest.GlobalPosition.DistanceTo(me.GlobalPosition))
                {
                    nearest = mob;
                }
            }

            GD.Print($"[{delta}]{me.GetInstanceId()}: shooting {nearest.GetInstanceId()}");
            me.FireAt(nearest.GlobalPosition);

            this.Status = TaskStatus.Succeeded;
        }
    }

    public class StandingOnResourceTask : BehaviorTreeTask<Bot>
    {
        public StandingOnResourceTask(Bot node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run(float delta)
        {
            var overlappingAreas = this.Node.GetOverlappingAreas().Cast<Area2D>();
            var resource = overlappingAreas.OfType<WorldResource>().FirstOrDefault();
            if (resource != null)
            {
                this.Node.ResourceAtFoot = resource;
                this.Status = TaskStatus.Succeeded;
            }
            else
            {
                GD.Print("StandingOnResourceTask: NOT Standing on a resource!");
                this.Node.ResourceAtFoot = null;
                this.Status = TaskStatus.Failed;
            }
        }
    }

    public class GatherResourceTask : BehaviorTreeTask<Bot>
    {
        public GatherResourceTask(Bot node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run(float delta)
        {
            if (this.Node?.ResourceAtFoot == null)
            {
                this.Status = TaskStatus.Failed;
                return;
            }

            WorldState.I.AddCrystals(this.Node.ResourceAtFoot.PopResources());
            this.Status = TaskStatus.Succeeded;
        }
    }

    public class FindNearestResource : BehaviorTreeTask<Bot>
    {
        public FindNearestResource(Bot node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run(float delta)
        {
            var bot = this.Node;
            var resources = bot.World.GetNode("Map/Resources").GetChildren().Cast<WorldResource>().Where(r => r.Claimed == false).ToArray();
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
                closest.Claimed = true;
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
}

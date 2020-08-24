using System;
using System.Linq;
using Godot;
using Safelight.Props;

namespace Safelight.Actors
{
    public class BuildBuildable : BehaviorTreeTask<Bot>
    {
        public BuildBuildable(Bot node, Func<BehaviorTreeTask, bool> guard = null) : base(node, guard) { }

        public override void Run(float delta)
        {
            var me = this.Node;

            var overlappingAreas = this.Node.GetOverlappingAreas();
            var toBuild = overlappingAreas.OfType<Buildable>().FirstOrDefault(a => a.GetParent().Name == "ToBuild");
            if (toBuild != null)
            {
                GD.Print($"BuildBuildable: Standing on a {toBuild.Name} buildable!");
                var cost = WorldState.I.Cost[toBuild.Name];
                if (toBuild.Hp == 0) WorldState.I.SubtractCrystals(cost);
                toBuild.Hp += toBuild.MaxHp / 100;
                this.Status = TaskStatus.Running;
                if (toBuild.Hp == toBuild.MaxHp)
                {
                    this.Status = TaskStatus.Succeeded;
                    me.World.MarkBuildComplete(toBuild);
                }
            }
            else
            {
                GD.Print("BuildBuildable: NOT Standing on a buildable!");
                this.Status = TaskStatus.Failed;
            }
        }
    }

    public class ShootNearestBaddie<T> : BehaviorTreeTask<T>
        where T : Node2D, IShoot
    {
        public ShootNearestBaddie(T node, Func<BehaviorTreeTask, bool> guard = null) : base(node, guard) { }

        public override void Run(float delta)
        {
            var me = this.Node;

            if (me.VisibleMobs.Any() == false) return;

            Mob nearest = null;
            foreach (var mob in me.VisibleMobs)
            {
                if (me.World.HasLineOfSightToPoint(me, me.GlobalPosition, mob.GlobalPosition) &&
                    (nearest == null ||
                        mob.GlobalPosition.DistanceTo(me.GlobalPosition) < nearest.GlobalPosition.DistanceTo(me.GlobalPosition)))
                {
                    nearest = mob;
                }
            }

            if (nearest != null)
            {
                //GD.Print($"[{delta}]{me.GetInstanceId()}: shooting {nearest.GetInstanceId()}");
                me.FireAt(nearest.GlobalPosition);
                this.Status = TaskStatus.Succeeded;
            }
            else
            {
                //GD.Print($"[{delta}]{me.Name}: no LoS to any mobs.");
                this.Status = TaskStatus.Failed;
            }
        }
    }

    public class StandingOnResourceTask : BehaviorTreeTask<Bot>
    {
        public StandingOnResourceTask(Bot node, Func<BehaviorTreeTask, bool> guard = null) : base(node, guard) { }

        public override void Run(float delta)
        {
            var overlappingAreas = this.Node.GetOverlappingAreas().Cast<Area2D>();
            var resource = overlappingAreas.OfType<WorldResource>().FirstOrDefault();
            if (resource != null)
            {
                if (this.Node.ResourceAtFoot == null) GD.Print("StandingOnResourceTask: Standing on a resource!");
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
        public GatherResourceTask(Bot node, Func<BehaviorTreeTask, bool> guard = null) : base(node, guard) { }

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

    public class FindNearest<T> : BehaviorTreeTask<Bot>
        where T : Node2D
    {
        private readonly Node group;

        public FindNearest(Bot node, Node group, Func<BehaviorTreeTask, bool> guard = null) : base(node, guard)
        {
            this.group = group;
        }

        public override void Run(float delta)
        {
            var bot = this.Node;
            var nodes = this.group.GetChildren();
            if (nodes.Count > 0)
            {
                Node2D closest = null;
                foreach (var node in nodes)
                {
                    if (node is T thing && !bot.World.Claimed.Contains(thing.GetInstanceId()))
                    {
                        if (closest == null)
                        {
                            closest = thing;
                            continue;
                        }

                        if (thing.GlobalPosition.DistanceTo(bot.GlobalPosition) < closest.GlobalPosition.DistanceTo(bot.GlobalPosition))
                        {
                            closest = thing;
                        }
                    }
                }

                if (closest != null)
                {
                    bot.Path = bot.World.GetNode<Map>("Map").GetPath(bot.Position, closest.Position).ToList();
                    bot.World.Claimed.Add(closest.GetInstanceId());
                    GD.Print($"FindNearest: Found a {typeof(T)}:{closest.Name}!");

                    this.Status = TaskStatus.Succeeded;
                }
            }

            if (this.Status != TaskStatus.Succeeded)
            {
                GD.Print($"FindNearest: Didn't find a {typeof(T)}!");
                this.Status = TaskStatus.Failed;
            }
        }
    }
}

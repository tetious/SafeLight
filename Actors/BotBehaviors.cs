using System;
using System.Linq;
using Godot;

namespace Safelight.Actors
{
    public class StandingOnResourceTask : BehaviorTreeTask<Bot>
    {
        public StandingOnResourceTask(Bot node, Func<bool> guard = null) : base(node, guard) { }

        public override void Run()
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

        public override void Run()
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

        public override void Run()
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

using System;
using System.Linq;
using Godot;

namespace Safelight.Actors
{
    public class FindLight : BehaviorTreeTask<Mob>
    {
        public override void Run(float delta)
        {
            var me = this.Node;

            me.LightTargets.Clear();
            foreach (var visibleLight in me.World.Lights.Where(l => l.Enabled && me.SightRect.Intersects(l.LightRect)))
            {
                var offset = new Vector2(0, 8).Rotated(me.GetAngleTo(visibleLight.GlobalPosition));
                this.CheckAndAdd(me.Position, visibleLight.GlobalPosition);
                this.CheckAndAdd(me.Position + offset, visibleLight.GlobalPosition);
                this.CheckAndAdd(me.Position - offset, visibleLight.GlobalPosition);
            }

            var target = Vector2.Zero;
            foreach (var candidate in me.LightTargets.Where(c => c.obstacle.Count == 0))
            {
                if (candidate.dest.DistanceTo(me.Position) < target.DistanceTo(me.Position))
                {
                    target = candidate.dest;
                }
            }

            if (target != Vector2.Zero && me.Target != target)
            {
                //GD.Print("NEW TARGET");
                me.Path = me.World.Map.GetPath(me.GlobalPosition, target).ToList();
                me.Target = target;
                this.Status = TaskStatus.Succeeded;
            }
            else
            {
                //GD.Print("F TARGET");
                this.Status = TaskStatus.Failed;
            }
        }

        private void CheckAndAdd(Vector2 source, Vector2 dest)
        {
            var obstacle = this.Node.GetWorld2d().DirectSpaceState.IntersectRay(source, dest, null, 0b1);
            this.Node.LightTargets.Add((source, dest, obstacle));
        }

        public FindLight(Mob node, Func<bool> guard = null) : base(node, guard) { }
    }
}

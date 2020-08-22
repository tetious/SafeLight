using System;
using Godot;

namespace Safelight.Actors
{
    public interface ITargeted
    {
        int WalkSpeed { get; }

        Vector2 Target { get; set; }
    }

    public class MoveTowardTarget<T> : BehaviorTreeTask<T>
        where T : Node2D, ITargeted
    {
        public MoveTowardTarget(T node, Func<bool> guard = null) : base(node,
            guard ?? (() => node.Target != Vector2.Zero && node.Position != node.Target)) { }

        public override void Run(float delta)
        {
            this.Status = TaskStatus.Succeeded;

            if (this.Node.Target == Vector2.Zero) GD.PrintErr("Target is 0,0 for ", this.Node.GetInstanceId());

            var me = this.Node;
            var start = me.Position;
            var toNext = start.DistanceTo(me.Target);
            var toMove = me.WalkSpeed * delta / toNext;
            if (toNext < 1)
            {
                me.Position = me.Target;
            }
            else
            {
                me.Position = me.Position.LinearInterpolate(me.Target, toMove);
            }

            if (me.Position == me.Target)
            {
                GD.Print($"[{me.GetInstanceId()}] Hit target {me.Target}!");
                this.Reset();
                return;
            }

            var movedDistance = toNext - me.Position.DistanceTo(start);
            if (movedDistance < 0.1 && toNext > 1)
            {
                GD.Print($"[{me.GetInstanceId()}] {toNext} Only moved {movedDistance} and expected to move at least {toMove / 2}.");

                this.Status = TaskStatus.Failed;
            }
        }
    }
}

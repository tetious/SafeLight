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
        where T : KinematicBody2D, ITargeted
    {
        public MoveTowardTarget(T node, Func<BehaviorTreeTask, bool> guard = null) : base(node,
            guard ?? (_ => node.Target != Vector2.Zero && node.Position != node.Target)) { }

        public override void Run(float delta)
        {
            this.Status = TaskStatus.Running;

            if (this.Node.Target == Vector2.Zero) GD.PrintErr("Target is 0,0 for ", this.Node.GetInstanceId());

            var me = this.Node;
            var startPosition = me.Position;
            var distanceToTarget = startPosition.DistanceTo(me.Target);
            var toMove = me.WalkSpeed;
            if (distanceToTarget < me.WalkSpeed * delta)
            {
                me.Position = me.Target;
            }
            else
            {
                me.MoveAndSlide(me.Position.DirectionTo(me.Target) * toMove);
            }

            if (me.Position == me.Target)
            {
                GD.Print($"[{me.GetInstanceId()}] Hit target {me.Target}!");
                this.Status = TaskStatus.Succeeded;
                this.Reset();
                return;
            }

            var movedDistance = distanceToTarget - me.Position.DistanceTo(me.Target);

            if (movedDistance < 1 && distanceToTarget > 1)
            {
                GD.Print($"[{me.GetInstanceId()}] {distanceToTarget} Only moved {movedDistance}.");
                this.Status = TaskStatus.Failed;
            }
        }
    }
}

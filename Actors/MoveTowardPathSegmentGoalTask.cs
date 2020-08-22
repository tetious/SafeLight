using System;
using Godot;

namespace Safelight.Actors
{
    public class MoveTowardPathSegmentGoalTask<T> : BehaviorTreeTask<T>
        where T : Node2D, IPathed
    {
        public MoveTowardPathSegmentGoalTask(T node, Func<bool> guard = null) : base(node,
            guard ?? (() => node.PathSegmentGoal != Vector2.Zero)) { }

        public override void Run(float delta)
        {
            this.Status = TaskStatus.Running;

            if (this.Node.PathSegmentGoal == Vector2.Zero) GD.PrintErr("PathGoal is 0,0 for ", this.Node.GetInstanceId());

            var me = this.Node;
            var start = me.Position;
            var toNext = start.DistanceTo(me.PathSegmentGoal);
            var toMove = me.WalkSpeed * delta / toNext;
            if (toNext < 1)
            {
                me.Position = me.PathSegmentGoal;
            }
            else
            {
                me.Position = me.Position.LinearInterpolate(me.PathSegmentGoal, toMove);
            }

            if (me.Position == me.PathSegmentGoal)
            {
                GD.Print("Hit dest! ", me.PathSegmentGoal);
                me.PathSegmentGoal = Vector2.Zero;
                this.Status = TaskStatus.Succeeded;
            }

            var movedDistance = toNext - me.Position.DistanceTo(start);
            if (toMove - movedDistance < toMove / 4)
            {
                this.Status = TaskStatus.Failed;
            }
        }
    }
}

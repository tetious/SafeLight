using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Safelight.Actors
{
    public interface IPathed
    {
        int WalkSpeed { get; }

        Vector2 PathSegmentGoal { get; set; }

        List<Vector2> Path { get; }
    }

    public class NextPathSegmentTask<T> : BehaviorTreeTask<T>
        where T : Node2D, IPathed
    {
        public NextPathSegmentTask(T node, Func<bool> guard = null) : base(node,
            guard ?? (() => node.Path.Any() || node.PathSegmentGoal != Vector2.Zero)) { }

        public override void Run(float delta)
        {
            this.Status = TaskStatus.Succeeded;
            //GD.Print("MoveAlongPathTask");
            var bot = this.Node;
            var distance = bot.WalkSpeed * WorldState.I.TickLengthMs;

            var start = bot.Position;
            //GD.Print(string.Join(",", bot.Path));
            while (bot.Path.Any())
            {
                var tip = bot.Path.First();
                var toNext = start.DistanceTo(tip);

                bot.PathSegmentGoal = start.LinearInterpolate(tip, distance / toNext);
                //GD.Print(bot.GetInstanceId(), "-> Setting PathGoal:", bot.PathSegmentGoal);

                if (distance < toNext)
                    break;

                distance -= toNext;
                start = tip;
                bot.PathSegmentGoal = tip;
                bot.Path.RemoveAt(0);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Safelight.Actors
{
    public interface IPathed
    {
        int WalkSpeed { get; }

        List<Vector2> Path { get; }

        Vector2 PathGoal { get; set; }
    }

    public class MoveAlongPathTask<T> : BehaviorTreeTask<T>
        where T : Node2D, IPathed
    {
        public MoveAlongPathTask(T node, Func<bool> guard = null) : base(node, guard)
        {
            if (guard == null) guard = () => node.Path.Any() || node.PathGoal != Vector2.Zero;
        }

        public override void Run()
        {
            //GD.Print("MoveAlongPathTask");
            var bot = this.Node;
            var distance = bot.WalkSpeed * WorldState.I.TickLengthMs;
            this.Status = bot.Path.Any() || bot.PathGoal != Vector2.Zero ? TaskStatus.Running : TaskStatus.Succeeded;
            if (bot.PathGoal != Vector2.Zero) return;

            var start = bot.Position;
            //GD.Print(string.Join(",", bot.Path));
            while (bot.Path.Any())
            {
                var tip = bot.Path.First();
                var toNext = start.DistanceTo(tip);

                bot.PathGoal = start.LinearInterpolate(tip, distance / toNext);
                //GD.Print("Setting PathGoal:", bot.PathGoal);

                if (distance < toNext)
                    break;

                distance -= toNext;
                start = tip;
                bot.PathGoal = tip;
                bot.Path.RemoveAt(0);
            }
        }
    }
}

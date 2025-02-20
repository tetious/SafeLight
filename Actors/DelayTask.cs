using System;
using Godot;

namespace Safelight.Actors
{
    public class DelayTask : BehaviorTreeTask
    {
        private readonly Func<int> delayMs;

        public DelayTask(Func<int> delayMs, Func<BehaviorTreeTask, bool> guard = null) : base(guard)
        {
            this.delayMs = delayMs;
        }

        private int delay = 0;

        public override void Run(float delta)
        {
            this.delay += (int)(delta * 1000);

            this.Status = TaskStatus.Running;
            if (this.delay >= this.delayMs())
            {
                this.delay = 0;
                this.Status = TaskStatus.Succeeded;
            }
        }
    }
}

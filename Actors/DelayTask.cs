using System;

namespace Safelight.Actors
{
    public class DelayTask : BehaviorTreeTask
    {
        private readonly Func<int> delayMs;

        public DelayTask(Func<int> delayMs, Func<BehaviorTreeTask, bool> guard = null) : base(guard)
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
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Safelight.Actors
{
    public enum TaskStatus { Fresh, Running, Failed, Succeeded, Cancelled }

    public abstract class BehaviorTreeTask
    {
        public TaskStatus Status { get; protected set; }

        public Func<bool> Guard { get; }

        private List<BehaviorTreeTask> children = new List<BehaviorTreeTask>();

        public BehaviorTreeTask[] Children => this.children.Where(c => c.Guard == null || c.Guard()).ToArray();

        public abstract void Run();

        public virtual void Reset()
        {
            this.children.ForEach(c => c.Reset());
            this.Status = TaskStatus.Fresh;
        }

        public void AddChildren(params BehaviorTreeTask[] children) => this.children.AddRange(children);

        protected BehaviorTreeTask(Func<bool> guard = null)
        {
            this.Guard = guard;
        }
    }

    public abstract class BehaviorTreeTask<T> : BehaviorTreeTask
        where T : Node
    {
        public T Node { get; }

        protected BehaviorTreeTask(T node, Func<bool> guard = null) : base(guard)
        {
            this.Node = node;
        }
    }

    public class ParallelSequence : BehaviorTreeTask
    {
        public override void Run()
        {
            var children = this.Children;
            if (!children.Any()) return;
            this.Status = TaskStatus.Running;
            foreach (var child in children)
            {
                if (child.Status == TaskStatus.Running || child.Status == TaskStatus.Fresh) child.Run();

                if (child.Status == TaskStatus.Failed)
                {
                    child.Reset();
                    continue;
                }

                if (child.Status == TaskStatus.Succeeded)
                {
                    child.Reset();
                    continue;
                }
            }
        }

        public ParallelSequence(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(guard)
        {
            this.AddChildren(children);
        }
    }

    public class Selector : BehaviorTreeTask
    {
        public override void Run()
        {
            if (!this.Children.Any()) return;
            this.Status = TaskStatus.Running;
            foreach (var child in this.Children)
            {
                if (child.Status == TaskStatus.Running || child.Status == TaskStatus.Fresh) child.Run();

                if (child.Status == TaskStatus.Running) return;

                if (child.Status == TaskStatus.Failed)
                {
                    this.Status = TaskStatus.Failed;
                    continue;
                }

                if (child.Status == TaskStatus.Succeeded)
                {
                    this.Reset();
                    this.Status = TaskStatus.Succeeded;
                    return;
                }
            }
        }

        public Selector(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(guard)
        {
            this.AddChildren(children);
        }
    }

    public class Sequence : BehaviorTreeTask
    {
        public override void Run()
        {
            if (!this.Children.Any()) return;
            this.Status = TaskStatus.Running;
            foreach (var child in this.Children)
            {
                if (child.Status == TaskStatus.Running || child.Status == TaskStatus.Fresh) child.Run();

                if (child.Status == TaskStatus.Running) return;

                if (child.Status == TaskStatus.Succeeded)
                {
                    this.Status = TaskStatus.Succeeded;
                }
                else if (child.Status == TaskStatus.Failed)
                {
                    this.Reset();
                    this.Status = TaskStatus.Failed;
                    return;
                }
            }
        }

        public Sequence(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(guard)
        {
            this.AddChildren(children);
        }
    }
}

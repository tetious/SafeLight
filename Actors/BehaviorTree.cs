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

        public Node Node { get; }

        private List<BehaviorTreeTask> children = new List<BehaviorTreeTask>();

        public BehaviorTreeTask[] Children => this.children.Where(c => c.Guard == null || c.Guard()).ToArray();

        public abstract void Run();

        public virtual void Reset()
        {
            this.children.ForEach(c => c.Reset());
            this.Status = TaskStatus.Fresh;
        }

        public void AddChildren(params BehaviorTreeTask[] children) => this.children.AddRange(children);

        protected BehaviorTreeTask(Node node, Func<bool> guard = null)
        {
            this.Node = node;
            this.Guard = guard;
        }
    }

    public class Selector : BehaviorTreeTask
    {
        private int currentTaskIndex = 0;

        public override void Reset()
        {
            this.currentTaskIndex = 0;
            base.Reset();
        }

        public override void Run()
        {
            if (!this.Children.Any()) return;
            this.Status = TaskStatus.Running;
            var child = this.Children[this.currentTaskIndex];

            if (child.Status == TaskStatus.Running || child.Status == TaskStatus.Fresh)
            {
                child.Run();
            }

            if (child.Status == TaskStatus.Failed)
            {
                this.currentTaskIndex++;
                if (this.currentTaskIndex == this.Children.Length)
                {
                    this.currentTaskIndex = 0;
                }
                else
                {
                    this.Run();
                }
            }

            if (child.Status == TaskStatus.Succeeded)
            {
                this.Reset();
                this.Status = TaskStatus.Succeeded;
            }
        }

        public Selector(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(null, guard)
        {
            this.AddChildren(children);
        }
    }

    public class Sequence : BehaviorTreeTask
    {
        private int currentTaskIndex = 0;

        public override void Reset()
        {
            this.currentTaskIndex = 0;
            base.Reset();
        }

        public override void Run()
        {
            if (!this.Children.Any()) return;
            this.Status = TaskStatus.Running;
            var child = this.Children[this.currentTaskIndex];

            if (child.Status == TaskStatus.Running || child.Status == TaskStatus.Fresh) child.Run();

            if (child.Status == TaskStatus.Succeeded)
            {
                this.currentTaskIndex++;
                if (this.currentTaskIndex == this.Children.Length)
                {
                    this.Reset();
                    this.Status = TaskStatus.Succeeded;
                }
                else
                {
                    this.Run();
                }
            }

            if (child.Status == TaskStatus.Failed)
            {
                this.Reset();
                this.Status = TaskStatus.Failed;
            }
        }

        public Sequence(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(null, guard)
        {
            this.AddChildren(children);
        }
    }
}

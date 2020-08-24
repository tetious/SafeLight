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

        public Func<BehaviorTreeTask, bool> Guard { get; }

        public bool CanRun => this.Guard == null || this.Guard(this);

        private List<BehaviorTreeTask> children = new List<BehaviorTreeTask>();

        public BehaviorTreeTask[] Children => this.children.ToArray();

        public string Tag { get; protected set; }

        public abstract void Run(float delta);

        public virtual void Reset()
        {
            //this.children.ForEach(c => c.Reset());
            this.Status = TaskStatus.Fresh;
        }

        public void AddChildren(params BehaviorTreeTask[] children) => this.children.AddRange(children);

        protected BehaviorTreeTask(Func<BehaviorTreeTask, bool> guard = null)
        {
            this.Guard = guard;
        }
    }

    public abstract class BehaviorTreeTask<T> : BehaviorTreeTask
        where T : Node
    {
        public T Node { get; }

        protected BehaviorTreeTask(T node, Func<BehaviorTreeTask, bool> guard = null) : base(guard)
        {
            this.Node = node;
        }
    }

    public class Selector : Branchy
    {
        public override void Run(float delta)
        {
            if (!this.AnyCanRun) return;
            this.PopNextIfNeeded();
            if (this.Current != null)
            {
                this.Current.Run(delta);
                //GD.Print($"Selector:{this.Tag}.Run: {this.Current.GetType().Name}:{this.Current.Tag}.{this.Current.Status}.");

                if (this.Current.Status == TaskStatus.Failed)
                {
                    //GD.Print($"Selector:{this.Tag}.Failed. Moving to next.");
                    this.Status = TaskStatus.Failed;
                    this.Current.Reset();
                }

                if (this.Current.Status == TaskStatus.Succeeded)
                {
                    //GD.Print($"Selector:{this.Tag}.Succeeded. Restarting.");
                    this.Status = TaskStatus.Succeeded;
                    this.Current.Reset();
                    this.Restart();
                }
            }
        }

        public Selector(string tag, params BehaviorTreeTask[] children) : this(tag, null, children) { }

        public Selector(string tag, Func<Selector, bool> guard = null, params BehaviorTreeTask[] children)
            : base(tag, t => guard == null || guard(t as Selector), children) { }
    }

    public class Sequence : Branchy
    {
        public override void Run(float delta)
        {
            if (!this.AnyCanRun) return;
            this.PopNextIfNeeded();
            var seen = 1;
            while (this.Current != null && seen <= this.Children.Length)
            {
                this.Current.Run(delta);
                // GD.Print($"Sequence:{this.Tag}.Run: {this.Current.GetType().Name}:{this.Current.Tag}.{this.Current.Status}.");

                this.Status = TaskStatus.Running;
                if (this.Current.Status == TaskStatus.Succeeded)
                {
                    this.Current.Reset();
                }

                if (this.Current.Status == TaskStatus.Failed)
                {
                    //GD.Print($"Sequence:{this.Tag}.Failed. Restarting.");
                    this.Current.Reset();
                    this.Status = TaskStatus.Failed;
                    this.Restart();
                    return;
                }

                this.PopNextIfNeeded();
                seen++;
            }

            this.Status = TaskStatus.Succeeded;
        }

        public Sequence(string tag, params BehaviorTreeTask[] children) : this(tag, null, children) { }

        public Sequence(string tag, Func<BehaviorTreeTask, bool> guard = null, params BehaviorTreeTask[] children) : base(tag, guard, children) { }
    }

    public abstract class Branchy : BehaviorTreeTask
    {
        private int? currentIndex = null;

        protected BehaviorTreeTask Current => this.currentIndex.HasValue ? this.Children[this.currentIndex.Value] : null;

        public bool AnyCanRun => this.Children.Any(c => c.CanRun);

        public int RunnableCount => this.Children.Count(c => c.CanRun);

        protected Branchy(string tag, params BehaviorTreeTask[] children) : this(tag, null, children) { }

        protected Branchy(string tag, Func<BehaviorTreeTask, bool> guard = null, params BehaviorTreeTask[] children) : base(guard)
        {
            this.Tag = tag;
            this.AddChildren(children);
        }

        public void Restart()
        {
            this.currentIndex = null;
        }

        protected void PopNextIfNeeded()
        {
            if (this.Current != null && this.Current.Status == TaskStatus.Running)
            {
                return;
            }

            var saw = 0;
            while (saw < this.Children.Length)
            {
                if (this.currentIndex.HasValue)
                {
                    this.currentIndex++;
                }
                else
                {
                    this.currentIndex = 0;
                }

                if (this.currentIndex == this.Children.Length) this.currentIndex = 0;
                if (this.Current.CanRun) return;
                saw++;
            }

            this.currentIndex = null;
        }
    }
}

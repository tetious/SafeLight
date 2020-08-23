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

        public BehaviorTreeTask[] Children => this.children.ToArray();

        public abstract void Run(float delta);

        public virtual void Reset()
        {
            //this.children.ForEach(c => c.Reset());
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

    // public class ParallelSequence : BehaviorTreeTask
    // {
    //     public override void Run(float delta)
    //     {
    //         var children = this.Children;
    //         if (!children.Any())
    //         {
    //             this.Status = TaskStatus.Succeeded;
    //         }
    //
    //         this.Status = TaskStatus.Running;
    //         foreach (var child in children)
    //         {
    //             if (child.Status == TaskStatus.Running || child.Status == TaskStatus.Fresh)
    //             {
    //                 child.Run(delta);
    //             }
    //             else
    //             {
    //                 child.Reset();
    //             }
    //         }
    //     }
    //
    //     public ParallelSequence(params BehaviorTreeTask[] children) : this(null, children) { }
    //
    //     public ParallelSequence(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(guard)
    //     {
    //         this.AddChildren(children);
    //     }
    // }

    public class Selector : Branchy
    {
        public override void Run(float delta)
        {
            this.PopNextIfNeeded();
            this.Status = TaskStatus.Succeeded;
            if (this.Current != null)
            {
                this.Current.Run(delta);

                if (this.Current.Status == TaskStatus.Failed)
                {
                    this.Status = TaskStatus.Failed;
                    this.Current.Reset();
                }

                if (this.Current.Status == TaskStatus.Succeeded)
                {
                    this.Current.Reset();
                    this.Restart();
                }
            }
        }

        public Selector(params BehaviorTreeTask[] children) : this(null, children) { }

        public Selector(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(guard, children) { }
    }

    public class Sequence : Branchy
    {
        public override void Run(float delta)
        {
            this.PopNextIfNeeded();
            this.Status = TaskStatus.Succeeded;
            if (this.Current != null)
            {
                this.Current.Run(delta);

                if (this.Current.Status == TaskStatus.Succeeded)
                {
                    this.Current.Reset();
                }

                if (this.Current.Status == TaskStatus.Failed)
                {
                    this.Current.Reset();
                    this.Status = TaskStatus.Failed;
                }
            }
        }

        public Sequence(params BehaviorTreeTask[] children) : this(null, children) { }

        public Sequence(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(guard, children) { }
    }

    public class ParallelSequence : Branchy
    {
        public override void Run(float delta)
        {
            this.PopNextIfNeeded();
            var seen = 1;
            this.Status = TaskStatus.Succeeded;
            while (this.Current != null && seen <= this.Children.Length)
            {
                this.Current.Run(delta);

                if (this.Current.Status == TaskStatus.Succeeded)
                {
                    this.Current.Reset();
                }

                if (this.Current.Status == TaskStatus.Failed)
                {
                    this.Current.Reset();
                    this.Status = TaskStatus.Failed;
                }

                this.PopNextIfNeeded();
                seen++;
            }
        }

        public ParallelSequence(params BehaviorTreeTask[] children) : this(null, children) { }

        public ParallelSequence(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(guard, children) { }
    }

    public abstract class Branchy : BehaviorTreeTask
    {
        private int? currentIndex = null;

        protected BehaviorTreeTask Current => this.currentIndex.HasValue ? this.Children[this.currentIndex.Value] : null;

        protected Branchy(params BehaviorTreeTask[] children) : this(null, children) { }

        protected Branchy(Func<bool> guard = null, params BehaviorTreeTask[] children) : base(guard)
        {
            this.AddChildren(children);
        }

        public void Restart()
        {
            this.currentIndex = null;
        }

        protected void PopNextIfNeeded()
        {
            if (this.Current != null && (this.Current.Status == TaskStatus.Running))
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
                if (this.Current.Guard == null || this.Current.Guard()) return;
                saw++;
            }

            this.currentIndex = null;
        }
    }
}

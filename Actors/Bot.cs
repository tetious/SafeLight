using System;
using System.Collections.Generic;
using Godot;
using System.Linq;
using Safelight.Actors;

public enum BotType { None, Gatherer }

public class Bot : Area2D, IPathed
{
    public WorldManager World { get; private set; }

    private readonly Selector root;

    public WorldResource ResourceAtFoot { get; set; } = null;

    [Export]
    public int WalkSpeed { get; set; } = 100;

    [Export]
    public BotType Type { get; set; }

    public List<Vector2> Path { get; set; } = new List<Vector2>();

    public Vector2 PathGoal { get; set; } = Vector2.Zero;

    public bool IsPathing => this.Path.Any() || this.PathGoal != Vector2.Zero;

    public Bot()
    {
        var gatherer = new Selector(() => this.Type == BotType.Gatherer,
            new Sequence(null, new StandingOnResourceTask(this), new GatherResourceTask(this)),
            new FindNearestResource(this)
        );

        this.root = new Selector(null, new MoveAlongPathTask<Bot>(this, () => this.IsPathing), gatherer);
    }

    public override void _Ready()
    {
        this.World = (WorldManager)this.FindParent("WorldManager");
    }

    public override void _Draw()
    {
        if (this.Path.Any())
        {
            this.DrawMultiline(this.Path.Select(i => i - this.Position).ToArray(), Colors.Aqua);
            this.DrawCircle(this.Path.Last() - this.Position, 2, Colors.Aqua);
        }

        if (this.PathGoal != Vector2.Zero) this.DrawCircle(this.PathGoal - this.Position, 2, Colors.Pink);
    }

    public override void _Process(float delta) { }

    public override void _PhysicsProcess(float delta)
    {
        if (this.PathGoal != Vector2.Zero)
        {
            var toNext = this.Position.DistanceTo(this.PathGoal);
            if (toNext < 1)
            {
                this.Position = this.PathGoal;
            }
            else
            {
                this.Position = this.Position.LinearInterpolate(this.PathGoal, this.WalkSpeed * delta / toNext);
            }

            if (this.Position == this.PathGoal)
            {
                GD.Print("Hit dest! ", this.PathGoal);
                this.PathGoal = Vector2.Zero;
            }
        }

        this.Update();

        this.root.Run();
    }
}

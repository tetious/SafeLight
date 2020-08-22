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

    public Vector2 PathSegmentGoal { get; set; } = Vector2.Zero;

    public Bot()
    {
        var gatherer = new Selector(() => this.Type == BotType.Gatherer,
            new Sequence(new StandingOnResourceTask(this), new GatherResourceTask(this)),
            new FindNearestResource(this)
        );

        var move = new Selector(new MoveTowardPathSegmentGoalTask<Bot>(this), new NextPathSegmentTask<Bot>(this));
        this.root = new Selector(move, gatherer);
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

        if (this.PathSegmentGoal != Vector2.Zero) this.DrawCircle(this.PathSegmentGoal - this.Position, 2, Colors.Pink);
    }

    public override void _Process(float delta) { }

    public override void _PhysicsProcess(float delta)
    {
        this.Update();
        this.root.Run(delta);
    }
}

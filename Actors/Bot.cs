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

    public HashSet<Mob> VisibleMobs { get;} = new HashSet<Mob>();

    [Export]
    public int SightDistance { get; set; } = 400;

    public Rect2 SightRect => new Rect2(this.GlobalPosition - new Vector2(this.SightDistance, this.SightDistance) / 2,
        new Vector2(this.SightDistance, this.SightDistance));

    public Bot()
    {
        var selfDefense = new Selector();

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
        this.GetNode<Area2D>("DetectionArea").Connect("body_entered", this, nameof(this.DetectedBodyEntered));
        this.GetNode<Area2D>("DetectionArea").Connect("body_exited", this, nameof(this.DetectedBodyExited));
    }

    public override void _Draw()
    {
        this.DrawSetTransform(new Vector2() - this.GlobalPosition, 0, Vector2.One);

        if (this.Path.Any())
        {
            this.DrawMultiline(new []{ this.Position }.Concat(this.Path).ToArray(), Colors.Aqua);
            this.DrawCircle(this.Path.Last(), 2, Colors.Aqua);
        }

        if (this.PathSegmentGoal != Vector2.Zero) this.DrawCircle(this.PathSegmentGoal, 2, Colors.Pink);
    }

    public override void _Process(float delta) { }

    public override void _PhysicsProcess(float delta)
    {
        this.Update();
        this.root.Run(delta);
    }

    public void DetectedBodyEntered(Node node)
    {
        this.VisibleMobs.Add(node as Mob);
        GD.Print("Detected ", this.VisibleMobs.Count);
    }

    public void DetectedBodyExited(Node node)
    {
        this.VisibleMobs.Remove(node as Mob);
        GD.Print("Left ", this.VisibleMobs.Count);
    }
}

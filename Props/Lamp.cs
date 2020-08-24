using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Safelight;
using Safelight.Actors;

public class Lamp : Area2D, IShoot
{
    public WorldManager World { get; private set; }

    public HashSet<Mob> VisibleMobs { get; } = new HashSet<Mob>();

    private readonly PackedScene laserShot = GD.Load<PackedScene>("res://Actors/LaserShot.tscn");

    private BehaviorTreeTask root;

    public override void _Ready()
    {
        this.World = (WorldManager)this.FindParent("WorldManager");


        this.root = new Sequence("SelfDefense", _ => this.VisibleMobs.Any(), new ShootNearestBaddie<Lamp>(this), new DelayTask(() => 500));

        this.GetNode<Area2D>("DetectionArea").Connect("body_entered", this, nameof(this.DetectedBodyEntered));
        this.GetNode<Area2D>("DetectionArea").Connect("body_exited", this, nameof(this.DetectedBodyExited));
    }

    public override void _PhysicsProcess(float delta)
    {
        //this.Update();
        this.root.Run(delta);
    }

    public void FireAt(Vector2 spot)
    {
        var laserShot = this.laserShot.Instance() as LaserShot;
        laserShot.Position = this.Position + this.Position.DirectionTo(spot) * 34;
        laserShot.Velocity = this.Position.DirectionTo(spot) * 400;
        laserShot.Rotation = this.Position.DirectionTo(spot).Angle();
        this.GetParent().AddChild(laserShot);
    }

    public void DetectedBodyEntered(Node node)
    {
        this.VisibleMobs.Add(node as Mob);
        GD.Print(this.GetInstanceId(), " Detected ", this.VisibleMobs.Count);
    }

    public void DetectedBodyExited(Node node)
    {
        this.VisibleMobs.Remove(node as Mob);
        GD.Print(this.GetInstanceId(), " Left ", this.VisibleMobs.Count);
    }
}

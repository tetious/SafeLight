using Godot;
using System;

public class LaserShot : Area2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    private Vector2 startPoint;
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Connect("body_entered", this, nameof(this.BodyEntered));
    }

    public void BodyEntered(Node body)
    {
        body.EmitSignal("Hit", 10);
        this.QueueFree();
    }

    public override void _PhysicsProcess(float delta)
    {
        if(this.startPoint == Vector2.Zero) this.startPoint = this.Position;
        this.Position += this.Velocity * delta;
        if(this.Position.DistanceTo(this.startPoint) > 500) this.QueueFree();
    }
}

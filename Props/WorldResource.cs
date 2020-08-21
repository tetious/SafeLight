using Godot;
using System;

public class WorldResource : Area2D
{
    public bool Claimed = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.Connect("area_exited", this, nameof(this.OnAreaExited));
    }

    public void OnAreaExited(Area2D area)
    {
        this.Claimed = false;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

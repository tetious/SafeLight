using Godot;
using System;

public class WorldResource : Area2D
{
    [Export]
    public decimal AvailableResources { get; private set; } = 10000;

    [Export]
    public decimal BaseGatherRate { get; private set; } = .1m;

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

    public decimal PopResources()
    {
        if (this.AvailableResources == 0) return 0;
        this.AvailableResources -= this.BaseGatherRate;
        var gathered = this.AvailableResources >= 0 ? this.BaseGatherRate : this.BaseGatherRate + this.AvailableResources;
        if (this.AvailableResources <= 0)
        {
            this.QueueFree();
        }

        return gathered;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

using Godot;
using System;
using System.Collections.Generic;

public class WorldState : Node
{
    private readonly Timer tickTimer = new Timer { ProcessMode = Timer.TimerProcessMode.Physics };

    [Signal]
    public delegate void WorldTick();

    [Signal]
    public delegate void StateChanged();

    public static WorldState I { get; private set; }

    private WorldState()
    {
        I = this;
    }

    public int BotCount { get; private set; } = 5;

    public int Gatherers { get; private set; }

    public int StoredPower { get; private set; }

    private float crystalCount;

    public int CrystalCount => (int)Math.Floor(this.crystalCount);

    public int MaxPower { get; private set; } = 100;

    public float TickLengthMs { get; private set; } = 0.1f;

    public Dictionary<string, int> Cost { get; } = new Dictionary<string, int>
    {
        { "SolarPanel", 1000 },
        { "Lamp", 5000 },
    };

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.tickTimer.Connect("timeout", this, "tickTimer_timeout");
        this.AddChild(this.tickTimer);
        this.tickTimer.Start(this.TickLengthMs);
    }

    public void tickTimer_timeout() => this.EmitSignal("WorldTick");


    public int set_gatherers(int value)
    {
        if (value < 0) return this.Gatherers;
        var nonGatherers = this.IdleBots + this.Gatherers;
        if (value <= nonGatherers)
        {
            this.Gatherers = value;
            this.EmitSignal("StateChanged");
        }

        return this.Gatherers;
    }

    public void AddCrystals(float num)
    {
        this.crystalCount += num;
        this.EmitSignal("StateChanged");
    }

    public void SubtractCrystals(float num)
    {
        this.crystalCount -= num;
        this.EmitSignal("StateChanged");
    }

    public void AddPower(int eu)
    {
        this.StoredPower = this.StoredPower + eu > this.MaxPower ? this.MaxPower : this.StoredPower + eu;
        this.EmitSignal("StateChanged");
    }

    public void AddMaxPower(int eu)
    {
        this.MaxPower += eu;
        this.EmitSignal("StateChanged");
    }

    public int IdleBots => this.BotCount - this.Gatherers;

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

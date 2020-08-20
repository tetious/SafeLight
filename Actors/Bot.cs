using System.Collections.Generic;
using Godot;
using System.Linq;
using System.Threading.Tasks;

public enum BotType { None, Gatherer }



public class Bot : Area2D
{
    private WorldManager world;

    [Export]
    public int WalkSpeed { get; set; } = 400;

    [Export]
    public BotType Type { get; set; }

    public List<Vector2> Path { get; set; } = new List<Vector2>();

    public override void _Ready()
    {
        this.world = (WorldManager)this.FindParent("WorldManager");
        WorldState.I.Connect("WorldTick", this, "WorldTick");
    }

    public override void _Draw() { }

    public override void _Process(float delta)
    {
        if (this.Path.Count != 0)
        {
            this.MoveAlongPath(this.WalkSpeed * delta);
        }
    }

    private void WorldTick()
    {
        switch (Type)
        {
            case BotType.Gatherer:
                this.GatherTick();
                break;
        }
    }

    private void GatherTick()
    {
        if (this.Path.Any()) return;
        var overlappingAreas = this.GetOverlappingAreas().Cast<Area2D>();
        if (overlappingAreas.Any(a => a is WorldResource))
        {
            GD.Print("Standing on a resource!");
        }
        else
        {
            var resources = this.world.GetNode("Map/Resources").GetChildren().Cast<WorldResource>().ToArray();
            if (resources.Length > 0)
            {
                var closest = resources.First();
                foreach (var resource in resources)
                {
                    if (resource.GlobalPosition.DistanceTo(this.GlobalPosition) < closest.GlobalPosition.DistanceTo(this.GlobalPosition))
                    {
                        closest = resource;
                    }
                }

                this.Path = this.world.GetNode<Map>("Map").GetPath(this.Position, closest.Position).ToList();
            }
        }
    }

    private void MoveAlongPath(float distance)
    {
        var start = this.Position;
        for (var i = 0; i < this.Path.Count; i++)
        {
            var tip = this.Path.First();
            var toNext = start.DistanceTo(tip);
            if (distance <= toNext && distance >= 0)
            {
                this.Position = start.LinearInterpolate(tip, distance / toNext);
                break;
            }

            if (distance < 0)
            {
                this.Position = tip;
            }

            distance -= toNext;
            start = tip;
            this.Path.RemoveAt(0);
        }
    }
}

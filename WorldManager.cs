using Godot;
using System;
using System.Linq;
using Safelight.Actors;
using Safelight.Props;


public class WorldManager : Node2D
{
    [Export]
    public bool DEBUG = false;

    public SafeLight[] Lights { get; private set; } = new SafeLight[0];

    public Map Map { get; private set; }

    public override void _Ready()
    {
        this.Map = this.GetNode<Map>("Map");
        this.Lights = this.GetTree().GetNodesInGroup("safelight").Cast<SafeLight>().ToArray();
    }

    public override void _Process(float delta) { }

    public override void _PhysicsProcess(float delta)
    {
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == (int)ButtonList.Left && mouseButton.Pressed)
            {
                var navTo = this.GetGlobalMousePosition();
                var bot = this.GetNode<Bot>("Bots/Bot");
                bot.Path = this.Map.GetPath(bot.Position, navTo).ToList();
            }
        }
    }
}

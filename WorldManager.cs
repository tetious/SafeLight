using Godot;
using System;
using System.Linq;
using Safelight.Actors;


public class WorldManager : Node2D
{
    [Export]
    public bool DEBUG = false;

    public Light2D[] Lights { get; } = new Light2D[0];

    public override void _Ready() { }

    public override void _Process(float delta) { }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == (int)ButtonList.Left && mouseButton.Pressed)
            {
                var navTo = this.GetGlobalMousePosition();
                var bot = this.GetNode<Bot>("Bots/Bot");
                bot.Path = this.GetNode<Map>("Map").GetPath(bot.Position, navTo).ToList();
            }
        }
    }
}

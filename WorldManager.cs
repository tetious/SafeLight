using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Safelight.Actors;
using Safelight.Props;
using Array = Godot.Collections.Array;


public class WorldManager : Node2D
{
    [Export]
    public bool DEBUG = false;

    [Export]
    public ButtonGroup buildBarGroup;

    private Node propsContainer;

    public bool BuildMode { get; set; }

    public bool BuildWall => this.BuildWallIndex != -1;

    public int BuildWallIndex { get; set; }

    public string BuildPropName { get; set; }

    public SafeLight[] Lights { get; private set; } = new SafeLight[0];

    public Map Map { get; private set; }

    public readonly HashSet<ulong> Claimed = new HashSet<ulong>();

    private Node buildGUI;
    private Node toBuild;

    private readonly Color buildRed = Color.Color8(255, 0, 0, 128);
    private readonly Color buildWhite = Color.Color8(255, 255, 255, 128);
    private readonly Color toBuildColor = Color.Color8(255, 255, 255, 200);

    public override void _Ready()
    {
        this.Map = this.GetNode<Map>("Map");
        this.Lights = this.GetTree().GetNodesInGroup("safelight").Cast<SafeLight>().ToArray();
        this.propsContainer = this.GetNode("Map/Props");
        this.buildGUI = this.GetNode("Map/BuildGUI");
        this.toBuild = this.GetNode("Map/ToBuild");
    }

    public override void _Draw()
    {
        //this.DrawCircle(this.GetGlobalMousePosition(), 2, Colors.Purple);
        //this.DrawRect(this.Map.GetTileRect(this.GetGlobalMousePosition()), Colors.Brown, false);
    }

    public override void _Process(float delta) { }

    public override void _PhysicsProcess(float delta) { }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (Input.IsActionPressed("escape") && this.BuildMode)
        {
            this.buildBarGroup.GetPressedButton().Pressed = false;
            this.BuildMode = false;
            this.buildTemp.QueueFree();
            this.buildTemp = null;
        }

        if (this.BuildMode)
        {
            DrawBuildTemp();
        }

        if (evt is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == (int)ButtonList.Left && mouseButton.Pressed)
            {
                if (this.BuildMode)
                {
                    if (this.buildTemp != null && this.buildOk)
                    {
                        this.buildTemp.Scale = Vector2.One;
                        this.buildTemp.Modulate = this.toBuildColor;
                        this.buildGUI.RemoveChild(this.buildTemp);
                        this.toBuild.AddChild(this.buildTemp);
                        this.buildTemp = null;

                        //this.buildBarGroup.GetPressedButton().Pressed = false;
                        //this.BuildMode = false;
                    }
                }
                else
                {
                    var navTo = this.GetGlobalMousePosition();
                    var bot = this.GetNode<Bot>("Bots/Bot");
                    bot.Path = this.Map.GetPath(bot.Position, navTo).ToList();
                }
            }
        }

        Update();
    }

    private void UpdateBuildProgress(Area2D node)
    {
        node.Scale = Vector2.One;
        node.Modulate = this.toBuildColor;
    }

    public void MarkBuildComplete(Area2D node)
    {
        node.Scale = Vector2.One;
        node.Modulate = Colors.White;
        this.toBuild.RemoveChild(node);
        if (node.Name != "Wall")
        {
            this.Map.AddStaticBodyCollider(node);
            this.propsContainer.AddChild(node);
            GD.Print("BUILT ", node.Name);
        }
        else
        {
            this.Map.SetTile(node.Position, ((Wall)node).tileIndex);
            node.QueueFree();
        }
    }

    public bool HasLineOfSightToLight(Vector2 position)
    {
        var lightTargets = new List<(Vector2 src, Vector2 dest, Dictionary obstacle)>();

        void CheckAndAdd(Vector2 source, Vector2 dest)
        {
            var obstacle = this.GetWorld2d().DirectSpaceState.IntersectRay(source, dest, null, 0b1);
            lightTargets.Add((source, dest, obstacle));
        }

        foreach (var visibleLight in this.Lights.Where(l => l.Enabled))
        {
            var offset = new Vector2(0, 8).Rotated(this.GetAngleTo(visibleLight.GlobalPosition));
            CheckAndAdd(position, visibleLight.GlobalPosition);
            CheckAndAdd(position + offset, visibleLight.GlobalPosition);
            CheckAndAdd(position - offset, visibleLight.GlobalPosition);
        }

        return lightTargets.Any(c => c.obstacle.Count == 0);
    }

    public bool HasLineOfSightToPoint(Node2D me, Vector2 src, Vector2 dest)
    {
        var obstacle = this.GetWorld2d().DirectSpaceState.IntersectRay(src, dest, new Array { me }, 0b1);
        return obstacle.Count == 0;
    }

    private Area2D buildTemp;

    private BaseButton pressedButton;

    private bool buildOk = false;

    public void DrawBuildTemp()
    {
        var pressed = this.buildBarGroup.GetPressedButton();
        if (pressed != this.pressedButton)
        {
            if (this.buildTemp != null)
            {
                this.buildTemp.QueueFree();
                this.buildTemp = null;
            }

            this.pressedButton = pressed;
        }

        if (this.pressedButton == null) return;

        if (this.buildTemp == null)
        {
            var propScene = this.BuildWall ? GD.Load<PackedScene>($"Props/Wall.tscn") : GD.Load<PackedScene>($"Props/{this.BuildPropName}.tscn");


            var prop = propScene.Instance() as Area2D;
            prop.Scale *= .99f;
            this.buildTemp = prop;
            if (this.BuildWall)
            {
                var sprite = this.buildTemp.GetNode<Sprite>("Sprite");
                var (texture, offset) = this.Map.GetTileTexture(this.BuildWallIndex);
                sprite.Texture = texture;
                sprite.RegionRect = new Rect2(offset, 16, 16);
            }

            this.buildGUI.AddChild(this.buildTemp);
        }

        var shape = this.buildTemp.GetNode<CollisionShape2D>("Shape");
        var rect = shape.Shape as RectangleShape2D;
        var extents = rect.Extents;

        this.buildTemp.Position = this.Map.GetBuildPosition(this.GetGlobalMousePosition()) + extents;

        var bodies = this.buildTemp.GetOverlappingBodies();
        var areas = this.buildTemp.GetOverlappingAreas();
        this.buildOk = areas.Count + bodies.Count == 0;
        this.buildTemp.Modulate = this.buildOk ? this.buildWhite : this.buildRed;

        // this.rayStart = this.buildTemp.GlobalPosition;
        // this.rayEnd = this.buildTemp.GlobalPosition + extents;
        //
        // var obstacle = this.GetWorld2d().DirectSpaceState.IntersectRay(rayStart, rayEnd, new Godot.Collections.Array { this.buildTemp }, 1024, true, true);
        // this.buildTemp.Modulate = obstacle.Count == 0 ? this.buildWhite : this.buildRed;
    }
}

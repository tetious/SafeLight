using Godot;
using System;
using System.Linq;
using Godot.Collections;

public class Map : Node2D
{
    private readonly AStar2D astar = new AStar2D();
    private TileMap tileMap;
    private Rect2 usedRect;

    private static readonly RectangleShape2D tileBounds = new RectangleShape2D { Extents = new Vector2(8, 8) };

    public override void _Ready()
    {
        this.tileMap = this.GetNode<TileMap>("TileMap");
        this.usedRect = this.tileMap.GetUsedRect();

        foreach (Area2D prop in this.GetNode("Props").GetChildren())
        {
            var shape = prop.GetNode<CollisionShape2D>("Shape");

            var body = new StaticBody2D();
            body.AddChild(shape.Duplicate());
            body.CollisionLayer = 1024;
            prop.AddChild(body);
        }

        var usedTiles = this.tileMap.GetUsedCells().Cast<Vector2>().ToArray();
        foreach (var position in usedTiles)
        {
            this.astar.AddPoint(this.IdForPoint(position), position);
        }

        bool TileIsImpassable(int tileIndex)
        {
            if (tileIndex == -1) return true;

            if (this.tileMap.TileSet.TileGetShapeCount(tileIndex) == 0) return false;
            var rawShapes = this.tileMap.TileSet.TileGetShapes(tileIndex);
            if (rawShapes.Count == 1 && rawShapes[0] is Dictionary shapes)
            {
                return shapes.Contains("shape");
            }

            return false;
        }

        bool TileIsOccupied(Vector2 position)
        {
            var tileTransform = new Transform2D(0, this.tileMap.MapToWorld(position) + new Vector2(8, 8));
            var things = this.GetNode("Props").GetChildren().Cast<Area2D>()
                .Where(b => b.GetCollisionLayerBit(10) &&
                    b.GetNode<CollisionShape2D>("Shape").Shape.Collide(b.GlobalTransform, tileBounds, tileTransform));

            return things.Any();
        }

        foreach (var position in usedTiles)
        {
            var id = this.IdForPoint(position);
            var positionTileIndex = this.tileMap.GetCellv(position);
            if (TileIsOccupied(position) || TileIsImpassable(positionTileIndex)) continue;

            for (var x = 0; x < 3; x++)
            {
                for (var y = 0; y < 3; y++)
                {
                    var target = position + new Vector2(x - 1, y - 1);
                    var targetId = this.IdForPoint(target);
                    if (position == target || this.astar.HasPoint(targetId) == false) continue;

                    var tileIndex = this.tileMap.GetCellv(target);
                    if (!TileIsOccupied(position) && !TileIsImpassable(tileIndex)) this.astar.ConnectPoints(id, targetId);
                }
            }
        }
    }

    public Vector2[] GetPath(Vector2 start, Vector2 end)
    {
        var startId = this.IdForPoint(this.tileMap.WorldToMap(start));
        var endId = this.IdForPoint(this.tileMap.WorldToMap(end));
        return this.astar.GetPointPath(startId, endId).Select(p => this.tileMap.MapToWorld(p) + new Vector2(8, 8)).ToArray();
    }

    public Rect2 GetTileRect(Vector2 position)
    {
        var rawPos = this.tileMap.MapToWorld(this.tileMap.WorldToMap(position));
        return new Rect2(rawPos + this.Position, this.tileMap.CellSize * 2);
    }

    public Vector2 GetBuildPosition(Vector2 position)
    {
        return this.tileMap.MapToWorld(this.tileMap.WorldToMap(position));
    }

    public (Texture texture, Vector2 offset) GetTileTexture(int index)
    {
        return (this.tileMap.TileSet.TileGetTexture(index), this.tileMap.TileSet.TileGetTextureOffset(index));
    }

    private int IdForPoint(Vector2 point)
    {
        var x = point.x - this.usedRect.Position.x;
        var y = point.y - this.usedRect.Position.y;

        return (int)(x + y * this.usedRect.Size.x);
    }
}

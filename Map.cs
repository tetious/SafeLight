using Godot;
using System;
using System.Linq;
using Godot.Collections;

public class Map : Node2D
{
    private readonly AStar2D astar = new AStar2D();
    private TileMap tileMap;
    private Rect2 usedRect;

    public override void _Ready()
    {
        this.tileMap = this.GetNode<TileMap>("TileMap");
        this.usedRect = this.tileMap.GetUsedRect();

        var usedTiles = this.tileMap.GetUsedCells().Cast<Vector2>().ToArray();
        foreach (var position in usedTiles)
        {
            this.astar.AddPoint(this.IdForPoint(position), position);
        }

        foreach (var position in usedTiles)
        {
            var id = this.IdForPoint(position);

            for (var x = 0; x < 3; x++)
            for (var y = 0; y < 3; y++)
            {
                var target = position + new Vector2(x - 1, y - 1);
                var targetId = this.IdForPoint(target);
                if (position == target || this.astar.HasPoint(targetId) == false) continue;

                var cellIndex = this.tileMap.GetCellv(target);
                var rawShapes = this.tileMap.TileSet.TileGetShapes(cellIndex);
                if (rawShapes.Count == 1 && rawShapes[0] is Dictionary shapes)
                {
                    if (shapes.Contains("shape") == false) this.astar.ConnectPoints(id, targetId);
                }
                else
                {
                    this.astar.ConnectPoints(id, targetId);
                }
            }
        }
    }

    public Vector2[] GetPath(Vector2 start, Vector2 end)
    {
        var startId = this.IdForPoint(this.tileMap.WorldToMap(start));
        var endId = this.IdForPoint(this.tileMap.WorldToMap(end));
        return this.astar.GetPointPath(startId, endId).Select(p => this.tileMap.MapToWorld(p)).ToArray();
    }

    private int IdForPoint(Vector2 point)
    {
        var x = point.x - this.usedRect.Position.x;
        var y = point.y - this.usedRect.Position.y;

        return (int)(x + y * this.usedRect.Size.x);
    }
}

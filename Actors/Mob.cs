using System;
using System.Collections.Generic;
using Godot;
using System.Linq;


namespace Safelight.Actors
{
    public class Mob : Area2D
    {
        public WorldManager World { get; private set; }

        public List<Vector2> Path { get; set; } = new List<Vector2>();

        public override void _Ready()
        {
            this.World = (WorldManager)this.FindParent("WorldManager");
            WorldState.I.Connect("WorldTick", this, "WorldTick");
        }

        public override void _Process(float delta)
        {
            //if (this.Path.Count != 0) this.MoveAlongPath(this.WalkSpeed * delta);
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
}

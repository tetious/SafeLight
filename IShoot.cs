using System.Collections.Generic;
using Godot;
using Safelight.Actors;

namespace Safelight
{
    public interface IShoot
    {
        WorldManager World { get; }

        HashSet<Mob> VisibleMobs { get; }

        void FireAt(Vector2 spot);
    }
}

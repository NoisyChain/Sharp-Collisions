using FixMath.NET;
using Godot;

namespace SharpCollisions
{
    [System.Serializable]
    public class CollisionManifold2D : Object
    {
        public SharpBody2D CollidedWith;
        public FixVector2 Normal;
        public FixVector2 Depth;
        public FixVector2 ContactPoint;

        public CollisionManifold2D() {}

        public CollisionManifold2D(SharpBody2D body, FixVector2 normal, FixVector2 depth, FixVector2 contact)
        {
            CollidedWith = body;
            Normal = normal;
            Depth = depth;
            ContactPoint = contact;
        }
    }
}

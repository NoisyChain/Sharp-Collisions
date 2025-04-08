using FixMath.NET;
using Godot;

namespace SharpCollisions.Sharp2D
{
    [System.Serializable]
    public partial class CollisionManifold2D : GodotObject
    {
        public SharpBody2D CollidedWith;
        public int ColliderA;
        public int ColliderB;
        public FixVector2 Normal;
        public FixVector2 Depth;
        public FixVector2 ContactPoint;

        public SharpCollider2D Collider => CollidedWith.GetCollider(ColliderB);

        public CollisionManifold2D() {}

        public CollisionManifold2D(SharpBody2D body, int colA, int colB, FixVector2 normal, FixVector2 depth, FixVector2 contact)
        {
            CollidedWith = body;
            ColliderA = colA;
            ColliderB = colB;
            Normal = normal;
            Depth = depth;
            ContactPoint = contact;
        }
    }
}

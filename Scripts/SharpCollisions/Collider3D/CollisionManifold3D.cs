using FixMath.NET;
using Godot;

namespace SharpCollisions
{
    [System.Serializable]
    public partial class CollisionManifold3D : GodotObject
    {
        public SharpBody3D CollidedWith;
        public FixVector3 Normal;
        public FixVector3 Depth;
        public FixVector3 ContactPoint;

        public CollisionManifold3D() {}

        public CollisionManifold3D(SharpBody3D body, FixVector3 normal, FixVector3 depth, FixVector3 contact)
        {
            CollidedWith = body;
            Normal = normal;
            Depth = depth;
            ContactPoint = contact;
        }
    }
}

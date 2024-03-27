using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class AABBCollider3D : SharpCollider3D
    {
        public FixVector3 Extents => (FixVector3)extents;

        [Export] private Vector3 extents = Vector3.One;

        public override void _Ready()
        {
            base._Ready();
            Shape = CollisionType3D.AABB;
        }

        public override void DebugDrawShapes()
        {
            if (!DrawDebug) return;
            if (Draw3D == null) return;

            Draw3D.Call("clear");
            Draw3D.Call("cube_normal", (Vector3)Center, Vector3.Forward, (Vector3)Extents, debugColor);
        }

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdateAABBBoundingBox();
        }

        public override void UpdatePoints(SharpBody3D body)
        {
            base.UpdatePoints(body);
        }

        public FixVolume UpdateAABBBoundingBox()
        {
            Fix64 minX = Center.x - Extents.x;
            Fix64 minY = Center.y - Extents.y;
            Fix64 minZ = Center.z - Extents.z;
            Fix64 maxX = Center.x + Extents.x;
            Fix64 maxY = Center.y + Extents.y;
            Fix64 maxZ = Center.z + Extents.z;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }
    }
}

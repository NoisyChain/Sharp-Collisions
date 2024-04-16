using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public partial class AABBCollider3D : SharpCollider3D
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

            Transform3D debugTransform = new(ParentNode.GlobalBasis, (Vector3)Center);
            DebugDraw.Box(debugTransform, (Vector3)Extents * 2, debugColor);
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

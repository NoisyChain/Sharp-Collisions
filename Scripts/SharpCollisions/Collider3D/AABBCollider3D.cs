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

            Vector3 fCenter = (Vector3)Center;
            Vector3 fExtents = (Vector3)Extents;

            float minX = fCenter.X - fExtents.X;
            float minY = fCenter.Y - fExtents.Y;
            float minZ = fCenter.Z - fExtents.Z;
            float maxX = fCenter.X + fExtents.X;
            float maxY = fCenter.Y + fExtents.Y;
            float maxZ = fCenter.Z + fExtents.Z;

            Vector3 point1 = new Vector3(minX, minY, minZ);
            Vector3 point2 = new Vector3(maxX, minY, minZ);
            Vector3 point3 = new Vector3(maxX, maxY, minZ);
            Vector3 point4 = new Vector3(minX, maxY, minZ);
            Vector3 point5 = new Vector3(minX, minY, maxZ);
            Vector3 point6 = new Vector3(maxX, minY, maxZ);
            Vector3 point7 = new Vector3(maxX, maxY, maxZ);
            Vector3 point8 = new Vector3(minX, maxY, maxZ);

            //Draw Lower quad
            DebugDraw3D.DrawLine(point1, point2, debugColor);
            DebugDraw3D.DrawLine(point2, point3, debugColor);
            DebugDraw3D.DrawLine(point3, point4, debugColor);
            DebugDraw3D.DrawLine(point4, point1, debugColor);
            //Draw Upper quad
            DebugDraw3D.DrawLine(point5, point6, debugColor);
            DebugDraw3D.DrawLine(point6, point7, debugColor);
            DebugDraw3D.DrawLine(point7, point8, debugColor);
            DebugDraw3D.DrawLine(point8, point5, debugColor);
            //Connect both quads
            DebugDraw3D.DrawLine(point1, point5, debugColor);
            DebugDraw3D.DrawLine(point2, point6, debugColor);
            DebugDraw3D.DrawLine(point3, point7, debugColor);
            DebugDraw3D.DrawLine(point4, point8, debugColor);
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

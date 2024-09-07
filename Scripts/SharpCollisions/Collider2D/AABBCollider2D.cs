using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public partial class AABBCollider2D : SharpCollider2D
    {
        public FixVector2 Extents => (FixVector2)extents;

        [Export] private Vector2 extents = Vector2.One;

        public override void _Ready()
        {
            base._Ready();
            Shape = CollisionType2D.AABB;
        }

        public override void DebugDrawShapes()
        {
            if (!DrawDebug) return;
            
            Vector2 fCenter = (Vector2)Center;
            Vector2 fExtents = (Vector2)Extents;

            float minX = fCenter.X - fExtents.X;
            float minY = fCenter.Y - fExtents.Y;
            float maxX = fCenter.X + fExtents.X;
            float maxY = fCenter.Y + fExtents.Y;

            Vector3 point1 = new Vector3(minX, minY, 0);
            Vector3 point2 = new Vector3(maxX, minY, 0);
            Vector3 point3 = new Vector3(maxX, maxY, 0);
            Vector3 point4 = new Vector3(minX, maxY, 0);

            DebugDraw3D.DrawLine(point1, point2, debugColor);
            DebugDraw3D.DrawLine(point2, point3, debugColor);
            DebugDraw3D.DrawLine(point3, point4, debugColor);
            DebugDraw3D.DrawLine(point4, point1, debugColor);
        }

        protected override FixRect GetBoundingBoxPoints()
        {
            return UpdateAABBBoundingBox();
        }

        public override void UpdatePoints(SharpBody2D body)
        {
            base.UpdatePoints(body);
        }

        public FixRect UpdateAABBBoundingBox()
        {
            Fix64 minX = Center.x - Extents.x;
            Fix64 minY = Center.y - Extents.y;
            Fix64 maxX = Center.x + Extents.x;
            Fix64 maxY = Center.y + Extents.y;

            return new FixRect(minX, minY, maxX, maxY);
        }
    }
}

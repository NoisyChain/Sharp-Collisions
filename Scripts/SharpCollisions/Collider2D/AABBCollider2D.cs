using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class AABBCollider2D : SharpCollider2D
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
            if (Draw3D == null) return;

            Draw3D.Call("clear");
            Draw3D.Call("cube_normal", (Vector3)Center, Vector3.Forward, (Vector3)Extents + Vector3.Forward, debugColor);
            //DebugDrawCS.DrawBox((Vector3)Center, (Vector3)Extents * 2 + Vector3.Forward, debugColor, true);
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

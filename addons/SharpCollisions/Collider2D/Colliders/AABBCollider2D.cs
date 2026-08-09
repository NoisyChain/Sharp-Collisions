using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class AABBCollider2D : SharpCollider2D
    {
        [Export] private Vector2 _extents
        {
            get => new Vector2((float)Fix64.FromRaw(raw_Extents_X), (float)Fix64.FromRaw(raw_Extents_Y));
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_Extents_X = ((Fix64)((decimal)value.X)).RawValue;
                    raw_Extents_Y = ((Fix64)((decimal)value.Y)).RawValue;
                    Extents = new FixVector2(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y));
                }
            }
        }

        [ExportSubgroup("Raw Values")]
        [Export] private long raw_extents_x
        {
            get => raw_Extents_X;
            set
            {
                raw_Extents_X = value;
                Extents = new FixVector2(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y));
            }
        }
        [Export] private long raw_extents_y
        {
            get => raw_Extents_Y;
            set
            {
                raw_Extents_Y = value;
                Extents = new FixVector2(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y));
            }
        }

        private long raw_Extents_X;
        private long raw_Extents_Y;

        public FixVector2 Extents = new FixVector2();

        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType2D.AABB;
        }

        public override void DebugDrawShapes(SharpBody2D reference, bool selected)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;

            Color drawColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            Vector2 fCenter = (Vector2)reference.FixedPosition + (Vector2)PositionOffset;
            Vector2 fExtents = (Vector2)Extents;

            float minX = fCenter.X - fExtents.X;
            float minY = fCenter.Y - fExtents.Y;
            float maxX = fCenter.X + fExtents.X;
            float maxY = fCenter.Y + fExtents.Y;

            Vector3 point1 = new Vector3(minX, minY, 0);
            Vector3 point2 = new Vector3(maxX, minY, 0);
            Vector3 point3 = new Vector3(maxX, maxY, 0);
            Vector3 point4 = new Vector3(minX, maxY, 0);

            DebugDraw3D.DrawLine(point1, point2, drawColor);
            DebugDraw3D.DrawLine(point2, point3, drawColor);
            DebugDraw3D.DrawLine(point3, point4, drawColor);
            DebugDraw3D.DrawLine(point4, point1, drawColor);
        }

        public override void UpdatePoints(FixVector2 position, Fix64 rotation)
        {
            base.UpdatePoints(position, rotation);
        }

        public override void UpdateBoundingBox()
        {
            BoundingBox = CollisionMath2D.UpdateAABBBoundingBox(Center, Extents);
        }
    }
}

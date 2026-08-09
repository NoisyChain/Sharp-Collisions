using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class AABBCollider3D : SharpCollider3D
    {
        [Export] private Vector3 _extents
        {
            get => new Vector3((float)Fix64.FromRaw(raw_Extents_X), (float)Fix64.FromRaw(raw_Extents_Y), (float)Fix64.FromRaw(raw_Extents_Z));
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_Extents_X = ((Fix64)((decimal)value.X)).RawValue;
                    raw_Extents_Y = ((Fix64)((decimal)value.Y)).RawValue;
                    raw_Extents_Z = ((Fix64)((decimal)value.Z)).RawValue;
                    Extents = new FixVector3(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y), Fix64.FromRaw(raw_Extents_Z));
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
                Extents = new FixVector3(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y), Fix64.FromRaw(raw_Extents_Z));
            }
        }
        [Export] private long raw_extents_y
        {
            get => raw_Extents_Y;
            set
            {
                raw_Extents_Y = value;
                Extents = new FixVector3(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y), Fix64.FromRaw(raw_Extents_Z));
            }
        }
        [Export] private long raw_extents_z
        {
            get => raw_Extents_Z;
            set
            {
                raw_Extents_Z = value;
                Extents = new FixVector3(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y), Fix64.FromRaw(raw_Extents_Z));
            }
        }

        private long raw_Extents_X;
        private long raw_Extents_Y;
        private long raw_Extents_Z;

        public FixVector3 Extents = new FixVector3();

        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType3D.AABB;
        }

        public override void DebugDrawShapes(SharpBody3D reference, bool selected)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;

            Color drawColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            Vector3 pos = (Vector3)PositionOffset;
            Vector3 newPos = SharpHelpers.Transform3D(pos, (Vector3)reference.FixedPosition, (Vector3)reference.FixedRotation);

            DebugDraw3D.DrawBox(newPos, Quaternion.Identity, (Vector3)Extents * 2, drawColor, true);
        }

        public override void UpdateBoundingBox() 
        {
            BoundingBox = CollisionMath3D.UpdateAABBBoundingBox(Center, Extents);
        }

        public override void UpdatePoints(FixVector3 position, FixVector3 rotation)
        {
            base.UpdatePoints(position, rotation);
        }
    }
}

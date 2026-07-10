using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool]
    [GlobalClass]
    public partial class BoxCollider2D : ConvexShapeCollider2D
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
        [Export] private long raw_extents_X
        {
            get => raw_Extents_X;
            set
            {
                raw_Extents_X = value;
                Extents = new FixVector2(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y));
            }
        }
        [Export] private long raw_extents_Y
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

        public FixVector2 Extents;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void CreatePolygonPoints()
        {
            RawPoints = new FixVector2[]
            {
                new FixVector2(-Extents.x, Extents.y),
                new FixVector2(-Extents.x, -Extents.y),
                new FixVector2(Extents.x, -Extents.y),
                new FixVector2(Extents.x, Extents.y)
            };

            Points = new FixVector2[RawPoints.Length];
        }

        public override void DebugDrawShapesEditor(SharpBody2D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebugShape) return;

            Color finalColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            Vector3 PosOffset = new Vector3((float)PositionOffset.x, (float)PositionOffset.y, 0);
            Vector3 RotOffset = new Vector3(0, 0, (float)RotationOffset);
            Vector3 scaledExtents = new Vector3((float)Extents.x * 2, (float)Extents.y * 2, 0.1f);

            Vector3 FinalRot = new Vector3(0.0f, 0.0f, (float)reference.FixedRotation);

            Vector3 rotPos = SharpHelpers.RotateDeg3D(PosOffset, RotOffset);
            Vector3 newPos = SharpHelpers.Transform3D(rotPos, (Vector3)reference.FixedPosition, FinalRot);

            DebugDraw3D.DrawBox(newPos, Quaternion.FromEuler(FinalRot), scaledExtents, finalColor, true);
        }
    }
}

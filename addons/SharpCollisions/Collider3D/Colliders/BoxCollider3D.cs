using Godot;
using Godot.Collections;
using FixMath.NET;
using System;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class BoxCollider3D : ConvexShapeCollider3D
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
        }

        public override void DebugDrawShapes(SharpBody3D reference, bool selected)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;

            Color drawColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            Vector3 scaledPosOffset = (Vector3)PositionOffset;
            Vector3 scaledRotOffset = (Vector3)RotationOffset;

            Vector3 rotPos = SharpHelpers.Rotate3D(scaledPosOffset, scaledRotOffset);
            Vector3 newPos = SharpHelpers.Transform3D(rotPos, (Vector3)reference.FixedPosition, (Vector3)reference.FixedRotation);

            DebugDraw3D.DrawBox(newPos, Quaternion.FromEuler((Vector3)reference.FixedRotation + scaledRotOffset), (Vector3)Extents * 2, drawColor, true);
        }

        protected override void CreatePolygonPoints()
        {
            RawPoints = new FixVector3[]
            {
                new FixVector3(-Extents.x, -Extents.y, Extents.z),
                new FixVector3(-Extents.x, -Extents.y, -Extents.z),
                new FixVector3(Extents.x, -Extents.y, -Extents.z),
                new FixVector3(Extents.x, -Extents.y, Extents.z),
                new FixVector3(-Extents.x, Extents.y, Extents.z),
                new FixVector3(-Extents.x, Extents.y, -Extents.z),
                new FixVector3(Extents.x, Extents.y, -Extents.z),
                new FixVector3(Extents.x, Extents.y, Extents.z)
            };

            Points = new FixVector3[RawPoints.Length];
        }

        protected override void CreateFaces()
        {
            Faces = new Array<Vector3I>()
            {
                new Vector3I(0, 1, 2),
                new Vector3I(0, 2, 3),
                new Vector3I(4, 1, 0),
                new Vector3I(1, 4, 5),
                new Vector3I(5, 2, 1),
                new Vector3I(2, 5, 6),
                new Vector3I(7, 3, 2),
                new Vector3I(6, 7, 2),
                new Vector3I(0, 3, 4),
                new Vector3I(7, 4, 3),
                new Vector3I(6, 5, 4),
                new Vector3I(7, 6, 4),
            };
        }
    }
}

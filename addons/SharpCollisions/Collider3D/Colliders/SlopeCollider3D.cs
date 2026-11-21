using Godot;
using Godot.Collections;
using FixMath.NET;
using System;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class SlopeCollider3D : ConvexShapeCollider3D
    {
        [Export] private Vector3 _extents
        {
            get => (Vector3)Extents;
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_Extents_X = ((Fix64)value.X).RawValue;
                    raw_Extents_Y = ((Fix64)value.Y).RawValue;
                    raw_Extents_Z = ((Fix64)value.Z).RawValue;
                    Extents = new FixVector3(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y), Fix64.FromRaw(raw_Extents_Z));
                }
            }
        }

        [ExportSubgroup("Raw Values")]
        [Export] private long raw_extents_x
        {
            get => Extents.x.RawValue;
            set
            {
                raw_Extents_X = value;
                Extents = new FixVector3(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y), Fix64.FromRaw(raw_Extents_Z));
            }
        }
        [Export] private long raw_extents_y
        {
            get => Extents.y.RawValue;
            set
            {
                raw_Extents_Y = value;
                Extents = new FixVector3(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y), Fix64.FromRaw(raw_Extents_Z));
            }
        }
        [Export] private long raw_extents_z
        {
            get => Extents.z.RawValue;
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

            FixVector3[] points = new FixVector3[]
            {
                new FixVector3(-Extents.x, -Extents.y, Extents.z),
                new FixVector3(-Extents.x, -Extents.y, -Extents.z),
                new FixVector3(Extents.x, -Extents.y, -Extents.z),
                new FixVector3(Extents.x, -Extents.y, Extents.z),
                new FixVector3(-Extents.x, Extents.y, -Extents.z),
                new FixVector3(Extents.x, Extents.y, -Extents.z),
            };

            for (int i = 0; i < points.Length; i++)
            {
                FixVector3 rotPoints = FixVector3.Rotate(points[i] + PositionOffset, RotationOffset * Fix64.DegToRad);
				points[i] = FixVector3.Transform(rotPoints + PositionOffset, reference.FixedPosition, reference.FixedRotation * Fix64.DegToRad);
            }

            //Draw Lower quad
            DebugDraw3D.DrawLine((Vector3)points[0], (Vector3)points[1], drawColor);
            DebugDraw3D.DrawLine((Vector3)points[1], (Vector3)points[2], drawColor);
            DebugDraw3D.DrawLine((Vector3)points[2], (Vector3)points[3], drawColor);
            DebugDraw3D.DrawLine((Vector3)points[3], (Vector3)points[0], drawColor);
            //Draw Upper quad
            DebugDraw3D.DrawLine((Vector3)points[4], (Vector3)points[5], drawColor);
            DebugDraw3D.DrawLine((Vector3)points[5], (Vector3)points[2], drawColor);
            DebugDraw3D.DrawLine((Vector3)points[4], (Vector3)points[1], drawColor);
            //Connect both quads
            DebugDraw3D.DrawLine((Vector3)points[0], (Vector3)points[4], drawColor);
            DebugDraw3D.DrawLine((Vector3)points[3], (Vector3)points[5], drawColor);
        }

        protected override void CreatePolygonPoints()
        {
            RawPoints = new FixVector3[]
            {
                new FixVector3(-Extents.x, -Extents.y, Extents.z),
                new FixVector3(-Extents.x, -Extents.y, -Extents.z),
                new FixVector3(Extents.x, -Extents.y, -Extents.z),
                new FixVector3(Extents.x, -Extents.y, Extents.z),
                new FixVector3(-Extents.x, Extents.y, -Extents.z),
                new FixVector3(Extents.x, Extents.y, -Extents.z),
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
                new Vector3I(0, 3, 4),
            };
        }
    }
}

using Godot;
using Godot.Collections;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class ConvexShapeCollider3D : SharpCollider3D
    {
        [Export] private Array<Vector3> _points
        {
            get
            {
                Array<Vector3> ret = new Array<Vector3>();
                if (RawPoints != null)
                {                
                    for (int i = 0; i < RawPoints.Length; i++)
                    {
                        ret.Add((Vector3)RawPoints[i]);
                    }
                }
                return ret;
            }
            set
            {
                if (Engine.IsEditorHint())  // Avoid any float values changing fixed point raw values when the game runs
                {
                    rawPoints_X = new long[value.Count];
                    rawPoints_Y = new long[value.Count];
                    rawPoints_Z = new long[value.Count];
                    RawPoints = new FixVector3[value.Count];
                    for (int i = 0; i < RawPoints.Length; i++)
                    {
                        rawPoints_X[i] = ((Fix64)value[i].X).RawValue;
                        rawPoints_Y[i] = ((Fix64)value[i].Y).RawValue;
                        rawPoints_Z[i] = ((Fix64)value[i].Z).RawValue;
                        RawPoints[i] = new FixVector3(Fix64.FromRaw(rawPoints_X[i]), Fix64.FromRaw(rawPoints_Y[i]), Fix64.FromRaw(rawPoints_Z[i]));
                    }
                }
            }
        }

        [Export] public Array<Vector3I> Faces;

        [ExportSubgroup("Raw Values")]
        [Export] private long[] raw_points_x
        {
            get
            {
                long[] ret = new long[RawPoints.Length];
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = RawPoints[i].x.RawValue;
                }
                return ret;
            }
            set
            {
                rawPoints_X = value;
                RawPoints = new FixVector3[value.Length];
                for (int i = 0; i < RawPoints.Length; i++)
                {
                    RawPoints[i] = new FixVector3(Fix64.FromRaw(rawPoints_X[i]), Fix64.FromRaw(rawPoints_Y[i]), Fix64.FromRaw(rawPoints_Z[i]));
                }
            }
        }
        [Export] private long[] raw_points_y
        {
            get
            {
                long[] ret = new long[RawPoints.Length];
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = RawPoints[i].y.RawValue;
                }
                return ret;
            }
            set
            {
                rawPoints_Y = value;
                RawPoints = new FixVector3[value.Length];
                for (int i = 0; i < RawPoints.Length; i++)
                {
                    RawPoints[i] = new FixVector3(Fix64.FromRaw(rawPoints_X[i]), Fix64.FromRaw(rawPoints_Y[i]), Fix64.FromRaw(rawPoints_Z[i]));
                }
            }
        }
        [Export] private long[] raw_points_z
        {
            get
            {
                long[] ret = new long[RawPoints.Length];
                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = RawPoints[i].z.RawValue;
                }
                return ret;
            }
            set
            {
                rawPoints_Z = value;
                RawPoints = new FixVector3[value.Length];
                for (int i = 0; i < RawPoints.Length; i++)
                {
                    RawPoints[i] = new FixVector3(Fix64.FromRaw(rawPoints_X[i]), Fix64.FromRaw(rawPoints_Y[i]), Fix64.FromRaw(rawPoints_Z[i]));
                }
            }
        }

        private long[] rawPoints_X;
        private long[] rawPoints_Y;
        private long[] rawPoints_Z;
        
        public FixVector3[] RawPoints;
		public FixVector3[] Points;        

        private bool defaultShape = false;

        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType3D.Polygon;
            CreatePolygonPoints();
            CreateFaces();
        }

        protected virtual void CreatePolygonPoints()
        {
            //If there is no enough vertices to create a 3D shape,
            //create a simple tetrahedron as the default shape
            if (RawPoints == null || RawPoints.Length < 4)
            {
                RawPoints = new FixVector3[]
                {
                    new FixVector3(Fix64.Zero, Fix64.NegativeOne, Fix64.One),
                    new FixVector3(Fix64.NegativeOne, Fix64.NegativeOne, Fix64.NegativeOne),
                    new FixVector3(Fix64.One, Fix64.NegativeOne, Fix64.NegativeOne),
                    new FixVector3(Fix64.Zero, Fix64.One, Fix64.Zero),
                };
                GD.PushWarning("Polygon shapes cannot be simpler than a tetrahedron.");
                defaultShape = true;
            }

            Points = new FixVector3[RawPoints.Length];
        }

        protected virtual void CreateFaces()
        {
            //If the default shape is confirmed, create faces for it
            if (defaultShape)
            {
                Faces = new Array<Vector3I>()
                {
                    new Vector3I(0, 1, 2),
                    new Vector3I(3, 1, 0),
                    new Vector3I(0, 2, 3),
                    new Vector3I(3, 2, 1)
                };
            }
        }

        private void UpdatePolygonPoints(FixVector3 position, FixVector3 rotation)
        {
            CreatePolygonPoints();
            for (int i = 0; i < RawPoints.Length; i++)
            {
                FixVector3 rotPoints = FixVector3.Rotate(RawPoints[i], RotationOffset);
				Points[i] = FixVector3.Transform(rotPoints + PositionOffset, position, rotation);
            }
        }

        public override void DebugDrawShapes(SharpBody3D reference, bool selected)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;
            if (_points == null || _points.Count <= 0) return;
            if (Faces == null || Faces.Count <= 0) return;

            Color drawColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            Vector3 scaledPosOffset = (Vector3)PositionOffset;
            Vector3 scaledRotOffset = (Vector3)(RotationOffset * Fix64.DegToRad);

            Vector3 position = (Vector3)reference.FixedPosition;
            Vector3 rotation = (Vector3)(reference.FixedRotation * Fix64.DegToRad);

            for (int i = 0; i < Faces.Count; i++)
            {
                Vector3 rotPointA = SharpHelpers.Rotate3D(_points[Faces[i].X], scaledRotOffset);
                Vector3 pointA = SharpHelpers.Transform3D(rotPointA + scaledPosOffset, position, rotation);
                Vector3 rotPointB = SharpHelpers.Rotate3D(_points[Faces[i].Y], scaledRotOffset);
                Vector3 pointB = SharpHelpers.Transform3D(rotPointB + scaledPosOffset, position, rotation);
                Vector3 rotPointC = SharpHelpers.Rotate3D(_points[Faces[i].Z], scaledRotOffset);
                Vector3 pointC = SharpHelpers.Transform3D(rotPointC + scaledPosOffset, position, rotation);

                DebugDraw3D.DrawLine(pointA, pointB, drawColor);
                DebugDraw3D.DrawLine(pointB, pointC, drawColor);
                DebugDraw3D.DrawLine(pointC, pointA, drawColor);
            }
        }

        public override void UpdateBoundingBox()
        {
            BoundingBox = CollisionMath3D.UpdatePolygonBoundingBox(Points);
        }

        public override void UpdatePoints(FixVector3 position, FixVector3 rotation)
        {
            UpdatePolygonPoints(position, rotation);
            base.UpdatePoints(position, rotation);
        }

        public Vector3I GetNearestFace(FixVector3 normal, out FixVector3 finalNormal)
        {
            Fix64 dot = Fix64.NegativeOne;
            Fix64 dist = Fix64.MaxValue;
            Vector3I face = Vector3I.Zero;
            finalNormal = FixVector3.Zero;
            
            for (int i = 0; i < Faces.Count; i++)
            {
                Vector3I f = Faces[i];
                FixVector3 faceNormal = FixVector3.GetPlaneNormal(Points[f.X], Points[f.Y], Points[f.Z]);
                FixVector3 faceCenter = FixVector3.FindTriangleCentroid(Points[f.X], Points[f.Y], Points[f.Z]);
                Fix64 d = FixVector3.Dot(normal, faceNormal);

                if (dot < d)
                {
                    face = f;
                    finalNormal = faceNormal;
                    dot = d;
                }
            }
            return face;
        }
    }
}

using Godot;
using Godot.Collections;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class ConvexShapeCollider2D : SharpCollider2D
    {              
        [Export] private Array<Vector2> _points
        {
            get
            {
                Array<Vector2> ret = new Array<Vector2>();
                if (RawPoints != null)
                {                
                    for (int i = 0; i < RawPoints.Length; i++)
                    {
                        ret.Add((Vector2)RawPoints[i]);
                    }
                }
                return ret;
            }
            set
            {
                if (Engine.IsEditorHint()) // Avoid any float values changing fixed point raw values when the game runs
                {
                    raw_Points_X = new long[value.Count];
                    raw_Points_Y = new long[value.Count];
                    RawPoints = new FixVector2[value.Count];
                    for (int i = 0; i < RawPoints.Length; i++)
                    {
                        raw_Points_X[i] = ((Fix64)value[i].X).RawValue;
                        raw_Points_Y[i] = ((Fix64)value[i].Y).RawValue;
                        RawPoints[i] = new FixVector2(Fix64.FromRaw(raw_Points_X[i]), Fix64.FromRaw(raw_Points_Y[i]));
                    }
                }
            }
        }

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
                raw_Points_X = value;
                RawPoints = new FixVector2[value.Length];
                for (int i = 0; i < RawPoints.Length; i++)
                {
                    RawPoints[i] = new FixVector2(Fix64.FromRaw(raw_Points_X[i]), Fix64.FromRaw(raw_Points_Y[i]));
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
                raw_Points_Y = value;
                RawPoints = new FixVector2[value.Length];
                for (int i = 0; i < RawPoints.Length; i++)
                {
                    RawPoints[i] = new FixVector2(Fix64.FromRaw(raw_Points_X[i]), Fix64.FromRaw(raw_Points_Y[i]));
                }
            }
        }

        private long[] raw_Points_X;
        private long[] raw_Points_Y;

        public FixVector2[] RawPoints;
		public FixVector2[] Points;
        
        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType2D.Polygon;
            CreatePolygonPoints();
        }

        public virtual void CreatePolygonPoints()
        {
            //If there is no enough vertices to create a 2D shape,
            //create a simple triangle as the default shape
            if (RawPoints == null || RawPoints.Length < 3)
            {
                RawPoints = new FixVector2[] {
                    new FixVector2(Fix64.NegativeOne, Fix64.NegativeOne),
                    new FixVector2(Fix64.Zero, Fix64.One),
                    new FixVector2(Fix64.One, Fix64.NegativeOne)
                };
                GD.PushWarning("Polygon shape cannot be simpler than a triangle.");
            }
            
            Points = new FixVector2[RawPoints.Length];
        }

        private void UpdatePolygonPoints(FixVector2 position, Fix64 rotation)
        {
            CreatePolygonPoints();
            for (int i = 0; i < RawPoints.Length; i++)
            {
                Points[i] = FixVector2.Rotate(RawPoints[i], RotationOffset);
				Points[i] = FixVector2.Transform(Points[i] + PositionOffset, position, rotation);
            }
        }

        public override void DebugDrawShapes(SharpBody2D reference, bool selected)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;
            if (_points == null || _points.Count <= 0) return;

            Color drawColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            Vector2 PosOffset = (Vector2)PositionOffset;
            float RotOffset = (float)(RotationOffset * Fix64.DegToRad);

            Vector3 position = (Vector3)reference.FixedPosition;
            float rotation = (float)(reference.FixedRotation * Fix64.DegToRad);

            for (int i = 0; i < _points.Count; i++)
            {
                Vector2 start = _points[i];
                Vector2 end = _points[(i + 1) % _points.Count];

                Vector2 rotPointA = SharpHelpers.Rotate2D(start, RotOffset);
                Vector3 pointA = SharpHelpers.Transform2D3D(rotPointA + PosOffset, position, rotation);
                Vector2 rotPointB = SharpHelpers.Rotate2D(end, RotOffset);
                Vector3 pointB = SharpHelpers.Transform2D3D(rotPointB + PosOffset, position, rotation);

                DebugDraw3D.DrawLine(pointA, pointB, drawColor);
            }
        }

        public override void UpdatePoints(FixVector2 position, Fix64 rotation)
        {
            UpdatePolygonPoints(position, rotation);
            base.UpdatePoints(position, rotation);
        }

        public override void UpdateBoundingBox()
        {
            BoundingBox = CollisionMath2D.UpdatePolygonBoundingBox(Points);
        }
    }
}

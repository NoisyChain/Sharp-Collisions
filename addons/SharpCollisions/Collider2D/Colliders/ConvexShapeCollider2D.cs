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
                    _rawPoints = new long[value.Count * 2];
                    RawPoints = new FixVector2[value.Count];
                    for (int i = 0; i < RawPoints.Length; i++)
                    {
                        _rawPoints[i * 2] = ((Fix64)value[i].X).RawValue;
                        _rawPoints[i * 2 + 1] = ((Fix64)value[i].Y).RawValue;
                        RawPoints[i] = new FixVector2(Fix64.FromRaw(_rawPoints[i * 2]), Fix64.FromRaw(_rawPoints[i * 2 + 1]));
                    }
                }
            }
        }

        [ExportSubgroup("Raw Values")]
        [Export] private long[] _raw_points
        {
            get
            {
                long[] ret = new long[RawPoints.Length * 2];
                for (int i = 0; i < RawPoints.Length; i++)
                {
                    ret[i * 2] = RawPoints[i].x.RawValue;
                    ret[i * 2 + 1] = RawPoints[i].y.RawValue;
                }
                return ret;
            }
            set
            {
                _rawPoints = value;
                RawPoints = new FixVector2[_rawPoints.Length / 2];
                for (int i = 0; i < RawPoints.Length; i++)
                {
                    RawPoints[i] = new FixVector2(Fix64.FromRaw(_rawPoints[i * 2]), Fix64.FromRaw(_rawPoints[i * 2 + 1]));
                    _points[i] = (Vector2)RawPoints[i];
                }
            }
        }

        private long[] _rawPoints = [];

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

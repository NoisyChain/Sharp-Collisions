using Godot;
using Godot.Collections;
using FixMath.NET;
using SharpCollisions.Sharp2D.GJK;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class ConvexShapeCollider2D : SharpCollider2D
    {      
        public GJK2D GJK;
        
        [Export] private Array<Vector2> _points
        {
            get{
                Array<Vector2> ret = new Array<Vector2>();
                if (raw_Points_X != null && raw_Points_Y != null)
                {
                    int length = Mathf.Min(raw_Points_X.Length, raw_Points_Y.Length);
                
                    for (int i = 0; i < length; i++)
                    {
                        ret.Add(new Vector2((float)Fix64.FromRaw(raw_Points_X[i]), (float)Fix64.FromRaw(raw_Points_Y[i])));
                    }
                }
                return ret;
            }
            set{
                if (Engine.IsEditorHint())  // Avoid any float values changing fixed point raw values when the game runs
                {
                    raw_Points_X = new long[value.Count];
                    raw_Points_Y = new long[value.Count];
                    RawPoints = new FixVector2[value.Count];
                    for (int i = 0; i < value.Count; i++)
                    {
                        raw_Points_X[i] = ((Fix64)((decimal)value[i].X)).RawValue;
                        raw_Points_Y[i] = ((Fix64)((decimal)value[i].Y)).RawValue;
                        RawPoints[i] = new FixVector2(Fix64.FromRaw(raw_Points_X[i]), Fix64.FromRaw(raw_Points_Y[i]));
                    }
                }
            }
        }

        [ExportSubgroup("Raw Values")]
        [Export] private long[] raw_points_x
        {
            get => raw_Points_X;
            set
            {
                raw_Points_X = value;
                RawPoints = new FixVector2[_points.Count];
                for (int i = 0; i < _points.Count; i++)
                {
                    RawPoints[i] = new FixVector2(Fix64.FromRaw(raw_Points_X[i]), Fix64.FromRaw(raw_Points_Y[i]));
                }
            }
        }
        [Export] private long[] raw_points_y
        {
            get => raw_Points_Y;
            set
            {
                raw_Points_Y = value;
                RawPoints = new FixVector2[_points.Count];
                for (int i = 0; i < _points.Count; i++)
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
            GJK = new GJK2D();
            base.Initialize();
            Shape = CollisionType2D.Polygon;
            CreatePolygonPoints();
        }

        public override bool CollisionDetection(SharpCollider2D other, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

            if (other.Shape == CollisionType2D.AABB) return false;

			return GJK.PolygonCollision(this, other, out Normal, out Depth, out ContactPoint);
		}

        public virtual void CreatePolygonPoints()
        {
            //If there is no enough vertices to create a 2D shape,
            //create a simple triangle as the default shape
            if (RawPoints == null || RawPoints.Length < 3)
            {
                RawPoints = new FixVector2[]
                {
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

        public override void DebugDrawShapes(SharpBody2D reference)
        {
            if (!DrawDebug) return;

            for (int i = 0; i < Points.Length; i++)
            {
                Vector3 start = (Vector3)Points[i];
                Vector3 end = (Vector3)Points[(i + 1) % Points.Length];
                DebugDraw3D.DrawLine(start, end, debugColor);
            }
        }

        public override void DebugDrawShapesEditor(Node3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebug) return;
            if (_points == null || _points.Count <= 0) return;

            Color finalColor = selected && DrawDebug ? selectedColor : debugColor;

            Vector2 PosOffset = _positionOffset;
            float RotOffset = _rotationOffset;

            Vector3 position = reference.GlobalPosition;
            float rotation = reference.GlobalRotation.Z;

            for (int i = 0; i < _points.Count; i++)
            {
                Vector2 start = _points[i];
                Vector2 end = _points[(i + 1) % _points.Count];

                Vector2 rotPointA = SharpHelpers.Rotate2D(start, Mathf.DegToRad(RotOffset));
                Vector3 pointA = SharpHelpers.Transform2D3D(rotPointA + PosOffset, position, rotation);
                Vector2 rotPointB = SharpHelpers.Rotate2D(end, Mathf.DegToRad(RotOffset));
                Vector3 pointB = SharpHelpers.Transform2D3D(rotPointB + PosOffset, position, rotation);

                DebugDraw3D.DrawLine(pointA, pointB, finalColor);
            }
        }

        protected override FixRect GetBoundingBoxPoints()
        {
            return UpdatePolygonBoundingBox();
        }

        public override void UpdatePoints(FixVector2 position, Fix64 rotation)
        {
            UpdatePolygonPoints(position, rotation);
            base.UpdatePoints(position, rotation);
        }

        public override FixVector2 Support(FixVector2 direction)
        {
            FixVector2 maxPoint = FixVector2.Zero;
			Fix64 maxDistance = Fix64.MinValue;

			for (int i = 0; i < Points.Length; i++)
			{
				Fix64 dist = FixVector2.Dot(Points[i], direction);
				if (dist > maxDistance)
				{
					maxDistance = dist;
					maxPoint = Points[i];
				}
			}
			return maxPoint;
        }

        public FixRect UpdatePolygonBoundingBox()
        {
            Fix64 minX = Fix64.MaxValue;
			Fix64 minY = Fix64.MaxValue;
			Fix64 maxX = Fix64.MinValue;
			Fix64 maxY = Fix64.MinValue;

            for (int p = 0; p < Points.Length; p++)
            {
                FixVector2 v = Points[p];

                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
            }

            return new FixRect(minX, minY, maxX, maxY);
        }
    }
}

using Godot;
using FixMath.NET;
using SharpCollisions.Sharp2D.GJK;

namespace SharpCollisions.Sharp2D
{
    [GlobalClass]
    public partial class PolygonCollider2D : SharpCollider2D
    {
        public GJK2D GJK;

        public FixVector2[] RawPoints;
		public FixVector2[] Points;

        [Export] private Vector2[] vertices = new Vector2[0];

        public override void Initialize()
        {
            GJK = new GJK2D(DrawDebugPolytope);
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

        private void CreatePolygonPoints()
        {
            RawPoints = new FixVector2[vertices.Length];
            for (int i = 0; i < RawPoints.Length; i++)
                RawPoints[i] = (FixVector2)vertices[i] + Offset;
            
            Points = new FixVector2[RawPoints.Length];
        }

        private void UpdatePolygonPoints(SharpBody2D body)
        {
            for (int i = 0; i < RawPoints.Length; i++)
				Points[i] = FixVector2.Transform(RawPoints[i], body);
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

        protected override FixRect GetBoundingBoxPoints()
        {
            return UpdatePolygonBoundingBox();
        }

        public override void UpdatePoints(SharpBody2D body)
        {
            UpdatePolygonPoints(body);
            base.UpdatePoints(body);
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

            FixVector2[] points = Points;

            for (int p = 0; p < points.Length; p++)
            {
                FixVector2 v = points[p];

                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
            }

            return new FixRect(minX, minY, maxX, maxY);
        }
    }
}
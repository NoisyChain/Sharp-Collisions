using Godot;
using FixMath.NET;
using SharpCollisions.Sharp3D.GJK;

namespace SharpCollisions.Sharp3D
{
    [GlobalClass]
    public partial class PolygonCollider3D : SharpCollider3D
    {
        public GJK3D GJK;

        public FixVector3[] RawPoints;
		public FixVector3[] Points;

        [Export] private Vector3[] vertices = new Vector3[0];

        public override void Initialize()
        {
             GJK = new GJK3D(DrawDebugPolytope);
            base.Initialize();
            Shape = CollisionType3D.Polygon;
            CreatePolygonPoints();
        }

        public override bool CollisionDetection(SharpCollider3D other, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

            if (other.Shape == CollisionType3D.AABB) return false;

            return GJK.PolygonCollision(this, other, out Normal, out Depth, out ContactPoint);
		}

        private void CreatePolygonPoints()
        {
            RawPoints = new FixVector3[vertices.Length];
            for (int i = 0; i < RawPoints.Length; i++)
                RawPoints[i] = (FixVector3)vertices[i] + Offset;
            
            Points = new FixVector3[RawPoints.Length];
        }

        private void UpdatePolygonPoints(SharpBody3D body)
        {
            for (int i = 0; i < RawPoints.Length; i++)
				Points[i] = FixVector3.Transform(RawPoints[i], body);
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!DrawDebug) return;

            for (int i = 0; i < Points.Length; i++)
            {
                Vector3 start = (Vector3)Points[i];
                Vector3 end = (Vector3)Points[(i + 1) % Points.Length];
                DebugDraw3D.DrawLine(start, end, debugColor);
            }
        }

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdatePolygonBoundingBox();
        }

        public override void UpdatePoints(SharpBody3D body)
        {
            UpdatePolygonPoints(body);
            base.UpdatePoints(body);
        }

        public override FixVector3 Support(FixVector3 direction)
        {
            FixVector3 maxPoint = FixVector3.Zero;
			Fix64 maxDistance = Fix64.MinValue;

			for (int i = 0; i < Points.Length; i++)
			{
				Fix64 dist = FixVector3.Dot(Points[i], direction);
				if (dist > maxDistance)
				{
					maxDistance = dist;
					maxPoint = Points[i];
				}
			}
			return maxPoint;
        }

        public FixVolume UpdatePolygonBoundingBox()
        {
            Fix64 minX = Fix64.MaxValue;
			Fix64 minY = Fix64.MaxValue;
            Fix64 minZ = Fix64.MaxValue;
			Fix64 maxX = Fix64.MinValue;
			Fix64 maxY = Fix64.MinValue;
            Fix64 maxZ = Fix64.MinValue;

            FixVector3[] points = Points;

            for (int p = 0; p < points.Length; p++)
            {
                FixVector3 v = points[p];

                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
                if (v.z < minZ) minZ = v.z;
                if (v.z > maxZ) maxZ = v.z;
            }

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }
    }
}

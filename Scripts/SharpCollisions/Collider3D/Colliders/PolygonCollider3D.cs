using Godot;
using Godot.Collections;
using FixMath.NET;
using SharpCollisions.Sharp3D.GJK;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class PolygonCollider3D : SharpCollider3D
    {
        [Export] protected bool DrawDebugPolytope;
        
        public GJK3D GJK;
        public FixVector3[] RawPoints;
		public FixVector3[] Points;

        [Export] private Vector3[] vertices = new Vector3[0];
        [Export] public Array<Vector3I> Faces;

        public override void Initialize()
        {
            GJK = new GJK3D(DrawDebugPolytope);
            base.Initialize();
            Shape = CollisionType3D.Polygon;
            CreatePolygonPoints();
            CreateFaces();
        }

        public override bool CollisionDetection(SharpCollider3D other, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

            if (other.Shape == CollisionType3D.AABB) return false;

            return GJK.PolygonCollision(this, other, out Normal, out Depth, out ContactPoint);
		}

        protected virtual void CreatePolygonPoints()
        {
            RawPoints = new FixVector3[vertices.Length];
            for (int i = 0; i < RawPoints.Length; i++)
                RawPoints[i] = (FixVector3)vertices[i] + Offset;
            
            Points = new FixVector3[RawPoints.Length];
        }

        protected virtual void CreateFaces()
        {
            if (Faces.Count == 0)
            {
                Faces = new Array<Vector3I>()
                {
                    new Vector3I(0, 1, 2)
                };
            }
        }

        private void UpdatePolygonPoints(SharpBody3D body)
        {
            for (int i = 0; i < RawPoints.Length; i++)
				Points[i] = FixVector3.Transform(RawPoints[i], body);
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!DrawDebug) return;

            for (int i = 0; i < Faces.Count; i++)
            {
                DebugDraw3D.DrawLine((Vector3)Points[Faces[i].X], (Vector3)Points[Faces[i].Y], debugColor);
                DebugDraw3D.DrawLine((Vector3)Points[Faces[i].Y], (Vector3)Points[Faces[i].Z], debugColor);
                DebugDraw3D.DrawLine((Vector3)Points[Faces[i].Z], (Vector3)Points[Faces[i].X], debugColor);
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
                //FixVector3 directionNormal = FixVector3.Normalize(point - fCenter);
                Fix64 d = FixVector3.Dot(normal, faceNormal);

                
                //Fix64 minPointDistance = FixVector3.LengthSq(fCenter - point);

                if (dot < d)
                {
                    face = f;
                    finalNormal = faceNormal;
                    dot = d;
                    //dist = minPointDistance;
                }
            }
            return face;
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

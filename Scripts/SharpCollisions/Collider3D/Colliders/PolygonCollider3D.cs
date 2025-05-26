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

        [Export] private Array<Vector3I> vertices;
        [Export] public Array<Vector3I> Faces;

        private bool defaultShape = false;

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
            //If there is no enough vertices to create a 3D shape,
            //create a simple tetrahedron as the default shape
            if (vertices.Count < 4)
            {
                vertices = new Array<Vector3I>()
                {
                    new Vector3I(0, -1, 1) * SharpNode.nodeScale,
                    new Vector3I(-1, -1, -1) * SharpNode.nodeScale,
                    new Vector3I(1, -1, -1) * SharpNode.nodeScale,
                    new Vector3I(0, 1, 0) * SharpNode.nodeScale,
                };
                GD.PushWarning("Polygon shapes cannot be simpler than a tetrahedron.");
                defaultShape = true;
            }

            RawPoints = new FixVector3[vertices.Count];
            for (int i = 0; i < RawPoints.Length; i++)
            {
                RawPoints[i] = new FixVector3(
                    (Fix64)vertices[i].X / SharpNode.NodeScale,
                    (Fix64)vertices[i].Y / SharpNode.NodeScale,
                    (Fix64)vertices[i].Z / SharpNode.NodeScale
                );
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
            for (int i = 0; i < RawPoints.Length; i++)
            {
                FixVector3 rotPoints = FixVector3.Rotate(RawPoints[i], RotationOffset);
				Points[i] = FixVector3.Transform(rotPoints + PositionOffset, position, rotation);
            }
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!Active) return;
            if (!DrawDebug) return;
            if (Faces == null || Faces.Count <= 0) return;

            for (int i = 0; i < Faces.Count; i++)
            {
                DebugDraw3D.DrawLine((Vector3)Points[Faces[i].X], (Vector3)Points[Faces[i].Y], debugColor);
                DebugDraw3D.DrawLine((Vector3)Points[Faces[i].Y], (Vector3)Points[Faces[i].Z], debugColor);
                DebugDraw3D.DrawLine((Vector3)Points[Faces[i].Z], (Vector3)Points[Faces[i].X], debugColor);

                FixVector3 origin = FixVector3.FindTriangleCentroid(Points[Faces[i].X], Points[Faces[i].Y], Points[Faces[i].Z]);
                FixVector3 normal = FixVector3.GetPlaneNormal(Points[Faces[i].X], Points[Faces[i].Y], Points[Faces[i].Z]);
                Vector3 dir = (Vector3)origin + ((Vector3)normal * 0.5f);
                DebugDraw3D.DrawLine((Vector3)origin, dir, new Color(0, 1, 0));
            }
        }

        public override void DebugDrawShapesEditor(Node3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebug) return;

            //We still want to see the gizmo even if there is no shape to preview
            Color finalColor = selected ? selectedColor : debugColor;
            if (selected) DebugDraw3D.DrawGizmo(reference.Transform, finalColor, true);

            if (vertices == null || vertices.Count <= 0) return;
            if (Faces == null || Faces.Count <= 0) return;

            Vector3 scaledPosOffset = (Vector3)positionOffset / SharpNode.nodeScale;
            Vector3 scaledRotOffset = (Vector3)rotationOffset / SharpNode.nodeRotation;

            Vector3 position = reference.GlobalPosition;
            Vector3 rotation = reference.GlobalRotation;

            for (int i = 0; i < Faces.Count; i++)
            {
                Vector3 rotPointA = SharpHelpers.RotateDeg3D((Vector3)vertices[Faces[i].X] / SharpNode.nodeScale, scaledRotOffset);
                Vector3 pointA = SharpHelpers.Transform3D(rotPointA + scaledPosOffset, position, rotation);
                Vector3 rotPointB = SharpHelpers.RotateDeg3D((Vector3)vertices[Faces[i].Y] / SharpNode.nodeScale, scaledRotOffset);
                Vector3 pointB = SharpHelpers.Transform3D(rotPointB + scaledPosOffset, position, rotation);
                Vector3 rotPointC = SharpHelpers.RotateDeg3D((Vector3)vertices[Faces[i].Z] / SharpNode.nodeScale, scaledRotOffset);
                Vector3 pointC = SharpHelpers.Transform3D(rotPointC + scaledPosOffset, position, rotation);

                DebugDraw3D.DrawLine(pointA, pointB, finalColor);
                DebugDraw3D.DrawLine(pointB, pointC, finalColor);
                DebugDraw3D.DrawLine(pointC, pointA, finalColor);
            }
        }

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdatePolygonBoundingBox();
        }

        public override void UpdatePoints(FixVector3 position, FixVector3 rotation)
        {
            UpdatePolygonPoints(position, rotation);
            base.UpdatePoints(position, rotation);
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

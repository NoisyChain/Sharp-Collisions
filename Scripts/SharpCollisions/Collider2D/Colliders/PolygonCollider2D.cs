using Godot;
using Godot.Collections;
using FixMath.NET;
using SharpCollisions.Sharp2D.GJK;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class PolygonCollider2D : SharpCollider2D
    {
        [Export] protected bool DrawDebugPolytope;
        
        public GJK2D GJK;
        public FixVector2[] RawPoints;
		public FixVector2[] Points;

        [Export] private Array<Vector2I> vertices;

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
            //If there is no enough vertices to create a 2D shape,
            //create a simple triangle as the default shape
            if (vertices.Count < 3)
            {
                vertices = new Array<Vector2I>
                {
                    new Vector2I(-1, -1) * SharpNode.nodeScale,
                    new Vector2I(0, 1) * SharpNode.nodeScale,
                    new Vector2I(1, -1) * SharpNode.nodeScale
                };
            }
            RawPoints = new FixVector2[vertices.Count];
            for (int i = 0; i < RawPoints.Length; i++)
            {
                RawPoints[i] = new FixVector2(
                    (Fix64)vertices[i].X / SharpNode.NodeScale,
                    (Fix64)vertices[i].Y / SharpNode.NodeScale
                );
            }
            
            Points = new FixVector2[RawPoints.Length];
        }

        private void UpdatePolygonPoints(FixVector2 position, Fix64 rotation)
        {
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

        public override void DebugDrawShapesEditor(Node3D reference)
        {
            if (!DrawDebug) return;
            if (vertices == null || vertices.Count <= 0) return;

            Vector2 scaledPosOffset = (Vector2)positionOffset / SharpNode.nodeScale;
            float scaledRotOffset = (float)rotationOffset / SharpNode.nodeRotation;

            Vector3 position = reference.GlobalPosition;
            float rotation = reference.GlobalRotation.Z;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 start = (Vector2)vertices[i];
                Vector2 end =  (Vector2)vertices[(i + 1) % vertices.Count];

                Vector2 rotPointA = SharpHelpers.Rotate2D(start / SharpNode.nodeScale, Mathf.DegToRad(scaledRotOffset));
                Vector3 pointA = SharpHelpers.Transform2D3D(rotPointA + scaledPosOffset, position, rotation);
                Vector2 rotPointB = SharpHelpers.Rotate2D(end / SharpNode.nodeScale, Mathf.DegToRad(scaledRotOffset));
                Vector3 pointB = SharpHelpers.Transform2D3D(rotPointB + scaledPosOffset, position, rotation);
                
                DebugDraw3D.DrawLine(pointA, pointB, debugColor);
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

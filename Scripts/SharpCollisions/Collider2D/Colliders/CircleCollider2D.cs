using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class CircleCollider2D : SharpCollider2D
    {
        public Fix64 Radius;
        [Export] protected int radius;
        
        public override void Initialize()
        {
            base.Initialize();
            Radius = (Fix64)radius / SharpNode.NodeScale;
            Shape = CollisionType2D.Circle;
        }

        public override bool CollisionDetection(SharpCollider2D other, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

            if (other.Shape == CollisionType2D.AABB) return false;

			if (other.Shape == CollisionType2D.Circle)
                return CircleToCircleCollision(this, other as CircleCollider2D, out Normal, out Depth, out ContactPoint);
            else if (other.Shape == CollisionType2D.Capsule)
				return CircleToCapsuleCollision(this, other as CapsuleCollider2D, out Normal, out Depth, out ContactPoint);
			else if (other.Shape == CollisionType2D.Polygon)
            {
                PolygonCollider2D pol = other as PolygonCollider2D;
                return pol.GJK.PolygonCollision(this, other, out Normal, out Depth, out ContactPoint);
            }
            return false;
		}

        public override void DebugDrawShapes(SharpBody2D reference)
        {
			if (!Active) return;
            if (!DrawDebug) return;

            Vector3 DirX = (Vector3)reference.Right;
            Vector3 DirY = (Vector3)reference.Up;

            DebugDraw3D.DrawSimpleSphere((Vector3)Center, DirX, DirY, Vector3.Zero, (float)Radius + 0.005f, debugColor);
        }

		public override void DebugDrawShapesEditor(Node3D reference, bool selected)
		{
			if (!Active) return;
			if (!selected && !DrawDebug) return;

			Color finalColor = selected ? selectedColor : debugColor;

			Vector3 DirX = reference.Basis.X;
			Vector3 DirY = reference.Basis.Y;
			Vector3 DirZ = reference.Basis.Z;
			Vector3 pos = new Vector3(positionOffset.X, positionOffset.Y, 0) / SharpNode.nodeScale;
			Vector3 newPos = SharpHelpers.Transform3D(pos, reference.GlobalPosition, reference.GlobalRotation);

			DebugDraw3D.DrawSimpleSphere(newPos, DirX, DirY, DirZ,
											((float)radius / SharpNode.nodeScale) + 0.005f, finalColor);
											
			if (selected) DebugDraw3D.DrawGizmo(reference.Transform, finalColor, true);
		}

        protected override FixRect GetBoundingBoxPoints()
		{
			return UpdateCircleBoundingBox();
		}

        public override void UpdatePoints(FixVector2 position, Fix64 rotation)
        {
            base.UpdatePoints(position, rotation);
        }

		public override FixVector2 Support(FixVector2 direction)
		{
			FixVector2 NormalizedDirection = FixVector2.Normalize(direction);
			return Center + Radius * NormalizedDirection;
		}

        public FixRect UpdateCircleBoundingBox()
        {
            Fix64 minX = Center.x - Radius;
            Fix64 minY = Center.y - Radius;
            Fix64 maxX = Center.x + Radius;
            Fix64 maxY = Center.y + Radius;

            return new FixRect(minX, minY, maxX, maxY);
        }

        public bool CircleToCircleCollision(CircleCollider2D colliderA, CircleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			Fix64 distance = FixVector2.Distance(colliderA.Center, colliderB.Center);
			Fix64 radii = colliderA.Radius + colliderB.Radius;
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector2.Normalize(colliderB.Center - colliderA.Center);
				Depth = Normal * (radii - distance);
				ContactPoint = CircleContactPoint(colliderA.Center, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
			}
			
			return collision;
		}

        public bool CircleToCapsuleCollision(CircleCollider2D colliderA, CapsuleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			LineToPointDistance(colliderB.UpperPoint, colliderB.LowerPoint, colliderA.Center, out FixVector2 CapsulePoint);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector2.Distance(CapsulePoint, colliderA.Center);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector2.Normalize(CapsulePoint - colliderA.Center);
				Depth = Normal * (radii - distance);
				ContactPoint = CircleContactPoint(CapsulePoint, colliderB.Radius, colliderA.Center, colliderA.Radius, Normal);
			}
			
			return collision;
		}

		public FixVector2 CircleContactPoint(FixVector2 centerA, Fix64 radiusA, FixVector2 centerB, Fix64 radiusB, FixVector2 direction)
		{
			FixVector2 ContactA = centerA + (direction * radiusA);
            FixVector2 ContactB = centerB - (direction * radiusB);
			return (ContactA + ContactB) / Fix64.Two;
		}
    }
}

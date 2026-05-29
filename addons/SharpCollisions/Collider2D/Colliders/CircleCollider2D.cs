using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class CircleCollider2D : SharpCollider2D
    {
		[Export] private float _radius
        {
            get =>(float)Fix64.FromRaw(raw_Radius);
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_Radius = ((Fix64)((decimal)value)).RawValue;
                    Radius = Fix64.FromRaw(raw_Radius);
                }
            }
        }

		[ExportSubgroup("Raw Values")]
        [Export] private long raw_radius
        {
            get => raw_Radius;
            set
            {
                raw_Radius = value;
                Radius = Fix64.FromRaw(raw_Radius);
            }
        }

        private long raw_Radius;

        public Fix64 Radius;
        
        public override void Initialize()
        {
            base.Initialize();
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
                ConvexShapeCollider2D pol = other as ConvexShapeCollider2D;
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
			Vector3 pos = new Vector3(_positionOffset.X, _positionOffset.Y, 0) ;
			Vector3 newPos = SharpHelpers.Transform3D(pos, reference.GlobalPosition, reference.GlobalRotation);

			DebugDraw3D.DrawSimpleSphere(newPos, DirX, DirY, DirZ, _radius + 0.005f, finalColor);
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

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 radiiSq = radii * radii;
			Fix64 distance = FixVector2.DistanceSq(colliderA.Center, colliderB.Center);
			
			bool collision = distance <= radiiSq;
			
			if (collision)
			{
				Normal = FixVector2.Normalize(colliderB.Center - colliderA.Center);
				Depth = Normal * (radii - Fix64.Sqrt(distance));
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
			Fix64 radiiSq = radii * radii;
			Fix64 distance = FixVector2.DistanceSq(CapsulePoint, colliderA.Center);
			
			bool collision = distance <= radiiSq;
			
			if (collision)
			{
				Normal = FixVector2.Normalize(CapsulePoint - colliderA.Center);
				Depth = Normal * (radii - Fix64.Sqrt(distance));
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

using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class SphereCollider3D : SharpCollider3D
    {
        public Fix64 Radius;
        [Export] protected int radius;
        
        public override void Initialize()
        {
            base.Initialize();
            Radius = (Fix64)radius / SharpNode.convertedScale;
            Shape = CollisionType3D.Sphere;
        }

        public override bool CollisionDetection(SharpCollider3D other, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

            if (other.Shape == CollisionType3D.AABB) return false;

			if (other.Shape == CollisionType3D.Sphere)
                return SphereToSphereCollision(this, other as SphereCollider3D, out Normal, out Depth, out ContactPoint);
            else if (other.Shape == CollisionType3D.Capsule)
				return SphereToCapsuleCollision(this, other as CapsuleCollider3D, out Normal, out Depth, out ContactPoint);
			else if (other.Shape == CollisionType3D.Polygon)
            {
                PolygonCollider3D pol = other as PolygonCollider3D;
                return pol.GJK.PolygonCollision(this, other, out Normal, out Depth, out ContactPoint);
            }
            return false;
		}

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!DrawDebug) return;

            Vector3 DirX = (Vector3)reference.Right;
            Vector3 DirY = (Vector3)reference.Up;
            Vector3 DirZ = (Vector3)reference.Forward;

            DebugDraw3D.DrawSimpleSphere((Vector3)Center, DirX, DirY, DirZ, (float)Radius + 0.005f, debugColor);
        }

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdateSphereBoundingBox();
        }

        public override void UpdatePoints(SharpBody3D body)
        {
            base.UpdatePoints(body);
        }

		public override FixVector3 Support(FixVector3 direction)
		{
			FixVector3 NormalizedDirection = FixVector3.Normalize(direction);
			return Center + Radius * NormalizedDirection;
		}

        public FixVolume UpdateSphereBoundingBox()
        {
            Fix64 minX = Center.x - Radius;
            Fix64 minY = Center.y - Radius;
            Fix64 minZ = Center.z - Radius;
            Fix64 maxX = Center.x + Radius;
            Fix64 maxY = Center.y + Radius;
            Fix64 maxZ = Center.z + Radius;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }

        public bool SphereToSphereCollision(SphereCollider3D colliderA, SphereCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			Fix64 distance = FixVector3.Distance(colliderA.Center, colliderB.Center);
			Fix64 radii = colliderA.Radius + colliderB.Radius;
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector3.Normalize(colliderB.Center - colliderA.Center);
				Depth = Normal * Fix64.Abs(radii - distance);
				ContactPoint = SphereContactPoint(colliderA.Center, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
			}
			
			return collision;
		}

        //Just doing this because I don't know how to invert the normal in the previous function yet lol
		public bool SphereToCapsuleCollision(SphereCollider3D colliderA, CapsuleCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			LineToPointDistance(colliderB.UpperPoint, colliderB.LowerPoint, colliderA.Center, out FixVector3 CapsulePoint);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector3.Distance(CapsulePoint, colliderA.Center);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector3.Normalize(CapsulePoint - colliderA.Center);
				Depth = Normal * Fix64.Abs(radii - distance);
				ContactPoint = SphereContactPoint(CapsulePoint, colliderB.Radius, colliderA.Center, colliderA.Radius, Normal);
			}
			
			return collision;
		}

		public FixVector3 SphereContactPoint(FixVector3 centerA, Fix64 radiusA, FixVector3 centerB, Fix64 radiusB, FixVector3 direction)
		{
			FixVector3 ContactA = centerA + (direction * radiusA);
            FixVector3 ContactB = centerB - (direction * radiusB);
			return (ContactA + ContactB) / Fix64.Two;
		}
    }
}

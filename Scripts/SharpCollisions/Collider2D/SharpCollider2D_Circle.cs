using FixMath.NET;

namespace SharpCollisions
{
	public partial class SharpCollider2D
	{
		public Fix64 Radius;

		public FixVector2 GetCircleContactPoint(SharpCollider2D A, SharpCollider2D B)
		{
			FixVector2 Direction = B.Center - A.Center;
			FixVector2 ContactA = A.CircleSupport(Direction);
			FixVector2 ContactB = B.CircleSupport(-Direction);

			return (ContactA + ContactB) / Fix64.Two;
			
			/*//The mdeiant point between the two flat positions
			FixVector2 mediant = (A.Center + B.Center) / half;
			//The length of the contact surface
			FixVector2 contactLengthA = FixVector2.Normalize(B.Center - A.Center) * A.Radius;
			FixVector2 contactLengthB = FixVector2.Normalize(A.Center - B.Center) * B.Radius;

			//The actual contact surface 
			FixVector2 contactSurfaceA = A.Center + contactLengthA;
			FixVector2 contactSurfaceB = B.Center + contactLengthB;
			//The inverse contact surface
			FixVector2 inverseContactSurfaceA = A.Center - contactLengthA;
			FixVector2 inverseContactSurfaceB = B.Center - contactLengthB;

			//Selecting the references checking which ones are inside the bigger collider (like the Y axis but way more convoluted)
			FixVector2 nearestReferencePoint = FixVector2.Distance(contactSurfaceA, mediant) < FixVector2.Distance(inverseContactSurfaceB, mediant) ? 
									contactSurfaceA : inverseContactSurfaceB;
			FixVector2 nearestInverseReferencePoint = FixVector2.Distance(contactSurfaceB, mediant) < FixVector2.Distance(inverseContactSurfaceA, mediant) ? 
									contactSurfaceB : inverseContactSurfaceA;
			//Building the Contact Location
			return (nearestReferencePoint + nearestInverseReferencePoint) / half;*/
		}
		
		public bool CircleToCircleCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			Fix64 distance = FixVector2.Distance(colliderA.Center, colliderB.Center);
			Fix64 radii = colliderA.Radius + colliderB.Radius;
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				ContactPoint = GetCircleContactPoint(colliderA, colliderB);
				Normal = colliderB.Center - colliderA.Center;
				Depth = Normal * (radii - distance);
			}
			
			return collision;
		}

		public FixVector2 CircleSupport(FixVector2 direction)
		{
			FixVector2 NormalizedDirection = FixVector2.Normalize(direction);
			return Center + Radius * NormalizedDirection;
		}
	}
}

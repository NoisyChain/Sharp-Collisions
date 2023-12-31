using Godot;
using FixMath.NET;

namespace SharpCollisions
{
	public partial class SharpCollider2D : Node
	{
		public Fix64 Radius;

		public FixVector2 GetCircleContactPoint(SharpCollider2D A, SharpCollider2D B)
		{
			FixVector2 Direction = B.Center - A.Center;
			FixVector2 ContactA = A.CircleSupport(Direction);
			FixVector2 ContactB = B.CircleSupport(-Direction);

			return (ContactA + ContactB) / Fix64.Two;
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

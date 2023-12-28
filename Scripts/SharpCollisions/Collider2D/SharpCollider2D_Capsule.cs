using FixMath.NET;

namespace SharpCollisions
{
	public partial class SharpCollider2D
	{
		public Fix64 Height;
		public FixVector2 RawUpperPoint;
		public FixVector2 RawLowerPoint;
		public FixVector2 UpperPoint;
		public FixVector2 LowerPoint;
		public FixVector2 GetCapsuleContactPoint(SharpCollider2D A, SharpCollider2D B)
		{
			FixVector2 Direction = B.Center - A.Center;
			FixVector2 ContactA = A.CapsuleSupport(Direction);
			FixVector2 ContactB = B.CapsuleSupport(-Direction);

			return (ContactA + ContactB) / Fix64.Two;
		}

		public FixVector2 GetCapsuleCircleContactPoint(SharpCollider2D A, SharpCollider2D B)
		{
			FixVector2 Direction = B.Center - A.Center;
			FixVector2 ContactA = A.CapsuleSupport(Direction);
			FixVector2 ContactB = B.CircleSupport(-Direction);

			return (ContactA + ContactB) / Fix64.Two;
		}
		
		public bool CapsuleToCapsuleCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			LineToLineDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.UpperPoint, colliderB.LowerPoint, out FixVector2 r1, out FixVector2 r2);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector2.Distance(r1, r2);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				ContactPoint = GetCapsuleContactPoint(colliderA, colliderB);
				Normal = r2 - r1;
				Depth = Normal * (radii - distance);
			}
			
			return collision;
		}

		public bool CapsuleToSphereCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			LineToPointDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.Center, out FixVector2 CapsulePoint);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector2.Distance(CapsulePoint, colliderB.Center);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				ContactPoint = GetCapsuleCircleContactPoint(colliderA, colliderB);
				//The normal value returning from this operation is too short for some reason
				//Doubling it solves the issue for now
				Normal = (colliderB.Center - CapsulePoint) * Fix64.Two;
				Depth = Normal * (radii - distance);
			}
			
			return collision;
		}

		public FixVector2 CapsuleSupport(FixVector2 direction)
		{
			FixVector2 NormalizedDirection = FixVector2.Normalize(direction);
			Fix64 Dy = FixVector2.Dot(NormalizedDirection, UpperPoint - LowerPoint);

			if (Dy == Fix64.Zero) return Center + Radius * NormalizedDirection;
			else return (Dy < Fix64.Zero ? LowerPoint : UpperPoint) + Radius * NormalizedDirection;
		}

		public void LineToLineDistance(FixVector2 p1, FixVector2 p2, FixVector2 p3, FixVector2 p4, out FixVector2 r1, out FixVector2 r2)
		{
			var r = p3 - p1;
			var u = p2 - p1;
			var v = p4 - p3;
			var ru = FixVector2.Dot(r, u);
			var rv = FixVector2.Dot(r, v);
			var uu = FixVector2.Dot(u, u);
			var uv = FixVector2.Dot(u, v);
			var vv = FixVector2.Dot(v, v);
			var det = uu * vv - uv * uv;

			Fix64 s, t;
			if (det < Fix64.ETA * uu * vv)
			{
				s = Fix64.Clamp01(ru / uu);
				t = Fix64.Zero;
			} 
			else
			{
				s = Fix64.Clamp01((ru * vv - rv * uv) / det);
				t = Fix64.Clamp01((ru * uv - rv * uu) / det);
			}

			var S = Fix64.Clamp01((t * uv + ru) / uu);
			var T = Fix64.Clamp01((s * uv - rv) / vv);

			r1 = p1 + S * u;
			r2 = p3 + T * v;
		}

		//Line to point collision code taken from Noah Zuo's Blog
		//https://arrowinmyknee.com/2021/03/15/some-math-about-capsule-collision/
		public void LineToPointDistance(FixVector2 p1, FixVector2 p2, FixVector2 p3, out FixVector2 r1)
		{
			FixVector2 ab = p2 - p1;
			Fix64 length = FixVector2.Dot(p3 - p1, ab);
			if (length <= Fix64.ETA) 
			{
				r1 = p1;
			} 
			else 
			{
				Fix64 denom = FixVector2.Dot(ab, ab);
				if (length >= denom) 
				{
					r1 = p2;
				} 
				else 
				{
					length = length / denom;
					r1 = p1 + length * ab;
				}
			}
		}
	}
}

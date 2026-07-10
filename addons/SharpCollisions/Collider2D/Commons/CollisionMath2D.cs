using FixMath.NET;
using Godot;
using SharpCollisions.Sharp2D.GJK;

namespace SharpCollisions.Sharp2D
{
	public static class CollisionMath2D
	{
		public static bool IsOverlapping(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			if (colliderA.Shape == CollisionType2D.Null || colliderB.Shape == CollisionType2D.Null)
			{
				GD.PrintErr("Invalid collider shape");
				return false;
			}

			if (colliderA.Shape == CollisionType2D.AABB && colliderB.Shape == CollisionType2D.AABB)
			{
				return AABBtoAABBCollision(colliderA as AABBCollider2D, colliderB as AABBCollider2D, out Normal, out Depth, out ContactPoint);
			}
			else
			{
				if (colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Circle)
					return CircleToCircleCollision(colliderA as CircleCollider2D, colliderB as CircleCollider2D, out Normal, out Depth, out ContactPoint);
				else if (colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Capsule)
					return CircleToCapsuleCollision(colliderA as CircleCollider2D, colliderB as CapsuleCollider2D, out Normal, out Depth, out ContactPoint);
				else if (colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Circle)
					return CapsuleToCircleCollision(colliderA as CapsuleCollider2D, colliderB as CircleCollider2D, out Normal, out Depth, out ContactPoint);
				else if (colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Capsule)
					return CapsuleToCapsuleCollision(colliderA as CapsuleCollider2D, colliderB as CapsuleCollider2D, out Normal, out Depth, out ContactPoint);
				else
					return GJKCollision(colliderA, colliderB, out Normal, out Depth, out ContactPoint);
			}
		}

		public static void GetCollisionFlags(SharpCollider2D collider, FixVector2 normal, SharpBody2D body)
		{
			if (FixVector2.Dot(normal, body.Up) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Below;
			if (FixVector2.Dot(normal, body.Down) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Above;
			if (FixVector2.Dot(normal, body.Left) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Right;
			if (FixVector2.Dot(normal, body.Right) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Left;
		}

		public static void GetGlobalCollisionFlags(SharpCollider2D collider, FixVector2 normal)
		{
			if (FixVector2.Dot(normal, FixVector2.Up) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Below;
			if (FixVector2.Dot(normal, FixVector2.Down) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Above;
			if (FixVector2.Dot(normal, FixVector2.Left) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Right;
			if (FixVector2.Dot(normal, FixVector2.Right) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Left;
		}

		public static FixRect UpdateAABBBoundingBox(FixVector2 center, FixVector2 extents)
        {
            Fix64 minX = center.x - extents.x;
            Fix64 minY = center.y - extents.y;
            Fix64 maxX = center.x + extents.x;
            Fix64 maxY = center.y + extents.y;

            return new FixRect(minX, minY, maxX, maxY);
        }

		public static FixRect UpdateCircleBoundingBox(FixVector2 center, Fix64 radius)
        {
            Fix64 minX = center.x - radius;
            Fix64 minY = center.y - radius;
            Fix64 maxX = center.x + radius;
            Fix64 maxY = center.y + radius;

            return new FixRect(minX, minY, maxX, maxY);
        }

		public static FixRect UpdateCapsuleBoundingBox(FixVector2 upperPoint, FixVector2 lowerPoint, Fix64 radius)
        {
            Fix64 minX = upperPoint.x - radius;
            Fix64 minY = upperPoint.y - radius;
            Fix64 maxX = upperPoint.x + radius;
            Fix64 maxY = upperPoint.y + radius;

            if (lowerPoint.x < upperPoint.x)
                minX = lowerPoint.x - radius;
            if (lowerPoint.x > upperPoint.x)
                maxX = lowerPoint.x + radius;
            if (lowerPoint.y < upperPoint.y)
                minY = lowerPoint.y - radius;
            if (lowerPoint.y > upperPoint.y)
                maxY = lowerPoint.y + radius;

            return new FixRect(minX, minY, maxX, maxY);
        }

		public static FixRect UpdatePolygonBoundingBox(FixVector2[] points)
        {
            Fix64 minX = Fix64.MaxValue;
			Fix64 minY = Fix64.MaxValue;
			Fix64 maxX = Fix64.MinValue;
			Fix64 maxY = Fix64.MinValue;

            for (int p = 0; p < points.Length; p++)
            {
                FixVector2 v = points[p];

                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
            }

            return new FixRect(minX, minY, maxX, maxY);
        }

        public static FixVector2 FindAABBNormals(AABBCollider2D colliderA, AABBCollider2D colliderB)
        {
            FixVector2 finalNormal;
            FixVector2 length = colliderB.Center - colliderA.Center;

            Fix64 ExtentsX = colliderB.Extents.x + colliderA.Extents.x;
            Fix64 ExtentsY = colliderB.Extents.y + colliderA.Extents.y;

            // calculate normal of collided surface
            if (Fix64.Abs(length.x) + ExtentsY > Fix64.Abs(length.y) + ExtentsX)
            {
                if (colliderA.Center.x < colliderB.Center.x)
                {
                    finalNormal = FixVector2.Right;
                } 
                else
                {
                    finalNormal = FixVector2.Left;
                }
            }
            else
            {
                if (colliderA.Center.y < colliderB.Center.y)
                {
                    finalNormal = FixVector2.Up;
                }
                else
                {
                    finalNormal = FixVector2.Down;
                }
            }
            return finalNormal;
        }

		public static bool AABBtoAABBCollision(AABBCollider2D colliderA, AABBCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
        {
            Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

            bool collisionX = colliderA.Center.x - colliderA.Extents.x <= colliderB.Center.x + colliderB.Extents.x &&
                colliderA.Center.x + colliderA.Extents.x >= colliderB.Center.x - colliderB.Extents.x;

            bool collisionY = colliderA.Center.y - colliderA.Extents.y <= colliderB.Center.y + colliderB.Extents.y &&
                colliderA.Center.y + colliderA.Extents.y >= colliderB.Center.y - colliderB.Extents.y;

            if (collisionX && collisionY)
            {
                ContactPoint = AABBContactPoint(colliderA, colliderB);

                FixVector2 length = colliderB.Center - colliderA.Center;

                FixVector2 newDepth = FixVector2.Zero;
                newDepth.x = colliderA.Extents.x + colliderB.Extents.x;
                newDepth.y = colliderA.Extents.y + colliderB.Extents.y;
                newDepth.x -= Fix64.Abs(length.x);
                newDepth.y -= Fix64.Abs(length.y);
                Normal = FindAABBNormals(colliderA, colliderB);
                Depth = Normal * newDepth;
            }

            return collisionX && collisionY;
        }

		public static bool CircleToCircleCollision(CircleCollider2D colliderA, CircleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
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

        public static bool CircleToCapsuleCollision(CircleCollider2D colliderA, CapsuleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
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

		public static bool CapsuleToCircleCollision(CapsuleCollider2D colliderA, CircleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
        {
            Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

            LineToPointDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.Center, out FixVector2 CapsulePoint);

            Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 radiiSq = radii * radii;
            Fix64 distance = FixVector2.DistanceSq(CapsulePoint, colliderB.Center);

            bool collision = distance <= radiiSq;

            if (collision)
            {
                Normal = FixVector2.Normalize(colliderB.Center - CapsulePoint);
                Depth = Normal * (radii - Fix64.Sqrt(distance));
                ContactPoint = CircleContactPoint(CapsulePoint, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
            }

            return collision;
        }

        public static bool CapsuleToCapsuleCollision(CapsuleCollider2D colliderA, CapsuleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
        {
            Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

            FixVector2 r1 = FixVector2.Zero;
            FixVector2 r2 = FixVector2.Zero;

            bool colA_Sphere = colliderA.Radius >= colliderA.Height;
            bool colB_Sphere = colliderB.Radius >= colliderB.Height;

            if (colA_Sphere && colB_Sphere)
            {
                r1 = (colliderA.UpperPoint + colliderA.LowerPoint) / Fix64.Two;
                r2 = (colliderA.UpperPoint + colliderA.LowerPoint) / Fix64.Two;
            }
            else if (!colA_Sphere && colB_Sphere)
            {
                r2 = (colliderB.UpperPoint + colliderB.LowerPoint) / Fix64.Two;
                LineToPointDistance(colliderA.UpperPoint, colliderA.LowerPoint, r2, out r1);
            }
            else if (colA_Sphere && !colB_Sphere)
            {
                r1 = (colliderA.UpperPoint + colliderA.LowerPoint) / Fix64.Two;
                LineToPointDistance(colliderB.UpperPoint, colliderB.LowerPoint, r1, out r2);
            }
            else 
                LineToLineDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.UpperPoint, colliderB.LowerPoint, out r1, out r2);

            Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 radiiSq = radii * radii;
            Fix64 distance = FixVector2.DistanceSq(r1, r2);

            bool collision = distance <= radiiSq;

            if (collision)
            {
                Normal = FixVector2.Normalize(r2 - r1);
                Depth = Normal * (radii - Fix64.Sqrt(distance));
                if (!colA_Sphere && !colB_Sphere)
                {
                    ContactPoint = CapsuleContactPoint
                    (
                        colliderA.UpperPoint, colliderA.LowerPoint,
                        colliderB.UpperPoint, colliderB.LowerPoint,
                        colliderA.Radius, colliderB.Radius, Normal
                    ); 
                }
                else
                {
                    ContactPoint = CircleContactPoint(r1, colliderA.Radius, r2, colliderB.Radius, Normal);
                }
            }

            return collision;
        }

		public static bool GJKCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			GJK2D GJK = new GJK2D();
			Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

			return GJK.PolygonCollision(colliderA, colliderB, out Normal, out Depth, out ContactPoint);
		}

        public static FixVector2 AABBContactPoint(AABBCollider2D A, AABBCollider2D B)
        {
            Fix64 minPointX = Fix64.Min(A.Center.x + A.Extents.x, B.Center.x + B.Extents.x);
            Fix64 maxPointX = Fix64.Max(A.Center.x - A.Extents.x, B.Center.x - B.Extents.x);
            Fix64 minPointY = Fix64.Min(A.Center.y + A.Extents.y, B.Center.y + B.Extents.y);
            Fix64 maxPointY = Fix64.Max(A.Center.y - A.Extents.y, B.Center.y - B.Extents.y);
            Fix64 mediantX = (minPointX + maxPointX) / Fix64.Two;
            Fix64 mediantY = (minPointY + maxPointY) / Fix64.Two;
            return new FixVector2(mediantX, mediantY);
        }

		public static FixVector2 CircleContactPoint(FixVector2 centerA, Fix64 radiusA, FixVector2 centerB, Fix64 radiusB, FixVector2 direction)
		{
			FixVector2 ContactA = centerA + (direction * radiusA);
            FixVector2 ContactB = centerB - (direction * radiusB);
			return (ContactA + ContactB) / Fix64.Two;
		}

		public static FixVector2 CapsuleContactPoint(FixVector2 upperA, FixVector2 lowerA, FixVector2 upperB, FixVector2 lowerB, Fix64 radiusA, Fix64 radiusB, FixVector2 direction)
        {
            LineToPointDistance(upperB, lowerB, upperA, out FixVector2 r1);
            LineToPointDistance(upperB, lowerB, lowerA, out FixVector2 r3);
            LineToPointDistance(upperA, lowerA, upperB, out FixVector2 r2);
            LineToPointDistance(upperA, lowerA, lowerB, out FixVector2 r4);

            FixVector2 p1 = r1 - (direction * radiusB);
            FixVector2 p2 = r2 + (direction * radiusA);
            FixVector2 p3 = r3 - (direction * radiusB);
            FixVector2 p4 = r4 + (direction * radiusA);

            FixVector2 contact1 = (p1 + p2) / Fix64.Two;
            FixVector2 contact2 = (p3 + p4) / Fix64.Two;
            return (contact1 + contact2) / Fix64.Two;
        }

		public static void LineToLineDistance(FixVector2 p1, FixVector2 p2, FixVector2 p3, FixVector2 p4, out FixVector2 r1, out FixVector2 r2)
		{
			FixVector2 r = p3 - p1;
			FixVector2 u = p2 - p1;
			FixVector2 v = p4 - p3;
			Fix64 ru = FixVector2.Dot(r, u);
			Fix64 rv = FixVector2.Dot(r, v);
			Fix64 uu = FixVector2.Dot(u, u);
			Fix64 uv = FixVector2.Dot(u, v);
			Fix64 vv = FixVector2.Dot(v, v);
			Fix64 det = uu * vv - uv * uv;

			Fix64 s, t;
			if (det < Fix64.Epsilon * uu * vv)
			{
				s = Fix64.Clamp01(ru / uu);
				t = Fix64.Zero;
			} 
			else
			{
				s = Fix64.Clamp01((ru * vv - rv * uv) / det);
				t = Fix64.Clamp01((ru * uv - rv * uu) / det);
			}

			Fix64 S = Fix64.Clamp01((t * uv + ru) / uu);
			Fix64 T = Fix64.Clamp01((s * uv - rv) / vv);

			r1 = p1 + S * u;
			r2 = p3 + T * v;
		}

		//Line to point collision code taken from Noah Zuo's Blog
		//https://arrowinmyknee.com/2021/03/15/some-math-about-capsule-collision/
		public static void LineToPointDistance(FixVector2 p1, FixVector2 p2, FixVector2 p3, out FixVector2 r1)
		{
			FixVector2 ab = p2 - p1;
			Fix64 length = FixVector2.Dot(p3 - p1, ab);
			if (length <= Fix64.Epsilon) 
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


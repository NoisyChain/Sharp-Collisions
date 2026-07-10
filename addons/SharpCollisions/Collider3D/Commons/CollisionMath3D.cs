using FixMath.NET;
using Godot;
using SharpCollisions.Sharp3D.GJK;

namespace SharpCollisions.Sharp3D
{
	public static class CollisionMath3D
	{
		public static bool IsOverlapping(SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			if (colliderA.Shape == CollisionType3D.Null || colliderB.Shape == CollisionType3D.Null)
			{
				GD.PrintErr("Invalid collider shape");
				return false;
			}

			if (colliderA.Shape == CollisionType3D.AABB && colliderB.Shape == CollisionType3D.AABB)
			{
				return AABBtoAABBCollision(colliderA as AABBCollider3D, colliderB as AABBCollider3D, out Normal, out Depth, out ContactPoint);
			}
			else
			{
                if ((colliderA.Shape == CollisionType3D.AABB && colliderB.Shape != CollisionType3D.AABB)||
                    (colliderA.Shape != CollisionType3D.AABB && colliderB.Shape == CollisionType3D.AABB))
                    return false;
				else if (colliderA.Shape == CollisionType3D.Sphere && colliderB.Shape == CollisionType3D.Sphere)
					return SphereToSphereCollision(colliderA as SphereCollider3D, colliderB as SphereCollider3D, out Normal, out Depth, out ContactPoint);
				else if (colliderA.Shape == CollisionType3D.Sphere && colliderB.Shape == CollisionType3D.Capsule)
					return SphereToCapsuleCollision(colliderA as SphereCollider3D, colliderB as CapsuleCollider3D, out Normal, out Depth, out ContactPoint);
				else if (colliderA.Shape == CollisionType3D.Capsule && colliderB.Shape == CollisionType3D.Sphere)
					return CapsuleToSphereCollision(colliderA as CapsuleCollider3D, colliderB as SphereCollider3D, out Normal, out Depth, out ContactPoint);
				else if (colliderA.Shape == CollisionType3D.Capsule && colliderB.Shape == CollisionType3D.Capsule)
					return CapsuleToCapsuleCollision(colliderA as CapsuleCollider3D, colliderB as CapsuleCollider3D, out Normal, out Depth, out ContactPoint);
				else
					return GJKCollision(colliderA, colliderB, out Normal, out Depth, out ContactPoint);
			}
		}

		public static void GetCollisionFlags(SharpCollider3D collider, FixVector3 normal, SharpBody3D body)
		{
			if (FixVector3.Dot(normal, body.Up) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Below;
			if (FixVector3.Dot(normal, body.Down) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Above;
			if (FixVector3.Dot(normal, body.Left) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Right;
			if (FixVector3.Dot(normal, body.Right) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Left;
			if (FixVector3.Dot(normal, body.Back) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Forward;
			if (FixVector3.Dot(normal, body.Forward) > Fix64.Epsilon)
				collider.collisionFlags |= CollisionFlags.Back;
		}

		public static void GetGlobalCollisionFlags(SharpCollider3D collider, FixVector3 normal)
		{
			if (FixVector3.Dot(normal, FixVector3.Up) > Fix64.Epsilon)
				collider.globalCollisionFlags |= CollisionFlags.Below;
			if (FixVector3.Dot(normal, FixVector3.Down) > Fix64.Epsilon)
				collider.globalCollisionFlags |= CollisionFlags.Above;
			if (FixVector3.Dot(normal, FixVector3.Left) > Fix64.Epsilon)
				collider.globalCollisionFlags |= CollisionFlags.Right;
			if (FixVector3.Dot(normal, FixVector3.Right) > Fix64.Epsilon)
				collider.globalCollisionFlags |= CollisionFlags.Left;
			if (FixVector3.Dot(normal, FixVector3.Back) > Fix64.Epsilon)
				collider.globalCollisionFlags |= CollisionFlags.Forward;
			if (FixVector3.Dot(normal, FixVector3.Forward) > Fix64.Epsilon)
				collider.globalCollisionFlags |= CollisionFlags.Back;
		}

		public static FixVolume UpdateAABBBoundingBox(FixVector3 center, FixVector3 extents)
        {
            Fix64 minX = center.x - extents.x;
            Fix64 minY = center.y - extents.y;
            Fix64 minZ = center.z - extents.z;
            Fix64 maxX = center.x + extents.x;
            Fix64 maxY = center.y + extents.y;
            Fix64 maxZ = center.z + extents.z;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }

		public static FixVolume UpdateSphereBoundingBox(FixVector3 center, Fix64 radius)
        {
            Fix64 minX = center.x - radius;
            Fix64 minY = center.y - radius;
            Fix64 minZ = center.z - radius;
            Fix64 maxX = center.x + radius;
            Fix64 maxY = center.y + radius;
            Fix64 maxZ = center.z + radius;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }

		public static FixVolume UpdateCapsuleBoundingBox(FixVector3 upperPoint, FixVector3 lowerPoint, Fix64 radius)
        {
            Fix64 minX = upperPoint.x - radius;
            Fix64 minY = upperPoint.y - radius;
			Fix64 minZ = upperPoint.z - radius;
            Fix64 maxX = upperPoint.x + radius;
            Fix64 maxY = upperPoint.y + radius;
			Fix64 maxZ = upperPoint.z + radius;

            if (lowerPoint.x < upperPoint.x)
                minX = lowerPoint.x - radius;
            if (lowerPoint.x > upperPoint.x)
                maxX = lowerPoint.x + radius;
            if (lowerPoint.y < upperPoint.y)
                minY = lowerPoint.y - radius;
            if (lowerPoint.y > upperPoint.y)
                maxY = lowerPoint.y + radius;
			if (lowerPoint.z < upperPoint.z)
                minZ = lowerPoint.z - radius;
            if (lowerPoint.z > upperPoint.z)
                maxZ = lowerPoint.z + radius;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }

		public static FixVolume UpdateCylinderBoundingBox(FixVector3 upperPoint, FixVector3 lowerPoint, Fix64 radius)
        {
            Fix64 minX = upperPoint.x - radius;
            Fix64 minY = upperPoint.y - radius;
			Fix64 minZ = upperPoint.z - radius;
            Fix64 maxX = upperPoint.x + radius;
            Fix64 maxY = upperPoint.y + radius;
			Fix64 maxZ = upperPoint.z + radius;

            if (lowerPoint.x < upperPoint.x)
                minX = lowerPoint.x - radius;
            if (lowerPoint.x > upperPoint.x)
                maxX = lowerPoint.x + radius;
            if (lowerPoint.y < upperPoint.y)
                minY = lowerPoint.y - radius;
            if (lowerPoint.y > upperPoint.y)
                maxY = lowerPoint.y + radius;
			if (lowerPoint.z < upperPoint.z)
                minZ = lowerPoint.z - radius;
            if (lowerPoint.z > upperPoint.z)
                maxZ = lowerPoint.z + radius;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }

		public static FixVolume UpdatePolygonBoundingBox(FixVector3[] points)
        {
            Fix64 minX = Fix64.MaxValue;
			Fix64 minY = Fix64.MaxValue;
            Fix64 minZ = Fix64.MaxValue;
			Fix64 maxX = Fix64.MinValue;
			Fix64 maxY = Fix64.MinValue;
            Fix64 maxZ = Fix64.MinValue;

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

        public static FixVector3 FindAABBNormals(AABBCollider3D colliderA, AABBCollider3D colliderB)
        {
            FixVector3 finalNormal;
            FixVector3 length = colliderB.Center - colliderA.Center;

            Fix64 ExtentsX = colliderB.Extents.x + colliderA.Extents.x;
            Fix64 ExtentsY = colliderB.Extents.y + colliderA.Extents.y;
            Fix64 ExtentsZ = colliderB.Extents.z + colliderA.Extents.z;

            // calculate normal of collided surface
			if (Fix64.Abs(length.x) + ExtentsY > Fix64.Abs(length.y) + ExtentsX || 
				Fix64.Abs(length.z) + ExtentsY > Fix64.Abs(length.y) + ExtentsZ)
			{
				if (Fix64.Abs(length.x) + ExtentsZ > Fix64.Abs(length.z) + ExtentsX)
				{
					
					if (colliderA.Center.x < colliderB.Center.x)
					{
						finalNormal = FixVector3.Right;
					} 
					else
					{
						finalNormal = FixVector3.Left;
					}
				}
				else
				{
					if (colliderA.Center.z < colliderB.Center.z)
					{
						finalNormal = FixVector3.Forward;
					} 
					else
					{
						finalNormal = FixVector3.Back;
					}
				}
			}
			else
			{
				if (colliderA.Center.y < colliderB.Center.y)
				{
					finalNormal = FixVector3.Up;
				} 
				else
				{
					finalNormal = FixVector3.Down;
				}
			}
            return finalNormal;
        }

		public static bool AABBtoAABBCollision(AABBCollider3D colliderA, AABBCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
        {
            Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

            bool collisionX = colliderA.Center.x - colliderA.Extents.x <= colliderB.Center.x + colliderB.Extents.x &&
                colliderA.Center.x + colliderA.Extents.x >= colliderB.Center.x - colliderB.Extents.x;

            bool collisionY = colliderA.Center.y - colliderA.Extents.y <= colliderB.Center.y + colliderB.Extents.y &&
                colliderA.Center.y + colliderA.Extents.y >= colliderB.Center.y - colliderB.Extents.y;
            
            bool collisionZ = colliderA.Center.z - colliderA.Extents.z <= colliderB.Center.z + colliderB.Extents.z &&
                colliderA.Center.z + colliderA.Extents.z >= colliderB.Center.z - colliderB.Extents.z;


            if (collisionX && collisionY && collisionZ)
            {
                ContactPoint = AABBContactPoint(colliderA, colliderB);

                FixVector3 length = colliderB.Center - colliderA.Center;

                FixVector3 newDepth = FixVector3.Zero;
                newDepth.x = colliderA.Extents.x + colliderB.Extents.x;
                newDepth.y = colliderA.Extents.y + colliderB.Extents.y;
                newDepth.z = colliderA.Extents.z + colliderB.Extents.z;
                newDepth.x -= Fix64.Abs(length.x);
                newDepth.y -= Fix64.Abs(length.y);
                newDepth.z -= Fix64.Abs(length.z);
                Normal = FindAABBNormals(colliderA, colliderB);
                Depth = Normal * newDepth;
            }

            return collisionX && collisionY && collisionZ;
        }

		public static bool SphereToSphereCollision(SphereCollider3D colliderA, SphereCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

            Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 radiiSq = radii * radii;
            Fix64 distance = FixVector3.DistanceSq(colliderA.Center, colliderB.Center);
			
			bool collision = distance <= radiiSq;
			
			if (collision)
			{
				Normal = FixVector3.Normalize(colliderB.Center - colliderA.Center);
				Depth = Normal * Fix64.Abs(radii - Fix64.Sqrt(distance));
				ContactPoint = SphereContactPoint(colliderA.Center, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
			}
			
			return collision;
		}

        //Just doing this because I don't know how to invert the normal in the previous function yet lol
		public static bool SphereToCapsuleCollision(SphereCollider3D colliderA, CapsuleCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			LineToPointDistance(colliderB.UpperPoint, colliderB.LowerPoint, colliderA.Center, out FixVector3 CapsulePoint);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 radiiSq = radii * radii;
			Fix64 distance = FixVector3.DistanceSq(CapsulePoint, colliderA.Center);
			
			bool collision = distance <= radiiSq;
			
			if (collision)
			{
				Normal = FixVector3.Normalize(CapsulePoint - colliderA.Center);
				Depth = Normal * Fix64.Abs(radii - Fix64.Sqrt(distance));
				ContactPoint = SphereContactPoint(CapsulePoint, colliderB.Radius, colliderA.Center, colliderA.Radius, Normal);
			}
			
			return collision;
		}

		public static bool CapsuleToSphereCollision(CapsuleCollider3D colliderA, SphereCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
        {
            Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

            LineToPointDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.Center, out FixVector3 CapsulePoint);

            Fix64 radii = colliderA.Radius + colliderB.Radius;
            Fix64 radiiSq = radii * radii;
            Fix64 distance = FixVector3.DistanceSq(CapsulePoint, colliderB.Center);

            bool collision = distance <= radiiSq;

            if (collision)
            {
                Normal = FixVector3.Normalize(colliderB.Center - CapsulePoint);
                Depth = Normal * Fix64.Abs(radii - Fix64.Sqrt(distance));
                ContactPoint = SphereContactPoint(CapsulePoint, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
            }

            return collision;
        }

		public static bool CapsuleToCapsuleCollision(CapsuleCollider3D colliderA, CapsuleCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
        {
            Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

            FixVector3 r1 = FixVector3.Zero;
            FixVector3 r2 = FixVector3.Zero;

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
            Fix64 distance = FixVector3.DistanceSq(r1, r2);

            bool collision = distance <= radiiSq;

            if (collision)
            {
                Normal = FixVector3.Normalize(r2 - r1);
                Depth = Normal * Fix64.Abs(radii - Fix64.Sqrt(distance));
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
                    ContactPoint = SphereContactPoint(r1, colliderA.Radius, r2, colliderB.Radius, Normal);
                }
            }

            return collision;
        }

		public static bool GJKCollision(SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			GJK3D GJK = new GJK3D();
			Normal = FixVector3.Zero;
            Depth = FixVector3.Zero;
            ContactPoint = FixVector3.Zero;

			return GJK.PolygonCollision(colliderA, colliderB, out Normal, out Depth, out ContactPoint);
		}

        public static FixVector3 AABBContactPoint(AABBCollider3D A, AABBCollider3D B)
        {
            Fix64 minPointX = Fix64.Min(A.Center.x + A.Extents.x, B.Center.x + B.Extents.x);
            Fix64 maxPointX = Fix64.Max(A.Center.x - A.Extents.x, B.Center.x - B.Extents.x);
            Fix64 minPointY = Fix64.Min(A.Center.y + A.Extents.y, B.Center.y + B.Extents.y);
            Fix64 maxPointY = Fix64.Max(A.Center.y - A.Extents.y, B.Center.y - B.Extents.y);
            Fix64 minPointZ = Fix64.Min(A.Center.z + A.Extents.z, B.Center.z + B.Extents.z);
            Fix64 maxPointZ = Fix64.Max(A.Center.z - A.Extents.z, B.Center.z - B.Extents.z);
            Fix64 mediantX = (minPointX + maxPointX) / Fix64.Two;
            Fix64 mediantY = (minPointY + maxPointY) / Fix64.Two;
            Fix64 mediantZ = (minPointZ + maxPointZ) / Fix64.Two;
            return new FixVector3(mediantX, mediantY, mediantZ);
        }

		public static FixVector3 SphereContactPoint(FixVector3 centerA, Fix64 radiusA, FixVector3 centerB, Fix64 radiusB, FixVector3 direction)
		{
			FixVector3 ContactA = centerA + (direction * radiusA);
            FixVector3 ContactB = centerB - (direction * radiusB);
			return (ContactA + ContactB) / Fix64.Two;
		}

		public static FixVector3 CapsuleContactPoint(FixVector3 upperA, FixVector3 lowerA, FixVector3 upperB, FixVector3 lowerB, Fix64 radiusA, Fix64 radiusB, FixVector3 direction)
        {
            LineToPointDistance(upperB, lowerB, upperA, out FixVector3 r1);
            LineToPointDistance(upperB, lowerB, lowerA, out FixVector3 r3);
            LineToPointDistance(upperA, lowerA, upperB, out FixVector3 r2);
            LineToPointDistance(upperA, lowerA, lowerB, out FixVector3 r4);

            FixVector3 p1 = r1 - (direction * radiusB);
            FixVector3 p2 = r2 + (direction * radiusA);
            FixVector3 p3 = r3 - (direction * radiusB);
            FixVector3 p4 = r4 + (direction * radiusA);

            FixVector3 contact1 = (p1 + p2) / Fix64.Two;
            FixVector3 contact2 = (p3 + p4) / Fix64.Two;
            return (contact1 + contact2) / Fix64.Two;
        }

		public static void LineToLineDistance(FixVector3 p1, FixVector3 p2, FixVector3 p3, FixVector3 p4, out FixVector3 r1, out FixVector3 r2)
		{
			FixVector3 r = p3 - p1;
			FixVector3 u = p2 - p1;
			FixVector3 v = p4 - p3;
			Fix64 ru = FixVector3.Dot(r, u);
			Fix64 rv = FixVector3.Dot(r, v);
			Fix64 uu = FixVector3.Dot(u, u);
			Fix64 uv = FixVector3.Dot(u, v);
			Fix64 vv = FixVector3.Dot(v, v);
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
		public static void LineToPointDistance(FixVector3 p1, FixVector3 p2, FixVector3 p3, out FixVector3 r1)
		{
			FixVector3 ab = p2 - p1;
			Fix64 length = FixVector3.Dot(p3 - p1, ab);
			if (length <= Fix64.Epsilon) 
			{
				r1 = p1;
			} 
			else 
			{
				Fix64 denom = FixVector3.Dot(ab, ab);
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

		public static void LineToPlaneIntersection(FixVector3 rayOrigin, FixVector3 rayEnd, FixVector3 normal, 
												FixVector3 coord, out FixVector3 r1, out FixVector3 r2)
		{
			// get d value
			Fix64 d = FixVector3.Dot(normal, coord);
			FixVector3 rayNormal = FixVector3.Normalize(rayEnd - rayOrigin);

			//Avoid divisions by zero
			if (FixVector3.Dot(normal, rayNormal) == Fix64.Zero)
			{
				r1 = FixVector3.Zero; // No intersection, the line is parallel to the plane
				r2 = FixVector3.Zero;
				return;
			}

			// Compute the X value for the directed line ray intersecting the plane
			Fix64 x = (d - FixVector3.Dot(normal, rayOrigin)) / FixVector3.Dot(normal, rayNormal);

			FixVector3 pointInFace = rayOrigin + rayNormal * x; //Make sure your ray vector is normalized

			LineToPointDistance(rayOrigin, rayEnd, pointInFace, out FixVector3 pointInLine);

			// output contact point
			r1 = pointInFace;
			r2 = pointInLine;
		}

		public static FixVector3 GetBarycentricCoordinates(FixVector3 p, FixVector3 a, FixVector3 b, FixVector3 c)
		{
			// Vectors from vertex A to vertices B and C
			FixVector3 v0 = b - a, v1 = c - a, v2 = p - a;

			// Compute dot products
			Fix64 d00 = FixVector3.Dot(v0, v0);
			Fix64 d01 = FixVector3.Dot(v0, v1);
			Fix64 d11 = FixVector3.Dot(v1, v1);
			Fix64 d20 = FixVector3.Dot(v2, v0);
			Fix64 d21 = FixVector3.Dot(v2, v1);
			Fix64 denom = d00 * d11 - d01 * d01;

			// Check for a zero denominator before division
			// I want a higher precision for this to avoid too many errors
			if (Fix64.Abs(denom) <= Fix64.EpsilonPlus)
			{
				GD.Print("Degenerate triangle found. Cancelling operation");
				return FixVector3.Zero;
			}

			// Compute barycentric coordinates
			Fix64 v = (d11 * d20 - d01 * d21) / denom;
			Fix64 w = (d00 * d21 - d01 * d20) / denom;
			Fix64 u = Fix64.One - v - w;

			return new FixVector3(u, v, w);
		}
	}
}
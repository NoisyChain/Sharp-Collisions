using System;
using FixMath.NET;

namespace SharpCollisions
{
	[Serializable]
	public partial class SharpCollider2D
	{
		public bool active => Size != FixVector2.Zero;
		public CollisionFlags collisionFlags;
		public CollisionType Shape;
		public FixVector2 Size;
		public FixVector2 Position;
		public FixVector2 Offset;
		public FixRect BoundingBox;
		public FixVector2 Center;

		private bool CollisionRequireUpdate = true;
		private bool BoundingBoxRequireUpdate = true;
		
		public SharpCollider2D(){}
		
		public SharpCollider2D(FixVector2 center, FixVector2 offset, FixVector2 size, FixVector2[] points, CollisionType shape)
		{
			Shape = shape;
			Position = center;
			Offset = offset;
			Radius = Fix64.Min(size.x, size.y) / Fix64.Two;
			Height = Fix64.Max(size.x, size.y) / Fix64.Two;
			Size = size;
			CreatePoints(points);
		}

		public bool IsOverlapping(SharpCollider2D other, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			if (!active) return false;
			if (!other.active) return false;
			
			bool isColliding = false;

			bool isPolygon = Shape == CollisionType.Polygon || Shape == CollisionType.Box;
			bool otherIsPolygon = other.Shape == CollisionType.Polygon || other.Shape == CollisionType.Box;

			if (Shape == CollisionType.AABB && other.Shape == CollisionType.AABB) //AABB/AABB
				isColliding = AABBtoAABBCollision(this, other, out Normal, out Depth, out ContactPoint);
			else if (Shape == CollisionType.Circle && other.Shape == CollisionType.Circle) //Circle/Circle
				isColliding = CircleToCircleCollision(this, other, out Normal, out Depth, out ContactPoint);
			else if (Shape == CollisionType.Capsule && other.Shape == CollisionType.Capsule) //Capsule/Capsule
				isColliding = CapsuleToCapsuleCollision(this, other, out Normal, out Depth, out ContactPoint);
			else if (Shape == CollisionType.Circle && other.Shape == CollisionType.Capsule) //Circle/Capsule
				isColliding = CapsuleToSphereCollision(other, this, out Normal, out Depth, out ContactPoint);
			else if (Shape == CollisionType.Capsule && other.Shape == CollisionType.Circle) //Capsule/Circle
				isColliding = CapsuleToSphereCollision(this, other, out Normal, out Depth, out ContactPoint);
			else 
			{
				if (isPolygon && otherIsPolygon || //Polygon/Polygon
					Shape == CollisionType.Circle && otherIsPolygon|| //Circle/Polygon
					isPolygon && other.Shape == CollisionType.Circle || //Polygon/Circle
					Shape == CollisionType.Capsule && otherIsPolygon || //Capsule/Polygon
					isPolygon && other.Shape == CollisionType.Capsule) //Polygon/Capsule
					isColliding = GJKPolygonCollision(this, other, out Normal, out Depth, out ContactPoint);
			}
			return isColliding;
		}

		public CollisionFlags GetCollisionFlags(CollisionManifold2D collisiondData, SharpBody2D body)
		{
			CollisionFlags flag = collisionFlags;

			if (FixVector2.Dot(collisiondData.Normal, body.Down) > Fix64.ETA)
				flag.Below = true;
			if (FixVector2.Dot(collisiondData.Normal, body.Up) > Fix64.ETA)
				flag.Above = true;
			if (FixVector2.Dot(collisiondData.Normal, body.Right) > Fix64.ETA)
				flag.Right = true;
			if (FixVector2.Dot(collisiondData.Normal, body.Left) > Fix64.ETA)
				flag.Left = true;
			
			return flag;
		}

		public FixVector2 GetAABBContactPoint(SharpCollider2D A, SharpCollider2D B)
		{
			Fix64 minPointX = Fix64.Min(A.Center.x + A.Size.x, B.Center.x + B.Size.x);
			Fix64 maxPointX = Fix64.Max(A.Center.x - A.Size.x, B.Center.x - B.Size.x);
			Fix64 minPointY = Fix64.Min(A.Center.y + A.Size.y, B.Center.y + B.Size.y);
			Fix64 maxPointY = Fix64.Max(A.Center.y - A.Size.y, B.Center.y - B.Size.y);
			Fix64 mediantX = (minPointX + maxPointX) / Fix64.Two;
			Fix64 mediantY = (minPointY + maxPointY) / Fix64.Two;
			return new FixVector2(mediantX, mediantY);
		}
		
		public bool AABBtoAABBCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			bool collisionX = colliderA.Center.x - (colliderA.Size.x / Fix64.Two) <= colliderB.Center.x + (colliderB.Size.x / Fix64.Two) &&
				colliderA.Center.x + (colliderA.Size.x / Fix64.Two) >= colliderB.Center.x - (colliderB.Size.x / Fix64.Two);

			bool collisionY = colliderA.Center.y - (colliderA.Size.y / Fix64.Two) <= colliderB.Center.y + (colliderB.Size.y / Fix64.Two) &&
				colliderA.Center.y + (colliderA.Size.y / Fix64.Two) >= colliderB.Center.y - (colliderB.Size.y / Fix64.Two);

			if (collisionX && collisionY)
			{
				ContactPoint = GetAABBContactPoint(colliderA, colliderB);

				FixVector2 length = colliderB.Center - colliderA.Center;

				FixVector2 newDepth = FixVector2.Zero;
				newDepth.x = (colliderA.Size.x + colliderB.Size.x) / Fix64.Two;
				newDepth.y = (colliderA.Size.y + colliderB.Size.y) / Fix64.Two;
				newDepth.x -= Fix64.Abs(length.x);
				newDepth.y -= Fix64.Abs(length.y);
				Normal = FindAABBNormals(colliderA, colliderB);
				Depth = Normal * newDepth;
			}

			return collisionX && collisionY;
		}

		public FixVector2 FindAABBNormals(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			FixVector2 finalNormal;
			FixVector2 length = colliderB.Center - colliderA.Center;

			Fix64 SizeX = (colliderB.Size.x + colliderA.Size.x) / Fix64.Two;
			Fix64 SizeY = (colliderB.Size.y + colliderA.Size.y) / Fix64.Two;

			// calculate normal of collided surface
			if (Fix64.Abs(length.x) + SizeX > Fix64.Abs(length.y) + SizeY)
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
		
		public void UpdateBoundingBox()
		{
			Fix64 minX = Fix64.MaxValue;
			Fix64 minY = Fix64.MaxValue;
			Fix64 maxX = Fix64.MinValue;
			Fix64 maxY = Fix64.MinValue;

			switch(Shape)
			{
				//For AABB collisions, the bounding box is the same as its extents
				case CollisionType.AABB:
					minX = Center.x - (Size.y / Fix64.Two);
					maxX = Center.x + (Size.y / Fix64.Two);
					minY = Center.y - (Size.y / Fix64.Two);
					maxY = Center.y + (Size.y / Fix64.Two);
					break;
				case CollisionType.Box:
				case CollisionType.Polygon:
					FixVector2[] points = Points;

					for (int p = 0; p < points.Length; p++)
					{
						FixVector2 v = points[p];

						if (v.x < minX) minX = v.x;
						if (v.x > maxX) maxX = v.x;
						if (v.y < minY) minY = v.y;
						if (v.y > maxY) maxY = v.y;
					}
					break;
				case CollisionType.Capsule:
					minX = UpperPoint.x - Radius;
					maxX = UpperPoint.x + Radius;
					minY = UpperPoint.y - Radius;
					maxY = UpperPoint.y + Radius;

					if (LowerPoint.x < UpperPoint.x)
						minX = LowerPoint.x - Radius;
					if (LowerPoint.x > UpperPoint.x)
						maxX = LowerPoint.x + Radius;
					if (LowerPoint.y < UpperPoint.y)
						minY = LowerPoint.y - Radius;
					if (LowerPoint.y > UpperPoint.y)
						maxY = LowerPoint.y + Radius;
					break;
				case CollisionType.Circle:
					minX = Center.x - Radius;
					maxX = Center.x + Radius;
					minY = Center.y - Radius;
					maxY = Center.y + Radius;
					break;
			}

			BoundingBox = new FixRect(minX, minY, maxX, maxY);
			BoundingBoxRequireUpdate = true;
		}
	}
}

public enum CollisionType
{
	AABB = 0,
	Box = 1,
	Circle = 2,
	Capsule = 3,
	Polygon = 4,
}

public enum CollisionType3D
{
	AABB = 0,
	Box = 1,
	Sphere = 2,
	Capsule = 3,
	Cylinder = 4,
	Polygon = 5,
}

/*[Flags]
public enum CollisionFlags : byte
{
	Null = 0,
	Below = 0 << 0,
	Above = 0 << 1,
	Right = 0 << 2,
	Left = 0 << 3,
	Walls = Right | Left
}

[Flags]
public enum CollisionFlags3D : byte
{
	Null = 0,
	Below = 0 << 0,
	Above = 0 << 1,
	Right = 0 << 2,
	Left = 0 << 3,
	Front = 0 << 4,
	Back = 0 << 5,
	Walls = Right | Left | Front | Back
}*/

public struct CollisionFlags
{
	public bool Below;
	public bool Above;
	public bool Right;
	public bool Left;

	public bool Walls => Right || Left;
	public bool Any  => Below || Above || Right || Left;

	public void Clear()
	{
		Below = false;
		Above = false;
		Right = false;
		Left = false;
	}

    public override string ToString()
    {
        return $"(Below: {Below}, Above: {Above}, Right: {Right}, Left: {Left})";
    }
}


using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
	[Tool] [GlobalClass]
	public partial class SharpCollider3D : Node
	{
		[Export] public bool Active = true;
		[Export] protected Vector3I positionOffset;
		[Export] protected Vector3I rotationOffset;
		[Export] public Color debugColor = new Color(0, 0, 1);
		[Export] protected bool DrawDebug;
		public CollisionFlags collisionFlags;
		public CollisionFlags globalCollisionFlags;
		public CollisionType3D Shape = CollisionType3D.Null;
		public FixVector3 Position;
		public FixVector3 PositionOffset;
		public FixVector3 RotationOffset;
		public FixVector3 Center;
		public FixVolume BoundingBox;

		protected bool CollisionRequireUpdate = true;
		protected bool BoundingBoxRequireUpdate = true;

		/*public SharpCollider2D(){}
		
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

		public override void _Ready()
		{
			ParentNode = GetParent() as SharpBody3D;
		}*/

		public virtual void Initialize()
		{
			PositionOffset = new FixVector3(
                (Fix64)positionOffset.X / SharpNode.convertedScale,
                (Fix64)positionOffset.Y / SharpNode.convertedScale,
                (Fix64)positionOffset.Z / SharpNode.convertedScale
            );

            RotationOffset = new FixVector3(
                (Fix64)rotationOffset.X / SharpNode.convertedScale,
                (Fix64)rotationOffset.Y / SharpNode.convertedScale,
                (Fix64)rotationOffset.Z / SharpNode.convertedScale
            );
			//PositionOffset = (FixVector3)positionOffset;
			//RotationOffset = (FixVector3)rotationOffset;
		}

		public virtual void DebugDrawShapes(SharpBody3D reference)
		{

		}

		public bool IsOverlapping(SharpCollider3D other, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			if (!Active) return false;
			if (!other.Active) return false;
			
			return CollisionDetection(other, out Normal, out Depth, out ContactPoint);
		}

		public virtual bool CollisionDetection(SharpCollider3D other, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			return false;
		}

		public void GetCollisionFlags(FixVector3 normal, SharpBody3D body)
		{
			if (FixVector3.Dot(normal, body.Up) > Fix64.Epsilon)
				collisionFlags.Below = true;
			if (FixVector3.Dot(normal, body.Down) > Fix64.Epsilon)
				collisionFlags.Above = true;
			if (FixVector3.Dot(normal, body.Left) > Fix64.Epsilon)
				collisionFlags.Right = true;
			if (FixVector3.Dot(normal, body.Right) > Fix64.Epsilon)
				collisionFlags.Left = true;
            if (FixVector3.Dot(normal, body.Back) > Fix64.Epsilon)
				collisionFlags.Forward = true;
			if (FixVector3.Dot(normal, body.Forward) > Fix64.Epsilon)
				collisionFlags.Back = true;
		}

		public void GetGlobalCollisionFlags(FixVector3 normal)
		{
			if (FixVector3.Dot(normal, FixVector3.Up) > Fix64.Epsilon)
				globalCollisionFlags.Below = true;
			if (FixVector3.Dot(normal, FixVector3.Down) > Fix64.Epsilon)
				globalCollisionFlags.Above = true;
			if (FixVector3.Dot(normal, FixVector3.Left) > Fix64.Epsilon)
				globalCollisionFlags.Right = true;
			if (FixVector3.Dot(normal, FixVector3.Right) > Fix64.Epsilon)
				globalCollisionFlags.Left = true;
            if (FixVector3.Dot(normal, FixVector3.Back) > Fix64.Epsilon)
				globalCollisionFlags.Forward = true;
			if (FixVector3.Dot(normal, FixVector3.Forward) > Fix64.Epsilon)
				globalCollisionFlags.Back = true;
		}

		public virtual FixVector3 Support(FixVector3 direction)
		{
			return direction;
		}
		
		public void UpdateBoundingBox()
		{
			BoundingBox = GetBoundingBoxPoints();
			BoundingBoxRequireUpdate = false;
		}

		protected virtual FixVolume GetBoundingBoxPoints()
		{
			return new FixVolume();
		}

		public virtual void UpdatePoints(SharpBody3D body)
		{
			Center = FixVector3.Transform(PositionOffset, body);
			CollisionRequireUpdate = false;
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

public enum CollisionType3D
{
	Null = -1,
	AABB = 0,
	Sphere = 1,
	Capsule = 2,
	Cylinder = 3,
	Polygon = 4,
}

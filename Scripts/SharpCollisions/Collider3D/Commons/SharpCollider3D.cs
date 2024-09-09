using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
	[Tool] [GlobalClass]
	public partial class SharpCollider3D : Node
	{
		[Export] public bool Active = true;
		[Export] protected Vector3 offset;
		[Export] public Color debugColor = new Color(0, 0, 1);
		[Export] protected bool DrawDebug;
		public CollisionFlags collisionFlags;
		public CollisionFlags globalCollisionFlags;
		public CollisionType3D Shape = CollisionType3D.Null;
		public FixVector3 Position;
		public FixVector3 Offset;
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
			Offset = (FixVector3)offset;
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

		public CollisionFlags GetCollisionFlags(CollisionManifold3D collisiondData, SharpBody3D body)
		{
			CollisionFlags flag = collisionFlags;

			if (FixVector3.Dot(collisiondData.Normal, body.Up) > Fix64.ETA)
				flag.Below = true;
			if (FixVector3.Dot(collisiondData.Normal, body.Down) > Fix64.ETA)
				flag.Above = true;
			if (FixVector3.Dot(collisiondData.Normal, body.Left) > Fix64.ETA)
				flag.Right = true;
			if (FixVector3.Dot(collisiondData.Normal, body.Right) > Fix64.ETA)
				flag.Left = true;
            if (FixVector3.Dot(collisiondData.Normal, body.Back) > Fix64.ETA)
				flag.Forward = true;
			if (FixVector3.Dot(collisiondData.Normal, body.Forward) > Fix64.ETA)
				flag.Back = true;
			
			return flag;
		}

		public CollisionFlags GetGlobalCollisionFlags(CollisionManifold3D collisiondData)
		{
			CollisionFlags flag = collisionFlags;

			if (FixVector3.Dot(collisiondData.Normal, FixVector3.Up) > Fix64.ETA)
				flag.Below = true;
			if (FixVector3.Dot(collisiondData.Normal, FixVector3.Down) > Fix64.ETA)
				flag.Above = true;
			if (FixVector3.Dot(collisiondData.Normal, FixVector3.Left) > Fix64.ETA)
				flag.Right = true;
			if (FixVector3.Dot(collisiondData.Normal, FixVector3.Right) > Fix64.ETA)
				flag.Left = true;
            if (FixVector3.Dot(collisiondData.Normal, FixVector3.Back) > Fix64.ETA)
				flag.Forward = true;
			if (FixVector3.Dot(collisiondData.Normal, FixVector3.Forward) > Fix64.ETA)
				flag.Back = true;
			
			return flag;
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
			Center = FixVector3.Transform(Offset, body);
			CollisionRequireUpdate = false;
		}

		public void LineToLineDistance(FixVector3 p1, FixVector3 p2, FixVector3 p3, FixVector3 p4, out FixVector3 r1, out FixVector3 r2)
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

			Fix64 S = Fix64.Clamp01((t * uv + ru) / uu);
			Fix64 T = Fix64.Clamp01((s * uv - rv) / vv);

			r1 = p1 + S * u;
			r2 = p3 + T * v;
		}

		//Line to point collision code taken from Noah Zuo's Blog
		//https://arrowinmyknee.com/2021/03/15/some-math-about-capsule-collision/
		public void LineToPointDistance(FixVector3 p1, FixVector3 p2, FixVector3 p3, out FixVector3 r1)
		{
			FixVector3 ab = p2 - p1;
			Fix64 length = FixVector3.Dot(p3 - p1, ab);
			if (length <= Fix64.ETA) 
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

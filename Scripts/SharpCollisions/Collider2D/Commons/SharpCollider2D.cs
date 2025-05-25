using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
	[Tool] [GlobalClass]
	public partial class SharpCollider2D  : Node
	{
		[Export] public bool Active = true;
		[Export] public bool isTrigger = false;
		[Export] protected Vector2I positionOffset;
		[Export] protected int rotationOffset;
		[Export] protected bool DrawDebug;
		[Export] public Color debugColor = new Color(0, 0, 1);
		[Export] public Color selectedColor = new Color(1, 0.6f, 0.1f);
		[Export(PropertyHint.Flags, "Layer1, Layer2, Layer3, Layer4, Layer5, Layer6, Layer7, Layer8")]
		public int CollisionLayers = 1;
		[Export(PropertyHint.Flags, "Layer1, Layer2, Layer3, Layer4, Layer5, Layer6, Layer7, Layer8")]
		public int CollisionMask = 1;
		
		public CollisionFlags collisionFlags;
		public CollisionFlags globalCollisionFlags;
		public CollisionType2D Shape = CollisionType2D.Null;
		public FixVector2 Position;
		public FixVector2 PositionOffset;
		public Fix64 RotationOffset;
		public FixVector2 Center;
		public FixRect BoundingBox;

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
		}*/

		public virtual void Initialize()
		{
			PositionOffset = new FixVector2(
				(Fix64)positionOffset.X  / SharpNode.NodeScale,
				(Fix64)positionOffset.Y  / SharpNode.NodeScale
			);
			RotationOffset = (Fix64)rotationOffset / SharpNode.NodeRotation;
			RotationOffset *= Fix64.DegToRad;
		}

		public virtual void DebugDrawShapes(SharpBody2D reference)
		{

		}

		public virtual void DebugDrawShapesEditor(Node3D reference, bool selected)
		{

		}

		public bool IsOverlapping(SharpCollider2D other, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			if (!Active) return false;
			if (!other.Active) return false;
			
			return CollisionDetection(other, out Normal, out Depth, out ContactPoint);
		}

		public virtual bool CollisionDetection(SharpCollider2D other, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			/*if (colliderA.Shape == CollisionType2D.AABB && colliderB.Shape == CollisionType2D.AABB)
                return AABBtoAABBCollision(colliderA as AABBCollider2D, colliderB as AABBCollider2D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Circle)
                return CircleToCircleCollision(colliderA as CircleCollider2D, colliderB as CircleCollider2D, out Normal, out Depth, out ContactPoint);
            else if (colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Capsule)
                return CapsuleToCapsuleCollision(colliderA as CapsuleCollider2D, colliderB as CapsuleCollider2D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Capsule)
				return CircleToCapsuleCollision(colliderA as CircleCollider2D, colliderB as CapsuleCollider2D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Circle)
				return CapsuleToCircleCollision(colliderA as CapsuleCollider2D, colliderB as CircleCollider2D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Polygon ||
					colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Circle ||
                    colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Polygon ||
					colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Capsule ||
                    colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Polygon)
                return GJKPolygonCollision(colliderA, colliderB, out Normal, out Depth, out ContactPoint);
			*/
			return false;
		}

		public void GetCollisionFlags(FixVector2 normal, SharpBody2D body)
		{
			if (FixVector2.Dot(normal, body.Up) > Fix64.Epsilon)
				collisionFlags.Below = true;
			if (FixVector2.Dot(normal, body.Down) > Fix64.Epsilon)
				collisionFlags.Above = true;
			if (FixVector2.Dot(normal, body.Left) > Fix64.Epsilon)
				collisionFlags.Right = true;
			if (FixVector2.Dot(normal, body.Right) > Fix64.Epsilon)
				collisionFlags.Left = true;
		}

		public void GetGlobalCollisionFlags(FixVector2 normal)
		{
			if (FixVector2.Dot(normal, FixVector2.Up) > Fix64.Epsilon)
				globalCollisionFlags.Below = true;
			if (FixVector2.Dot(normal, FixVector2.Down) > Fix64.Epsilon)
				globalCollisionFlags.Above = true;
			if (FixVector2.Dot(normal, FixVector2.Left) > Fix64.Epsilon)
				globalCollisionFlags.Right = true;
			if (FixVector2.Dot(normal, FixVector2.Right) > Fix64.Epsilon)
				globalCollisionFlags.Left = true;
		}

		public virtual FixVector2 Support(FixVector2 direction)
		{
			return direction;
		}
		
		public void UpdateBoundingBox()
		{
			BoundingBox = GetBoundingBoxPoints();
		}

		protected virtual FixRect GetBoundingBoxPoints()
		{
			return new FixRect();
		}

		public virtual void UpdatePoints(FixVector2 position, Fix64 rotation)
		{
			Center = FixVector2.Transform(PositionOffset, position, rotation);
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

public enum CollisionType2D
{
	Null = -1,
	AABB = 0,
	Circle = 1,
	Capsule = 2,
	Polygon = 3,
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
	public bool Forward;
	public bool Back;

	public bool Walls => Right || Left || Forward || Back;
	public bool Any  => Below || Above || Right || Left || Forward || Back;

	public void Clear()
	{
		Below = false;
		Above = false;
		Right = false;
		Left = false;
		Forward = false;
		Back = false;
	}
	public bool Compare(CollisionFlags compareTo)
	{
		return Below == compareTo.Below || Above == compareTo.Above || 
			Right == compareTo.Right || Left == compareTo.Left || 
			Forward == compareTo.Forward || Back == compareTo.Back;
	}
	public bool ComparePositive(CollisionFlags compareTo)
	{
		return (Below && Below == compareTo.Below) ||
			(Above && Above == compareTo.Above) || 
			(Right && Right == compareTo.Right) ||
			(Left && Left == compareTo.Left) || 
			(Forward && Forward == compareTo.Forward) ||
			(Back && Back == compareTo.Back);
	}
	
	public bool Equals(CollisionFlags compareTo)
	{
		return Below == compareTo.Below && Above == compareTo.Above &&
			Right == compareTo.Right && Left == compareTo.Left &&
			Forward == compareTo.Forward && Back == compareTo.Back;
	}
    public override string ToString()
    {
        return $"(Below: {Below}, Above: {Above}, Right: {Right}, Left: {Left}, Forward: {Forward}, Back: {Back})";
    }
}


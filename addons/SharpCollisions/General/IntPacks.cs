using System;
using FixMath.NET;

namespace SharpCollisions
{
    public enum BodyType { Static, Kinematic, Dynamic }
    public enum GJKCollisionLevel { Performance, Precision }
    public struct IntPack2
    {
        public int a;
        public int b;

        public IntPack2(int newA, int newB)
        {
            a = newA;
            b = newB;
        }

        public IntPack2 Inverse => new IntPack2(b, a);

        public bool IsEquals(IntPack2 compare)
        {
            return a == compare.a && b == compare.b;
        }

        public bool IsReverse(IntPack2 compare)
        {
            return a == compare.b && b == compare.a;
        }
    };

    public struct IntPack3
    {
        public int a;
        public int b;
        public int c;

        public IntPack3(int newA, int newB, int newC)
        {
            a = newA;
            b = newB;
            c = newC;
        }

        public bool IsEquals(IntPack3 compare)
        {
            return a == compare.a && b == compare.b && c == compare.c;
        }
    };

    public struct SupportPoint2D
    {
        public FixVector2 pointA;
        public FixVector2 pointB;
        public FixVector2 Point() { return pointA - pointB; }

        public SupportPoint2D(FixVector2 a, FixVector2 b)
        {
            pointA = a;
            pointB = b;
        }
    };

    public struct SupportPoint3D
    {
        public FixVector3 pointA;
        public FixVector3 pointB;
        public FixVector3 Point() { return pointA - pointB; }

        public SupportPoint3D(FixVector3 a, FixVector3 b)
        {
            pointA = a;
            pointB = b;
        }
    };

    public struct PossibleCollision
    {
        public int BodyA;
        public int BodyB;
        public int ColliderA;
        public int ColliderB;
        public int Priority;
        public Fix64 distance;

        public PossibleCollision(int newA, int newB, int colA, int colB, int priority, Fix64 newDist)
        {
            BodyA = newA;
            BodyB = newB;
            ColliderA = colA;
            ColliderB = colB;
            Priority = priority;
            distance = newDist;
        }
    };

    public struct CollisionProfile2D
	{
		public bool Collided;
		public Sharp2D.SharpBody2D BodyA;
		public Sharp2D.SharpBody2D BodyB;
        public int ColliderA;
        public int ColliderB;
        public FixVector2 Normal;
        public FixVector2 Depth;
        public FixVector2 ContactPoint;

		public CollisionProfile2D() {}
        public CollisionProfile2D(bool collided, Sharp2D.SharpBody2D bodyA, Sharp2D.SharpBody2D bodyB, int colA, int colB, FixVector2 normal, FixVector2 depth, FixVector2 contact)
        {
			Collided = collided;
			BodyA = bodyA;
            BodyB = bodyB;
            ColliderA = colA;
            ColliderB = colB;
            Normal = normal;
            Depth = depth;
            ContactPoint = contact;
        }
	};

    public struct CollisionProfile3D
	{
		public bool Collided;
		public Sharp3D.SharpBody3D BodyA;
		public Sharp3D.SharpBody3D BodyB;
        public int ColliderA;
        public int ColliderB;
        public FixVector3 Normal;
        public FixVector3 Depth;
        public FixVector3 ContactPoint;

		public CollisionProfile3D() {}
        public CollisionProfile3D(bool collided, Sharp3D.SharpBody3D bodyA, Sharp3D.SharpBody3D bodyB, int colA, int colB, FixVector3 normal, FixVector3 depth, FixVector3 contact)
        {
			Collided = collided;
			BodyA = bodyA;
            BodyB = bodyB;
            ColliderA = colA;
            ColliderB = colB;
            Normal = normal;
            Depth = depth;
            ContactPoint = contact;
        }
	};

    public struct SurfaceData2D
    {
        public int ID;
        public FixVector2 Normal;
    };

    public struct SurfaceData3D
    {
        public int ID;
        public FixVector3 Normal;
    };

    [Flags]
    public enum CollisionFlags : byte
    {
        Empty = 0,
        Below = 1 << 0,
        Above = 1 << 1,
        Right = 1 << 2,
        Left = 1 << 3,
        Forward = 1 << 4,
        Back = 1 << 5,
        Walls = Right | Left | Forward | Back,
        Any = Below | Above | Right | Left | Forward | Back,
    }
}
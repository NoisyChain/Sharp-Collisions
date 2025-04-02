using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public struct IntPack2
    {
        public int a;
        public int b;

        public IntPack2(int newA, int newB)
        {
            a = newA;
            b = newB;
        }

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
    }

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
    }

    public struct PossibleCollision
    {
        public int BodyA;
        public int BodyB;
        public int ColliderA;
        public int ColliderB;
        public Fix64 distance;

        public PossibleCollision(int newA, int newB, int colA, int colB, Fix64 newDist)
        {
            BodyA = newA;
            BodyB = newB;
            ColliderA = colA;
            ColliderB = colB;
            distance = newDist;
        }
    };
}
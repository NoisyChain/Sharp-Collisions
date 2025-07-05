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
}
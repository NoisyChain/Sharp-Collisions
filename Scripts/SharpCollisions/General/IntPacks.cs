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

    public struct PossibleCollision
    {
        public int BodyA;
        public int BodyB;
        public Fix64 distance;

        public PossibleCollision(int newA, int newB, Fix64 newDist)
        {
            BodyA = newA;
            BodyB = newB;
            distance = newDist;
        }
    };
}
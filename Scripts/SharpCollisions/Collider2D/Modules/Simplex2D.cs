using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions.Sharp2D
{
    [System.Serializable]
    public struct Simplex2D
    {
        public List<FixVector2> Points;
        public int Size;

        public Simplex2D()
        {
            Points = new List<FixVector2>() { FixVector2.Zero, FixVector2.Zero, FixVector2.Zero };
            Size = 0;
        }

        public Simplex2D(List<FixVector2> newList)
        {
            Points = newList;
            Size = 0;
        }

        public void MoveForward(FixVector2 newPoint)
        {
            //List<FixVector2> TempPoints = new List<FixVector2>()
            //{ newPoint, Points[0], Points[1] };
            //Points = TempPoints;
            Points[2] = Points[1];
            Points[1] = Points[0];
            Points[0] = newPoint;
            Size++;
            if (Size > 3) Size = 3;
        }

        public void Reset(List<FixVector2> newPoints)
        {
            for (int p = 0; p < newPoints.Count; p++)
            {
                Points[p] = newPoints[p];
            }
            Size = newPoints.Count;
        }

        public void Clear()
        {
            Points[2] = FixVector2.Zero;
            Points[1] = FixVector2.Zero;
            Points[0] = FixVector2.Zero;
            Size = 0;
        }

        public override string ToString()
        {
            return $"({Points[0]}, {Points[1]}, {Points[2]})";
        }
    };
}

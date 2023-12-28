using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions
{
    [System.Serializable]
    public struct Simplex2D
    {
        public List<FixVector2> Points;
        public int Size;

        public Simplex2D(List<FixVector2> newList)
        {
            Points = newList;
            Size = 0;
        }

        public void MoveForward(FixVector2 newPoint)
        {
            List<FixVector2> TempPoints = new List<FixVector2>()
            { newPoint, Points[0], Points[1] };
            Points = TempPoints;
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

        public override string ToString()
        {
            return $"({Points[0].ToString()}, {Points[1].ToString()}, {Points[2].ToString()})";
        }
    };
}

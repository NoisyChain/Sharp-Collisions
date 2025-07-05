using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions.Sharp2D
{
    [System.Serializable]
    public struct Simplex2D
    {
        public List<SupportPoint2D> Points;
        public int Size;

        public Simplex2D()
        {
            Points = new List<SupportPoint2D>() { new SupportPoint2D(), new SupportPoint2D(), new SupportPoint2D() };
            Size = 0;
        }

        public Simplex2D(List<SupportPoint2D> newList)
        {
            Points = newList;
            Size = 0;
        }

        public void MoveForward(SupportPoint2D newPoint)
        {
            Points[2] = Points[1];
            Points[1] = Points[0];
            Points[0] = newPoint;

            if (Size < 3) Size++;
        }

        public void Reset(List<SupportPoint2D> newPoints)
        {
            for (int p = 0; p < newPoints.Count; p++)
            {
                Points[p] = newPoints[p];
            }
            Size = newPoints.Count;
        }

        public void Clear()
        {
            for (int p = 0; p < Points.Count; p++)
            {
                Points[p] = new SupportPoint2D();
            }
            Size = 0;
        }

        public override string ToString()
        {
            return $"({Points[0]}, {Points[1]}, {Points[2]})";
        }
    };
}

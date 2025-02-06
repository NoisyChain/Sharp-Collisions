using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions.Sharp3D.GJK
{
    [System.Serializable]
    public struct Simplex3D
    {
        public List<SupportPoint3D> Points;
        public int Size;

        public Simplex3D()
        {
            Points = new List<SupportPoint3D>()
            { new SupportPoint3D(), new SupportPoint3D(), new SupportPoint3D(), new SupportPoint3D() };
            Size = 0;
        }

        public Simplex3D(List<SupportPoint3D> newList)
        {
            Points = newList;
            Size = 0;
        }

        public void MoveForward(SupportPoint3D newPoint)
        {
            //List<FixVector3> TempPoints = new List<FixVector3>()
            //{ newPoint, Points[0], Points[1] , Points[2]};
            //Points = TempPoints;
            Points[3] = Points[2];
            Points[2] = Points[1];
            Points[1] = Points[0];
            Points[0] = newPoint;
            Size++;
            if (Size > 4) Size = 4;
        }

        public void Reset(List<SupportPoint3D> newPoints)
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
                Points[p] = new SupportPoint3D();
            }
            Size = 0;
        }

        public override string ToString()
        {
            return $"({Points[0]}, {Points[1]}, {Points[2]}, {Points[3]})";
        }
    };
}

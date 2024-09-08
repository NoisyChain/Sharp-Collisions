using FixMath.NET;
using System.Collections.Generic;

namespace SharpCollisions.Sharp3D
{
    [System.Serializable]
    public struct Simplex3D
    {
        public List<FixVector3> Points;
        public int Size;

        public Simplex3D(List<FixVector3> newList)
        {
            Points = newList;
            Size = 0;
        }

        public void MoveForward(FixVector3 newPoint)
        {
            List<FixVector3> TempPoints = new List<FixVector3>()
            { newPoint, Points[0], Points[1] , Points[2]};
            Points = TempPoints;
            Size++;
            if (Size > 4) Size = 4;
        }

        public void Reset(List<FixVector3> newPoints)
        {
            for (int p = 0; p < newPoints.Count; p++)
            {
                Points[p] = newPoints[p];
            }
            Size = newPoints.Count;
        }

        public override string ToString()
        {
            return $"({Points[0]}, {Points[1]}, {Points[2]}, {Points[3]})";
        }
    };
}

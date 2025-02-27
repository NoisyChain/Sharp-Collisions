using Godot;
using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions.Sharp2D.GJK
{
    [System.Serializable]
    public struct Polytope2D
    {
        public List<SupportPoint2D> Vertices;

        public Polytope2D()
        {
            Vertices = new List<SupportPoint2D>();
        }

        public Polytope2D(List<SupportPoint2D> vertices)
        {
            Vertices = vertices;
        }

        public FixRect GetBoundingBox()
        {
            Fix64 minX = Fix64.MaxValue;
			Fix64 minY = Fix64.MaxValue;
			Fix64 maxX = Fix64.MinValue;
			Fix64 maxY = Fix64.MinValue;

            for (int p = 0; p < Vertices.Count; p++)
            {
                FixVector2 v = Vertices[p].Point();

                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
            }

            return new FixRect(minX, minY, maxX, maxY);
        }
    };
}

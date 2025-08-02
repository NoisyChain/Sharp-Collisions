using Godot;
using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions.Sharp3D.GJK
{
    [System.Serializable]
    public struct Polytope3D
    {
        public List<SupportPoint3D> Vertices;
		public List<IntPack3> Faces;
        public int ClosestFace;

        public Polytope3D()
        {
            Vertices = new List<SupportPoint3D>();
            Faces = new List<IntPack3>();
            ClosestFace = 0;
        }

        public Polytope3D(List<SupportPoint3D> vertices, List<IntPack3> faces, int closestFace)
        {
            Vertices = vertices;
            Faces = faces;
            ClosestFace = closestFace;
        }

        public IntPack3 GetClosestFace() => Faces[ClosestFace];

        public FixVolume GetBoundingBox()
        {
            Fix64 minX = Fix64.MaxValue;
			Fix64 minY = Fix64.MaxValue;
            Fix64 minZ = Fix64.MaxValue;
			Fix64 maxX = Fix64.MinValue;
			Fix64 maxY = Fix64.MinValue;
            Fix64 maxZ = Fix64.MinValue;

            for (int p = 0; p < Vertices.Count; p++)
            {
                FixVector3 v = Vertices[p].Point();

                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
                if (v.z < minZ) minZ = v.z;
                if (v.z > maxZ) maxZ = v.z;
            }

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }
    };
}

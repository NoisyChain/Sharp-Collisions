using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions.Sharp3D.Octree
{
    public class OcTree
    {
        public List<SharpBody3D> Content;
        public FixVolume Area;
        public int Limit;
        public bool IsFull;

        public OcTree NW_P;
        public OcTree NE_P;
        public OcTree SW_P;
        public OcTree SE_P;
        public OcTree NW_N;
        public OcTree NE_N;
        public OcTree SW_N;
        public OcTree SE_N;

        public OcTree(FixVolume area, int lim)
        {
            Content = new List<SharpBody3D>();
            Area = area;
            Limit = lim;
        }

        public void Compute(List<SharpBody3D> shapes)
        {
            Content.Clear();
            IsFull = false;
            NW_P = null;
            NE_P = null;
            SW_P = null;
            SE_P = null;
            NW_N = null;
            NE_N = null;
            SW_N = null;
            SE_N = null;

            foreach (SharpBody3D s in shapes)
            {
                if (Area.IsOverlapping(s.BoundingBox))
                    Content.Add(s);
            }
            if (Content.Count <= Limit) return;

            IsFull = true;

            NW_P = new OcTree(new FixVolume(Area.x, Area.y, Area.z, Area.Center().x, Area.Center().y, Area.Center().z), Limit);
            NE_P = new OcTree(new FixVolume(Area.Center().x, Area.y, Area.z, Area.w, Area.Center().y, Area.Center().z), Limit);
            SW_P = new OcTree(new FixVolume(Area.x, Area.Center().y, Area.z, Area.Center().x, Area.h, Area.Center().z), Limit);
            SE_P = new OcTree(new FixVolume(Area.Center().x, Area.Center().y, Area.z, Area.w, Area.h, Area.Center().z), Limit);
            NW_N = new OcTree(new FixVolume(Area.x, Area.y, Area.Center().z, Area.Center().x, Area.Center().y, Area.d), Limit);
            NE_N = new OcTree(new FixVolume(Area.Center().x, Area.y, Area.Center().z, Area.w, Area.Center().y, Area.d), Limit);
            SW_N = new OcTree(new FixVolume(Area.x, Area.Center().y, Area.Center().z, Area.Center().x, Area.h, Area.d), Limit);
            SE_N = new OcTree(new FixVolume(Area.Center().x, Area.Center().y, Area.Center().z, Area.w, Area.h, Area.d), Limit);

            NW_P.Compute(Content);
            NE_P.Compute(Content);
            SW_P.Compute(Content);
            SE_P.Compute(Content);
            NW_N.Compute(Content);
            NE_N.Compute(Content);
            SW_N.Compute(Content);
            SE_N.Compute(Content);
            Content.Clear();
        }

        public void Query(FixVolume queryArea, ref List<SharpBody3D> result)
        {
            if (IsFull)
            {
                NW_P.Query(queryArea, ref result);
                NE_P.Query(queryArea, ref result);
                SW_P.Query(queryArea, ref result);
                SE_P.Query(queryArea, ref result);
                NW_N.Query(queryArea, ref result);
                NE_N.Query(queryArea, ref result);
                SW_N.Query(queryArea, ref result);
                SE_N.Query(queryArea, ref result);
            }
            else
            {
                if (Content == null || Content.Count == 0) return;
                if (!Area.IsOverlapping(queryArea)) return;

                for (int i = 0; i < Content.Count; i++)
                {
                    if (result.Contains(Content[i])) continue;
                    if (!queryArea.IsOverlapping(Content[i].BoundingBox)) continue;

                    result.Add(Content[i]);
                }
            }
        }

        public void CapturePossibleCollisions(ref List<IntPack2> collisions)
        {
            if (IsFull)
            {
                NW_P.CapturePossibleCollisions(ref collisions);
                NE_P.CapturePossibleCollisions(ref collisions);
                SW_P.CapturePossibleCollisions(ref collisions);
                SE_P.CapturePossibleCollisions(ref collisions);
                NW_N.CapturePossibleCollisions(ref collisions);
                NE_N.CapturePossibleCollisions(ref collisions);
                SW_N.CapturePossibleCollisions(ref collisions);
                SE_N.CapturePossibleCollisions(ref collisions);
            }
            else
            {
                for (int i = 0; i < Content.Count; i++)
                {
                    for (int j = i + 1; j < Content.Count; j++)
                    {
                        if (Content[i].BodyMode == 2 && Content[j].BodyMode == 2) continue;

                        IntPack2 newCol = new IntPack2((int)Content[i].GetBodyID(), (int)Content[j].GetBodyID());
                        if (collisions.Contains(newCol)) continue;
                        if (collisions.Contains(newCol.Inverse)) continue;
                        if (!Content[i].BoundingBox.IsOverlapping(Content[j].BoundingBox)) continue;

                        collisions.Add(newCol);
                    }
                }
            }
        }
    }
}
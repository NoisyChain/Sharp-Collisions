using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions.Sharp2D.Quadtree
{
    public class QuadTree
    {
        public List<SharpBody2D> Content;
        public FixRect Area;
        public int Limit;
        public bool IsFull;

        public QuadTree NW;
        public QuadTree NE;
        public QuadTree SW;
        public QuadTree SE;

        public QuadTree(FixRect area, int lim)
        {
            Content = new List<SharpBody2D>();
            Area = area;
            Limit = lim;
        }

        public void Compute(List<SharpBody2D> shapes)
        {
            Content.Clear();
            IsFull = false;
            NW = null;
            NE = null;
            SW = null;
            SE = null;

            foreach (SharpBody2D s in shapes)
            {
                if (Area.IsOverlapping(s.BoundingBox))
                    Content.Add(s);
            }
            if (Content.Count <= Limit) return;

            IsFull = true;

            NW = new QuadTree(new FixRect(Area.x, Area.y, Area.Center().x, Area.Center().y), Limit);
            NE = new QuadTree(new FixRect(Area.Center().x, Area.y, Area.w, Area.Center().y), Limit);
            SW = new QuadTree(new FixRect(Area.x, Area.Center().y, Area.Center().x, Area.h), Limit);
            SE = new QuadTree(new FixRect(Area.Center().x, Area.Center().y, Area.w, Area.h), Limit);

            NW.Compute(Content);
            NE.Compute(Content);
            SW.Compute(Content);
            SE.Compute(Content);
            Content.Clear();
        }

        public void Query(FixRect queryArea, ref List<SharpBody2D> result)
        {
            if (IsFull)
            {
                NW.Query(queryArea, ref result);
                NE.Query(queryArea, ref result);
                SW.Query(queryArea, ref result);
                SE.Query(queryArea, ref result);
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
                NW.CapturePossibleCollisions(ref collisions);
                NE.CapturePossibleCollisions(ref collisions);
                SW.CapturePossibleCollisions(ref collisions);
                SE.CapturePossibleCollisions(ref collisions);
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
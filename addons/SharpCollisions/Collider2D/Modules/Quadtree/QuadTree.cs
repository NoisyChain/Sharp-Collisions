using System.Collections.Generic;
using FixMath.NET;
using Godot;

namespace SharpCollisions.Sharp2D.Quadtree
{
    public class QuadTree
    {
        public List<(SharpBody2D, int)> Content;
        public FixRect Area;
        public int Limit;
        public bool IsFull;

        public QuadTree NW;
        public QuadTree NE;
        public QuadTree SW;
        public QuadTree SE;

        public QuadTree(FixRect area, int lim)
        {
            Content = new List<(SharpBody2D, int)>();
            Area = area;
            Limit = lim;
        }

        public void Compute(List<SharpBody2D> shapes)
        {
            // Expand Content to contain colliders
            // Create new SharpBodies list to store the contained bodies
            // Use new list to compute next areas
            
            Content = new List<(SharpBody2D, int)>();
            List<SharpBody2D> ContainedBodies = new List<SharpBody2D>();
            IsFull = false;
            NW = null;
            NE = null;
            SW = null;
            SE = null;

            foreach (SharpBody2D body in shapes)
            {
                if (body == null) continue;
                if (!body.Active) continue;
                if (!body.HasColliders()) continue;
                
                for(int i = 0; i < body.GetColliders().Length; i++)
                {
                    SharpCollider2D col = body.GetCollider(i);
                    if (!col.Active) continue;
                    if (Area.IsOverlapping(body.GetCollider(i).BoundingBox))
                    {
                        Content.Add((body, i));
                        if (!ContainedBodies.Contains(body)) ContainedBodies.Add(body);
                    }
                }
            }
            if (Content.Count <= Limit) return;

            IsFull = true;

            NW = new QuadTree(new FixRect(Area.x, Area.y, Area.Center().x, Area.Center().y), Limit);
            NE = new QuadTree(new FixRect(Area.Center().x, Area.y, Area.w, Area.Center().y), Limit);
            SW = new QuadTree(new FixRect(Area.x, Area.Center().y, Area.Center().x, Area.h), Limit);
            SE = new QuadTree(new FixRect(Area.Center().x, Area.Center().y, Area.w, Area.h), Limit);

            NW.Compute(ContainedBodies);
            NE.Compute(ContainedBodies);
            SW.Compute(ContainedBodies);
            SE.Compute(ContainedBodies);
            Content.Clear();
            ContainedBodies.Clear();
        }

        public void Query(FixRect queryArea, ref List<(SharpBody2D, int)> result)
        {
            if (IsFull)
            {
                NW.Query(queryArea, ref result);
                NE.Query(queryArea, ref result);
                SW.Query(queryArea, ref result);
                SE.Query(queryArea, ref result);
                return;
            }

            if (Content == null || Content.Count == 0) return;
            if (!Area.IsOverlapping(queryArea)) return;

            foreach ((SharpBody2D, int) conts in Content)
            {
                if (result.Contains(conts)) continue;
                if (!queryArea.IsOverlapping(conts.Item1.GetCollider(conts.Item2).BoundingBox)) continue;

                result.Add(conts);
            }
        }

        public void CapturePossibleCollisions(ref List<PossibleCollision> collisions)
        {
            if (IsFull)
            {
                NW.CapturePossibleCollisions(ref collisions);
                NE.CapturePossibleCollisions(ref collisions);
                SW.CapturePossibleCollisions(ref collisions);
                SE.CapturePossibleCollisions(ref collisions);
                return;
            }

            for (int i = 0; i < Content.Count; i++)
            {
                var bodyA = Content[i].Item1;
                for (int j = i + 1; j < Content.Count; j++)
                {
                    var bodyB = Content[j].Item1;

                    if (bodyA.BodyMode == 2 && bodyB.BodyMode == 2) continue;
                    if (bodyA.IsIgnoringBody(bodyB)) continue;
                    if (!bodyA.CompareLayers(bodyB)) continue;

                    SharpCollider2D colA = Content[i].Item1.GetCollider(Content[i].Item2);
                    SharpCollider2D colB = Content[j].Item1.GetCollider(Content[j].Item2);

                    if ((colA.TriggerIgnoresSolid && !colB.IsTrigger) || (colB.TriggerIgnoresSolid && !colA.IsTrigger)) continue;
                    if (!colA.BoundingBox.IsOverlapping(colB.BoundingBox)) continue;

                    PossibleCollision newCol = new PossibleCollision(
                        (int)Content[i].Item1.GetBodyID(), (int)Content[j].Item1.GetBodyID(),
                        Content[i].Item2, Content[j].Item2,
                        Godot.Mathf.Max(Content[i].Item1.Priority, Content[j].Item1.Priority),
                        CollisionMath2D.GetCollisionDistance(colA, colB)
                    );
                    if (!collisions.Contains(newCol)) collisions.Add(newCol);  
                }
            }
        }

        public void Draw()
        {
            if (IsFull)
            {
                NW.Draw();
                NE.Draw();
                SW.Draw();
                SE.Draw();
                return;
            }

            Vector3[] points =
            [
                new Vector3((float)Area.x,(float)Area.y, 0.0f),
                new Vector3((float)Area.w,(float)Area.y, 0.0f),
                new Vector3((float)Area.w,(float)Area.h, 0.0f),
                new Vector3((float)Area.x,(float)Area.h, 0.0f),
            ];

            for (int i = 0; i < points.Length; i++)
            {
                Vector3 start = points[i];
                Vector3 end = points[(i + 1) % points.Length];
                DebugDraw3D.DrawLine(start, end, Colors.SeaGreen);
            }
        }
    }
}
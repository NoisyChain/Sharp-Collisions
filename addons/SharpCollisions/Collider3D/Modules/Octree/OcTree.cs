using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions.Sharp3D.Octree
{
    public class OcTree
    {
        public List<(SharpBody3D, int)> Content;
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
            Content = new List<(SharpBody3D, int)>();
            Area = area;
            Limit = lim;
        }

        public void Compute(List<SharpBody3D> shapes)
        {
            Content = new List<(SharpBody3D, int)>();
            List<SharpBody3D> ContainedBodies = new List<SharpBody3D>();
            IsFull = false;
            NW_P = null;
            NE_P = null;
            SW_P = null;
            SE_P = null;
            NW_N = null;
            NE_N = null;
            SW_N = null;
            SE_N = null;

            foreach (SharpBody3D body in shapes)
            {
                if (!body.Active) continue;
                if (!body.HasColliders()) continue;
                
                for(int i = 0; i < body.GetColliders().Length; i++)
                {
                    SharpCollider3D col = body.GetCollider(i);
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

            NW_P = new OcTree(new FixVolume(Area.x, Area.y, Area.z, Area.Center().x, Area.Center().y, Area.Center().z), Limit);
            NE_P = new OcTree(new FixVolume(Area.Center().x, Area.y, Area.z, Area.w, Area.Center().y, Area.Center().z), Limit);
            SW_P = new OcTree(new FixVolume(Area.x, Area.Center().y, Area.z, Area.Center().x, Area.h, Area.Center().z), Limit);
            SE_P = new OcTree(new FixVolume(Area.Center().x, Area.Center().y, Area.z, Area.w, Area.h, Area.Center().z), Limit);
            NW_N = new OcTree(new FixVolume(Area.x, Area.y, Area.Center().z, Area.Center().x, Area.Center().y, Area.d), Limit);
            NE_N = new OcTree(new FixVolume(Area.Center().x, Area.y, Area.Center().z, Area.w, Area.Center().y, Area.d), Limit);
            SW_N = new OcTree(new FixVolume(Area.x, Area.Center().y, Area.Center().z, Area.Center().x, Area.h, Area.d), Limit);
            SE_N = new OcTree(new FixVolume(Area.Center().x, Area.Center().y, Area.Center().z, Area.w, Area.h, Area.d), Limit);

            NW_P.Compute(ContainedBodies);
            NE_P.Compute(ContainedBodies);
            SW_P.Compute(ContainedBodies);
            SE_P.Compute(ContainedBodies);
            NW_N.Compute(ContainedBodies);
            NE_N.Compute(ContainedBodies);
            SW_N.Compute(ContainedBodies);
            SE_N.Compute(ContainedBodies);
            Content.Clear();
            ContainedBodies.Clear();
        }

        public void Query(FixVolume queryArea, ref List<(SharpBody3D, int)> result)
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
                return;
            }

            if (Content == null || Content.Count == 0) return;
            if (!Area.IsOverlapping(queryArea)) return;

            foreach ((SharpBody3D, int) conts in Content)
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
                NW_P.CapturePossibleCollisions(ref collisions);
                NE_P.CapturePossibleCollisions(ref collisions);
                SW_P.CapturePossibleCollisions(ref collisions);
                SE_P.CapturePossibleCollisions(ref collisions);
                NW_N.CapturePossibleCollisions(ref collisions);
                NE_N.CapturePossibleCollisions(ref collisions);
                SW_N.CapturePossibleCollisions(ref collisions);
                SE_N.CapturePossibleCollisions(ref collisions);
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

                    SharpCollider3D colA = Content[i].Item1.GetCollider(Content[i].Item2);
                    SharpCollider3D colB = Content[j].Item1.GetCollider(Content[j].Item2);

                    if ((colA.TriggerIgnoresSolid && !colB.IsTrigger) || (colB.TriggerIgnoresSolid && !colA.IsTrigger)) continue;
                    if (!colA.BoundingBox.IsOverlapping(colB.BoundingBox)) continue;

                    PossibleCollision newCol = new PossibleCollision(
                        (int)Content[i].Item1.GetBodyID(), (int)Content[j].Item1.GetBodyID(),
                        Content[i].Item2, Content[j].Item2,
                        Godot.Mathf.Max(Content[i].Item1.Priority, Content[j].Item1.Priority),
                        CollisionMath3D.GetCollisionDistance(colA, colB)
                    );
                    if (!collisions.Contains(newCol)) collisions.Add(newCol);  
                }
            }
        }
    }
}
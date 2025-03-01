using Godot;
using Godot.Collections;
using FixMath.NET;
using System;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class BoxCollider3D : PolygonCollider3D
    {
        public FixVector3 Extents;
        [Export] private Vector3I extents = Vector3I.One;

        public override void Initialize()
        {
            Extents = new FixVector3(
                (Fix64)extents.X / SharpNode.convertedScale,
                (Fix64)extents.Y / SharpNode.convertedScale,
                (Fix64)extents.Z / SharpNode.convertedScale
            );
            base.Initialize();
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!DrawDebug) return;

            //Draw Lower quad
            DebugDraw3D.DrawLine((Vector3)Points[0], (Vector3)Points[1], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[1], (Vector3)Points[2], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[2], (Vector3)Points[3], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[3], (Vector3)Points[0], debugColor);
            //Draw Upper quad
            DebugDraw3D.DrawLine((Vector3)Points[4], (Vector3)Points[5], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[5], (Vector3)Points[6], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[6], (Vector3)Points[7], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[7], (Vector3)Points[4], debugColor);
            //Connect both quads
            DebugDraw3D.DrawLine((Vector3)Points[0], (Vector3)Points[4], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[1], (Vector3)Points[5], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[2], (Vector3)Points[6], debugColor);
            DebugDraw3D.DrawLine((Vector3)Points[3], (Vector3)Points[7], debugColor);

            foreach(Vector3I faces in Faces)
            {
                FixVector3 origin = FixVector3.FindTriangleCentroid(Points[faces[0]], Points[faces[1]], Points[faces[2]]);
                FixVector3 normal = FixVector3.GetPlaneNormal(Points[faces[0]], Points[faces[1]], Points[faces[2]]);
                Vector3 dir = (Vector3)origin + ((Vector3)normal * 0.5f);
                DebugDraw3D.DrawLine((Vector3)origin, dir, new Color(0, 1, 0));
            }

            //DebugDraw3D.DrawSphere((Vector3)BoundingBox.Center(), 0.12f, new Color(1, 1, 0));
        }

        protected override void CreatePolygonPoints()
        {
            RawPoints = new FixVector3[]
            {
                new FixVector3(-Extents.x, -Extents.y, Extents.z),
                new FixVector3(-Extents.x, -Extents.y, -Extents.z),
                new FixVector3(Extents.x, -Extents.y, -Extents.z),
                new FixVector3(Extents.x, -Extents.y, Extents.z),
                new FixVector3(-Extents.x, Extents.y, Extents.z),
                new FixVector3(-Extents.x, Extents.y, -Extents.z),
                new FixVector3(Extents.x, Extents.y, -Extents.z),
                new FixVector3(Extents.x, Extents.y, Extents.z)
            };
            
            Points = new FixVector3[RawPoints.Length];
        }

        protected override void CreateFaces()
        {
            Faces = new Array<Vector3I>()
            {
                new Vector3I(0, 1, 2),
                new Vector3I(0, 2, 3),
                new Vector3I(4, 1, 0),
                new Vector3I(1, 4, 5),
                new Vector3I(5, 2, 1),
                new Vector3I(2, 5, 6),
                new Vector3I(7, 3, 2),
                new Vector3I(6, 7, 2),
                new Vector3I(0, 3, 4),
                new Vector3I(7, 4, 3),
                new Vector3I(6, 5, 4),
                new Vector3I(7, 6, 4),
            };
        }
    }
}

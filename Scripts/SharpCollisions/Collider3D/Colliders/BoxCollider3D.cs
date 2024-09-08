using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [GlobalClass]
    public partial class BoxCollider3D : PolygonCollider3D
    {
        public FixVector3 Extents;

        [Export] private Vector3 extents = Vector3.One;

        public override void Initialize()
        {
            base.Initialize();
            Extents = (FixVector3)extents;
            CreateBoxPoints();
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
        }

        private void CreateBoxPoints()
        {
            RawPoints = new FixVector3[]
            {
                new FixVector3(Offset.x - Extents.x, Offset.y - Extents.y, Offset.z + Extents.z),
                new FixVector3(Offset.x - Extents.x, Offset.y - Extents.y, Offset.z - Extents.z),
                new FixVector3(Offset.x + Extents.x, Offset.y - Extents.y, Offset.z - Extents.z),
                new FixVector3(Offset.x + Extents.x, Offset.y - Extents.y, Offset.z + Extents.z),
                new FixVector3(Offset.x - Extents.x, Offset.y + Extents.y, Offset.z + Extents.z),
                new FixVector3(Offset.x - Extents.x, Offset.y + Extents.y, Offset.z - Extents.z),
                new FixVector3(Offset.x + Extents.x, Offset.y + Extents.y, Offset.z - Extents.z),
                new FixVector3(Offset.x + Extents.x, Offset.y + Extents.y, Offset.z + Extents.z)
            };
            
            Points = new FixVector3[RawPoints.Length];
        }
    }
}

using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public partial class BoxCollider3D : PolygonCollider3D
    {
        public FixVector3 Extents => (FixVector3)extents;

        [Export] private Vector3 extents = Vector3.One;

        public override void _Ready()
        {
            base._Ready();
            CreateBoxPoints();
        }

        public override void DebugDrawShapes()
        {
            if (!DrawDebug) return;

            //Transform3D debugTransform = new(ParentNode.GlobalBasis, (Vector3)Center);
            //DebugDraw.Box(debugTransform, (Vector3)Extents * 2, debugColor);
            //Draw Lower quad
            DebugDraw.Line((Vector3)Points[0], (Vector3)Points[1], debugColor);
            DebugDraw.Line((Vector3)Points[1], (Vector3)Points[2], debugColor);
            DebugDraw.Line((Vector3)Points[2], (Vector3)Points[3], debugColor);
            DebugDraw.Line((Vector3)Points[3], (Vector3)Points[0], debugColor);
            //Draw Upper quad
            DebugDraw.Line((Vector3)Points[4], (Vector3)Points[5], debugColor);
            DebugDraw.Line((Vector3)Points[5], (Vector3)Points[6], debugColor);
            DebugDraw.Line((Vector3)Points[6], (Vector3)Points[7], debugColor);
            DebugDraw.Line((Vector3)Points[7], (Vector3)Points[4], debugColor);
            //Connect both quads
            DebugDraw.Line((Vector3)Points[0], (Vector3)Points[4], debugColor);
            DebugDraw.Line((Vector3)Points[1], (Vector3)Points[5], debugColor);
            DebugDraw.Line((Vector3)Points[2], (Vector3)Points[6], debugColor);
            DebugDraw.Line((Vector3)Points[3], (Vector3)Points[7], debugColor);
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

using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class BoxCollider3D : PolygonCollider3D
    {
        public FixVector3 Extents => (FixVector3)extents;

        [Export] private Vector3 extents = Vector3.One;

        public override void _Ready()
        {
            base._Ready();
            CreateBoxPoints();
        }

        private void CreateBoxPoints()
        {
            RawPoints = new FixVector3[]
            {
                new FixVector3(Offset.x - Extents.x, Offset.y + Extents.y, Offset.z - Extents.z),
                new FixVector3(Offset.x - Extents.x, Offset.y - Extents.y, Offset.z - Extents.z),
                new FixVector3(Offset.x + Extents.x, Offset.y - Extents.y, Offset.z - Extents.z),
                new FixVector3(Offset.x + Extents.x, Offset.y + Extents.y, Offset.z - Extents.z),
                new FixVector3(Offset.x - Extents.x, Offset.y + Extents.y, Offset.z + Extents.z),
                new FixVector3(Offset.x - Extents.x, Offset.y - Extents.y, Offset.z + Extents.z),
                new FixVector3(Offset.x + Extents.x, Offset.y - Extents.y, Offset.z + Extents.z),
                new FixVector3(Offset.x + Extents.x, Offset.y + Extents.y, Offset.z + Extents.z)
            };
            
            Points = new FixVector3[RawPoints.Length];
        }
    }
}

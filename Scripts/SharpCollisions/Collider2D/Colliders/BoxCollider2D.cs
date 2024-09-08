using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [GlobalClass]
    public partial class BoxCollider2D : PolygonCollider2D
    {
        public FixVector2 Extents;

        [Export] private Vector2 extents = Vector2.One;

        public override void Initialize()
        {
            base.Initialize();
            Extents = (FixVector2)extents;
            CreateBoxPoints();
        }

        private void CreateBoxPoints()
        {
            RawPoints = new FixVector2[]
            {
                new FixVector2(Offset.x - Extents.x, Offset.y + Extents.y),
                new FixVector2(Offset.x - Extents.x, Offset.y - Extents.y),
                new FixVector2(Offset.x + Extents.x, Offset.y - Extents.y),
                new FixVector2(Offset.x + Extents.x, Offset.y + Extents.y)
            };
            
            Points = new FixVector2[RawPoints.Length];
        }
    }
}

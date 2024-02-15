using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class BoxCollider2D : PolygonCollider2D
    {
        public FixVector2 Extents => (FixVector2)extents;

        [Export] private Vector2 extents = Vector2.One;

        public override void _Ready()
        {
            base._Ready();
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

using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class BoxCollider2D : PolygonCollider2D
    {
        public FixVector2 Extents;

        [Export] private Vector2 extents
        {
            get => (Vector2)Extents;
            set => Extents = (FixVector2)value;
        }

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

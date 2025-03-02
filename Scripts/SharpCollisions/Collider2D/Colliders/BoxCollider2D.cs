using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class BoxCollider2D : PolygonCollider2D
    {
        public FixVector2 Extents;

        [Export] private Vector2I extents = Vector2I.One;

        public override void Initialize()
        {
            base.Initialize();
            Extents = new FixVector2(
                (Fix64)extents.X / SharpNode.convertedScale,
                (Fix64)extents.Y / SharpNode.convertedScale
            );
            CreateBoxPoints();
        }

        private void CreateBoxPoints()
        {
            RawPoints = new FixVector2[]
            {
                new FixVector2(-Extents.x, Extents.y),
                new FixVector2(-Extents.x, -Extents.y),
                new FixVector2(Extents.x, -Extents.y),
                new FixVector2(Extents.x, Extents.y)
            };
            
            Points = new FixVector2[RawPoints.Length];
        }
    }
}

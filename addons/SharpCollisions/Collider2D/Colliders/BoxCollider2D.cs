using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool]
    [GlobalClass]
    public partial class BoxCollider2D : ConvexShapeCollider2D
    {
        [Export] private Vector2 _extents
        {
            get => (Vector2)Extents;
            set
            {
                if (Engine.IsEditorHint()) // Avoid any float values changing fixed point raw values when the game runs
                {
                    raw_Extents_X = ((Fix64)value.X).RawValue;
                    raw_Extents_Y = ((Fix64)value.Y).RawValue;
                    Extents = new FixVector2(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y));
                }
            }
        }

        [ExportSubgroup("Raw Values")]
        [Export] private long raw_extents_X
        {
            get => Extents.x.RawValue;
            set
            {
                raw_Extents_X = value;
                Extents = new FixVector2(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y));
            }
        }
        [Export] private long raw_extents_Y
        {
            get => Extents.y.RawValue;
            set
            {
                raw_Extents_Y = value;
                Extents = new FixVector2(Fix64.FromRaw(raw_Extents_X), Fix64.FromRaw(raw_Extents_Y));
            }
        }

        private long raw_Extents_X;
        private long raw_Extents_Y;

        public FixVector2 Extents;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void CreatePolygonPoints()
        {
            RawPoints = new FixVector2[] {
                new FixVector2(-Extents.x, Extents.y),
                new FixVector2(-Extents.x, -Extents.y),
                new FixVector2(Extents.x, -Extents.y),
                new FixVector2(Extents.x, Extents.y)
            };

            Points = new FixVector2[RawPoints.Length];
        }
    }
}

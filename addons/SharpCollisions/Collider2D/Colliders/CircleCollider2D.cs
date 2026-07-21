using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class CircleCollider2D : SharpCollider2D
    {
		[Export] private float _radius
        {
            get =>(float)Fix64.FromRaw(raw_Radius);
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_Radius = ((Fix64)((decimal)value)).RawValue;
                    Radius = Fix64.FromRaw(raw_Radius);
                }
            }
        }

		[ExportSubgroup("Raw Values")]
        [Export] private long raw_radius
        {
            get => raw_Radius;
            set
            {
                raw_Radius = value;
                Radius = Fix64.FromRaw(raw_Radius);
            }
        }

        private long raw_Radius;

        public Fix64 Radius;
        
        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType2D.Circle;
        }

        public override void DebugDrawShapes(SharpBody2D reference)
        {
			if (!Active) return;
            if (!DrawDebugShape) return;

            Vector3 DirX = (Vector3)reference.Right;
            Vector3 DirY = (Vector3)reference.Up;

            CustomDebugDraw.DrawCircle((Vector3)Center, DirX, DirY, (float)Radius + 0.005f, DebugShapeColor);
        }

		public override void DebugDrawShapesEditor(SharpBody2D reference, bool selected)
		{
			if (!Active) return;
			if (!selected && !DrawDebugShape) return;

			Color finalColor = selected ? DebugShapeColorSelected : DebugShapeColor;

			Vector3 DirX = (Vector3)reference.Right;
			Vector3 DirY = (Vector3)reference.Up;
			Vector3 pos = new Vector3((float)PositionOffset.x, (float)PositionOffset.y, 0);
			Vector3 newPos = SharpHelpers.Transform3D(pos, (Vector3)reference.FixedPosition, new Vector3(0.0f, 0.0f, (float)reference.FixedRotation));

			CustomDebugDraw.DrawCircle(newPos, DirX, DirY, (float)Radius + 0.005f, finalColor);
		}

        public override void UpdateBoundingBox()
		{
			BoundingBox = CollisionMath2D.UpdateCircleBoundingBox(Center, Radius);
		}

        public override void UpdatePoints(FixVector2 position, Fix64 rotation)
        {
            base.UpdatePoints(position, rotation);
        }
    }
}

using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public partial class CircleCollider2D : SharpCollider2D
    {
        public Fix64 Radius => (Fix64)radius;
        [Export] protected float radius;
        
        public override void _Ready()
        {
            base._Ready();
            Shape = CollisionType2D.Circle;
        }

        public override void DebugDrawShapes()
        {
            if (!DrawDebug) return;
            Transform3D debugTransform = new(ParentNode.GlobalBasis, (Vector3)Center);
            DebugDraw.Sphere(debugTransform, (float)Radius, debugColor);
        }

        protected override FixRect GetBoundingBoxPoints()
        {
            return UpdateCircleBoundingBox();
        }

        public override void UpdatePoints(SharpBody2D body)
        {
            base.UpdatePoints(body);
        }

		public override FixVector2 Support(FixVector2 direction)
		{
			FixVector2 NormalizedDirection = FixVector2.Normalize(direction);
			return Center + Radius * NormalizedDirection;
		}

        public FixRect UpdateCircleBoundingBox()
        {
            Fix64 minX = Center.x - Radius;
            Fix64 minY = Center.y - Radius;
            Fix64 maxX = Center.x + Radius;
            Fix64 maxY = Center.y + Radius;

            return new FixRect(minX, minY, maxX, maxY);
        }
    }
}

using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class CircleCollider2D : SharpCollider2D
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
            if (Draw3D == null) return;

            Draw3D.Call("clear");
            Draw3D.Call("circle_normal", (Vector3)Center, Vector3.Forward, (float)Radius, debugColor);
            //DebugDrawCS.DrawArcLine((Vector3)Center, Vector3.Right, Vector3.Up, (float)Radius, debugColor);
            //DebugDrawCS.DrawArcLine((Vector3)Center, Vector3.Left, Vector3.Up, (float)Radius, debugColor);
            //DebugDrawCS.DrawArcLine((Vector3)Center, Vector3.Right, Vector3.Down, (float)Radius, debugColor);
            //DebugDrawCS.DrawArcLine((Vector3)Center, Vector3.Left, Vector3.Down, (float)Radius, debugColor);
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

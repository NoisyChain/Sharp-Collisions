using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class CapsuleCollider2D : SharpCollider2D
    {
        public Fix64 Radius;
        public Fix64 Height;
        public FixVector2 RawUpperPoint;
		public FixVector2 RawLowerPoint;
		public FixVector2 UpperPoint;
		public FixVector2 LowerPoint;

        [Export] protected float radius
		{
			get => (float)Radius;
			set => Radius = (Fix64)value;
		}
        [Export] protected float height
		{
			get => (float)Height;
			set => Height = (Fix64)value;
		}

        [Export(PropertyHint.Enum, "X-Axis,Y-Axis")]
		private int AxisDirection = 0;

        public override void _Ready()
        {
            base._Ready();
            Shape = CollisionType2D.Capsule;
            CreateCapsulePoints();
        }

        private void CreateCapsulePoints()
        {
            bool isYAxis = AxisDirection != 0;

			FixVector2 CapsuleDirection = isYAxis ? 
				new FixVector2(Fix64.Zero, Height - Radius) : 
				new FixVector2(Height - Radius, Fix64.Zero);

			RawUpperPoint = Offset + CapsuleDirection;
			RawLowerPoint = Offset - CapsuleDirection;
        }

        private void UpdateCapsulePoints(SharpBody2D body)
        {
            UpperPoint = FixVector2.Transform(RawUpperPoint, body);
			LowerPoint = FixVector2.Transform(RawLowerPoint, body);
        }

        public override void DebugDrawShapes()
        {
            if (!DrawDebug) return;

            Vector3 LineVector = (Vector3)FixVector2.GetNormal(UpperPoint, LowerPoint);
            Vector3 LineSpacing = LineVector * (float)Radius;
            Vector3 LineDirection = (Vector3)FixVector2.Normalize(UpperPoint - LowerPoint);

            DebugDrawCS.DrawArcLine((Vector3)UpperPoint, LineVector, LineDirection, (float)Radius, debugColor);
            DebugDrawCS.DrawArcLine((Vector3)UpperPoint, -LineVector, LineDirection, (float)Radius, debugColor);
            DebugDrawCS.DrawArcLine((Vector3)LowerPoint, LineVector, -LineDirection, (float)Radius, debugColor);
            DebugDrawCS.DrawArcLine((Vector3)LowerPoint, -LineVector, -LineDirection, (float)Radius, debugColor);
            DebugDrawCS.DrawLine((Vector3)UpperPoint + LineSpacing, (Vector3)LowerPoint + LineSpacing, debugColor);
            DebugDrawCS.DrawLine((Vector3)UpperPoint - LineSpacing, (Vector3)LowerPoint - LineSpacing, debugColor);
		}

        protected override FixRect GetBoundingBoxPoints()
        {
            return UpdateCapsuleBoundingBox();
        }

        public override void UpdatePoints(SharpBody2D body)
        {
            UpdateCapsulePoints(body);
            base.UpdatePoints(body);
        }

		public override FixVector2 Support(FixVector2 direction)
		{
			FixVector2 NormalizedDirection = FixVector2.Normalize(direction);
			Fix64 Dy = FixVector2.Dot(NormalizedDirection, UpperPoint - LowerPoint);

			if (Dy == Fix64.Zero) return Center + Radius * NormalizedDirection;
			else return (Dy < Fix64.Zero ? LowerPoint : UpperPoint) + Radius * NormalizedDirection;
		}

        public FixRect UpdateCapsuleBoundingBox()
        {
            Fix64 minX = UpperPoint.x - Radius;
            Fix64 minY = UpperPoint.y - Radius;
            Fix64 maxX = UpperPoint.x + Radius;
            Fix64 maxY = UpperPoint.y + Radius;

            if (LowerPoint.x < UpperPoint.x)
                minX = LowerPoint.x - Radius;
            if (LowerPoint.x > UpperPoint.x)
                maxX = LowerPoint.x + Radius;
            if (LowerPoint.y < UpperPoint.y)
                minY = LowerPoint.y - Radius;
            if (LowerPoint.y > UpperPoint.y)
                maxY = LowerPoint.y + Radius;

            return new FixRect(minX, minY, maxX, maxY);
        }
    }
}

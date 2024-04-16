using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public partial class CapsuleCollider2D : SharpCollider2D
    {
        public Fix64 Radius => (Fix64)radius;
        public Fix64 Height => (Fix64)height;
        public FixVector2 RawUpperPoint;
		public FixVector2 RawLowerPoint;
		public FixVector2 UpperPoint;
		public FixVector2 LowerPoint;

        [Export] protected float radius;
        [Export] protected float height;
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

            ///Vector3 LineVector = (Vector3)FixVector2.GetNormal(UpperPoint, LowerPoint);
            ///Vector3 LineSpacing = LineVector * (float)Radius;
            ///Vector3 LineDirection = (Vector3)FixVector2.Normalize(UpperPoint - LowerPoint);

            Transform3D debugTransform = new(ParentNode.GlobalBasis, (Vector3)Center);
            DebugDraw.Capsule(debugTransform, (float)Radius, (float)Height * 2, debugColor);
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

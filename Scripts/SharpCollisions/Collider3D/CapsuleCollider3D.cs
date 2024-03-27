using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class CapsuleCollider3D : SharpCollider3D
    {
        public Fix64 Radius => (Fix64)radius;
        public Fix64 Height => (Fix64)height;
        public FixVector3 RawUpperPoint;
		public FixVector3 RawLowerPoint;
		public FixVector3 UpperPoint;
		public FixVector3 LowerPoint;

        [Export] protected float radius;
        [Export] protected float height;
        [Export(PropertyHint.Enum, "X-Axis,Y-Axis,Z-Axix")]
		private int AxisDirection = 0;

        public override void _Ready()
        {
            base._Ready();
            Shape = CollisionType3D.Capsule;
            CreateCapsulePoints();
        }

        private void CreateCapsulePoints()
        {
            FixVector3 CapsuleDirection = FixVector3.Zero;

            switch (AxisDirection)
            {
                case 0:
                    CapsuleDirection = new FixVector3(Height - Radius, Fix64.Zero, Fix64.Zero);
                    break;
                case 1:
                    CapsuleDirection = new FixVector3(Fix64.Zero, Height - Radius, Fix64.Zero);
                    break;
                case 2:
                    CapsuleDirection = new FixVector3(Fix64.Zero, Fix64.Zero, Height - Radius);
                    break;
            }
			RawUpperPoint = Offset + CapsuleDirection;
			RawLowerPoint = Offset - CapsuleDirection;
        }

        private void UpdateCapsulePoints(SharpBody3D body)
        {
            UpperPoint = FixVector3.Transform(RawUpperPoint, body);
			LowerPoint = FixVector3.Transform(RawLowerPoint, body);
        }

        public override void DebugDrawShapes()
        {
            if (!DrawDebug) return;
            if (Draw3D == null) return;

            //Vector3 LineVector = (Vector3)FixVector3.GetNormal(UpperPoint, LowerPoint);
            //Vector3 LineSpacing = LineVector * (float)Radius;
            //Vector3 LineDirection = (Vector3)FixVector3.Normalize(UpperPoint - LowerPoint);

            Draw3D.Call("clear");
            Draw3D.Call("circle_normal", (Vector3)LowerPoint, Vector3.Forward, (float)Radius, debugColor);
            Draw3D.Call("circle_normal", (Vector3)LowerPoint, Vector3.Right, (float)Radius, debugColor);
            Draw3D.Call("circle_normal", (Vector3)LowerPoint, Vector3.Up, (float)Radius, debugColor);
            Draw3D.Call("circle_normal", (Vector3)UpperPoint, Vector3.Forward, (float)Radius, debugColor);
            Draw3D.Call("circle_normal", (Vector3)UpperPoint, Vector3.Right, (float)Radius, debugColor);
            Draw3D.Call("circle_normal", (Vector3)UpperPoint, Vector3.Up, (float)Radius, debugColor);
            //Draw3D.Call("line_segment", (Vector3)UpperPoint + LineSpacing, (Vector3)LowerPoint + LineSpacing, debugColor);
            //Draw3D.Call("line_segment", (Vector3)UpperPoint - LineSpacing, (Vector3)LowerPoint - LineSpacing, debugColor);
		}

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdateCapsuleBoundingBox();
        }

        public override void UpdatePoints(SharpBody3D body)
        {
            UpdateCapsulePoints(body);
            base.UpdatePoints(body);
        }

		public override FixVector3 Support(FixVector3 direction)
		{
			FixVector3 NormalizedDirection = FixVector3.Normalize(direction);
			Fix64 Dy = FixVector3.Dot(NormalizedDirection, UpperPoint - LowerPoint);

			if (Dy == Fix64.Zero) return Center + Radius * NormalizedDirection;
			else return (Dy < Fix64.Zero ? LowerPoint : UpperPoint) + Radius * NormalizedDirection;
		}

        public FixVolume UpdateCapsuleBoundingBox()
        {
            Fix64 minX = UpperPoint.x - Radius;
            Fix64 minY = UpperPoint.y - Radius;
            Fix64 minZ = UpperPoint.z - Radius;
            Fix64 maxX = UpperPoint.x + Radius;
            Fix64 maxY = UpperPoint.y + Radius;
            Fix64 maxZ = UpperPoint.z + Radius;

            if (LowerPoint.x < UpperPoint.x)
                minX = LowerPoint.x - Radius;
            if (LowerPoint.x > UpperPoint.x)
                maxX = LowerPoint.x + Radius;
            if (LowerPoint.y < UpperPoint.y)
                minY = LowerPoint.y - Radius;
            if (LowerPoint.y > UpperPoint.y)
                maxY = LowerPoint.y + Radius;
            if (LowerPoint.z < UpperPoint.z)
                minZ = LowerPoint.z - Radius;
            if (LowerPoint.z > UpperPoint.z)
                maxZ = LowerPoint.z + Radius;

            return new FixVolume(minX, minY, minZ, maxX, maxY, maxZ);
        }
    }
}

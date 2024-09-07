using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public partial class CapsuleCollider3D : SharpCollider3D
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

            Vector3 DirX = (Vector3)ParentNode.Right;
            Vector3 DirY = (Vector3)ParentNode.Up;
            Vector3 DirZ = (Vector3)ParentNode.Forward;

            float inflatedRadius = (float)Radius + 0.005f;

            Vector3 LineSpacingX = DirX * inflatedRadius;
            Vector3 LineSpacingY = DirY * inflatedRadius;
            Vector3 LineSpacingZ = DirZ * inflatedRadius;

            Vector3 LineSpacing1 = Vector3.Zero;
            Vector3 LineSpacing2 = Vector3.Zero;

            switch (AxisDirection)
            {
                case 0:
                    LineSpacing1 = LineSpacingY;
                    LineSpacing2 = LineSpacingZ;
                    break;
                case 1:
                    LineSpacing1 = LineSpacingX;
                    LineSpacing2 = LineSpacingZ;
                    break;
                case 2:
                    LineSpacing1 = LineSpacingX;
                    LineSpacing2 = LineSpacingY;
                    break;
            }

            DebugDraw3D.DrawSimpleSphere((Vector3)UpperPoint, DirX, DirY, DirZ, inflatedRadius, debugColor);
            DebugDraw3D.DrawSimpleSphere((Vector3)LowerPoint, DirX, DirY, DirZ, inflatedRadius, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint + LineSpacing1, (Vector3)LowerPoint + LineSpacing1, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint - LineSpacing1, (Vector3)LowerPoint - LineSpacing1, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint + LineSpacing2, (Vector3)LowerPoint + LineSpacing2, debugColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint - LineSpacing2, (Vector3)LowerPoint - LineSpacing2, debugColor);
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

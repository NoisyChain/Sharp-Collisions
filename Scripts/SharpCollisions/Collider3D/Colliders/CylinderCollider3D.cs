using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class CylinderCollider3D : SharpCollider3D
    {
        public Fix64 Radius;
        public Fix64 Height;
        public FixVector3 RawUpperPoint;
		public FixVector3 RawLowerPoint;
		public FixVector3 UpperPoint;
		public FixVector3 LowerPoint;

        [Export] protected float radius;
        [Export] protected float height;
        [Export(PropertyHint.Enum, "X-Axis,Y-Axis,Z-Axix")]
		private int AxisDirection = 0;

        public override void Initialize()
        {
            base.Initialize();
            Radius = (Fix64)radius;
            Height = (Fix64)height;
            Shape = CollisionType3D.Cylinder;
            CreateCylinderPoints();
        }

        private void CreateCylinderPoints()
        {
            FixVector3 CapsuleDirection = FixVector3.Zero;

            switch (AxisDirection)
            {
                case 0:
                    CapsuleDirection = new FixVector3(Height, Fix64.Zero, Fix64.Zero);
                    break;
                case 1:
                    CapsuleDirection = new FixVector3(Fix64.Zero, Height, Fix64.Zero);
                    break;
                case 2:
                    CapsuleDirection = new FixVector3(Fix64.Zero, Fix64.Zero, Height);
                    break;
            }
			RawUpperPoint = Offset + CapsuleDirection;
			RawLowerPoint = Offset - CapsuleDirection;
        }

        private void UpdateCylinderPoints(SharpBody3D body)
        {
            UpperPoint = FixVector3.Transform(RawUpperPoint, body);
			LowerPoint = FixVector3.Transform(RawLowerPoint, body);
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!DrawDebug) return;

            //Transform3D debugTransform = new(ParentNode.GlobalBasis, (Vector3)Center);
            //DebugDraw.Cylinder(debugTransform, (float)Radius, (float)Height * 2);
        }

        protected override FixVolume GetBoundingBoxPoints()
        {
            return UpdateCylinderBoundingBox();
        }

        public override void UpdatePoints(SharpBody3D body)
        {
            UpdateCylinderPoints(body);
            base.UpdatePoints(body);
        }

		public override FixVector3 Support(FixVector3 direction)
		{
			FixVector3 NormalizedDirection = FixVector3.Normalize(direction);
			Fix64 Dy = FixVector3.Dot(NormalizedDirection, UpperPoint - LowerPoint);

			if (Dy == Fix64.Zero) return Center + Radius * NormalizedDirection;
			else return (Dy < Fix64.Zero ? LowerPoint : UpperPoint) + Radius * NormalizedDirection;
		}

        public FixVolume UpdateCylinderBoundingBox()
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

using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool]// [GlobalClass]
    public partial class CylinderCollider3D : SharpCollider3D
    {
        public Fix64 Radius;
        public Fix64 Height;
        public FixVector3 RawUpperPoint;
		public FixVector3 RawLowerPoint;
		public FixVector3 UpperPoint;
		public FixVector3 LowerPoint;

        [Export] protected int startingRadius;
        [Export] protected int startingHeight;

        public override void Initialize()
        {
            base.Initialize();
            Radius = (Fix64)startingRadius / SharpNode.NodeScale;
            Height = (Fix64)startingHeight / SharpNode.NodeScale;
            Shape = CollisionType3D.Cylinder;
            CreateCylinderPoints();
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!DrawDebugShape) return;
        }

        public override void DebugDrawShapesEditor(SharpBody3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebugShape) return;
        }

        public override void UpdateBoundingBox() 
        {
            BoundingBox = CollisionMath3D.UpdateCylinderBoundingBox(UpperPoint, LowerPoint, Radius);
        }

        private void CreateCylinderPoints()
        {
            FixVector3 CapsuleDirection = new FixVector3(Fix64.Zero, Fix64.Zero, Height);

			RawUpperPoint = CapsuleDirection;
			RawLowerPoint = -CapsuleDirection;
        }

        private void UpdateCylinderPoints(FixVector3 position, FixVector3 rotation)
        {
            UpperPoint = FixVector3.Rotate(RawUpperPoint, RotationOffset);
			LowerPoint = FixVector3.Rotate(RawLowerPoint, RotationOffset);
            UpperPoint = FixVector3.Transform(UpperPoint + PositionOffset, position, rotation);
			LowerPoint = FixVector3.Transform(LowerPoint + PositionOffset, position, rotation);
        }

        public override void UpdatePoints(FixVector3 position, FixVector3 rotation)
        {
            UpdateCylinderPoints(position, rotation);
            base.UpdatePoints(position, rotation);
        }
    }
}

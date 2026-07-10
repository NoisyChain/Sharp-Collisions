using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class CylinderCollider3D : SharpCollider3D
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

        [Export] private float _height
        {
            get =>(float)Fix64.FromRaw(raw_Height);
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_Height = ((Fix64)((decimal)value)).RawValue;
                    Height = Fix64.FromRaw(raw_Height);
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
        [Export] private long raw_height
        {
            get => raw_Height;
            set
            {
                raw_Height = value;
                Height = Fix64.FromRaw(raw_Height);
            }
        }

        private long raw_Radius;
        private long raw_Height;

        public Fix64 Radius = new Fix64();
        public Fix64 Height = new Fix64();
        public FixVector3 RawUpperPoint { get; private set; }
        public FixVector3 RawLowerPoint { get; private set; }
        public FixVector3 UpperPoint { get; private set; }
        public FixVector3 LowerPoint { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType3D.Cylinder;
            CreateCylinderPoints();
        }

        public override void DebugDrawShapes(SharpBody3D reference)
        {
            if (!DrawDebugShape) return;

            Vector3 DirY = (Vector3)FixVector3.Normalize(UpperPoint - LowerPoint);
            Vector3 DirX = SharpHelpers.GetLineNormal3D(DirY, (Vector3)reference.Forward, (Vector3)reference.Up);
            Vector3 DirZ = DirX.Cross(DirY);

            float inflatedRadius = (float)Radius + 0.005f;

            Vector3 LineSpacing1 = DirX * inflatedRadius;
            Vector3 LineSpacing2 = DirZ * inflatedRadius;

            DebugDraw3D.DrawCircle((Vector3)UpperPoint, DirX, DirZ, inflatedRadius, DebugShapeColor);
            DebugDraw3D.DrawCircle((Vector3)LowerPoint, DirX, DirZ, inflatedRadius, DebugShapeColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint + LineSpacing1, (Vector3)LowerPoint + LineSpacing1, DebugShapeColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint - LineSpacing1, (Vector3)LowerPoint - LineSpacing1, DebugShapeColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint + LineSpacing2, (Vector3)LowerPoint + LineSpacing2, DebugShapeColor);
            DebugDraw3D.DrawLine((Vector3)UpperPoint - LineSpacing2, (Vector3)LowerPoint - LineSpacing2, DebugShapeColor);
        }

        public override void DebugDrawShapesEditor(SharpBody3D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebugShape) return;

            Color finalColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            float scaledHeight = (float)Height;
            float scaledRadius = (float)Radius;

            Vector3 scaledPosOffset = (Vector3)PositionOffset;
            Vector3 scaledRotOffset = (Vector3)RotationOffset;

            Vector3 upPoint = scaledPosOffset + (Vector3.Up * scaledHeight);
            Vector3 lowPoint = scaledPosOffset - (Vector3.Up * scaledHeight);

            Vector3 upperPoint0 = SharpHelpers.Rotate3D(upPoint, scaledRotOffset);
            Vector3 lowerPoint0 = SharpHelpers.Rotate3D(lowPoint, scaledRotOffset);
            Vector3 upperPoint = SharpHelpers.Transform3D(upperPoint0, (Vector3)reference.FixedPosition, (Vector3)reference.FixedRotation);
            Vector3 lowerPoint = SharpHelpers.Transform3D(lowerPoint0, (Vector3)reference.FixedPosition, (Vector3)reference.FixedRotation);

            Vector3 DirY = (upperPoint - lowerPoint).Normalized();
            Vector3 DirX = SharpHelpers.GetLineNormal3D(DirY, (Vector3)reference.Forward, (Vector3)reference.Up);
            Vector3 DirZ = DirX.Cross(DirY);

            float inflatedRadius = scaledRadius + 0.005f;

            Vector3 LineSpacing1 = DirX * inflatedRadius;
            Vector3 LineSpacing2 = DirZ * inflatedRadius;

            DebugDraw3D.DrawCircle(upperPoint, DirX, DirZ, inflatedRadius, finalColor);
            DebugDraw3D.DrawCircle(lowerPoint, DirX, DirZ, inflatedRadius, finalColor);
            DebugDraw3D.DrawLine(upperPoint + LineSpacing1, lowerPoint + LineSpacing1, finalColor);
            DebugDraw3D.DrawLine(upperPoint - LineSpacing1, lowerPoint - LineSpacing1, finalColor);
            DebugDraw3D.DrawLine(upperPoint + LineSpacing2, lowerPoint + LineSpacing2, finalColor);
            DebugDraw3D.DrawLine(upperPoint - LineSpacing2, lowerPoint - LineSpacing2, finalColor);
        }

        public override void UpdateBoundingBox() 
        {
            BoundingBox = CollisionMath3D.UpdateCylinderBoundingBox(UpperPoint, LowerPoint, Radius);
        }

        private void CreateCylinderPoints()
        {
            FixVector3 CylinderDirection = new FixVector3(Fix64.Zero, Height, Fix64.Zero);

			RawUpperPoint = CylinderDirection;
			RawLowerPoint = -CylinderDirection;
        }

        private void UpdateCylinderPoints(FixVector3 position, FixVector3 rotation)
        {
            CreateCylinderPoints();
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

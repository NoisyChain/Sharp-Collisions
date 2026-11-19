using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool]
    [GlobalClass]
    public partial class CapsuleCollider3D : SharpCollider3D
    {
        [Export] private float _radius
        {
            get =>(float)Radius;
            set
            {
                if (Engine.IsEditorHint()) // Avoid any float values changing fixed point raw values when the game runs
                {
                    raw_Radius = ((Fix64)value).RawValue;
                    Radius = Fix64.FromRaw(raw_Radius);
                }
            }
        }

        [Export] private float _height
        {
            get =>(float)Height;
            set
            {
                if (Engine.IsEditorHint()) // Avoid any float values changing fixed point raw values when the game runs
                {
                    raw_Height = ((Fix64)value).RawValue;
                    Height = Fix64.FromRaw(raw_Height);
                }
            }
        }

        [ExportSubgroup("Raw Values")]
        [Export] private long raw_radius
        {
            get => Radius.RawValue;
            set
            {
                raw_Radius = value;
                Radius = Fix64.FromRaw(raw_Radius);
            }
        }
        [Export] private long raw_height
        {
            get => Height.RawValue;
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
        public FixVector3 UpperPoint { get; private set; }
        public FixVector3 LowerPoint { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType3D.Capsule;
            CreateCapsulePoints();
        }

        public override void DebugDrawShapes(SharpBody3D reference, bool selected)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;

            Color drawColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            float clampedHeight = Mathf.Max((float)Height, (float)Radius + 0.001f);

            float scaledHeight = clampedHeight;
            float scaledRadius = (float)Radius;

            Vector3 scaledPosOffset = (Vector3)PositionOffset;
            Vector3 scaledRotOffset = (Vector3)(RotationOffset * Fix64.DegToRad);

            Vector3 upPoint = scaledPosOffset + (Vector3.Up * (scaledHeight - scaledRadius));
            Vector3 lowPoint = scaledPosOffset - (Vector3.Up * (scaledHeight - scaledRadius));

            Vector3 upperPoint0 = SharpHelpers.Rotate3D(upPoint, scaledRotOffset);
            Vector3 lowerPoint0 = SharpHelpers.Rotate3D(lowPoint, scaledRotOffset);
            Vector3 upperPoint = SharpHelpers.Transform3D(upperPoint0, (Vector3)reference.FixedPosition, (Vector3)(reference.FixedRotation * Fix64.DegToRad));
            Vector3 lowerPoint = SharpHelpers.Transform3D(lowerPoint0, (Vector3)reference.FixedPosition, (Vector3)(reference.FixedRotation * Fix64.DegToRad));

            Vector3 DirY = (upperPoint - lowerPoint).Normalized();
            Vector3 DirX = SharpHelpers.GetLineNormal3D(DirY, (Vector3)reference.Forward, (Vector3)reference.Up);
            Vector3 DirZ = DirX.Cross(DirY);

            float inflatedRadius = scaledRadius + 0.005f;

            Vector3 LineSpacing1 = DirX * inflatedRadius;
            Vector3 LineSpacing2 = DirZ * inflatedRadius;

            if (Radius >= Height)
            {
                CustomDebugDraw.DrawSimpleSphere((upperPoint + lowerPoint) * 0.5f, DirX, DirY, DirZ, inflatedRadius, drawColor);
            }
            else
            {
                CustomDebugDraw.DrawHalfSphereY(upperPoint, DirX, DirY, DirZ, false, inflatedRadius, drawColor);
                CustomDebugDraw.DrawHalfSphereY(lowerPoint, DirX, DirY, DirZ, true, inflatedRadius, drawColor);
                DebugDraw3D.DrawLine(upperPoint + LineSpacing1, lowerPoint + LineSpacing1, drawColor);
                DebugDraw3D.DrawLine(upperPoint - LineSpacing1, lowerPoint - LineSpacing1, drawColor);
                DebugDraw3D.DrawLine(upperPoint + LineSpacing2, lowerPoint + LineSpacing2, drawColor);
                DebugDraw3D.DrawLine(upperPoint - LineSpacing2, lowerPoint - LineSpacing2, drawColor);
            }
        }

        public override void UpdateBoundingBox() 
        {
            BoundingBox = CollisionMath3D.UpdateCapsuleBoundingBox(UpperPoint, LowerPoint, Radius);
        }

        private void CreateCapsulePoints()
        {
            FixVector3 CapsuleDirection = new FixVector3(Fix64.Zero, Height - Radius, Fix64.Zero);

            UpperPoint = FixVector3.Rotate(CapsuleDirection, RotationOffset * Fix64.DegToRad);
            LowerPoint = FixVector3.Rotate(-CapsuleDirection, RotationOffset * Fix64.DegToRad);
        }

        private void UpdateCapsulePoints(FixVector3 position, FixVector3 rotation)
        {
            CreateCapsulePoints();
            
            UpperPoint = FixVector3.Transform(UpperPoint + PositionOffset, position, rotation);
            LowerPoint = FixVector3.Transform(LowerPoint + PositionOffset, position, rotation);
        }

        public override void UpdatePoints(FixVector3 position, FixVector3 rotation)
        {
            UpdateCapsulePoints(position, rotation);
            base.UpdatePoints(position, rotation);
        }
    }
}

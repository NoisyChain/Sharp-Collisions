using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool]
    [GlobalClass]
    public partial class CapsuleCollider2D : SharpCollider2D
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

        public FixVector2 UpperPoint { get; private set; }
        public FixVector2 LowerPoint { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType2D.Capsule;
            CreateCapsulePoints();
        }

        public override void DebugDrawShapes(SharpBody2D reference, bool selected)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;

            Color drawColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            float scaledHeight = (float)Height;
            float scaledRadius = (float)Radius;

            Vector2 PosOffset = (Vector2)PositionOffset;
            float RotOffset = (float)RotationOffset;

            Vector2 upPoint = PosOffset + (Vector2.Up * (scaledHeight - scaledRadius));
            Vector2 lowPoint = PosOffset - (Vector2.Up * (scaledHeight - scaledRadius));

            Vector2 upperPoint0 = SharpHelpers.Rotate2D(upPoint, RotOffset);
            Vector2 lowerPoint0 = SharpHelpers.Rotate2D(lowPoint, RotOffset);
            Vector2 upperPoint = SharpHelpers.Transform2D(upperPoint0, (Vector2)reference.FixedPosition, (float)reference.FixedRotation);
            Vector2 lowerPoint = SharpHelpers.Transform2D(lowerPoint0, (Vector2)reference.FixedPosition, (float)reference.FixedRotation);

            Vector2 direction = (upperPoint - lowerPoint).Normalized();

            float inflatedRadius = scaledRadius + 0.005f;

            Vector2 nor = SharpHelpers.GetNormal2D(upperPoint, lowerPoint);
            Vector3 LineNormal = new Vector3(nor.X, nor.Y, 0);
            Vector3 Dir = new Vector3(direction.X, direction.Y, 0);
            Vector3 LineSpacing = LineNormal * inflatedRadius;
            Vector3 Up = new Vector3(upperPoint.X, upperPoint.Y, 0);
            Vector3 Low = new Vector3(lowerPoint.X, lowerPoint.Y, 0);

            if (_radius >= _height)
            {
                CustomDebugDraw.DrawSimpleSphere((Up + Low) * 0.5f, LineNormal, Dir, Vector3.Zero, inflatedRadius, drawColor);
            }
            else
            {
                CustomDebugDraw.DrawHalfSphereY(Up, LineNormal, Dir, Vector3.Zero, false, inflatedRadius, drawColor);
                CustomDebugDraw.DrawHalfSphereY(Low, LineNormal, Dir, Vector3.Zero, true, inflatedRadius, drawColor);
                DebugDraw3D.DrawLine(Up, Low, drawColor);
                DebugDraw3D.DrawLine(Up + LineSpacing, Low + LineSpacing, drawColor);
                DebugDraw3D.DrawLine(Up - LineSpacing, Low - LineSpacing, drawColor);
            }
        }

        public override void UpdateBoundingBox()
		{
            BoundingBox = CollisionMath2D.UpdateCapsuleBoundingBox(UpperPoint, LowerPoint, Radius);
		}
        
        private void CreateCapsulePoints()
        {
            FixVector2 CapsuleDirection = new FixVector2(Fix64.Zero, Height - Radius);

            UpperPoint = FixVector2.Rotate(CapsuleDirection, RotationOffset);
            LowerPoint = FixVector2.Rotate(-CapsuleDirection, RotationOffset);
        }

        private void UpdateCapsulePoints(FixVector2 position, Fix64 rotation)
        {
            CreateCapsulePoints();
            
            UpperPoint = FixVector2.Transform(UpperPoint + PositionOffset, position, rotation);
            LowerPoint = FixVector2.Transform(LowerPoint + PositionOffset, position, rotation);
        }

        public override void UpdatePoints(FixVector2 position, Fix64 rotation)
        {
            UpdateCapsulePoints(position, rotation);
            base.UpdatePoints(position, rotation);
        }
    }
}

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

        public FixVector2 RawUpperPoint { get; private set; }
        public FixVector2 RawLowerPoint { get; private set; }
        public FixVector2 UpperPoint { get; private set; }
        public FixVector2 LowerPoint { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            Shape = CollisionType2D.Capsule;
            CreateCapsulePoints();
        }

        public override void DebugDrawShapes(SharpBody2D reference)
        {
            if (!Active) return;
            if (!DrawDebugShape) return;

            Vector3 Dir = (Vector3)FixVector2.Normalize(UpperPoint - LowerPoint);

            float inflatedRadius = (float)Radius + 0.005f;

            Vector3 LineNormal = (Vector3)FixVector2.GetNormal(UpperPoint, LowerPoint);
            Vector3 LineSpacing = LineNormal * inflatedRadius;

            if (Radius >= Height)
            {
                DebugDraw3D.DrawSimpleSphere((Vector3)(UpperPoint + LowerPoint) * 0.5f, LineNormal, Dir, Vector3.Zero, inflatedRadius, DebugShapeColor);
            }
            else
            {
                DebugDraw3D.DrawHalfSphereY((Vector3)UpperPoint, LineNormal, Dir, Vector3.Zero, false, inflatedRadius, DebugShapeColor);
                DebugDraw3D.DrawHalfSphereY((Vector3)LowerPoint, LineNormal, Dir, Vector3.Zero, true, inflatedRadius, DebugShapeColor);
                DebugDraw3D.DrawLine((Vector3)UpperPoint, (Vector3)LowerPoint, DebugShapeColor);
                DebugDraw3D.DrawLine((Vector3)UpperPoint + LineSpacing, (Vector3)LowerPoint + LineSpacing, DebugShapeColor);
                DebugDraw3D.DrawLine((Vector3)UpperPoint - LineSpacing, (Vector3)LowerPoint - LineSpacing, DebugShapeColor);
            }
        }

        public override void DebugDrawShapesEditor(SharpBody2D reference, bool selected)
        {
            if (!Active) return;
            if (!selected && !DrawDebugShape) return;

            Color finalColor = selected ? DebugShapeColorSelected : DebugShapeColor;

            float scaledHeight = (float)_height;
            float scaledRadius = (float)_radius;

            Vector2 PosOffset = _positionOffset;
            float RotOffset = _rotationOffset;

            Vector2 upPoint = PosOffset + (Vector2.Up * (scaledHeight - scaledRadius));
            Vector2 lowPoint = PosOffset - (Vector2.Up * (scaledHeight - scaledRadius));

            Vector2 upperPoint0 = SharpHelpers.Rotate2D(upPoint, Mathf.DegToRad(RotOffset));
            Vector2 lowerPoint0 = SharpHelpers.Rotate2D(lowPoint, Mathf.DegToRad(RotOffset));
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
                DebugDraw3D.DrawSimpleSphere((Up + Low) * 0.5f, LineNormal, Dir, Vector3.Zero, inflatedRadius, finalColor);
            }
            else
            {
                DebugDraw3D.DrawHalfSphereY(Up, LineNormal, Dir, Vector3.Zero, false, inflatedRadius, finalColor);
                DebugDraw3D.DrawHalfSphereY(Low, LineNormal, Dir, Vector3.Zero, true, inflatedRadius, finalColor);
                DebugDraw3D.DrawLine(Up, Low, finalColor);
                DebugDraw3D.DrawLine(Up + LineSpacing, Low + LineSpacing, finalColor);
                DebugDraw3D.DrawLine(Up - LineSpacing, Low - LineSpacing, finalColor);
            }
        }

        public override void UpdateBoundingBox()
		{
            BoundingBox = CollisionMath2D.UpdateCapsuleBoundingBox(UpperPoint, LowerPoint, Radius);
		}
        
        private void CreateCapsulePoints()
        {
            FixVector2 CapsuleDirection = new FixVector2(Fix64.Zero, Height - Radius);

            RawUpperPoint = CapsuleDirection;
            RawLowerPoint = -CapsuleDirection;
        }

        private void UpdateCapsulePoints(FixVector2 position, Fix64 rotation)
        {
            CreateCapsulePoints();
            UpperPoint = FixVector2.Rotate(RawUpperPoint, RotationOffset);
            LowerPoint = FixVector2.Rotate(RawLowerPoint, RotationOffset);
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

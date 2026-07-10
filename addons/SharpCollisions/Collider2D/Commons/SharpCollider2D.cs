using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
	[Tool] [GlobalClass]
	public partial class SharpCollider2D : Node
	{
		[Export] public bool Active = true;
		/// <summary>
        /// Don't use this variable in the simulation, its only purpose is to inject its value to the actual position
        /// </summary>
        [Export] protected Vector2 _positionOffset
        {
            get => new Vector2((float)Fix64.FromRaw(raw_PositionOffset_X), (float)Fix64.FromRaw(raw_PositionOffset_Y));
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_PositionOffset_X = ((Fix64)((decimal)value.X)).RawValue;
                    raw_PositionOffset_Y = ((Fix64)((decimal)value.Y)).RawValue;
					PositionOffset = new FixVector2(Fix64.FromRaw(raw_PositionOffset_X), Fix64.FromRaw(raw_PositionOffset_Y));
                }
            }
        }
		/// <summary>
        /// Don't use this variable in the simulation, its only purpose is to inject its value to the actual rotation
        /// </summary>
        [Export] protected float _rotationOffset
        {
            get => (float)Fix64.FromRaw(raw_RotationOffset);
            set
            {
                if (Engine.IsEditorHint()) // Avoid any float values changing fixed point raw values when the game runs
                {
                    raw_RotationOffset = ((Fix64)((decimal)value)).RawValue;
					RotationOffset = Fix64.FromRaw(raw_RotationOffset) * Fix64.DegToRad;
                }
            }
        }
		[Export] public bool IsTrigger = false;
		[Export] public bool TriggerDetectsSolidBodies = true;
		[Export] protected bool DrawDebugShape;
		[Export] public Color DebugShapeColor = new Color(0, 0, 1);
		[Export] public Color DebugShapeColorSelected = new Color(1, 0.6f, 0.1f);

		[ExportSubgroup("Raw Values")]
        [Export] private long raw_positionOffset_X
        {
            get => raw_PositionOffset_X;
            set
            {
                raw_PositionOffset_X = value;
                PositionOffset = new FixVector2(Fix64.FromRaw(raw_PositionOffset_X), Fix64.FromRaw(raw_PositionOffset_Y));
            }
        }
        [Export] private long raw_positionOffset_Y
        {
            get => raw_PositionOffset_Y;
            set
            {
                raw_PositionOffset_Y = value;
                PositionOffset = new FixVector2(Fix64.FromRaw(raw_PositionOffset_X), Fix64.FromRaw(raw_PositionOffset_Y));
            }
        }
        [Export] private long raw_rotationOffset
        {
            get => raw_RotationOffset;
            set
            {
                raw_RotationOffset = value;
               	RotationOffset = Fix64.FromRaw(raw_RotationOffset) * Fix64.DegToRad;
            }
        }

        private long raw_PositionOffset_X;
        private long raw_PositionOffset_Y;
        private long raw_RotationOffset;
		
		public CollisionFlags collisionFlags;
		public CollisionFlags globalCollisionFlags;
		public CollisionType2D Shape = CollisionType2D.Null;
		public FixVector2 Position;
		public FixVector2 PositionOffset;
		public Fix64 RotationOffset;
		public FixVector2 Center;
		public FixRect BoundingBox;

		public bool TriggerIgnoresSolid => IsTrigger && !TriggerDetectsSolidBodies;

		/*public SharpCollider2D(){}
		
		public SharpCollider2D(FixVector2 center, FixVector2 offset, FixVector2 size, FixVector2[] points, CollisionType shape)
		{
			Shape = shape;
			Position = center;
			Offset = offset;
			Radius = Fix64.Min(size.x, size.y) / Fix64.Two;
			Height = Fix64.Max(size.x, size.y) / Fix64.Two;
			Size = size;
			CreatePoints(points);
		}*/

		public virtual void Initialize()
		{
			/*PositionOffset = new FixVector2(
				(Fix64)startingPositionOffset.X  / SharpNode.NodeScale,
				(Fix64)startingPositionOffset.Y  / SharpNode.NodeScale
			);
			RotationOffset = (Fix64)startingRotationOffset / SharpNode.NodeRotation;
			RotationOffset *= Fix64.DegToRad;*/
		}

		public virtual void DebugDrawShapes(SharpBody2D reference)
		{

		}

		public virtual void DebugDrawShapesEditor(SharpBody2D reference, bool selected)
		{

		}
		
		public virtual void UpdateBoundingBox()
		{
			BoundingBox = new FixRect();
		}

		public virtual void UpdatePoints(FixVector2 position, Fix64 rotation)
		{
			Center = FixVector2.Transform(PositionOffset, position, rotation);
		}
	}
}

public enum CollisionType2D
{
	Null = -1,
	AABB = 0,
	Circle = 1,
	Capsule = 2,
	Polygon = 3,
}
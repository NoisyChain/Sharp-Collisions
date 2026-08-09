using Godot;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
	[Tool] [GlobalClass]
	public partial class SharpCollider3D : Node
	{
		[Export] public bool Active = true;
		
		/// <summary>
        /// Don't use this variable in the simulation, its only purpose is to inject its value to the actual position
        /// </summary>
        [Export] protected Vector3 _positionOffset
        {
            get => new Vector3((float)Fix64.FromRaw(raw_PositionOffset_X), (float)Fix64.FromRaw(raw_PositionOffset_Y), (float)Fix64.FromRaw(raw_PositionOffset_Z));
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_PositionOffset_X = ((Fix64)((decimal)value.X)).RawValue;
                    raw_PositionOffset_Y = ((Fix64)((decimal)value.Y)).RawValue;
                    raw_PositionOffset_Z = ((Fix64)((decimal)value.Z)).RawValue;
                    PositionOffset = new FixVector3(Fix64.FromRaw(raw_PositionOffset_X), Fix64.FromRaw(raw_PositionOffset_Y), Fix64.FromRaw(raw_PositionOffset_Z));
                }
            }
        }
		//[Export] protected Vector2I startingPositionOffset;
		/// <summary>
        /// Don't use this variable in the simulation, its only purpose is to inject its value to the actual rotation
        /// </summary>
        [Export] protected Vector3 _rotationOffset
        {
            get => new Vector3((float)Fix64.FromRaw(raw_RotationOffset_X), (float)Fix64.FromRaw(raw_RotationOffset_Y), (float)Fix64.FromRaw(raw_RotationOffset_Z));
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_RotationOffset_X = ((Fix64)((decimal)value.X)).RawValue;
                    raw_RotationOffset_Y = ((Fix64)((decimal)value.Y)).RawValue;
                    raw_RotationOffset_Z = ((Fix64)((decimal)value.Z)).RawValue;
                    RotationOffset = new FixVector3(Fix64.FromRaw(raw_RotationOffset_X), Fix64.FromRaw(raw_RotationOffset_Y), Fix64.FromRaw(raw_RotationOffset_Z)) * Fix64.DegToRad;
                }
            }
        }

		[Export] public bool IsTrigger = false;
		[Export] public bool TriggerDetectsSolidBodies = true;
		[Export] protected bool DrawDebugShape;
        [Export] protected bool DrawBoundingBox;
		[Export] public Color DebugShapeColor = new Color(0, 0, 1);
		[Export] public Color DebugShapeColorSelected = new Color(1, 0.6f, 0.1f);

		[ExportSubgroup("Raw Values")]
        [Export] private long raw_positionOffset_x
        {
            get => raw_PositionOffset_X;
            set
            {
                raw_PositionOffset_X = value;
                PositionOffset = new FixVector3(Fix64.FromRaw(raw_PositionOffset_X), Fix64.FromRaw(raw_PositionOffset_Y), Fix64.FromRaw(raw_PositionOffset_Z));
            }
        }
        [Export] private long raw_positionOffset_y
        {
            get => raw_PositionOffset_Y;
            set
            {
                raw_PositionOffset_Y = value;
                PositionOffset = new FixVector3(Fix64.FromRaw(raw_PositionOffset_X), Fix64.FromRaw(raw_PositionOffset_Y), Fix64.FromRaw(raw_PositionOffset_Z));
            }
        }
		[Export] private long raw_positionOffset_z
        {
            get => raw_PositionOffset_Z;
            set
            {
                raw_PositionOffset_Z = value;
                PositionOffset = new FixVector3(Fix64.FromRaw(raw_PositionOffset_X), Fix64.FromRaw(raw_PositionOffset_Y), Fix64.FromRaw(raw_PositionOffset_Z));
            }
        }
        [Export] private long raw_rotationOffset_x
        {
            get => raw_RotationOffset_X;
            set
            {
                raw_RotationOffset_X = value;
               	RotationOffset = new FixVector3(Fix64.FromRaw(raw_RotationOffset_X), Fix64.FromRaw(raw_RotationOffset_Y), Fix64.FromRaw(raw_RotationOffset_Z)) * Fix64.DegToRad;
            }
        }[Export] private long raw_rotationOffset_y
        {
            get => raw_RotationOffset_Y;
            set
            {
                raw_RotationOffset_Y = value;
               	RotationOffset = new FixVector3(Fix64.FromRaw(raw_RotationOffset_X), Fix64.FromRaw(raw_RotationOffset_Y), Fix64.FromRaw(raw_RotationOffset_Z)) * Fix64.DegToRad;
            }
        }
		[Export] private long raw_rotationOffset_z
        {
            get => raw_RotationOffset_Z;
            set
            {
                raw_RotationOffset_Z = value;
               	RotationOffset = new FixVector3(Fix64.FromRaw(raw_RotationOffset_X), Fix64.FromRaw(raw_RotationOffset_Y), Fix64.FromRaw(raw_RotationOffset_Z)) * Fix64.DegToRad;
            }
        }

        private long raw_PositionOffset_X;
        private long raw_PositionOffset_Y;
		private long raw_PositionOffset_Z;
        private long raw_RotationOffset_X;
		private long raw_RotationOffset_Y;
		private long raw_RotationOffset_Z;

		public CollisionType3D Shape = CollisionType3D.Null;

		public CollisionFlags collisionFlags;
		public CollisionFlags globalCollisionFlags;
		
		public FixVector3 Center;
		public FixVector3 PositionOffset;
		public FixVector3 RotationOffset;
		
		public FixVolume BoundingBox;

		public bool TriggerIgnoresSolid => IsTrigger && !TriggerDetectsSolidBodies;

		public virtual void Initialize() {}
		public virtual void DebugDrawShapes(SharpBody3D reference, bool selected) {}
		public virtual void UpdateBoundingBox() { BoundingBox = new FixVolume(); }
        public void DebugDrawBoundingBox()
        {
            if (!DrawBoundingBox) return;

            Vector3[] points =
            [
                new Vector3((float)BoundingBox.x,(float)BoundingBox.y, (float)BoundingBox.z),
                new Vector3((float)BoundingBox.w,(float)BoundingBox.y, (float)BoundingBox.z),
                new Vector3((float)BoundingBox.w,(float)BoundingBox.h, (float)BoundingBox.z),
                new Vector3((float)BoundingBox.x,(float)BoundingBox.h, (float)BoundingBox.z),
                new Vector3((float)BoundingBox.x,(float)BoundingBox.y, (float)BoundingBox.d),
                new Vector3((float)BoundingBox.w,(float)BoundingBox.y, (float)BoundingBox.d),
                new Vector3((float)BoundingBox.w,(float)BoundingBox.h, (float)BoundingBox.d),
                new Vector3((float)BoundingBox.x,(float)BoundingBox.h, (float)BoundingBox.d),
            ];

            //Draw Lower quad
            DebugDraw3D.DrawLine(points[0], points[1], Colors.Cyan);
            DebugDraw3D.DrawLine(points[1], points[2], Colors.Cyan);
            DebugDraw3D.DrawLine(points[2], points[3], Colors.Cyan);
            DebugDraw3D.DrawLine(points[3], points[0], Colors.Cyan);
            //Draw Upper quad
            DebugDraw3D.DrawLine(points[4], points[5], Colors.Cyan);
            DebugDraw3D.DrawLine(points[5], points[6], Colors.Cyan);
            DebugDraw3D.DrawLine(points[6], points[7], Colors.Cyan);
            DebugDraw3D.DrawLine(points[7], points[4], Colors.Cyan);
            //Connect both quads
            DebugDraw3D.DrawLine(points[0], points[4], Colors.Cyan);
            DebugDraw3D.DrawLine(points[1], points[5], Colors.Cyan);
            DebugDraw3D.DrawLine(points[2], points[6], Colors.Cyan);
            DebugDraw3D.DrawLine(points[3], points[7], Colors.Cyan);
        }

		public virtual void UpdatePoints(FixVector3 position, FixVector3 rotation)
		{
			Center = FixVector3.Transform(PositionOffset, position, rotation);
		}
	}

	public enum CollisionType3D
	{
		Null = -1,
		AABB = 0,
		Sphere = 1,
		Capsule = 2,
		Cylinder = 3,
		Polygon = 4,
	}
}
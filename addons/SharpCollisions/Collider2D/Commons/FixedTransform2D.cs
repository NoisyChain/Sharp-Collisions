using Godot;
using Godot.Collections;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class FixedTransform2D : SharpNode
    {
        [Export] protected Node3D Renderer3D;
        [Export] protected Node2D Renderer2D;
        
        /// <summary>
        /// Don't use this variable in the simulation, its only purpose is to inject its value to the actual position
        /// </summary>
        [Export] private Vector2 _fixedPosition
        {
            get => (Vector2)FixedPosition;
            set
            {
                if (Engine.IsEditorHint())
                {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_FixedPosition_X = ((Fix64)value.X).RawValue;
                    raw_FixedPosition_Y = ((Fix64)value.Y).RawValue;
                    FixedPosition = new FixVector2(Fix64.FromRaw(raw_FixedPosition_X), Fix64.FromRaw(raw_FixedPosition_Y));
                }
            }
        }

        /// <summary>
        /// Don't use this variable in the simulation, its only purpose is to inject its value to the actual rotation
        /// </summary>
        [Export] private float _fixedRotation
        {
            get => (float)FixedRotation;
            set
            {
                if (Engine.IsEditorHint()) // Avoid any float values changing fixed point raw values when the game runs
                {
                    raw_FixedRotation = ((Fix64)value).RawValue;
                    FixedRotation = Fix64.FromRaw(raw_FixedRotation);
                }
            }
        }
            
        [ExportSubgroup("Raw Values")]
        [Export] private long raw_fixedPosition_x
        {
            get => FixedPosition.x.RawValue;
            set
            {
                raw_FixedPosition_X = value;
                FixedPosition = new FixVector2(Fix64.FromRaw(raw_FixedPosition_X), Fix64.FromRaw(raw_FixedPosition_Y));
            }
        }
        [Export] private long raw_fixedPosition_y
        {
            get => FixedPosition.y.RawValue;
            set
            {
                raw_FixedPosition_Y = value;
                FixedPosition = new FixVector2(Fix64.FromRaw(raw_FixedPosition_X), Fix64.FromRaw(raw_FixedPosition_Y));
            }
        }
        [Export] private long raw_fixedRotation
        {
            get => FixedRotation.RawValue;
            set
            {
                raw_FixedRotation = value;
                FixedRotation = Fix64.FromRaw(raw_FixedRotation);
            }
        }

        private long raw_FixedPosition_X;
        private long raw_FixedPosition_Y;
        private long raw_FixedRotation;

        public FixVector2 FixedPosition = new FixVector2();
        public Fix64 FixedRotation = new Fix64();

        public FixVector2 Right => FixVector2.RotateDegrees(FixVector2.Right, FixedRotation);
        public FixVector2 Up => FixVector2.RotateDegrees(FixVector2.Up, FixedRotation);
        public FixVector2 Left => -Right;
        public FixVector2 Down => -Up;

        public override void _Instance()
        {
            base._Instance();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (Engine.IsEditorHint()) PreviewNode(true);
        }

        public override void RenderNode(bool debug)
        {
            if (Renderer3D != null)
            {
                Renderer3D.Visible = Active;
			
                Renderer3D.GlobalPosition = (Vector3)FixedPosition;
                Renderer3D.GlobalRotationDegrees = new Vector3(0, 0, (float)FixedRotation);
            }

            if (Renderer2D != null)
            {
                Renderer2D.Visible = Active;
			
                Renderer2D.GlobalPosition = (Vector2)FixedPosition;
                Renderer2D.GlobalRotationDegrees = (float)FixedRotation;
            }
        }

        public override void PreviewNode(bool debug)
        {
            if (Renderer3D != null)
            {
                Renderer3D.Visible = Active;

                Renderer3D.GlobalPosition = new Vector3(_fixedPosition.X, _fixedPosition.Y, 0);
                Renderer3D.GlobalRotationDegrees = new Vector3(0, 0, _fixedRotation);
            }

            if (Renderer2D != null)
            {
                Renderer2D.Visible = Active;

                Renderer2D.GlobalPosition = new Vector2(_fixedPosition.X, _fixedPosition.Y);
                Renderer2D.GlobalRotationDegrees = _fixedRotation;
            }
        }

        public static FixVector2 LocalToWorld(FixVector2 v)
        {
            return v;
        }

        public static FixVector2 WorldToLocal(FixVector2 v)
        {
            return v;
        }
    }
}

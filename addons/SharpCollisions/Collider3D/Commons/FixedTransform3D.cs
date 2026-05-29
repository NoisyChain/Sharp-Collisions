using Godot;
using FixMath.NET;

/// NOTE: Parenting support is cancelled
/// It might be too complicated for me to do it properly right now
/// Feel free to finish my work if you want lol
namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class FixedTransform3D : SharpNode
    {
        [Export] protected Node3D Renderer;
        //public FixedTransform3D Parent;
        //[Export] public FixedTransform3D[] Children;
        //[Export] public Vector3I localFixedPosition;
        //[Export] public Vector3I localFixedRotation;

        /// <summary>
        /// Don't use this variable in the simulation, its only purpose is to inject its value to the actual position
        /// </summary>
        [Export] private Vector3 _fixedPosition
        {
            get => new Vector3((float)Fix64.FromRaw(raw_FixedPosition_X), (float)Fix64.FromRaw(raw_FixedPosition_Y), (float)Fix64.FromRaw(raw_FixedPosition_Z));
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_FixedPosition_X = ((Fix64)((decimal)value.X)).RawValue;
                    raw_FixedPosition_Y = ((Fix64)((decimal)value.Y)).RawValue;
                    raw_FixedPosition_Z = ((Fix64)((decimal)value.Z)).RawValue;
                    FixedPosition = new FixVector3(Fix64.FromRaw(raw_FixedPosition_X), Fix64.FromRaw(raw_FixedPosition_Y), Fix64.FromRaw(raw_FixedPosition_Z));
                }
            }
        }

        /// <summary>
        /// Don't use this variable in the simulation, its only purpose is to inject its value to the actual rotation
        /// </summary>
        [Export] private Vector3 _fixedRotation
        {
            get => new Vector3((float)Fix64.FromRaw(raw_FixedRotation_X), (float)Fix64.FromRaw(raw_FixedRotation_Y), (float)Fix64.FromRaw(raw_FixedRotation_Z));
            set {
                if (Engine.IsEditorHint()) {  // Avoid any float values changing fixed point raw values when the game runs
                    raw_FixedRotation_X = ((Fix64)((decimal)value.X)).RawValue;
                    raw_FixedRotation_Y = ((Fix64)((decimal)value.Y)).RawValue;
                    raw_FixedRotation_Z = ((Fix64)((decimal)value.Z)).RawValue;
                    FixedRotation = new FixVector3(Fix64.FromRaw(raw_FixedRotation_X), Fix64.FromRaw(raw_FixedRotation_Y), Fix64.FromRaw(raw_FixedRotation_Z)) * Fix64.DegToRad;
                }
            }
        }
            
        [ExportSubgroup("Raw Values")]
        [Export] private long raw_fixedPosition_x
        {
            get => raw_FixedPosition_X;
            set
            {
                raw_FixedPosition_X = value;
                FixedPosition = new FixVector3(Fix64.FromRaw(raw_FixedPosition_X), Fix64.FromRaw(raw_FixedPosition_Y), Fix64.FromRaw(raw_FixedPosition_Z));
            }
        }
        [Export] private long raw_fixedPosition_y
        {
            get => raw_FixedPosition_Y;
            set
            {
                raw_FixedPosition_Y = value;
                FixedPosition = new FixVector3(Fix64.FromRaw(raw_FixedPosition_X), Fix64.FromRaw(raw_FixedPosition_Y), Fix64.FromRaw(raw_FixedPosition_Z));
            }
        }
        [Export] private long raw_fixedPosition_z
        {
            get => raw_FixedPosition_Z;
            set
            {
                raw_FixedPosition_Z = value;
                FixedPosition = new FixVector3(Fix64.FromRaw(raw_FixedPosition_X), Fix64.FromRaw(raw_FixedPosition_Y), Fix64.FromRaw(raw_FixedPosition_Z));
            }
        }
        [Export] private long raw_fixedRotation_x
        {
            get => raw_FixedRotation_X;
            set
            {
                raw_FixedRotation_X = value;
                FixedRotation = new FixVector3(Fix64.FromRaw(raw_FixedRotation_X), Fix64.FromRaw(raw_FixedRotation_Y), Fix64.FromRaw(raw_FixedRotation_Z)) * Fix64.DegToRad;
            }
        }
        [Export] private long raw_fixedRotation_y
        {
            get => raw_FixedRotation_Y;
            set
            {
                raw_FixedRotation_Y = value;
                FixedRotation = new FixVector3(Fix64.FromRaw(raw_FixedRotation_X), Fix64.FromRaw(raw_FixedRotation_Y), Fix64.FromRaw(raw_FixedRotation_Z)) * Fix64.DegToRad;
            }
        }
        [Export] private long raw_fixedRotation_z
        {
            get => raw_FixedRotation_Z;
            set
            {
                raw_FixedRotation_Z = value;
                FixedRotation = new FixVector3(Fix64.FromRaw(raw_FixedRotation_X), Fix64.FromRaw(raw_FixedRotation_Y), Fix64.FromRaw(raw_FixedRotation_Z)) * Fix64.DegToRad;
            }
        }

        private long raw_FixedPosition_X;
        private long raw_FixedPosition_Y;
        private long raw_FixedPosition_Z;
        private long raw_FixedRotation_X;
        private long raw_FixedRotation_Y;
        private long raw_FixedRotation_Z;
        public FixVector3 FixedPosition = new FixVector3();
        public FixVector3 FixedRotation = new FixVector3();
        //public FixVector3 LocalFixedPosition;
        //public FixVector3 LocalFixedRotation;

        public FixVector3 Right => FixVector3.Rotate(FixVector3.Right, FixedRotation);
        public FixVector3 Up => FixVector3.Rotate(FixVector3.Up, FixedRotation);
        public FixVector3 Forward => FixVector3.Rotate(FixVector3.Forward, FixedRotation);
        public FixVector3 Left => -Right;
        public FixVector3 Down => -Up;
        public FixVector3 Back => -Forward;

        public override void _Instance()
        {
            base._Instance();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (Engine.IsEditorHint()) PreviewNode(true);
        }

        /*public bool HasChildren()
        {
            return Children != null && Children.Length >= 0;
        }

        public bool HasParent()
        {
            return Parent != null;
        }*/

        public override void RenderNode(bool debug)
        {
            if (Renderer == null) return;

            Renderer.Visible = Active;
			
			Renderer.GlobalPosition = (Vector3)FixedPosition;
			Renderer.GlobalRotation = (Vector3)FixedRotation;
        }

        public override void PreviewNode(bool debug)
        {
            if (Renderer == null) return;

            Renderer.Visible = Active;

			Renderer.GlobalPosition = _fixedPosition;
			Renderer.GlobalRotationDegrees = _fixedRotation;
        }

        /*public void SetParent(FixedTransform3D newParent)
        {
            if (newParent == null)
            {
                //GetParent<Node>().RemoveChild(this);
                Parent = null;

                FixedPosition = LocalFixedPosition;
                LocalFixedPosition = FixVector3.Zero;
                FixedRotation = LocalFixedRotation;
                LocalFixedRotation = FixVector3.Zero;
            }
            else
            {
                //GetParent<Node>().RemoveChild(this);
                //newParent.AddChild(this);
                Parent = newParent;

                LocalFixedPosition += FixedPosition;
                FixedPosition = Parent.FixedPosition;
                LocalFixedRotation += FixedRotation;
                FixedRotation = Parent.FixedRotation;
            }
        }

        public void UpdateParenting()
        {
            if (!HasChildren()) return;

            foreach(FixedTransform3D child in Children)
            {
                child.FixedRotation = FixedRotation;
                //FixVector3 vec = FixVector3.Transform(child.FixedPosition, FixedPosition + child.LocalFixedPosition, FixedRotation);
                child.FixedPosition = FixedPosition;
            }
        }*/

        //public FixVector3 TotalFixedRotation => FixedRotation + LocalFixedRotation;
        //public FixVector3 TotalFixedPosition => FixVector3.Transform(LocalFixedPosition, FixedPosition, FixedRotation);

        public static FixVector3 LocalToWorld(FixVector3 v)
        {
            return v;
        }

        public static FixVector3 WorldToLocal(FixVector3 v)
        {
            return v;
        }
    }
}

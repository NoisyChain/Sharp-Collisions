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
        [Export] private Node3D Renderer;
        //public FixedTransform3D Parent;
        //[Export] public FixedTransform3D[] Children;
        [Export] public Vector3I fixedPosition;
        [Export] public Vector3I fixedRotation;
        //[Export] public Vector3I localFixedPosition;
        //[Export] public Vector3I localFixedRotation;
        public FixVector3 FixedPosition;
        public FixVector3 FixedRotation;
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

            FixedPosition = new FixVector3(
                (Fix64)fixedPosition.X / NodeScale,
                (Fix64)fixedPosition.Y / NodeScale,
                (Fix64)fixedPosition.Z / NodeScale
            );

            FixedRotation = new FixVector3(
                (Fix64)fixedRotation.X / NodeRotation,
                (Fix64)fixedRotation.Y / NodeRotation,
                (Fix64)fixedRotation.Z / NodeRotation
            );
            FixedRotation *= Fix64.DegToRad;
            /*LocalFixedPosition = new FixVector3(
                (Fix64)localFixedPosition.X / NodeScale,
                (Fix64)localFixedPosition.Y / NodeScale,
                (Fix64)localFixedPosition.Z / NodeScale
            );

            LocalFixedRotation = new FixVector3(
                (Fix64)localFixedRotation.X / NodeRotation,
                (Fix64)localFixedRotation.Y / NodeRotation,
                (Fix64)localFixedRotation.Z / NodeRotation
            );
            LocalFixedRotation *= Fix64.DegToRad;

            if (HasChildren())
            {
                foreach(FixedTransform3D child in Children)
                {
                    child.SetParent(this);
                }
            }*/
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (Engine.IsEditorHint()) PreviewNode();
        }

        /*public bool HasChildren()
        {
            return Children != null && Children.Length >= 0;
        }

        public bool HasParent()
        {
            return Parent != null;
        }*/

        public override void RenderNode()
        {
            if (Renderer == null) return;

            Renderer.Visible = Active;
			
			Renderer.GlobalPosition = (Vector3)FixedPosition;
			Renderer.GlobalRotation = (Vector3)FixedRotation;
        }

        public override void PreviewNode()
        {
            if (Renderer == null) return;

            Renderer.Visible = Active;

			Renderer.GlobalPosition = (Vector3)fixedPosition / nodeScale;
			Renderer.GlobalRotationDegrees = (Vector3)fixedRotation / nodeRotation;
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

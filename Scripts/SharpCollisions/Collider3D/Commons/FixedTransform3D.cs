using Godot;
using Godot.Collections;
using FixMath.NET;

namespace SharpCollisions.Sharp3D
{
    [Tool] [GlobalClass]
    public partial class FixedTransform3D : SharpNode
    {
        public FixedTransform3D Parent;
        public Array<FixedTransform3D> Children;
        [Export] public Vector3I fixedPosition;
        [Export] public Vector3I fixedRotation;
        public FixVector3 FixedPosition;
        public FixVector3 FixedRotation;
        public FixVector3 LocalFixedPosition;
        public FixVector3 LocalFixedRotation;

        public FixVector3 Right => FixVector3.Rotate(FixVector3.Right, FixedRotation);
        public FixVector3 Up => FixVector3.Rotate(FixVector3.Up, FixedRotation);
        public FixVector3 Forward => FixVector3.Rotate(FixVector3.Forward, FixedRotation);
        public FixVector3 Left => -Right;
        public FixVector3 Down => -Up;
        public FixVector3 Back => -Forward;

        public override void _Ready()
        {
            base._Ready();

            FixedPosition = new FixVector3(
                (Fix64)fixedPosition.X / convertedScale,
                (Fix64)fixedPosition.Y / convertedScale,
                (Fix64)fixedPosition.Z / convertedScale
            );

            FixedRotation = new FixVector3(
                (Fix64)fixedRotation.X / convertedScale,
                (Fix64)fixedRotation.Y / convertedScale,
                (Fix64)fixedRotation.Z / convertedScale
            );
            //FixedPosition = (FixVector3)GlobalPosition;
            //FixedRotation = (FixVector3)GlobalRotation;
            //Parent = GetParent<Node3D>() as FixedTransform3D;
            //GD.Print(Parent != null ? Parent.Name : "No parent found.");
        }

        public override void _Process(double delta)
        {
            if (Engine.IsEditorHint()) return;

            //GlobalPosition = (Vector3)FixedPosition;
            //GlobalRotation = (Vector3)FixedRotation;
        }

        /*public override void _FixedPreProcess(Fix64 delta)
        {
            TransformWithParent();
        }*/

        public void SetParent(FixedTransform3D newParent)
        {
            if (newParent == null)
            {
                GetParent<Node3D>().RemoveChild(this);
                Parent = null;
            }
            else
            {
                GetParent<Node3D>().RemoveChild(this);
                newParent.AddChild(this);
                Parent = newParent;
            }
        }

        public void TransformWithParent() //This is broken lol
        {
            if (Parent == null) return;

            //FixVector3 localRotation = FixedRotation - Parent.FixedRotation;
            //FixedRotation = Parent.FixedRotation + localRotation;
            //FixedPosition = FixVector3.Transform(FixedPosition, Parent.FixedPosition, Parent.FixedRotation);
        }

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

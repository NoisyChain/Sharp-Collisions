using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    [Tool]
    public partial class FixedTransform3D : SharpNode
    {
        public FixVector3 FixedPosition;
        public FixVector3 FixedRotation;

        public FixVector3 Right => FixVector3.Rotate(FixVector3.Right, FixedRotation);
        public FixVector3 Up => FixVector3.Rotate(FixVector3.Up, FixedRotation);
        public FixVector3 Forward => FixVector3.Rotate(FixVector3.Forward, FixedRotation);
        public FixVector3 Left => -Right;
        public FixVector3 Down => -Up;
        public FixVector3 Back => -Forward;

        public override void _Ready()
        {
            base._Ready();

            FixedPosition = (FixVector3)GlobalPosition;
            FixedRotation = (FixVector3)GlobalRotation;
        }

        public override void _Process(double delta)
        {
            if (Engine.IsEditorHint()) return;

            GlobalPosition = (Vector3)FixedPosition;
            GlobalRotation = (Vector3)FixedRotation;
        }
    }
}

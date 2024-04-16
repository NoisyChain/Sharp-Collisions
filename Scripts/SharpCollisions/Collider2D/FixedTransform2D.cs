using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    [Tool]
    public partial class FixedTransform2D : SharpNode
    {
        public FixVector2 FixedPosition;
        public Fix64 FixedRotation;

        public FixVector2 Right => FixVector2.Rotate(FixVector2.Right, FixedRotation);
        public FixVector2 Up => FixVector2.Rotate(FixVector2.Up, FixedRotation);
        public FixVector2 Left => -Right;
        public FixVector2 Down => -Up;

        public override void _Ready()
        {
            base._Ready();

            FixedPosition = (FixVector2)GlobalPosition;
            FixedRotation = (Fix64)GlobalRotation.Z;
        }

        public override void _Process(double delta)
        {
            if (Engine.IsEditorHint()) return;

            GlobalPosition = (Vector3)FixedPosition;
            GlobalRotation = new Vector3(0, 0, (float)FixedRotation);
        }
    }
}

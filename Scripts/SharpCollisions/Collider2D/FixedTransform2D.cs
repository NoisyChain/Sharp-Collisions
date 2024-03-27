using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    [Tool]
    public class FixedTransform2D : Spatial
    {
        public FixVector2 FixedPosition;
        public Fix64 FixedRotation;

        public FixVector2 Right => FixVector2.Rotate(FixVector2.Right, FixedRotation);
        public FixVector2 Up => FixVector2.Rotate(FixVector2.Up, FixedRotation);
        public FixVector2 Left => -Right;
        public FixVector2 Down => -Up;

        protected PhysicsManager2D Manager;

        public override void _Ready()
        {
            if (Engine.EditorHint) return;

            FixedPosition = (FixVector2)GlobalTranslation;
            FixedRotation = (Fix64)GlobalRotation.z;
            Manager = GetTree().Root.GetNode<PhysicsManager2D>("Main/PhysicsManager");
            Manager.AddTransform(this);
        }

        public override void _Process(float delta)
        {
            if (Engine.EditorHint) return;

            GlobalTranslation = (Vector3)FixedPosition;
            GlobalRotation = new Vector3(0, 0, (float)FixedRotation);
        }

        /// <summary>
        /// Use this function if you want to execute your logic inside the physics loop
        /// </summary>
        /// <param name="delta"></param>
        public virtual void _FixedProcess(Fix64 delta) { }

        public virtual void _Destroy()
        {
            if (Manager.RemoveTransform(this))
                QueueFree();
        }
    }
}

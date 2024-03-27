using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    [Tool]
    public class FixedTransform3D : Spatial
    {
        public FixVector3 FixedPosition;
        public FixVector3 FixedRotation;

        public FixVector3 Right => FixVector3.Rotate(FixVector3.Right, FixedRotation);
        public FixVector3 Up => FixVector3.Rotate(FixVector3.Up, FixedRotation);
        public FixVector3 Forward => FixVector3.Rotate(FixVector3.Forward, FixedRotation);
        public FixVector3 Left => -Right;
        public FixVector3 Down => -Up;
        public FixVector3 Back => -Forward;

        protected PhysicsManager3D Manager;

        public override void _Ready()
        {
            if (Engine.EditorHint) return;

            FixedPosition = (FixVector3)GlobalTranslation;
            FixedRotation = (FixVector3)GlobalRotation;
            Manager = GetTree().Root.GetNode<PhysicsManager3D>("Main/PhysicsManager");
            Manager.AddTransform(this);
        }

        public override void _Process(float delta)
        {
            if (Engine.EditorHint) return;

            GlobalTranslation = (Vector3)FixedPosition;
            GlobalRotation = (Vector3)FixedRotation;
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

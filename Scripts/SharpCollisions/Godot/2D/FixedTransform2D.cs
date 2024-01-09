using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    [Tool]
    public class FixedTransform2D : Spatial
    {
        [Export] protected Vector2 position;
        [Export] protected float rotation;

        public FixVector2 Position;
        new public Fix64 Rotation;

        public FixVector2 Right => FixVector2.Rotate(FixVector2.Right, Rotation);
        public FixVector2 Up => FixVector2.Rotate(FixVector2.Up, Rotation);
        public FixVector2 Left => -Right;
        public FixVector2 Down => -Up;

        protected PhysicsManager2D Manager;

        public override void _Ready()
        {
            if (!Engine.EditorHint)
            {
                Position = (FixVector2)position;
                Rotation = (Fix64)rotation * Fix64.DegToRad;
                Manager = GetTree().Root.GetNode<PhysicsManager2D>("Main/PhysicsManager");
                Manager.AddBody(this);
            }
        }

        public override void _Process(float delta)
        {
            if (Engine.EditorHint)
            {
                GlobalTranslation = new Vector3(position.x, position.y, 0f);
                GlobalRotation = new Vector3(0, 0, Mathf.Deg2Rad(rotation));
            }
            else
            {
                GlobalTranslation = (Vector3)Position;
                GlobalRotation = new Vector3(0, 0, (float)Rotation);
            }
        }

        /// <summary>
        /// Use this function if you want to execute your logic inside the physics loop
        /// </summary>
        /// <param name="delta"></param>
        public virtual void _FixedProcess(Fix64 delta) { }

        public void _Destroy()
        {
            if (Manager.RemoveBody(this))
                QueueFree();
        }
    }
}

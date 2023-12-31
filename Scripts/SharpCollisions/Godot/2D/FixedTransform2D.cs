using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    public class FixedTransform2D : Node
    {
        [Export] public bool Visible = true;
        [Export] protected Vector2 position
        {
            get => (Vector2)Position;
            set => Position = (FixVector2)value;
        }
        [Export] protected float rotation
        {
            get => (float)(Rotation * Fix64.RadToDeg);
            set => Rotation = (Fix64)value * Fix64.DegToRad;
        }
        public FixVector2 Position;
        public Fix64 Rotation;

		public FixVector2 Right => FixVector2.Rotate(FixVector2.Right, Rotation);
		public FixVector2 Up => FixVector2.Rotate(FixVector2.Up, Rotation);
		public FixVector2 Left => -Right;
		public FixVector2 Down => -Up;

        protected Spatial Viewer;
        protected PhysicsManager2D Manager;

        public override void _Ready()
        {
            Manager = GetTree().Root.GetNode<PhysicsManager2D>("Main/PhysicsManager");
			Manager.AddBody(this);
            Viewer = GetNode<Spatial>("Viewer");
        }

        public override void _Process(float delta)
        {
            if (Viewer != null)
            {
                Viewer.Visible = Visible;
                Viewer.GlobalTranslation = new Vector3((float)Position.x, (float)Position.y, 0);
                Viewer.GlobalRotation = new Vector3(0, 0, (float)Rotation);
            }
        }

        public virtual void _FixedProcess(Fix64 delta)
        {

        }

        public void _Destroy()
        {
            if (Manager.RemoveBody(this))
                QueueFree();
        }
    }
}

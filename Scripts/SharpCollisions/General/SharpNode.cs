using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    [Tool]
    public partial class SharpNode : Node3D
    {
        protected PhysicsManager Manager;
        public override void _Ready()
        {
            if (Engine.IsEditorHint()) return;
            Manager = GetTree().Root.GetNode<PhysicsManager>("Main/PhysicsManager");
            Manager.AddNode(this);
        }

        /// <summary>
        /// Use this function if you want to execute your logic before the physics loop.
        /// </summary>
        /// <param name="delta">Fixed point delta time.</param>
        public virtual void _FixedPreProcess(Fix64 delta) { }

        /// <summary>
        /// Use this function if you want to execute your logic inside the physics loop.
        /// </summary>
        /// <param name="delta">Fixed point delta time.</param>
        public virtual void _FixedProcess(Fix64 delta) { }

        /// <summary>
        /// Use this function if you want to execute your logic after the physics loop.
        /// </summary>
        /// <param name="delta">Fixed point delta time.</param>
        public virtual void _FixedPostProcess(Fix64 delta) { }

        public virtual void _Destroy()
        {
            if (Manager.RemoveNode(this))
                QueueFree();
        }
    }
}

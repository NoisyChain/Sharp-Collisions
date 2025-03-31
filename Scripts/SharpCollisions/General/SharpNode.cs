using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    [Tool]
    public partial class SharpNode : Node
    {
        [Export] public bool Active = true;
        public static int nodeScale = 10000;
        public static Fix64 convertedScale => (Fix64)nodeScale;
        public override void _Ready()
        {
            if (Engine.IsEditorHint()) return;
            SharpManager.Instance.AddNode(this);
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

        public virtual void RenderNode() {}

        public virtual void _Destroy()
        {
            if (SharpManager.Instance.RemoveNode(this))
                QueueFree();
        }
    }
}

using Godot;
using FixMath.NET;

namespace SharpCollisions
{
    [Tool]
    public partial class SharpNode : Node
    {
        [Export] public bool Active = true;
        [Export] public bool AutoStart = true;
        public static int nodeScale = 1000;
        public static int nodeRotation = 10;
        public static Fix64 NodeScale => (Fix64)nodeScale;
        public static Fix64 NodeRotation => (Fix64)nodeRotation;
        public override void _Ready()
        {
            if (!AutoStart) return;

            if (Engine.IsEditorHint()) return;
            _Instance();
        }

        public virtual void _Instance()
        {
            SharpManager.Instance.AddNode(this);
        }

        /// <summary>
        /// Use this function if you want to execute your logic before the physics loop.
        /// </summary>
        /// <param name="delta">Fixed point delta time.</param>
        public virtual void _FixedPreProcess(Fix64 delta) {}

        /// <summary>
        /// Use this function if you want to execute your logic inside the physics loop.
        /// </summary>
        /// <param name="delta">Fixed point delta time.</param>
        public virtual void _FixedProcess(Fix64 delta) {}

        /// <summary>
        /// Use this function if you want to execute your logic after the physics loop.
        /// </summary>
        /// <param name="delta">Fixed point delta time.</param>
        public virtual void _FixedPostProcess(Fix64 delta) {}

        public virtual void RenderNode() {}

        public virtual void PreviewNode() {}

        public virtual void _Destroy()
        {
            if (SharpManager.Instance.RemoveNode(this))
                QueueFree();
        }
    }
}

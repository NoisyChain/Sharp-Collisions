using Godot;
using Godot.Collections;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class FixedTransform2D : SharpNode
    {
        private ISharpRenderer Renderer;
        public FixedTransform2D Parent;
        public Array<FixedTransform2D> Children;
        [Export] public Vector2I fixedPosition;
        [Export] public int fixedRotation;
        public FixVector2 FixedPosition;
        public Fix64 FixedRotation;
        public FixVector2 LocalFixedPosition;
        public Fix64 LocalFixedRotation;

        public FixVector2 Right => FixVector2.Rotate(FixVector2.Right, FixedRotation);
        public FixVector2 Up => FixVector2.Rotate(FixVector2.Up, FixedRotation);
        public FixVector2 Left => -Right;
        public FixVector2 Down => -Up;

        public override void _Ready()
        {
            base._Ready();
            //Renderer = this.GetNode("Graphics") as ISharpRenderer;
            FixedPosition = new FixVector2(
                (Fix64)fixedPosition.X / convertedScale,
                (Fix64)fixedPosition.Y / convertedScale
            );
            FixedRotation = (Fix64)fixedRotation / convertedScale;
            FixedRotation *= Fix64.DegToRad;

            //Parent = GetParent<Node3D>() as FixedTransform2D;
            //GD.Print(Parent != null ? Parent.Name : "No parent found.");
        }

        public void SetRenderer(ISharpRenderer rend) { Renderer = rend; }

        public override void RenderNode()
        {
            if (Renderer != null) Renderer.Render();
        }

        public void SetParent(FixedTransform2D newParent)
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

            //Fix64 localRotation = FixedRotation - Parent.FixedRotation;
            //FixedRotation = Parent.FixedRotation + localRotation;
            //FixedPosition = FixVector2.Transform(FixedPosition, Parent.FixedPosition, Parent.FixedRotation);
        }

        public static FixVector2 LocalToWorld(FixVector2 v)
        {
            return v;
        }

        public static FixVector2 WorldToLocal(FixVector2 v)
        {
            return v;
        }
    }
}

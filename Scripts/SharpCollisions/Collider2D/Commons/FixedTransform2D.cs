using Godot;
using Godot.Collections;
using FixMath.NET;

namespace SharpCollisions.Sharp2D
{
    [Tool] [GlobalClass]
    public partial class FixedTransform2D : SharpNode
    {
        [Export] private Node3D Renderer;
        //[Export] private Node2D Renderer2D;
        
        [Export] public Vector2I fixedPosition;
        [Export] public int fixedRotation;
        public FixVector2 FixedPosition;
        public Fix64 FixedRotation;

        public FixVector2 Right => FixVector2.Rotate(FixVector2.Right, FixedRotation);
        public FixVector2 Up => FixVector2.Rotate(FixVector2.Up, FixedRotation);
        public FixVector2 Left => -Right;
        public FixVector2 Down => -Up;

        public override void _Ready()
        {
            base._Ready();

            FixedPosition = new FixVector2(
                (Fix64)fixedPosition.X / NodeScale,
                (Fix64)fixedPosition.Y / NodeScale
            );
            FixedRotation = (Fix64)fixedRotation / NodeRotation;
            FixedRotation *= Fix64.DegToRad;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (Engine.IsEditorHint()) PreviewNode();
        }

        public override void RenderNode()
        {
            if (Renderer == null) return;

            Renderer.Visible = Active;
			
			Renderer.GlobalPosition = (Vector3)FixedPosition;
			Renderer.GlobalRotation = new Vector3(0, 0, (float)FixedRotation);
        }

        public override void PreviewNode()
        {
            if (Renderer == null) return;

            Renderer.Visible = Active;

			Renderer.GlobalPosition = new Vector3(
				fixedPosition.X / (float)nodeScale,
				fixedPosition.Y / (float)nodeScale,
				0
			);
			Renderer.GlobalRotationDegrees = new Vector3(0,	0,	fixedRotation / (float)NodeRotation);
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

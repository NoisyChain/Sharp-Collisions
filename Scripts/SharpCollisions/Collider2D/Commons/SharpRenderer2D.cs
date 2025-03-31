using Godot;

namespace SharpCollisions.Sharp2D
{
	[Tool] [GlobalClass]
	public partial class SharpRenderer2D : Node2D, ISharpRenderer
	{
		[Export] private FixedTransform2D reference;

		//God forbid referencing an interface from the editor...
		public override void _Ready()
        {
            base._Ready();
			reference.SetRenderer(this);
        }

		public void Render()
		{
			if (reference == null) return;

			Visible = reference.Active;
			
			GlobalPosition = (Vector2)reference.FixedPosition;
			GlobalRotation = (float)reference.FixedRotation;
		}

		public void Preview()
		{
			if (reference == null) return;

			Visible = reference.Active;

			GlobalPosition = new Vector2(
				reference.fixedPosition.X / (float)SharpNode.nodeScale,
				reference.fixedPosition.Y / (float)SharpNode.nodeScale
			);
			GlobalRotationDegrees = reference.fixedRotation / (float)SharpNode.nodeScale;
		}
	}
}

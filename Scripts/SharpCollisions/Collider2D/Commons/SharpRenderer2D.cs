using Godot;

namespace SharpCollisions.Sharp2D
{
	[Tool] [GlobalClass]
	public partial class SharpRenderer2D : Node2D
	{
		[Export] private FixedTransform2D reference;

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (reference == null) return;

			Visible = reference.Active;
			
			if (Engine.IsEditorHint())
			{
				GlobalPosition = new Vector2(
					reference.fixedPosition.X / (float)SharpNode.nodeScale,
					reference.fixedPosition.Y / (float)SharpNode.nodeScale
				);
				GlobalRotationDegrees = reference.fixedRotation / (float)SharpNode.nodeScale;
			}
			else
			{
				GlobalPosition = (Vector2)reference.FixedPosition;
				GlobalRotation = (float)reference.FixedRotation;
			}
		}
	}
}

using Godot;

namespace SharpCollisions.Sharp2D
{
	[Tool] [GlobalClass]
	public partial class SharpRenderer2D3D : Node3D
	{
		[Export] private FixedTransform2D reference;

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
			if (reference == null) return;

			Visible = reference.Active;
			
			if (Engine.IsEditorHint())
			{
				GlobalPosition = new Vector3(
					reference.fixedPosition.X / (float)SharpNode.nodeScale,
					reference.fixedPosition.Y / (float)SharpNode.nodeScale,
                    0
				);
				GlobalRotationDegrees = new Vector3(0, 0, reference.fixedRotation / (float)SharpNode.nodeScale);
			}
			else
			{
				GlobalPosition = (Vector3)reference.FixedPosition;
				GlobalRotation = new Vector3(0, 0, (float)reference.FixedRotation);
			}
		}
	}
}

using Godot;

namespace SharpCollisions.Sharp3D
{
	[Tool] [GlobalClass]
	public partial class SharpRenderer3D : Node3D
	{
		[Export] private FixedTransform3D reference;

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
					reference.fixedPosition.Z / (float)SharpNode.nodeScale
				);
				GlobalRotationDegrees = new Vector3(
					reference.fixedRotation.X / (float)SharpNode.nodeScale,
					reference.fixedRotation.Y / (float)SharpNode.nodeScale,
					reference.fixedRotation.Z / (float)SharpNode.nodeScale
				);
			}
			else
			{
				if (SharpManager.Instance.canRender)
				{
					GlobalPosition = (Vector3)reference.FixedPosition;
					GlobalRotation = (Vector3)reference.FixedRotation;
				}
			}
		}
	}
}

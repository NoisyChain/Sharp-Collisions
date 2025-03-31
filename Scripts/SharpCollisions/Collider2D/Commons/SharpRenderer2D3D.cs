using Godot;

namespace SharpCollisions.Sharp2D
{
	[Tool] [GlobalClass]
	public partial class SharpRenderer2D3D : Node3D, ISharpRenderer
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
			
			GlobalPosition = (Vector3)reference.FixedPosition;
			GlobalRotation = new Vector3(0, 0, (float)reference.FixedRotation);
		}

		public void Preview()
		{
			if (reference == null) return;

			Visible = reference.Active;

			GlobalPosition = new Vector3(
				reference.fixedPosition.X / (float)SharpNode.nodeScale,
				reference.fixedPosition.Y / (float)SharpNode.nodeScale,
				0
			);
			GlobalRotationDegrees = new Vector3(0, 0, reference.fixedRotation / (float)SharpNode.nodeScale);
		}
	}
}

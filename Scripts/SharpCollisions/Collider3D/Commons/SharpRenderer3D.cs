using Godot;

namespace SharpCollisions.Sharp3D
{
	[Tool] [GlobalClass]
	public partial class SharpRenderer3D : Node3D, ISharpRenderer
	{
		[Export] private FixedTransform3D reference;

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
			GlobalRotation = (Vector3)reference.FixedRotation;
		}

		public void Preview()
		{
			if (reference == null) return;

			Visible = reference.Active;

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
	}
}

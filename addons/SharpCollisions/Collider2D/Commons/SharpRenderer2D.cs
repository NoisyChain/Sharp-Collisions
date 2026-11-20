using Godot;
using SharpCollisions.Sharp2D;

[Tool] [GlobalClass]
public partial class SharpRenderer2D : SharpRenderer
{
	[Export] private FixedTransform2D transform;
	[Export] private Node3D viewerNode;
	public override void Render()
	{
		if (!IsInstanceValid(this)) return; // Necessary to avoid checking disposed nodes
		if (transform == null) return;
		if (viewerNode == null) return;

		if (ActiveAsVisible) viewerNode.Visible = transform.Active;
		if (ViewPosition) viewerNode.GlobalPosition = (Vector3)transform.FixedPosition;
		if (ViewRotation) viewerNode.GlobalRotationDegrees = new Vector3(0.0f, 0.0f, (float)transform.FixedRotation);
	}
}

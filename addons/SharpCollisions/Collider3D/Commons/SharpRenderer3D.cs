using Godot;
using SharpCollisions.Sharp3D;

[Tool] [GlobalClass]
public partial class SharpRenderer3D : SharpRenderer
{
	[Export] private FixedTransform3D transform;
	[Export] private Node3D viewerNode;
	public override void Render()
	{
		if (transform == null) return;
		if (viewerNode == null) return;
		
		if (ActiveAsVisible) viewerNode.Visible = transform.Active;
		if (ViewPosition) viewerNode.GlobalPosition = (Vector3)transform.FixedPosition;
		if (ViewRotation) viewerNode.GlobalRotationDegrees = (Vector3)transform.FixedRotation;
	}
}

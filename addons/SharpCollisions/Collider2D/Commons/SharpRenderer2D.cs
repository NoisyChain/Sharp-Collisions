using Godot;
using SharpCollisions.Sharp2D;

[Tool]
public partial class SharpRenderer2D : SharpRenderer
{

	[Export] private FixedTransform2D transform;
	[Export] private Node3D viewerNode;
	public override void Render()
	{
		if (ActiveAsVisible) viewerNode.Visible = transform.Active;
		if (ViewPosition) viewerNode.GlobalPosition = (Vector3)transform.FixedPosition;
		if (ViewRotation) viewerNode.GlobalRotationDegrees = new Vector3(0.0f, 0.0f, (float)transform.FixedRotation);
	}
}

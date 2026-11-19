using Godot;
using System;

public partial class SharpRenderer : Node
{
	[Export] protected bool ActiveAsVisible;
	[Export] protected bool ViewPosition;
	[Export] protected bool ViewRotation;
	[Export] protected bool InterpolatePosition;
	[Export] protected bool InterpolateRotation;

	public virtual void Render() {}
}

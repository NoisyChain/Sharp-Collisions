using Godot;
using System;

[Tool]
public partial class SharpRenderer : Node
{
	[Export] protected bool ActiveAsVisible = true;
	[Export] protected bool ViewPosition = true;
	[Export] protected bool ViewRotation = true;
	[Export] protected bool InterpolatePosition;
	[Export] protected bool InterpolateRotation;

	public virtual void Render() {}
}

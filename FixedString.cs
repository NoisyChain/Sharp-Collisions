using Godot;
using System;
using FixMath.NET;

public partial class FixedString : Node2D
{
	[Export] private Label texts;
	private Fix64 precision => new Fix64(1000);
	[Export] private Vector3I value1;
	[Export] private Vector3 value2;

	//[Export] private int decimalRange;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FixVector3 fValue1 = new FixVector3(new Fix64(value1.X), new Fix64(value1.Y), new Fix64(value1.Z)) / precision;
		FixVector3 fValue2 = new FixVector3((Fix64)value2.X, (Fix64)value2.Y, (Fix64)value2.Z);

		texts.Text = $"{fValue1}\n{fValue2}";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

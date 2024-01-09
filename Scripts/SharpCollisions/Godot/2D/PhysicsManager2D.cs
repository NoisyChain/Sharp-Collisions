using Godot;
using System.Collections.Generic;
using SharpCollisions;
using FixMath.NET;

public class PhysicsManager2D : Spatial
{
	private SharpWorld2D world;
	private List<FixedTransform2D> bodies;
	[Export] private int TicksPerSecond = 60;
	[Export] private int iterations = 2;

	/*[Export] private float TestValue
	{
        get => (float)_testValue;
        set => _testValue = (Fix64)value;
    }
	private Fix64 _testValue;*/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		world = new SharpWorld2D();
		bodies = new List<FixedTransform2D>();
	}

	public override void _PhysicsProcess(float delta)
	{
		Fix64 fixedDelta = Fix64.One / (Fix64)TicksPerSecond;
		for (int i = 0; i < bodies.Count; i++)
			bodies[i]._FixedProcess(fixedDelta);
		
		world.Simulate(TicksPerSecond, iterations);
	}
	
	public void AddBody(FixedTransform2D newBody)
	{
		if (world == null)
		{
			GD.Print("Physics World doesn't exist. Please create a Physics World before adding a body.");
			return;
		}
		bodies.Add(newBody);
		world.AddBody(newBody as SharpBody2D);
		GD.Print("Body created!");
	}

	public bool RemoveBody(FixedTransform2D body)
	{
		if (world == null)
		{
			GD.Print("Physics World doesn't exist. Please create a Physics World before adding a body.");
			return false;
		}
		return bodies.Remove(body) && world.RemoveBody(body as SharpBody2D);
	}

	public SharpBody2D GetBodyByIndex(int index)
	{
		return world.bodies[index];
	}
}

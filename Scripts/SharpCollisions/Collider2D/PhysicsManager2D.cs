using Godot;
//using System.Collections.Generic;
using Godot.Collections;
using SharpCollisions;
using FixMath.NET;

public class PhysicsManager2D : Spatial
{
	private SharpWorld2D world;
	private Array<FixedTransform2D> transforms;
	[Export] private int TicksPerSecond = 60;
	[Export] private int iterations = 2;
	public Fix64 fixedTPS => (Fix64)TicksPerSecond;
	public Fix64 fixedIterations => (Fix64)iterations;
	public Fix64 fixedDelta => Fix64.One / fixedTPS;

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
		transforms = new Array<FixedTransform2D>();
	}

	public override void _PhysicsProcess(float delta)
	{
		for (int i = 0; i < transforms.Count; i++)
			transforms[i]._FixedProcess(fixedDelta);
		
		world.Simulate(TicksPerSecond, iterations);
	}

	public void AddTransform(FixedTransform2D newTransform)
	{
		transforms.Add(newTransform);
	}
	
	public void AddBody(SharpBody2D newBody)
	{
		if (world == null)
		{
			GD.Print("Physics World doesn't exist. Please create a Physics World before adding a body.");
			return;
		}
		
		world.AddBody(newBody);
		GD.Print("Body created!");
	}

	public bool RemoveTransform(FixedTransform2D transf)
	{
		return transforms.Remove(transf);
	}

	public bool RemoveBody(SharpBody2D body)
	{
		if (world == null)
		{
			GD.Print("Physics World doesn't exist. Please create a Physics World before adding a body.");
			return false;
		}
		return world.RemoveBody(body);
	}

	public SharpBody2D GetBodyByIndex(int index)
	{
		return world.bodies[index];
	}
}

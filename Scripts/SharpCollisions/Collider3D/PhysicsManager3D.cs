using Godot;
//using System.Collections.Generic;
using Godot.Collections;
using SharpCollisions;
using FixMath.NET;

public class PhysicsManager3D : Spatial
{
	private SharpWorld3D world;
	private Array<FixedTransform3D> transforms;
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
		world = new SharpWorld3D();
		transforms = new Array<FixedTransform3D>();
	}

	public override void _PhysicsProcess(float delta)
	{
		for (int i = 0; i < transforms.Count; i++)
			transforms[i]._FixedProcess(fixedDelta);
		
		world.Simulate(TicksPerSecond, iterations);
	}

	public void AddTransform(FixedTransform3D newTransform)
	{
		transforms.Add(newTransform);
	}
	
	public void AddBody(SharpBody3D newBody)
	{
		if (world == null)
		{
			GD.Print("Physics World doesn't exist. Please create a Physics World before adding a body.");
			return;
		}
		
		world.AddBody(newBody);
		GD.Print("Body created!");
	}

	public bool RemoveTransform(FixedTransform3D transf)
	{
		return transforms.Remove(transf);
	}

	public bool RemoveBody(SharpBody3D body)
	{
		if (world == null)
		{
			GD.Print("Physics World doesn't exist. Please create a Physics World before adding a body.");
			return false;
		}
		return world.RemoveBody(body);
	}

	public SharpBody3D GetBodyByIndex(int index)
	{
		return world.bodies[index];
	}
}

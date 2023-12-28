using Godot;
//using System;
using System.Collections.Generic;
using SharpCollisions;
using FixMath.NET;

public class PhysicsManager2D : Spatial
{
	private SharpWorld2D world;
	private List<SharpCollisions.PhysicsBody2D> bodies;
	[Export] private int TicksPerSecond = 60;
	[Export] private int iterations = 2;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		world = new SharpWorld2D();
		bodies = new List<SharpCollisions.PhysicsBody2D>();
	}

	public override void _PhysicsProcess(float delta)
	{
		Fix64 fixedDelta = Fix64.One / (Fix64)TicksPerSecond;
		for (int i = 0; i < bodies.Count; i++)
			bodies[i]._SharpProcess(fixedDelta);
		
		world.Simulate(TicksPerSecond, iterations);
	}
	
	public void AddBody(SharpCollisions.PhysicsBody2D newBody)
	{
		if (world == null)
		{
			GD.Print("Physics World doesn't exist. Please create a Physics World before adding a body.");
			return;
		}
		bodies.Add(newBody);
		world.AddBody(newBody.body);
		GD.Print("Body created!");
	}

	public bool RemoveBody(SharpCollisions.PhysicsBody2D body)
	{
		if (world == null)
		{
			GD.Print("Physics World doesn't exist. Please create a Physics World before adding a body.");
			return false;
		}
		return bodies.Remove(body) && world.RemoveBody(body.body);
	}
}

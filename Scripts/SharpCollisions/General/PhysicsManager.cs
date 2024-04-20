using Godot;
using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions
{
	public partial class PhysicsManager : Node
	{
		private SharpWorld2D world2D;
		private SharpWorld3D world3D;
		private List<SharpNode> nodes;
		[Export] private int TicksPerSecond = 60;
		[Export] private int iterations = 4;
		[Export] private bool UseEngineSettings = false;
		public Fix64 fixedTPS => (Fix64)TicksPerSecond;
		public Fix64 fixedIterations => (Fix64)iterations;
		public Fix64 fixedDelta => Fix64.One / fixedTPS;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			if (UseEngineSettings)
			{
				TicksPerSecond = (int)ProjectSettings.GetSetting("physics/common/physics_ticks_per_second");
				iterations = (int)ProjectSettings.GetSetting("physics/common/max_physics_steps_per_frame");
			}
			world2D = new SharpWorld2D();
			world3D = new SharpWorld3D();
			nodes = new List<SharpNode>();
		}

		public override void _PhysicsProcess(double delta)
		{
			PhysicsLoop();
		}

		private void PhysicsLoop()
		{
			for (int i = 0; i < nodes.Count; i++)
				nodes[i]._FixedPreProcess(fixedDelta);
			
			for (int i = 0; i < nodes.Count; i++)
				nodes[i]._FixedProcess(fixedDelta);
			
			world2D.Simulate(TicksPerSecond, iterations);
			world3D.Simulate(TicksPerSecond, iterations);

			for (int i = 0; i < nodes.Count; i++)
				nodes[i]._FixedPostProcess(fixedDelta);
		}

		public void AddNode(SharpNode newNode)
		{
			nodes.Add(newNode);
		}

		public void AddBody(SharpBody2D newBody)
		{
			if (world2D == null)
			{
				GD.PrintErr("Physics World doesn't exist. Please create a Physics World before adding a body.");
				return;
			}
			
			world2D.AddBody(newBody);
			GD.Print("Body created!");
		}
		
		public void AddBody(SharpBody3D newBody)
		{
			if (world3D == null)
			{
				GD.PrintErr("Physics World doesn't exist. Please create a Physics World before adding a body.");
				return;
			}
			
			world3D.AddBody(newBody);
			GD.Print("Body created!");
		}

		public bool RemoveNode(SharpNode selectedNode)
		{
			return nodes.Remove(selectedNode);
		}

		public bool RemoveBody(SharpBody2D body)
		{
			if (world2D == null)
			{
				GD.PrintErr("Physics World doesn't exist, therefore there is no body to remove.");
				return false;
			}
			return world2D.RemoveBody(body);
		}
		
		public bool RemoveBody(SharpBody3D body)
		{
			if (world3D == null)
			{
				GD.PrintErr("Physics World doesn't exist, therefore there is no body to remove.");
				return false;
			}
			return world3D.RemoveBody(body);
		}

		public SharpBody2D GetBody2DByIndex(int index)
		{
			return world2D.bodies[index];
		}

		public SharpBody3D GetBody3DByIndex(int index)
		{
			return world3D.bodies[index];
		}
	}
}

using Godot;
using System.Collections.Generic;
using System.Threading;
using Godot.Collections;
using SharpCollisions.Sharp2D;
using SharpCollisions.Sharp3D;

namespace SharpCollisions
{
	/// <summary>
	/// SharpManager class: 
	/// This is an example implementation of a basic Sharp Collisions loop
	/// containing update methods and both 2D and 3D collisions. This is not an ideal
	/// implementation you should use in a commercial game.
	/// </summary>
	public partial class SharpManager : Node
	{
		public static SharpManager Instance;
		private SharpWorld2D world2D;
		private SharpWorld3D world3D;
		[Export] private Array<SharpNode> nodes;
		[Export] private int TicksPerSecond = 60;
		[Export] private int Substeps = 4;
		//[Export] private bool UseSpacePartitioning = false;
		[Export] private int AreaSize = 10;
		[Export] private int MaxBodiesPerPartition = 4;
		[Export] private float debugLineThickness = 0.025f;
		[Export] private bool UseEngineSettings = false;
		[Export] private bool RunOnDedicatedThread = false;
		[Export] private bool ShowDebugShapes = false;
		public bool canRender;

		private Thread physicsThread;
		private System.Threading.Mutex physicsMutex = new System.Threading.Mutex();

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			Instance = this;

			if (UseEngineSettings)
			{
				TicksPerSecond = (int)ProjectSettings.GetSetting("physics/common/physics_ticks_per_second");
				Substeps = (int)ProjectSettings.GetSetting("physics/common/max_physics_steps_per_frame");
			}

			SharpTime.Set(TicksPerSecond, Substeps);

			world2D = new SharpWorld2D(AreaSize, MaxBodiesPerPartition);
			world3D = new SharpWorld3D(AreaSize, MaxBodiesPerPartition);

			foreach (SharpNode node in nodes)
			{
				node._Instance();
			}

			DebugDraw3D.ScopedConfig().SetThickness(debugLineThickness);

			//Start physics simulation on a separate thread
			//Some weird flickering is happening because of this
			if (RunOnDedicatedThread)
			{
				physicsThread = new Thread(() => Loop());
				physicsThread.IsBackground = true;
				physicsThread.Start();
			}
			else canRender = true;
		}

        public override void _Process(double delta)
        {
            base._Process(delta);

			physicsMutex.WaitOne();

			foreach(SharpBody2D body in world2D.bodies)
				body.RenderNode(ShowDebugShapes);
			foreach(SharpBody3D body in world3D.bodies)
				body.RenderNode(ShowDebugShapes);
			
			physicsMutex.ReleaseMutex();
        }

        public override void _PhysicsProcess(double delta)
		{
			if (RunOnDedicatedThread) return;
			
			PhysicsLoop();
		}

        private void Loop()
		{
			while(true)
			{
				physicsMutex.WaitOne();
				PhysicsLoop();
				physicsMutex.ReleaseMutex();
				Thread.Sleep((int)(1f / TicksPerSecond * 1000));
			}
		}

		private void PhysicsLoop()
		{
			SharpTime.AdvanceFrame();
			
			for (int i = 0; i < nodes.Count; i++)
				nodes[i]._FixedPreProcess(SharpTime.DeltaTime);
			
			for (int i = 0; i < nodes.Count; i++)
				nodes[i]._FixedProcess(SharpTime.DeltaTime);
			
			world2D.Simulate();
			world3D.Simulate();

			for (int i = 0; i < nodes.Count; i++)
				nodes[i]._FixedPostProcess(SharpTime.DeltaTime);
		}

		public void AddNode(SharpNode newNode)
		{
			//nodes.Add(newNode);
		}

		public void AddBody(SharpBody2D newBody)
		{
			if (world2D == null)
			{
				GD.PrintErr("Physics World doesn't exist. Please create a Physics World before adding a body.");
				return;
			}
			
			world2D.AddBody(newBody);
			GD.Print("2D Body created!");
		}
		
		public void AddBody(SharpBody3D newBody)
		{
			if (world3D == null)
			{
				GD.PrintErr("Physics World doesn't exist. Please create a Physics World before adding a body.");
				return;
			}
			
			world3D.AddBody(newBody);
			GD.Print("3D Body created!");
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

using FixMath.NET;
using Godot;
using System.Collections.Generic;

namespace SharpCollisions
{
	[System.Serializable]
	public class SharpWorld3D
	{
		public List<SharpBody3D> bodies;
		
		public int BodyCount => bodies.Count;

		private int MinIterations = 1;
		private int MaxIterations = 64;
		private List<(int, int)> PossibleCollisions;

		public const int mask = 0b_1111_1111;
		
		public SharpWorld3D()
		{
			bodies = new List<SharpBody3D>();
			PossibleCollisions = new List<(int, int)>();
		}
		
		public void AddBody(SharpBody3D newBody)
		{
			bodies.Add(newBody);
			GD.Print(BodyCount);
		}
		
		public bool RemoveBody(SharpBody3D body)
		{
			return bodies.Remove(body);
		}

		public bool GetBody(int index, out SharpBody3D body)
		{
			body = null;

			if(index < 0 || index >= bodies.Count)
			{
				return false;
			}

			body = bodies[index];
			return true;
		}

		public void ResetAllIgnoreStates()
		{
			foreach(SharpBody3D body in bodies)
				body.ResetIgnoreBodies();
		}

		private bool CompareLayers(SharpBody3D bodyA, SharpBody3D bodyB)
		{
			/*for (int i = 0; i < 8; i++)
			{
				if ((bodyA.CollisionLayers & 1 << i) != 0)
					if ((bodyA.CollisionLayers & 1 << i) == (bodyB.CollisionLayers & 1 << i))
						return true;
			}
			return false;*/
			
			//DON'T ASK ME WHAT'S HAPPENING HERE
			return ((bodyA.CollisionMask & bodyB.CollisionLayers) & mask) != 0;
		}

		private void BroadPhase()
		{
			PossibleCollisions.Clear();

			for (int i = 0; i < bodies.Count; i++)
			{
				SharpBody3D bodyA = bodies[i];
				bodyA.Collider.collisionFlags.Clear();

				for (int j = i + 1; j < bodies.Count; j++)
				{
					SharpBody3D bodyB = bodies[j];
					
					if (!bodyA.Visible || !bodyB.Visible) continue;
					if (bodyA.BodyMode == 2 && bodyB.BodyMode == 2) continue;
					if (bodyA.BodiesToIgnore.Contains(bodyB)) continue;
					if (!CompareLayers(bodyA, bodyB)) continue;
					if (!bodyA.Collider.BoundingBox.IsOverlapping(bodyB.Collider.BoundingBox))
					{
						SetCollidedWith(bodyA, bodyB, false);
						SetCollidedWith(bodyB, bodyA, false);

						continue;
					}
					bodyA.Collisions.Clear();
					bodyB.Collisions.Clear();
					PossibleCollisions.Add((i, j));
				}
			}
		}

		private void NarrowPhase()
		{
			for(int i = 0; i < PossibleCollisions.Count; i ++)
			{
				SharpBody3D bodyA = bodies[PossibleCollisions[i].Item1];
				SharpBody3D bodyB = bodies[PossibleCollisions[i].Item2];

				if (bodyA.Collider.IsOverlapping(bodyB.Collider, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint))
				{
					if (!bodyA.isTrigger && !bodyB.isTrigger)
					{
						if (bodyA.BodyMode != 0)
							bodyB.PushAway(Depth);
						else if (bodyB.BodyMode != 0)
							bodyA.PushAway(-Depth);
						else
						{
							bodyA.PushAway(-Depth / Fix64.Two);
							bodyB.PushAway(Depth / Fix64.Two);
						}
					}
					CollisionManifold3D collisionA = new CollisionManifold3D
					(
						bodyB, -Normal, Depth, ContactPoint
					);
					CollisionManifold3D collisionB = new CollisionManifold3D
					(
						bodyA, Normal, Depth, ContactPoint
					);
					bodyA.Collisions.Add(collisionA);
					bodyB.Collisions.Add(collisionB);

					if (!bodyA.isTrigger && !bodyB.isTrigger)
					{
						bodyA.Collider.collisionFlags = bodyA.Collider.GetCollisionFlags(collisionA, bodyA);
						bodyB.Collider.collisionFlags = bodyB.Collider.GetCollisionFlags(collisionB, bodyB);
					}
					
					SetCollidedWith(bodyA, bodyB, true);
					SetCollidedWith(bodyB, bodyA, true);
					
					//GD.Print($"Body {PossibleCollisions[i].Item1} collided with body {PossibleCollisions[i].Item2}.");
				}
				else
				{
					SetCollidedWith(bodyA, bodyB, false);
					SetCollidedWith(bodyB, bodyA, false);
				}
			}
		}

		private void SetCollidedWith(SharpBody3D bodyA, SharpBody3D bodyB, bool hasCollided)
		{
			if (hasCollided)
			{
				if (!bodyA.CollidedWith.Contains(bodyB))
				{
					bodyA.BeginOverlap(bodyB);
					bodyA.CollidedWith.Add(bodyB);
				}
				else
					bodyA.DuringOverlap(bodyB);
			}
			else
			{
				if (bodyA.CollidedWith.Contains(bodyB))
				{
					bodyA.EndOverlap(bodyB);
					bodyA.CollidedWith.Remove(bodyB);
				}
			}
		}

		private void MoveBodies(int steps, int iterations)
		{
			for (int i = 0; i < bodies.Count; i++)
				bodies[i].Move(steps, iterations);
		}
		
		public void Simulate(int steps, int iterations)
		{
			if (BodyCount == 0) return;
			
			iterations = Mathf.Clamp(iterations, MinIterations, MaxIterations);

			for (int it = 0; it < iterations; it++)
			{
				MoveBodies(steps, iterations);
				BroadPhase();
				NarrowPhase();
			}
		}
	}
}
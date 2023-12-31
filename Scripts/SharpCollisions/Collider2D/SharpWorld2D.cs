using FixMath.NET;
using Godot;
using System.Collections.Generic;

namespace SharpCollisions
{
	[System.Serializable]
	public class SharpWorld2D
	{
		public List<SharpBody2D> bodies;
		
		public int BodyCount => bodies.Count;

		private int MinIterations = 1;
		private int MaxIterations = 64;
		private List<(int, int)> PossibleCollisions;

		const int mask = 0b_1111_1111;
		
		public SharpWorld2D()
		{
			bodies = new List<SharpBody2D>();
			PossibleCollisions = new List<(int, int)>();
		}
		
		public void AddBody(SharpBody2D newBody)
		{
			bodies.Add(newBody);
			GD.Print(BodyCount);
		}
		
		public bool RemoveBody(SharpBody2D body)
		{
			return bodies.Remove(body);
		}

		public bool GetBody(int index, out SharpBody2D body)
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
			foreach(SharpBody2D body in bodies)
				body.ResetIgnoreBodies();
		}

		private bool CompareLayers(SharpBody2D bodyA, SharpBody2D bodyB)
		{
			/*for (int i = 0; i < 8; i++)
			{
				if ((bodyA.CollisionLayers & 1 << i) != 0)
					if ((bodyA.CollisionLayers & 1 << i) == (bodyB.CollisionLayers & 1 << i))
						return true;
			}
			return false;*/
			
			//DON'T ASK ME WHAT'S HAPPENING HERE
			return ((bodyA.CollisionLayers & bodyB.CollisionLayers) & mask) != 0;
		}

		private void BroadPhase()
		{
			PossibleCollisions.Clear();

			for (int i = 0; i < bodies.Count; i++)
			{
				SharpBody2D bodyA = bodies[i];
				bodyA.Collisions.Clear();
				bodyA.Collider.collisionFlags.Clear();

				for (int j = 0; j < bodies.Count; j++)
				{
					SharpBody2D bodyB = bodies[j];
					
					if (i == j) continue;
					if (!bodyA.Visible || !bodyB.Visible) continue;
					if (bodyA.isStatic && bodyB.isStatic) continue;
					if (bodyA.BodiesToIgnore.Contains(bodyB)) continue;
					if (!CompareLayers(bodyA, bodyB)) continue;
					if (!bodyA.Collider.BoundingBox.IsOverlapping(bodyB.Collider.BoundingBox))
					{
						SetCollidedWith(bodyA, bodyB, false);
						continue;
					}

					PossibleCollisions.Add((i, j));
				}
			}
		}

		private void NarrowPhase()
		{
			for(int i = 0; i < PossibleCollisions.Count; i ++)
			{
				SharpBody2D bodyA = bodies[PossibleCollisions[i].Item1];
				SharpBody2D bodyB = bodies[PossibleCollisions[i].Item2];

				if (bodyA.Collider.IsOverlapping(bodyB.Collider, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint))
				{
					if (!bodyA.isTrigger && !bodyB.isTrigger)
					{
						if (bodyA.isStatic || !bodyA.isPushable)
							bodyB.PushAway(-Depth);
						else if (bodyB.isStatic || !bodyB.isPushable)
							bodyA.PushAway(Depth);
						else
						{
							bodyA.PushAway(Depth / (Fix64)2);
							bodyB.PushAway(-Depth / (Fix64)2);
						}
					}
					CollisionManifold2D collision = new CollisionManifold2D(
							bodyB, Normal, Depth, ContactPoint
						);
					bodyA.Collisions.Add(collision);

					if (!bodyA.isTrigger && !bodyB.isTrigger)
						bodyA.Collider.collisionFlags = bodyA.Collider.GetCollisionFlags(collision, bodyA);
					
					SetCollidedWith(bodyA, bodyB, true);
					
					//bodyB.DuringOverlap(bodyA);

					//GD.Print($"Body {PossibleCollisions[i].Item1} collided with body {PossibleCollisions[i].Item2}.");
				}
				else
				{
					SetCollidedWith(bodyA, bodyB, false);
				}
			}
		}

		private void SetCollidedWith(SharpBody2D bodyA, SharpBody2D bodyB, bool hasCollided)
		{
			if (hasCollided)
			{
				if (!bodyA.CollidedWith.Contains(bodyB))
				{
					bodyA.BeginOverlap(bodyB);
					bodyA.CollidedWith.Add(bodyB);
					//GD.Print($"Body {PossibleCollisions[i].Item1} has started colliding with body {PossibleCollisions[i].Item2}.");
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
					//GD.Print($"Body {PossibleCollisions[i].Item1} has stopped colliding with body {PossibleCollisions[i].Item2}.");
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
using FixMath.NET;
using Godot;
using System.Collections.Generic;

namespace SharpCollisions.Sharp2D
{
	[System.Serializable]
	public class SharpWorld2D
	{
		public List<SharpBody2D> bodies;
		
		public int BodyCount => bodies.Count;
		private uint CreatedBodies = 0;

		private int MinIterations = 1;
		private int MaxIterations = 64;
		private List<PossibleCollision> PossibleCollisions;
		private List<(int, int, int, int, bool)> ConfirmedCollisions;

		public const int mask = 0b_1111_1111;
		
		public SharpWorld2D()
		{
			bodies = new List<SharpBody2D>();
			PossibleCollisions = new List<PossibleCollision>();
			ConfirmedCollisions = new List<(int, int, int, int, bool)>();
		}
		
		public void AddBody(SharpBody2D newBody)
		{
			newBody.SetBodyID(CreatedBodies);
			bodies.Add(newBody);
			CreatedBodies++;
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

		private bool CompareLayers(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			/*for (int i = 0; i < 8; i++)
			{
				if ((bodyA.CollisionLayers & 1 << i) != 0)
					if ((bodyA.CollisionLayers & 1 << i) == (bodyB.CollisionLayers & 1 << i))
						return true;
			}
			return false;*/
			
			//DON'T ASK ME WHAT'S HAPPENING HERE
			return ((colliderA.CollisionMask & colliderB.CollisionLayers) & mask) != 0;
		}

		private void BroadPhase()
		{
			PossibleCollisions.Clear();
			foreach(SharpBody2D body in bodies)
			{
				body.ClearFlags();
				body.Collisions.Clear();
			}

			for (int i = 0; i < bodies.Count; i++)
			{
				for (int j = i + 1; j < bodies.Count; j++)
				{
					//Check every collider in each body
					CheckColliders(i, j);
				}
			}

			//Sort the colliders so the nearest colliders are checked first
			PossibleCollisions.Sort((a, b) => b.distance.CompareTo(a.distance));
			//Sort again to reorder by bodies keeping the distance
			PossibleCollisions.Sort((a, b) => a.BodyA.CompareTo(b.BodyA));
		}

		private void CheckColliders(int indA, int indB)
		{
			SharpBody2D bodyA = bodies[indA];
			SharpBody2D bodyB = bodies[indB];
			if (!bodyA.HasColliders() || !bodyB.HasColliders()) return;

			for (int i = 0; i < bodyA.Colliders.Length; i++)
			{
				for (int j = 0; j < bodyB.Colliders.Length; j++)
				{
					if (!bodyA.Active || !bodyB.Active)
					{ ClearCollision(indA, i, indB, j); continue; }
					if (bodyA.BodyMode == 2 && bodyB.BodyMode == 2)
					{ ClearCollision(indA, i, indB, j); continue; }
					if (bodyA.BodiesToIgnore.Contains(bodyB.GetBodyID()))
					{ ClearCollision(indA, i, indB, j); continue; }
					if (!bodyA.BoundingBox.IsOverlapping(bodyB.BoundingBox))
					{ ClearCollision(indA, i, indB, j); continue; }
					if (!bodyA.Colliders[i].Active || !bodyB.Colliders[j].Active)
					{ ClearCollision(indA, i, indB, j); continue; }
					if (!CompareLayers(bodyA.Colliders[i], bodyB.Colliders[j]))
					{ ClearCollision(indA, i, indB, j); continue; }
					if (!bodyA.Colliders[i].BoundingBox.IsOverlapping(bodyB.Colliders[j].BoundingBox))
					{ ClearCollision(indA, i, indB, j); continue; }

					PossibleCollisions.Add(new PossibleCollision(
						indA, indB, i, j,
						GetCollisionDistance(bodyA.Colliders[i], bodyB.Colliders[j])
					));
				}
			}
		}

		private void NarrowPhase()
		{
			for(int i = 0; i < PossibleCollisions.Count; i ++)
			{
				SharpBody2D bodyA = bodies[PossibleCollisions[i].BodyA];
				SharpBody2D bodyB = bodies[PossibleCollisions[i].BodyB];
				int colIndA = PossibleCollisions[i].ColliderA;
				int colIndB = PossibleCollisions[i].ColliderB;

				if (bodyA.Colliders[colIndA].IsOverlapping(bodyB.Colliders[colIndB], out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint))
				{
					if (!bodyA.Colliders[colIndA].isTrigger && !bodyB.Colliders[colIndB].isTrigger)
					{
						if (bodyA.BodyMode == 1 || bodyB.BodyMode == 1)
						{
							if (bodyA.BodyMode == 1)
								bodyA.PushAway(-Depth);
							else if (bodyB.BodyMode == 1)
								bodyB.PushAway(Depth);
						}
						else if (bodyA.BodyMode == 2)
							bodyB.PushAway(Depth);
						else if (bodyB.BodyMode == 2)
							bodyA.PushAway(-Depth);
						else if (bodyA.BodyMode == 0 && bodyB.BodyMode == 0)
						{
							bodyA.PushAway(-Depth / Fix64.Two);
							bodyB.PushAway(Depth / Fix64.Two);
						}

						//ResolvePhysics(bodyA, bodyB, Normal);
					}

					bodyA.Collisions.Add(new CollisionManifold2D(bodyB, colIndB, -Normal, Depth, ContactPoint));
					bodyB.Collisions.Add(new CollisionManifold2D(bodyA, colIndA, Normal, Depth, ContactPoint));

					if (!bodyA.Colliders[colIndA].isTrigger && !bodyB.Colliders[colIndB].isTrigger)
					{
						bodyA.Colliders[colIndA].GetCollisionFlags(-Normal, bodyA);
						bodyB.Colliders[colIndB].GetCollisionFlags(Normal, bodyB);
						bodyA.Colliders[colIndA].GetGlobalCollisionFlags(-Normal);
						bodyB.Colliders[colIndB].GetGlobalCollisionFlags(Normal);
					}
					
					AddConfirmedCollision((PossibleCollisions[i].BodyA, colIndA, PossibleCollisions[i].BodyB, colIndB, true));					
					//GD.Print($"Body {PossibleCollisions[i].Item1} collided with body {PossibleCollisions[i].Item2}.");
				}
				else
				{
					AddConfirmedCollision((PossibleCollisions[i].BodyA, colIndA, PossibleCollisions[i].BodyB, colIndB, false));
				}
			}
		}

		private void AddConfirmedCollision((int, int, int, int, bool) col)
		{
			if (!ConfirmedCollisions.Contains(col))
				ConfirmedCollisions.Add(col);
		}

		private void SetCollidedWith(SharpBody2D bodyA, SharpBody2D bodyB, int colB, bool hasCollided)
		{
			if (hasCollided)
			{
				if (!bodyA.CollidedWith.Contains((bodyB.GetBodyID(), colB)))
				{
					bodyA.BeginOverlap(bodyB);
					bodyA.CollidedWith.Add((bodyB.GetBodyID(), colB));
				}
				else
					bodyA.DuringOverlap(bodyB);
			}
			else
			{
				if (bodyA.CollidedWith.Contains((bodyB.GetBodyID(), colB)))
				{
					bodyA.EndOverlap(bodyB);
					bodyA.CollidedWith.Remove((bodyB.GetBodyID(), colB));
				}
			}
		}

		private void ClearCollision(int bodyA, int colA, int bodyB, int colB)
		{
			AddConfirmedCollision((bodyA, colA, bodyB, colB, false));
			AddConfirmedCollision((bodyB, colB, bodyA, colA, false));
			//bodyA.Collisions.Clear();
			//bodyB.Collisions.Clear();
		}

		private void MoveBodies(int steps, int iterations)
		{
			for (int i = 0; i < bodies.Count; i++)
				bodies[i].Move(steps, iterations);
		}

		private void CallCollisionEvents()
		{
			for (int i = 0; i < ConfirmedCollisions.Count; i++)
			{
				(int, int, int, int, bool) cur = ConfirmedCollisions[i];
				SetCollidedWith(bodies[cur.Item1], bodies[cur.Item3], cur.Item4, cur.Item5);
				SetCollidedWith(bodies[cur.Item3], bodies[cur.Item1], cur.Item2, cur.Item5);
			}
			ConfirmedCollisions.Clear();
		}

		/*void ResolvePhysics(SharpBody2D bodyA, SharpBody2D bodyB, FixVector2 normal)
		{
			FixVector2 relativeVelocity = bodyB.Velocity - bodyA.Velocity;

			if (FixVector2.IsSameDirection(relativeVelocity, normal))
			{
				return;
			}

			//Restituition (bounciness)
			Fix64 e = Fix64.Zero; //Min(bodyA.Restituition, bodyB.Restituition)

			Fix64 j = -(Fix64.One + e) * FixVector2.Dot(relativeVelocity, normal);

			j /= Fix64.Two;  //bodyA.InverseMass + bodyB.InverseMass

			FixVector2 impulse = j * normal;

			if (bodyA.BodyMode == 0) bodyA.Velocity -= impulse;
			if (bodyB.BodyMode == 0) bodyB.Velocity += impulse;
		}*/
		
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
			CallCollisionEvents();
		}

		public Fix64 GetCollisionDistance(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			FixVector2 length = colliderB.Center - colliderA.Center;

			FixVector2 newDepth = FixVector2.Zero;
			newDepth.x = (colliderA.BoundingBox.w - colliderA.Center.x) + (colliderB.BoundingBox.w - colliderB.Center.x);
			newDepth.y = (colliderA.BoundingBox.h - colliderA.Center.y) + (colliderB.BoundingBox.h - colliderB.Center.y);
			newDepth.x -= Fix64.Abs(length.x);
			newDepth.y -= Fix64.Abs(length.y);

			return FixVector2.Length(newDepth);
		}
	}
}
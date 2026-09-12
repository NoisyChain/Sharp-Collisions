using FixMath.NET;
using Godot;
using SharpCollisions.Sharp2D.Quadtree;
using System.Collections.Generic;

namespace SharpCollisions.Sharp2D
{
	[System.Serializable]
	public class SharpWorld2D
	{
		public List<SharpBody2D> bodies;
		private QuadTree qTree;
		
		public int BodyCount => bodies.Count;
		private uint CreatedBodies = 0;

		private int MinIterations = 1;
		private int MaxIterations = 64;
		private List<PossibleCollision> PossibleCollisions;
		private List<CollisionResult2D> CollisionResults = new List<CollisionResult2D>();

		public const int mask = 0b_1111_1111;

		public SharpWorld2D(int qtSize, int qtLimit)
		{
			bodies = new List<SharpBody2D>();
			PossibleCollisions = new List<PossibleCollision>();
			CollisionResults = new List<CollisionResult2D>();
			if (qtSize > 0)
			{
				Fix64 quadtreeSize = new Fix64(qtSize);
				qTree = new QuadTree(new FixRect(-quadtreeSize, -quadtreeSize, quadtreeSize, quadtreeSize), qtLimit);
			}
		}
		
		public void AddBody(SharpBody2D newBody)
		{
			if (bodies.Contains(newBody)) return;
			
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

		private bool CompareLayers(SharpBody2D colliderA, SharpBody2D colliderB)
		{
			//DON'T ASK ME WHAT'S HAPPENING HERE
			return ((colliderA.CollisionMask & colliderB.CollisionLayers) & mask) != 0;
		}

		private void BroadPhase()
		{
			PossibleCollisions.Clear();
			CollisionResults.Clear();
			for (int i = 0; i < bodies.Count; i++)
			{
				bodies[i].ClearFlags();
			}

			if (qTree == null) //Brute force the broad phase if no quadtree was created
			{
				for (int i = 0; i < bodies.Count; i++)
				{
					for (int j = i + 1; j < bodies.Count; j++)
					{
						//Check every collider in each body
						CheckColliders(i, j);
					}
				}
			}
			else //Use the quadtree otherwise
			{
				List<IntPack2> collisionQueries = new List<IntPack2>();
				qTree.Compute(bodies);
				qTree.CapturePossibleCollisions(ref collisionQueries);
				
				//GD.Print($"Registered {collisionQueries.Count} collision queries");
				
				foreach (IntPack2 q in collisionQueries)
					CheckColliders(q.a, q.b);
			}

			//Sort the colliders so the nearest colliders are checked first
			PossibleCollisions.Sort((a, b) => b.distance.CompareTo(a.distance));
			//Sort again to reorder by bodies keeping the distance
			PossibleCollisions.Sort((a, b) => a.BodyA.CompareTo(b.BodyA));
			//Sort again by priority
			PossibleCollisions.Sort((a, b) => a.Priority.CompareTo(b.Priority));
		}

		private void CheckColliders(int indA, int indB)
		{
			SharpBody2D bodyA = bodies[indA];
			SharpBody2D bodyB = bodies[indB];

			if (!bodyA.HasColliders() || !bodyB.HasColliders()) return;
			if (bodyA.BodyMode == 2 && bodyB.BodyMode == 2) return;

			if (!bodyA.Active || !bodyB.Active) return;
			if (bodyA.IsIgnoringBody(bodyB)) return;
			if (!CompareLayers(bodyA, bodyB)) return;

			for (int i = 0; i < bodyA.GetColliders().Length; i++)
			{
				var colA = bodyA.GetCollider(i);
				for (int j = 0; j < bodyB.GetColliders().Length; j++)
				{
					var colB = bodyB.GetCollider(j);

					if (!colA.Active || !colB.Active) continue;
					if ((colA.TriggerIgnoresSolid && !colB.IsTrigger) || (colB.TriggerIgnoresSolid && !colA.IsTrigger)) continue;
					if (!colA.BoundingBox.IsOverlapping(colB.BoundingBox)) continue;

					PossibleCollisions.Add(new PossibleCollision(
						indA, indB, i, j, Mathf.Max(bodyA.Priority, bodyB.Priority),
						GetCollisionDistance(colA, colB)
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

				if (CollisionMath2D.IsOverlapping(bodyA.GetCollider(colIndA), bodyB.GetCollider(colIndB), out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint))
				{
					if (!bodyA.GetCollider(colIndA).IsTrigger && !bodyB.GetCollider(colIndB).IsTrigger)
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

					if (!bodyA.GetCollider(colIndA).IsTrigger && !bodyB.GetCollider(colIndB).IsTrigger)
					{
						CollisionMath2D.GetCollisionFlags(bodyA.GetCollider(colIndA), -Normal, bodyA);
						CollisionMath2D.GetCollisionFlags(bodyB.GetCollider(colIndB), Normal, bodyB);
						CollisionMath2D.GetGlobalCollisionFlags(bodyA.GetCollider(colIndA), -Normal);
						CollisionMath2D.GetGlobalCollisionFlags(bodyB.GetCollider(colIndB), Normal);
					}
					
					CollisionResults.Add(new CollisionResult2D(true, bodyA, bodyB, colIndA, colIndB, Normal, Depth, ContactPoint));
					//GD.Print($"Body {PossibleCollisions[i].Item1} collided with body {PossibleCollisions[i].Item2}.");
				}
				else
				{
					CollisionResults.Add(new CollisionResult2D(false, bodyA, bodyB, colIndA, colIndB, Normal, Depth, ContactPoint));
				}
			}
		}

		private void ProcessCollisions()
		{
			foreach(var col in CollisionResults)
			{
				if (col.Collided)
				{
					CollisionManifold2D retA = new CollisionManifold2D(col.BodyB, col.ColliderA, col.ColliderB, -col.Normal, col.Depth, col.ContactPoint);
					CollisionManifold2D retB = new CollisionManifold2D(col.BodyA, col.ColliderB, col.ColliderA, col.Normal, col.Depth, col.ContactPoint);

					if (!col.BodyA.HasCollidedWith((col.BodyB.GetBodyID(), col.ColliderB)))
					{
						col.BodyA.OnBeginOverlap(retA);
						col.BodyA.ConfirmCollision((col.BodyB.GetBodyID(), col.ColliderB));
					}
					col.BodyA.OnOverlap(retA);

					if (!col.BodyB.HasCollidedWith((col.BodyA.GetBodyID(), col.ColliderA)))
					{
						col.BodyB.OnBeginOverlap(retB);
						col.BodyB.ConfirmCollision((col.BodyA.GetBodyID(), col.ColliderA));
					}
					col.BodyB.OnOverlap(retB);
				}
				else
				{
					CollisionManifold2D retA = new CollisionManifold2D(col.BodyB, col.ColliderA, col.ColliderB, FixVector2.Zero, FixVector2.Zero, FixVector2.Zero);
					CollisionManifold2D retB = new CollisionManifold2D(col.BodyA, col.ColliderB, col.ColliderA, FixVector2.Zero, FixVector2.Zero, FixVector2.Zero);
						
					if (col.BodyA.HasCollidedWith((col.BodyB.GetBodyID(), col.ColliderB)))
					{
						col.BodyA.OnEndOverlap(retA);
						col.BodyA.RemoveCollision((col.BodyB.GetBodyID(), col.ColliderB));
					}

					if (col.BodyB.HasCollidedWith((col.BodyA.GetBodyID(), col.ColliderA)))
					{
						col.BodyB.OnEndOverlap(retB);
						col.BodyB.RemoveCollision((col.BodyA.GetBodyID(), col.ColliderA));
					}
				}
			}
		}

		private void MoveBodies()
		{
			for (int i = 0; i < bodies.Count; i++)
				bodies[i].UpdateBody();
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
		
		public void Simulate()
		{
			if (BodyCount == 0) return;
			
			int iterations = Mathf.Clamp(SharpTime.Substeps, MinIterations, MaxIterations);

			for (int it = 0; it < iterations; it++)
			{
				MoveBodies();
				BroadPhase();
				NarrowPhase();
			}
			ProcessCollisions();
		}

		public Fix64 GetCollisionDistance(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			FixVector2 length = colliderB.Center - colliderA.Center;

			FixVector2 newDepth = FixVector2.Zero;
			newDepth.x = (colliderA.BoundingBox.w - colliderA.Center.x) + (colliderB.BoundingBox.w - colliderB.Center.x);
			newDepth.y = (colliderA.BoundingBox.h - colliderA.Center.y) + (colliderB.BoundingBox.h - colliderB.Center.y);
			newDepth.x -= Fix64.Abs(length.x);
			newDepth.y -= Fix64.Abs(length.y);

			return FixVector2.LengthSq(newDepth);
		}
	}

	public class CollisionResult2D
	{
		public bool Collided;
		public SharpBody2D BodyA;
		public SharpBody2D BodyB;
        public int ColliderA;
        public int ColliderB;
        public FixVector2 Normal;
        public FixVector2 Depth;
        public FixVector2 ContactPoint;

		public CollisionResult2D() {}
        public CollisionResult2D(bool collided, SharpBody2D bodyA, SharpBody2D bodyB, int colA, int colB, FixVector2 normal, FixVector2 depth, FixVector2 contact)
        {
			Collided = collided;
			BodyA = bodyA;
            BodyB = bodyB;
            ColliderA = colA;
            ColliderB = colB;
            Normal = normal;
            Depth = depth;
            ContactPoint = contact;
        }
	}
}
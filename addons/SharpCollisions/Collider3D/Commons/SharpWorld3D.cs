using FixMath.NET;
using Godot;
using SharpCollisions.Sharp3D.Octree;
using System.Collections.Generic;

namespace SharpCollisions.Sharp3D
{
	[System.Serializable]
	public class SharpWorld3D
	{
		public List<SharpBody3D> bodies = new List<SharpBody3D>();
		private OcTree oTree;
		
		public int BodyCount => bodies.Count;
		private uint CreatedBodies = 0;

		private int MinIterations = 1;
		private int MaxIterations = 64;
		private List<PossibleCollision> PossibleCollisions = new List<PossibleCollision>();
		private List<CollisionResult3D> CollisionResults = new List<CollisionResult3D>();

		public const int mask = 0b_1111_1111;

		public SharpWorld3D(int qtSize, int qtLimit)
		{
			bodies = new List<SharpBody3D>();
			PossibleCollisions = new List<PossibleCollision>();
			CollisionResults = new List<CollisionResult3D>();
			if (qtSize > 0)
			{
				Fix64 octreeSize = new Fix64(qtSize);
				oTree = new OcTree(
					new FixVolume(-octreeSize, -octreeSize, -octreeSize, octreeSize, octreeSize, octreeSize), qtLimit
				);
			}
		}
		
		public void AddBody(SharpBody3D newBody)
		{
			if (bodies.Contains(newBody)) return;
			
			newBody.SetBodyID(CreatedBodies);
			bodies.Add(newBody);
			CreatedBodies++;
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

		private bool CompareLayers(SharpBody3D colliderA, SharpBody3D colliderB)
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

			if (oTree == null) //Brute force the broad phase if no quadtree was created
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
			else //Use the octree otherwise
			{
				List<IntPack2> collisionQueries = new List<IntPack2>();
				oTree.Compute(bodies);
				oTree.CapturePossibleCollisions(ref collisionQueries);
				
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
			SharpBody3D bodyA = bodies[indA];
			SharpBody3D bodyB = bodies[indB];
			
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
				SharpBody3D bodyA = bodies[PossibleCollisions[i].BodyA];
				SharpBody3D bodyB = bodies[PossibleCollisions[i].BodyB];
				int colIndA = PossibleCollisions[i].ColliderA;
				int colIndB = PossibleCollisions[i].ColliderB;

				if (CollisionMath3D.IsOverlapping(bodyA.GetCollider(colIndA), bodyB.GetCollider(colIndB), out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint))
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
						CollisionMath3D.GetCollisionFlags(bodyA.GetCollider(colIndA), -Normal, bodyA);
						CollisionMath3D.GetCollisionFlags(bodyB.GetCollider(colIndB), Normal, bodyB);
						CollisionMath3D.GetGlobalCollisionFlags(bodyA.GetCollider(colIndA), -Normal);
						CollisionMath3D.GetGlobalCollisionFlags(bodyB.GetCollider(colIndB), Normal);
					}
					
					CollisionResults.Add(new CollisionResult3D(true, bodyA, bodyB, colIndA, colIndB, Normal, Depth, ContactPoint));
					//GD.Print($"Body {PossibleCollisions[i].Item1} collided with body {PossibleCollisions[i].Item2}.");
				}
				else
				{
					CollisionResults.Add(new CollisionResult3D(false, bodyA, bodyB, colIndA, colIndB, Normal, Depth, ContactPoint));
				}
			}
		}

		private void ProcessCollisions()
		{
			foreach(var col in CollisionResults)
			{
				if (col.Collided)
				{
					CollisionManifold3D retA = new CollisionManifold3D(col.BodyB, col.ColliderA, col.ColliderB, -col.Normal, col.Depth, col.ContactPoint);
					CollisionManifold3D retB = new CollisionManifold3D(col.BodyA, col.ColliderB, col.ColliderA, col.Normal, col.Depth, col.ContactPoint);

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
					CollisionManifold3D retA = new CollisionManifold3D(col.BodyB, col.ColliderA, col.ColliderB, FixVector3.Zero, FixVector3.Zero, FixVector3.Zero);
					CollisionManifold3D retB = new CollisionManifold3D(col.BodyA, col.ColliderB, col.ColliderA, FixVector3.Zero, FixVector3.Zero, FixVector3.Zero);
						
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

		/*void ResolvePhysics(SharpBody3D bodyA, SharpBody3D bodyB, FixVector3 normal)
		{
			FixVector3 relativeVelocity = bodyB.Velocity - bodyA.Velocity;

			if (FixVector3.IsSameDirection(relativeVelocity, normal))
			{
				return;
			}

			//Restituition (bounciness)
			Fix64 e = Fix64.Zero; //Min(bodyA.Restituition, bodyB.Restituition)

			Fix64 j = -(Fix64.One + e) * FixVector3.Dot(relativeVelocity, normal);

			j /= Fix64.Two; //bodyA.InverseMass + bodyB.InverseMass

			FixVector3 impulse = j * normal;

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

		public Fix64 GetCollisionDistance(SharpCollider3D colliderA, SharpCollider3D colliderB)
		{
			FixVector3 length = colliderB.Center - colliderA.Center;

			FixVector3 newDepth = FixVector3.Zero;
			newDepth.x = (colliderA.BoundingBox.w - colliderA.Center.x) + (colliderB.BoundingBox.w - colliderB.Center.x);
			newDepth.y = (colliderA.BoundingBox.h - colliderA.Center.y) + (colliderB.BoundingBox.h - colliderB.Center.y);
			newDepth.z = (colliderA.BoundingBox.d - colliderA.Center.z) + (colliderB.BoundingBox.d - colliderB.Center.z);
			newDepth.x -= Fix64.Abs(length.x);
			newDepth.y -= Fix64.Abs(length.y);
			newDepth.z -= Fix64.Abs(length.z);

			return FixVector3.LengthSq(newDepth);
		}
	}

	public class CollisionResult3D
	{
		public bool Collided;
		public SharpBody3D BodyA;
		public SharpBody3D BodyB;
        public int ColliderA;
        public int ColliderB;
        public FixVector3 Normal;
        public FixVector3 Depth;
        public FixVector3 ContactPoint;

		public CollisionResult3D() {}
        public CollisionResult3D(bool collided, SharpBody3D bodyA, SharpBody3D bodyB, int colA, int colB, FixVector3 normal, FixVector3 depth, FixVector3 contact)
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
	};
}
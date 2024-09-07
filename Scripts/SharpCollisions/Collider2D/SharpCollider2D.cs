using Godot;
using FixMath.NET;
using System.Collections.Generic;


namespace SharpCollisions
{
	public partial class SharpCollider2D  : Node
	{
		public const int MAX_GJK_ITERATIONS = 32;
		public const int MAX_EPA_ITERATIONS = 32;

		[Export] public Color debugColor = new Color(0, 0, 1);

		[Export] public bool Active = true;
		[Export] protected bool DrawDebug;
		[Export] protected bool DrawDebugPolytope;
		public CollisionFlags collisionFlags;
		public CollisionFlags globalCollisionFlags;
		public CollisionType2D Shape = CollisionType2D.Null;
		public FixVector2 Position;
		public FixVector2 Offset => (FixVector2)offset;
		public FixVector2 Center;
		public FixRect BoundingBox;
		[Export] protected Vector2 offset;
		protected SharpBody2D ParentNode;

		protected bool CollisionRequireUpdate = true;
		protected bool BoundingBoxRequireUpdate = true;

		/*public SharpCollider2D(){}
		
		public SharpCollider2D(FixVector2 center, FixVector2 offset, FixVector2 size, FixVector2[] points, CollisionType shape)
		{
			Shape = shape;
			Position = center;
			Offset = offset;
			Radius = Fix64.Min(size.x, size.y) / Fix64.Two;
			Height = Fix64.Max(size.x, size.y) / Fix64.Two;
			Size = size;
			CreatePoints(points);
		}*/

		public override void _Ready()
		{
			ParentNode = GetParent() as SharpBody2D;
		}

		public override void _Process(double delta)
		{
			DebugDrawShapes();
		}

		public virtual void DebugDrawShapes()
		{

		}

		public bool IsOverlapping(SharpCollider2D other, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			if (!Active) return false;
			if (!other.Active) return false;
			
			return CollisionDetection(this, other, out Normal, out Depth, out ContactPoint);
		}

		private bool CollisionDetection(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			if (colliderA.Shape == CollisionType2D.AABB && colliderB.Shape == CollisionType2D.AABB)
                return AABBtoAABBCollision(colliderA as AABBCollider2D, colliderB as AABBCollider2D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Circle)
                return CircleToCircleCollision(colliderA as CircleCollider2D, colliderB as CircleCollider2D, out Normal, out Depth, out ContactPoint);
            else if (colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Capsule)
                return CapsuleToCapsuleCollision(colliderA as CapsuleCollider2D, colliderB as CapsuleCollider2D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Capsule)
				return CircleToCapsuleCollision(colliderA as CircleCollider2D, colliderB as CapsuleCollider2D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Circle)
				return CapsuleToCircleCollision(colliderA as CapsuleCollider2D, colliderB as CircleCollider2D, out Normal, out Depth, out ContactPoint);
			else if (colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Polygon ||
					colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Circle ||
                    colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Polygon ||
					colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Capsule ||
                    colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Polygon)
                return GJKPolygonCollision(colliderA, colliderB, out Normal, out Depth, out ContactPoint);
			
			return false;
		}

		public CollisionFlags GetCollisionFlags(CollisionManifold2D collisiondData, SharpBody2D body)
		{
			CollisionFlags flag = collisionFlags;

			if (FixVector2.Dot(collisiondData.Normal, body.Up) > Fix64.ETA)
				flag.Below = true;
			if (FixVector2.Dot(collisiondData.Normal, body.Down) > Fix64.ETA)
				flag.Above = true;
			if (FixVector2.Dot(collisiondData.Normal, body.Left) > Fix64.ETA)
				flag.Right = true;
			if (FixVector2.Dot(collisiondData.Normal, body.Right) > Fix64.ETA)
				flag.Left = true;
			
			return flag;
		}

		public CollisionFlags GetGlobalCollisionFlags(CollisionManifold2D collisiondData)
		{
			CollisionFlags flag = collisionFlags;

			if (FixVector2.Dot(collisiondData.Normal, FixVector2.Up) > Fix64.ETA)
				flag.Below = true;
			if (FixVector2.Dot(collisiondData.Normal, FixVector2.Down) > Fix64.ETA)
				flag.Above = true;
			if (FixVector2.Dot(collisiondData.Normal, FixVector2.Left) > Fix64.ETA)
				flag.Right = true;
			if (FixVector2.Dot(collisiondData.Normal, FixVector2.Right) > Fix64.ETA)
				flag.Left = true;
			
			return flag;
		}

		private FixVector2 SupportFunction(SharpCollider2D colliderA, SharpCollider2D colliderB, FixVector2 direction)
		{
            FixVector2 SupportPointA = colliderA.Support(direction);
            FixVector2 SupportPointB = colliderB.Support(-direction);

			return SupportPointA - SupportPointB;
		}

		public FixVector2 FindAABBNormals(AABBCollider2D colliderA, AABBCollider2D colliderB)
        {
            FixVector2 finalNormal;
            FixVector2 length = colliderB.Center - colliderA.Center;

            Fix64 ExtentsX = colliderB.Extents.x + colliderA.Extents.x;
            Fix64 ExtentsY = colliderB.Extents.y + colliderA.Extents.y;

            // calculate normal of collided surface
            if (Fix64.Abs(length.x) + ExtentsY > Fix64.Abs(length.y) + ExtentsX)
            {
                if (colliderA.Center.x < colliderB.Center.x)
                {
                    finalNormal = FixVector2.Right;
                } 
                else
                {
                    finalNormal = FixVector2.Left;
                }
            }
            else
            {
                if (colliderA.Center.y < colliderB.Center.y)
                {
                    finalNormal = FixVector2.Up;
                }
                else
                {
                    finalNormal = FixVector2.Down;
                }
            }
            return finalNormal;
        }

		public bool AABBtoAABBCollision(AABBCollider2D colliderA, AABBCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
        {
            Normal = FixVector2.Zero;
            Depth = FixVector2.Zero;
            ContactPoint = FixVector2.Zero;

            bool collisionX = colliderA.Center.x - colliderA.Extents.x <= colliderB.Center.x + colliderB.Extents.x &&
                colliderA.Center.x + colliderA.Extents.x >= colliderB.Center.x - colliderB.Extents.x;

            bool collisionY = colliderA.Center.y - colliderA.Extents.y <= colliderB.Center.y + colliderB.Extents.y &&
                colliderA.Center.y + colliderA.Extents.y >= colliderB.Center.y - colliderB.Extents.y;

            if (collisionX && collisionY)
            {
                ContactPoint = AABBContactPoint(colliderA, colliderB);

                FixVector2 length = colliderB.Center - colliderA.Center;

                FixVector2 newDepth = FixVector2.Zero;
                newDepth.x = colliderA.Extents.x + colliderB.Extents.x;
                newDepth.y = colliderA.Extents.y + colliderB.Extents.y;
                newDepth.x -= Fix64.Abs(length.x);
                newDepth.y -= Fix64.Abs(length.y);
                Normal = FindAABBNormals(colliderA, colliderB);
                Depth = Normal * newDepth;
            }

            return collisionX && collisionY;
        }

		public bool CircleToCircleCollision(CircleCollider2D colliderA, CircleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			Fix64 distance = FixVector2.Distance(colliderA.Center, colliderB.Center);
			Fix64 radii = colliderA.Radius + colliderB.Radius;
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector2.Normalize(colliderB.Center - colliderA.Center);
				Depth = Normal * (radii - distance);
				ContactPoint = CircleContactPoint(colliderA, colliderB);
			}
			
			return collision;
		}

		public bool CapsuleToCapsuleCollision(CapsuleCollider2D colliderA, CapsuleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			LineToLineDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.UpperPoint, colliderB.LowerPoint, out FixVector2 r1, out FixVector2 r2);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector2.Distance(r1, r2);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector2.Normalize(r2 - r1);
				Depth = Normal * (radii - distance);
				ContactPoint = CapsuleContactPoint(r1, colliderA.Radius, r2, colliderB.Radius, Normal);
;
			}
			
			return collision;
		}

		public bool CapsuleToCircleCollision(CapsuleCollider2D colliderA, CircleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			LineToPointDistance(colliderA.UpperPoint, colliderA.LowerPoint, colliderB.Center, out FixVector2 CapsulePoint);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector2.Distance(CapsulePoint, colliderB.Center);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector2.Normalize(colliderB.Center - CapsulePoint);
				Depth = Normal * (radii - distance);
				ContactPoint = CapsuleContactPoint(CapsulePoint, colliderA.Radius, colliderB.Center, colliderB.Radius, Normal);
			}
			
			return collision;
		}

		//Just doing this because I don't know how to invert the normal in the previous function yet lol
		public bool CircleToCapsuleCollision(CircleCollider2D colliderA, CapsuleCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			LineToPointDistance(colliderB.UpperPoint, colliderB.LowerPoint, colliderA.Center, out FixVector2 CapsulePoint);

			Fix64 radii = colliderA.Radius + colliderB.Radius;
			Fix64 distance = FixVector2.Distance(CapsulePoint, colliderA.Center);
			
			bool collision = distance <= radii;
			
			if (collision)
			{
				Normal = FixVector2.Normalize(CapsulePoint - colliderA.Center);
				Depth = Normal * (radii - distance);
				ContactPoint = CapsuleContactPoint(CapsulePoint, colliderB.Radius, colliderA.Center, colliderA.Radius, Normal);
			}
			
			return collision;
		}

		public bool GJKPolygonCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			int maxIterations = 0;
			
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			Simplex2D Simplex = new Simplex2D(
				new List<FixVector2>() { FixVector2.Zero, FixVector2.Zero, FixVector2.Zero }
			);

			FixVector2 supportDirection = colliderB.Center - colliderA.Center;
			FixVector2 SupportPoint;

			while (true)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_GJK_ITERATIONS) return false;

				SupportPoint = SupportFunction(colliderA, colliderB, supportDirection);
				if (!FixVector2.IsSameDirection(SupportPoint, supportDirection))
					return false;

				Simplex.MoveForward(SupportPoint);

				if (CheckSimplex(ref Simplex, ref supportDirection))
				{
					EPA(Simplex, colliderA, colliderB, out Normal, out Fix64 newDepth, out ContactPoint);
					Depth = Normal * newDepth;
					return true;
				}
			}
		}

		private bool CheckSimplex(ref Simplex2D Simplex, ref FixVector2 supportDirection)
		{
			switch (Simplex.Size)
			{
				case 1: //Point
					return PointSimplex(ref supportDirection);
				case 2: //Line
					return LineSimplex(ref Simplex, ref supportDirection);
				case 3: //Triangle
					return TriangleSimplex(ref Simplex, ref supportDirection);
			}

			//Should never be here
			return false;
		}

		private bool PointSimplex(ref FixVector2 supportDirection)
		{
			supportDirection *= Fix64.NegativeOne;
			return false;
		}

		private bool LineSimplex(ref Simplex2D Simplex, ref FixVector2 supportDirection)
		{
			FixVector2 a = Simplex.Points[0];
			FixVector2 b = Simplex.Points[1];

			FixVector2 ab = b - a;
			FixVector2 ao = a * Fix64.NegativeOne;

			supportDirection = FixVector2.TripleProduct(ab, ao, ab);
			return false;
		}

		private bool TriangleSimplex(ref Simplex2D Simplex, ref FixVector2 supportDirection)
		{
			FixVector2 a = Simplex.Points[0];
			FixVector2 b = Simplex.Points[1];
			FixVector2 c = Simplex.Points[2];

			FixVector2 ab = b - a;
			FixVector2 ac = c - a;
			FixVector2 ao = a * Fix64.NegativeOne;

			FixVector2 abPerp = FixVector2.TripleProduct(ac, ab, ab);
			FixVector2 acPerp = FixVector2.TripleProduct(ab, ac, ac);

			if(FixVector2.IsSameDirection(abPerp, ao))
			{
                // the origin is outside line ab
                // get rid of c and add a new support in the direction of abPerp
				Simplex.Reset(new List<FixVector2>(){a, b});
                supportDirection = abPerp;
            }
            else if(FixVector2.IsSameDirection(acPerp, ao))
			{
                // the origin is outside line ac
                // get rid of b and add a new support in the direction of acPerp
				Simplex.Reset(new List<FixVector2>(){a, c});
                supportDirection = acPerp;
            }
            else
			{
                // the origin is inside both ab and ac,
                // so it must be inside the triangle!
                return true;
            }

			return false;
		}

		private bool GetPolytopeDirection(List<FixVector2> polytope)
		{
			Fix64 e0 = (polytope[1].x - polytope[0].x) * (polytope[1].y + polytope[0].y);
			Fix64 e1 = (polytope[2].x - polytope[1].x) * (polytope[2].y + polytope[1].y);
			Fix64 e2 = (polytope[0].x - polytope[2].x) * (polytope[0].y + polytope[2].y);

			return e0 + e1 + e2 > Fix64.Zero;
		}

		private void EPA(Simplex2D simplex, SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out Fix64 Depth, out FixVector2 Contact)
		{
			int maxIterations = 0;

			int minIndex = 0;
			Fix64 minDistance = Fix64.MaxValue;
			FixVector2 minNormal = FixVector2.Zero;
			List<FixVector2> polytope = simplex.Points;
			bool IsClockWise = GetPolytopeDirection(polytope);

			while (minDistance == Fix64.MaxValue)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_EPA_ITERATIONS) break;

				for (int i = 0; i < polytope.Count; i++)
				{
					int j = (i + 1) % polytope.Count;

					FixVector2 vertexI = polytope[i];
					FixVector2 vertexJ = polytope[j];


					FixVector2 normal = IsClockWise ?
						FixVector2.GetNormal(vertexI, vertexJ) :
						FixVector2.GetInvertedNormal(vertexI, vertexJ);

					Fix64 distance = FixVector2.Dot(normal, vertexI);

					if (distance < minDistance)
					{
						minDistance = distance;
						minNormal = normal;
						minIndex = j;
					}
				}
				FixVector2 support = SupportFunction(colliderA, colliderB, minNormal);
				Fix64 sDistance = FixVector2.Dot(minNormal, support);

				if (Fix64.Abs(sDistance - minDistance) > Fix64.ETA)
				{
					minDistance = Fix64.MaxValue;
					polytope.Insert(minIndex, support);
				}
			}
			DrawPolytope(polytope);

			Normal = minNormal;
			Depth = Fix64.Abs(minDistance) + Fix64.ETA;
			Contact = GJKGetContactPoint(colliderA, colliderB);
		}

		private void DrawPolytope(List<FixVector2> polytope)
		{
			if (!DrawDebug) return;
			if (DrawDebug && !DrawDebugPolytope) return;

			DebugDraw3D.DrawSphere(Vector3.Zero, 0.03f, new Color(0f, 0f, 0f));
			
			for (int i = 0; i < polytope.Count; i++)
			{
				Vector3 a = (Vector3)polytope[i];
				Vector3 b = (Vector3)polytope[(i + 1) % polytope.Count];
				DebugDraw3D.DrawLine(a, b);
			}
		}

		public FixVector2 AABBContactPoint(AABBCollider2D A, AABBCollider2D B)
        {
            Fix64 minPointX = Fix64.Min(A.Center.x + A.Extents.x, B.Center.x + B.Extents.x);
            Fix64 maxPointX = Fix64.Max(A.Center.x - A.Extents.x, B.Center.x - B.Extents.x);
            Fix64 minPointY = Fix64.Min(A.Center.y + A.Extents.y, B.Center.y + B.Extents.y);
            Fix64 maxPointY = Fix64.Max(A.Center.y - A.Extents.y, B.Center.y - B.Extents.y);
            Fix64 mediantX = (minPointX + maxPointX) / Fix64.Two;
            Fix64 mediantY = (minPointY + maxPointY) / Fix64.Two;
            return new FixVector2(mediantX, mediantY);
        }

		public FixVector2 CircleContactPoint(CircleCollider2D A, CircleCollider2D B)
		{
			FixVector2 Direction = FixVector2.Normalize(B.Center - A.Center);
			FixVector2 ContactA = A.Center + (A.Radius * Direction);
			FixVector2 ContactB = B.Center - (B.Radius * Direction);

			return (ContactA + ContactB) / Fix64.Two;
		}

		public FixVector2 CapsuleContactPoint(FixVector2 centerA, Fix64 radiusA, FixVector2 centerB, Fix64 radiusB, FixVector2 direction)
		{
			FixVector2 ContactA = centerA + (direction * radiusA);
            FixVector2 ContactB = centerB - (direction * radiusB);
			return (ContactA + ContactB) / Fix64.Two;
		}

		public FixVector2 GJKGetContactPoint(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			FixVector2 contacts = FixVector2.Zero;

			if (colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Polygon)//Polygon/Polygon
				contacts = PolygonsContactPoint(colliderA as PolygonCollider2D, colliderB as PolygonCollider2D);
			else if (colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Polygon) //Circle/Polygon
				contacts = PolygonCircleContactPoint(colliderB as PolygonCollider2D, colliderA as CircleCollider2D);
			else if (colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Circle) //Polygon/Circle
				contacts = PolygonCircleContactPoint(colliderA as PolygonCollider2D, colliderB as CircleCollider2D);
			else if (colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Polygon) //Capsule/Polygon
				contacts = PolygonCapsuleContactPoint(colliderB as PolygonCollider2D, colliderA as CapsuleCollider2D);
			else if (colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Capsule) //Polygon/Capsule
				contacts = PolygonCapsuleContactPoint(colliderA as PolygonCollider2D, colliderB as CapsuleCollider2D);

			return contacts;
		}

		public FixVector2 PolygonsContactPoint(PolygonCollider2D colliderA, PolygonCollider2D colliderB) 
		{
			FixVector2 Contact0 = FixVector2.Zero;
			FixVector2 Contact1 = FixVector2.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector2 point = colliderA.Points[i];
				for (int j = 0; j < colliderB.Points.Length; j++)
				{
					FixVector2 va = colliderB.Points[j];
					FixVector2 vb = colliderB.Points[(j + 1) % colliderB.Points.Length];

					LineToPointDistance(va, vb, point, out FixVector2 r1);

					Fix64 dist = FixVector2.Length(r1 - point);

					if (dist == minDist)
					{
						if (r1 != Contact0)
						{
							minDist = dist;
							Contact1 = r1;
						}
					}
					else if (dist < minDist)
					{
						minDist = dist;
						Contact0 = r1;
					}
				}
			}

			for (int i = 0; i < colliderB.Points.Length; i++)
			{
				FixVector2 point = colliderB.Points[i];
				for (int j = 0; j < colliderA.Points.Length; j++)
				{
					FixVector2 va = colliderA.Points[j];
					FixVector2 vb = colliderA.Points[(j + 1) % colliderA.Points.Length];

					LineToPointDistance(va, vb, point, out FixVector2 r1);

					Fix64 dist = FixVector2.Length(r1 - point);

					if (dist == minDist)
					{
						if (r1 != Contact0)
						{
							minDist = dist;
							Contact1 = r1;
						}
					}
					else if (dist < minDist)
					{
						minDist = dist;
						Contact0 = r1;
					}
				}
			}

			if (Contact1 != FixVector2.Zero)
				return (Contact0 + Contact1) / Fix64.Two;
			else
				return Contact0;
		}

		public FixVector2 PolygonCircleContactPoint(PolygonCollider2D colliderA, CircleCollider2D colliderB)
		{
			FixVector2 Contact = FixVector2.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector2 va = colliderA.Points[i];
				FixVector2 vb = colliderA.Points[(i + 1) % colliderA.Points.Length];

				LineToPointDistance(va, vb, colliderB.Center, out FixVector2 r1);

				Fix64 dist = FixVector2.Length(colliderB.Center - r1);

				if (dist < minDist)
				{
					minDist = dist;
					Contact = r1;
				}
			}

			return Contact;
		}

		public FixVector2 PolygonCapsuleContactPoint(PolygonCollider2D colliderA, CapsuleCollider2D colliderB)
		{
			FixVector2 Contact0 = FixVector2.Zero;

			Fix64 minDist = Fix64.MaxValue;

			for (int i = 0; i < colliderA.Points.Length; i++)
			{
				FixVector2 va = colliderA.Points[i];
				FixVector2 vb = colliderA.Points[(i + 1) % colliderA.Points.Length];

				LineToLineDistance(va, vb, colliderB.UpperPoint, colliderB.LowerPoint, out FixVector2 r1, out FixVector2 r2);

				Fix64 dist = FixVector2.Length(r2 - r1);
				FixVector2 dir = FixVector2.Normalize(r2 - r1);

				if (dist < minDist)
				{
					minDist = dist;
					Contact0 = r1;
				}
			}

			return Contact0;
		}

		public virtual FixVector2 Support(FixVector2 direction)
		{
			return direction;
		}
		
		public void UpdateBoundingBox()
		{
			BoundingBox = GetBoundingBoxPoints();
			BoundingBoxRequireUpdate = false;
		}

		protected virtual FixRect GetBoundingBoxPoints()
		{
			return new FixRect();
		}

		public virtual void UpdatePoints(SharpBody2D body)
		{
			Center = FixVector2.Transform(Offset, body);
			CollisionRequireUpdate = false;
		}

		public void LineToLineDistance(FixVector2 p1, FixVector2 p2, FixVector2 p3, FixVector2 p4, out FixVector2 r1, out FixVector2 r2)
		{
			FixVector2 r = p3 - p1;
			FixVector2 u = p2 - p1;
			FixVector2 v = p4 - p3;
			Fix64 ru = FixVector2.Dot(r, u);
			Fix64 rv = FixVector2.Dot(r, v);
			Fix64 uu = FixVector2.Dot(u, u);
			Fix64 uv = FixVector2.Dot(u, v);
			Fix64 vv = FixVector2.Dot(v, v);
			Fix64 det = uu * vv - uv * uv;

			Fix64 s, t;
			if (det < Fix64.ETA * uu * vv)
			{
				s = Fix64.Clamp01(ru / uu);
				t = Fix64.Zero;
			} 
			else
			{
				s = Fix64.Clamp01((ru * vv - rv * uv) / det);
				t = Fix64.Clamp01((ru * uv - rv * uu) / det);
			}

			Fix64 S = Fix64.Clamp01((t * uv + ru) / uu);
			Fix64 T = Fix64.Clamp01((s * uv - rv) / vv);

			r1 = p1 + S * u;
			r2 = p3 + T * v;
		}

		//Line to point collision code taken from Noah Zuo's Blog
		//https://arrowinmyknee.com/2021/03/15/some-math-about-capsule-collision/
		public void LineToPointDistance(FixVector2 p1, FixVector2 p2, FixVector2 p3, out FixVector2 r1)
		{
			FixVector2 ab = p2 - p1;
			Fix64 length = FixVector2.Dot(p3 - p1, ab);
			if (length <= Fix64.ETA) 
			{
				r1 = p1;
			} 
			else 
			{
				Fix64 denom = FixVector2.Dot(ab, ab);
				if (length >= denom) 
				{
					r1 = p2;
				} 
				else 
				{
					length = length / denom;
					r1 = p1 + length * ab;
				}
			}
		}
	}
}

public enum CollisionType2D
{
	Null = -1,
	AABB = 0,
	Circle = 1,
	Capsule = 2,
	Polygon = 3,
}

/*[Flags]
public enum CollisionFlags : byte
{
	Null = 0,
	Below = 0 << 0,
	Above = 0 << 1,
	Right = 0 << 2,
	Left = 0 << 3,
	Walls = Right | Left
}

[Flags]
public enum CollisionFlags3D : byte
{
	Null = 0,
	Below = 0 << 0,
	Above = 0 << 1,
	Right = 0 << 2,
	Left = 0 << 3,
	Front = 0 << 4,
	Back = 0 << 5,
	Walls = Right | Left | Front | Back
}*/

public struct CollisionFlags
{
	public bool Below;
	public bool Above;
	public bool Right;
	public bool Left;
	public bool Forward;
	public bool Back;

	public bool Walls => Right || Left || Forward || Back;
	public bool Any  => Below || Above || Right || Left || Forward || Back;

	public void Clear()
	{
		Below = false;
		Above = false;
		Right = false;
		Left = false;
		Forward = false;
		Back = false;
	}
	public bool Compare(CollisionFlags compareTo)
	{
		return Below == compareTo.Below || Above == compareTo.Above || 
			Right == compareTo.Right || Left == compareTo.Left || 
			Forward == compareTo.Forward || Back == compareTo.Back;
	}
	public bool ComparePositive(CollisionFlags compareTo)
	{
		return (Below && Below == compareTo.Below) ||
			(Above && Above == compareTo.Above) || 
			(Right && Right == compareTo.Right) ||
			(Left && Left == compareTo.Left) || 
			(Forward && Forward == compareTo.Forward) ||
			(Back && Back == compareTo.Back);
	}
	
	public bool Equals(CollisionFlags compareTo)
	{
		return Below == compareTo.Below && Above == compareTo.Above &&
			Right == compareTo.Right && Left == compareTo.Left &&
			Forward == compareTo.Forward && Back == compareTo.Back;
	}
    public override string ToString()
    {
        return $"(Below: {Below}, Above: {Above}, Right: {Right}, Left: {Left}, Forward: {Forward}, Back: {Back})";
    }
}


using Godot;
using System.Collections.Generic;
using FixMath.NET;
namespace SharpCollisions.Sharp2D.GJK
{
    public partial class GJK2D
    {
		public const int MAX_GJK_ITERATIONS = 32;
		public const int MAX_EPA_ITERATIONS = 32;
		private Simplex2D Simplex;
		private Polytope2D Polytope;

        public GJK2D()
		{
			Simplex = new Simplex2D();
			Polytope = new Polytope2D();
		}		
		private SupportPoint2D SupportFunction(SharpCollider2D colliderA, SharpCollider2D colliderB, FixVector2 direction)
		{
            FixVector2 SupportPointA = colliderA.Support(direction);
            FixVector2 SupportPointB = colliderB.Support(-direction);
			return new SupportPoint2D(SupportPointA, SupportPointB);
		}
        public bool PolygonCollision(SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out FixVector2 Depth, out FixVector2 ContactPoint)
		{
			int maxIterations = 0;
			
			Normal = FixVector2.Zero;
			Depth = FixVector2.Zero;
			ContactPoint = FixVector2.Zero;

			Simplex = new Simplex2D();

			FixVector2 supportDirection = colliderB.Center - colliderA.Center;
			SupportPoint2D SupportPoint;
			while (true)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_GJK_ITERATIONS) return false;
				SupportPoint = SupportFunction(colliderA, colliderB, supportDirection);
				if (!FixVector2.IsSameDirection(SupportPoint.Point(), supportDirection))
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
			FixVector2 a = Simplex.Points[0].Point();
			FixVector2 b = Simplex.Points[1].Point();
			FixVector2 ab = b - a;
			FixVector2 ao = a * Fix64.NegativeOne;
			supportDirection = FixVector2.TripleProduct(ab, ao, ab);
			return false;
		}
		private bool TriangleSimplex(ref Simplex2D Simplex, ref FixVector2 supportDirection)
		{
			SupportPoint2D a = Simplex.Points[0];
			SupportPoint2D b = Simplex.Points[1];
			SupportPoint2D c = Simplex.Points[2];
			FixVector2 ab = b.Point() - a.Point();
			FixVector2 ac = c.Point() - a.Point();
			FixVector2 ao = a.Point() * Fix64.NegativeOne;
			FixVector2 abPerp = FixVector2.TripleProduct(ac, ab, ab);
			FixVector2 acPerp = FixVector2.TripleProduct(ab, ac, ac);
			if(FixVector2.IsSameDirection(abPerp, ao))
			{
                // the origin is outside line ab
                // get rid of c and add a new support in the direction of abPerp
				Simplex.Reset(new List<SupportPoint2D>(){a, b});
                supportDirection = abPerp;
            }
            else if(FixVector2.IsSameDirection(acPerp, ao))
			{
                // the origin is outside line ac
                // get rid of b and add a new support in the direction of acPerp
				Simplex.Reset(new List<SupportPoint2D>(){a, c});
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
		private bool GetPolytopeDirection(Polytope2D polytope)
		{
			Fix64 e0 = (polytope.Vertices[1].Point().x - polytope.Vertices[0].Point().x) * (polytope.Vertices[1].Point().y + polytope.Vertices[0].Point().y);
			Fix64 e1 = (polytope.Vertices[2].Point().x - polytope.Vertices[1].Point().x) * (polytope.Vertices[2].Point().y + polytope.Vertices[1].Point().y);
			Fix64 e2 = (polytope.Vertices[0].Point().x - polytope.Vertices[2].Point().x) * (polytope.Vertices[0].Point().y + polytope.Vertices[2].Point().y);
			return e0 + e1 + e2 > Fix64.Zero;
		}
		private void EPA(Simplex2D simplex, SharpCollider2D colliderA, SharpCollider2D colliderB, out FixVector2 Normal, out Fix64 Depth, out FixVector2 Contact)
		{
			int maxIterations = 0;
			int minIndex = 0;
			Fix64 minDistance = Fix64.MaxValue;
			FixVector2 minNormal = FixVector2.Zero;
			Polytope = new Polytope2D(simplex.Points);
			bool IsClockWise = GetPolytopeDirection(Polytope);
			while (minDistance == Fix64.MaxValue)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_EPA_ITERATIONS) break;
				for (int i = 0; i < Polytope.Vertices.Count; i++)
				{
					int j = (i + 1) % Polytope.Vertices.Count;
					FixVector2 vertexI = Polytope.Vertices[i].Point();
					FixVector2 vertexJ = Polytope.Vertices[j].Point();
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
				SupportPoint2D support = SupportFunction(colliderA, colliderB, minNormal);
				Fix64 sDistance = FixVector2.Dot(minNormal, support.Point());
				if (Fix64.Abs(sDistance - minDistance) > Fix64.Epsilon)
				{
					minDistance = Fix64.MaxValue;
					Polytope.Vertices.Insert(minIndex, support);
				}
			}
			Normal = minNormal;
			Depth = Fix64.Abs(minDistance) + Fix64.Epsilon;
			Contact = GetContactPoint(colliderA, colliderB);
		}

        public FixVector2 GetContactPoint(SharpCollider2D colliderA, SharpCollider2D colliderB)
		{
			if (colliderA.Shape == CollisionType2D.Circle && colliderB.Shape == CollisionType2D.Polygon)
				return CirclePolygonContact(colliderA as CircleCollider2D, colliderB as ConvexShapeCollider2D);
			else if (colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Circle)
				return CirclePolygonContact(colliderB as CircleCollider2D, colliderA as ConvexShapeCollider2D);
			if (colliderA.Shape == CollisionType2D.Capsule && colliderB.Shape == CollisionType2D.Polygon)
				return CapsulePolygonContact(colliderA as CapsuleCollider2D, colliderB as ConvexShapeCollider2D);
			else if (colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Capsule)
				return CapsulePolygonContact(colliderB as CapsuleCollider2D, colliderA as ConvexShapeCollider2D);
			else if (colliderA.Shape == CollisionType2D.Polygon && colliderB.Shape == CollisionType2D.Polygon)
				return PolygonContact(colliderA as ConvexShapeCollider2D, colliderB as ConvexShapeCollider2D);
			
			return FixVector2.Zero;
		}

		public FixVector2 CirclePolygonContact(CircleCollider2D colliderA, ConvexShapeCollider2D colliderB)
		{
			FixVector2 contact = FixVector2.Zero;
            Fix64 minDistSq = Fix64.MaxValue;

            for(int i = 0; i < colliderB.Points.Length; i++)
            {
                FixVector2 va = colliderB.Points[i];
                FixVector2 vb = colliderB.Points[(i + 1) % colliderB.Points.Length];

                SharpCollider2D.LineToPointDistance(va, vb, colliderA.Center, out FixVector2 r1);
				Fix64 distSq = FixVector2.DistanceSq(colliderA.Center, r1);
				
				if(distSq < minDistSq)
                {
                    minDistSq = distSq;
                    contact = r1;
                }
            }

			return contact;
		}

		public FixVector2 CapsulePolygonContact(CapsuleCollider2D colliderA, ConvexShapeCollider2D colliderB)
		{
			FixVector2 contact1 = FixVector2.Zero;
            FixVector2 contact2 = FixVector2.Zero;

            Fix64 minDistSq = Fix64.MaxValue;

			for (int i = 0; i < colliderB.Points.Length; i++)
			{
				FixVector2 va = colliderB.Points[i];
				FixVector2 vb = colliderB.Points[(i + 1) % colliderB.Points.Length];

				SharpCollider2D.LineToLineDistance(va, vb, colliderA.UpperPoint, colliderA.LowerPoint, out FixVector2 r1, out FixVector2 r2);
				Fix64 distSq = FixVector2.DistanceSq(r2, r1);

				if (Fix64.Approximate(distSq, minDistSq))
				{
					if (!FixVector2.Approximate(r1, contact1))
					{
						contact2 = r1;
					}
				}
				else if (distSq < minDistSq)
				{
					minDistSq = distSq;
					contact1 = r1;
				}
				
				SharpCollider2D.LineToLineDistance(va, vb, colliderA.LowerPoint, colliderA.UpperPoint, out r1, out r2);
				distSq = FixVector2.DistanceSq(r2, r1);
				
				if(Fix64.Approximate(distSq, minDistSq))
				{
					if (!FixVector2.Approximate(r1, contact1))
					{
						contact2 = r1;
					}
				}
				else if(distSq < minDistSq)
				{
					minDistSq = distSq;
					contact1 = r1;
				}
            }

			if (contact2 == FixVector2.Zero)
				return contact1;
			else
				return (contact1 + contact2) / Fix64.Two;
		}

		public FixVector2 PolygonContact(ConvexShapeCollider2D colliderA, ConvexShapeCollider2D colliderB)
		{
			FixVector2 contact1 = FixVector2.Zero;
            FixVector2 contact2 = FixVector2.Zero;

            Fix64 minDistSq = Fix64.MaxValue;

            for(int i = 0; i < colliderA.Points.Length; i++)
            {
                FixVector2 pa = colliderA.Points[i];
				FixVector2 pb = colliderA.Points[(i + 1) % colliderA.Points.Length];

                for(int j = 0; j < colliderB.Points.Length; j++)
                {
                    FixVector2 va = colliderB.Points[j];
                    FixVector2 vb = colliderB.Points[(j + 1) % colliderB.Points.Length];

                    SharpCollider2D.LineToLineDistance(va, vb, pa, pb, out FixVector2 r1, out FixVector2 r2);
					Fix64 distSq = FixVector2.DistanceSq(r2, r1);

                    if(Fix64.Approximate(distSq, minDistSq))
                    {
                        if (!FixVector2.Approximate(r1, contact1))
                        {
                            contact2 = r1;
                        }
                    }
                    else if(distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contact1 = r1;
                    }
                }
            }

            for (int i = 0; i < colliderB.Points.Length; i++)
            {
                FixVector2 pa = colliderB.Points[i];
				FixVector2 pb = colliderB.Points[(i + 1) % colliderB.Points.Length];

                for (int j = 0; j < colliderA.Points.Length; j++)
                {
                    FixVector2 va = colliderA.Points[j];
                    FixVector2 vb = colliderA.Points[(j + 1) % colliderA.Points.Length];

                    SharpCollider2D.LineToLineDistance(va, vb, pa, pb, out FixVector2 r1, out FixVector2 r2);
					Fix64 distSq = FixVector2.DistanceSq(r2, r1);

                    if (Fix64.Approximate(distSq, minDistSq))
                    {
                        if (!FixVector2.Approximate(r1, contact1))
                        {
                            contact2 = r1;
                        }
                    }
                    else if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        contact1 = r1;
                    }
                }
            }

			if (contact2 == FixVector2.Zero)
				return contact1;
			else
				return (contact1 + contact2) / Fix64.Two;
		}
    }
}

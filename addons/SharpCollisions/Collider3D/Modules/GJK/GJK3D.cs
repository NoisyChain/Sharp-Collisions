using Godot;
using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions.Sharp3D.GJK
{
    public partial class GJK3D
    {
        public const int MAX_GJK_ITERATIONS = 32;
		public const int MAX_EPA_ITERATIONS = 32;

		private Simplex3D Simplex;
		private Polytope3D Polytope;
		bool useEPA;

		public GJK3D(bool epa)
		{
			Simplex = new Simplex3D();
			Polytope = new Polytope3D();
			useEPA = epa;
		}

        private SupportPoint3D SupportFunction(SharpCollider3D colliderA, SharpCollider3D colliderB, FixVector3 direction)
		{
            FixVector3 SupportPointA = colliderA.Support(direction);
            FixVector3 SupportPointB = colliderB.Support(-direction);

			return new SupportPoint3D { pointA = SupportPointA, pointB = SupportPointB};
		}

        public bool PolygonCollision(SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			int maxIterations = 0;

			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			Simplex = new Simplex3D();

			FixVector3 supportDirection = colliderB.Center - colliderA.Center;
			SupportPoint3D SupportPoint;

			while (true)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_GJK_ITERATIONS) return false;

				SupportPoint = SupportFunction(colliderA, colliderB, supportDirection);
				if (!FixVector3.IsSameDirection(SupportPoint.Point(), supportDirection))
					return false;

				Simplex.MoveForward(SupportPoint);

				if (CheckSimplex(ref Simplex, ref supportDirection))
				{
					EPA(Simplex, colliderA, colliderB, out Normal, out Fix64 newDepth, out ContactPoint);
					Depth = Normal * newDepth;

					/*if (useEPA)
					{
						EPA(Simplex, colliderA, colliderB, out Normal, out Fix64 newDepth, out ContactPoint);
						Depth = Normal * newDepth;
					}
					else
					{
						Normal = FixVector3.Zero;
						Depth = FixVector3.Zero;
						ContactPoint = FixVector3.Zero;
					}*/
					return true;
				}
			}
		}

		private bool CheckSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			switch (Simplex.Size)
			{
				case 1: //Point
					return PointSimplex(ref supportDirection);
				case 2: //Line
					return LineSimplex(ref Simplex, ref supportDirection);
				case 3: //Triangle
					return TriangleSimplex(ref Simplex, ref supportDirection);
				case 4: //Tetrahedron
                    return TetrahedronSimplex(ref Simplex, ref supportDirection);
			}

			//Should never be here
			return false;
		}

		private bool PointSimplex(ref FixVector3 supportDirection)
		{
			supportDirection *= Fix64.NegativeOne;
			return false;
		}

		private bool LineSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			FixVector3 a = Simplex.Points[0].Point();
			FixVector3 b = Simplex.Points[1].Point();

			FixVector3 ab = b - a;
			FixVector3 ao = a * Fix64.NegativeOne;

        	supportDirection = FixVector3.TripleProduct(ab, ao, ab);

			return false;
		}

		private bool TriangleSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			SupportPoint3D a = Simplex.Points[0];
			SupportPoint3D b = Simplex.Points[1];
			SupportPoint3D c = Simplex.Points[2];

			FixVector3 ab = b.Point() - a.Point();
			FixVector3 ac = c.Point() - a.Point();
			FixVector3 ao = a.Point() * Fix64.NegativeOne;
		
			FixVector3 abc = FixVector3.Cross(ab, ac);
		
			if (FixVector3.IsSameDirection(FixVector3.Cross(abc, ac), ao))
			{
				if (FixVector3.IsSameDirection(ac, ao))
				{
					Simplex.Reset(new List<SupportPoint3D>(){a, c});
					supportDirection = FixVector3.TripleProduct(ac, ao, ac);
				}

				else
				{
					Simplex.Reset(new List<SupportPoint3D>(){a, b});
					return LineSimplex(ref Simplex, ref supportDirection);
				}
			}
			else
			{
				if (FixVector3.IsSameDirection(FixVector3.Cross(ab, abc), ao))
				{
					Simplex.Reset(new List<SupportPoint3D>(){a, b});
					return LineSimplex(ref Simplex, ref supportDirection);
				}

				else
				{
					if (FixVector3.IsSameDirection(abc, ao))
					{
						supportDirection = abc;
					}

					else
					{
						Simplex.Reset(new List<SupportPoint3D>(){a, c, b});
						supportDirection = -abc;
					}
				}
			}

			return false;
		}
		bool TetrahedronSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			SupportPoint3D a = Simplex.Points[0];
			SupportPoint3D b = Simplex.Points[1];
			SupportPoint3D c = Simplex.Points[2];
			SupportPoint3D d = Simplex.Points[3];

			FixVector3 ab = b.Point() - a.Point();
			FixVector3 ac = c.Point() - a.Point();
			FixVector3 ad = d.Point() - a.Point();
			FixVector3 ao = a.Point() * Fix64.NegativeOne;
		
			FixVector3 abc = FixVector3.Cross(ab, ac);
			FixVector3 acd = FixVector3.Cross(ac, ad);
			FixVector3 adb = FixVector3.Cross(ad, ab);
		
			if (FixVector3.IsSameDirection(abc, ao))
			{
				Simplex.Reset(new List<SupportPoint3D>(){a, b, c});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
				
			if (FixVector3.IsSameDirection(acd, ao))
			{
				Simplex.Reset(new List<SupportPoint3D>(){a, c, d});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
		
			if (FixVector3.IsSameDirection(adb, ao))
			{
				Simplex.Reset(new List<SupportPoint3D>(){a, b, d});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
		
			return true;
		}

		private void Reconstruct(ref Polytope3D polytope, SupportPoint3D extendPoint)
		{
			//I do realize that this function can be done more efficietly
			List<int> removalFaces = new List<int>();
			for(int i = 0; i < polytope.Faces.Count; i++)
			{
				IntPack3 face = polytope.Faces[i];

				FixVector3 ab = polytope.Vertices[face.b].Point() - polytope.Vertices[face.a].Point();
				FixVector3 ac = polytope.Vertices[face.c].Point() - polytope.Vertices[face.a].Point();
				FixVector3 norm = FixVector3.Normalize(FixVector3.Cross(ab, ac));

				FixVector3 a0 = polytope.Vertices[face.a].Point() * Fix64.NegativeOne;
				if(FixVector3.IsSameDirection(a0, norm))
					norm *= Fix64.NegativeOne;

				if(FixVector3.IsSameDirection(norm, extendPoint.Point() - polytope.Vertices[face.a].Point()))
					removalFaces.Add(i);
			}

			//get the edges that are not shared between the faces that should be removed
			List<IntPack2> edges = new List<IntPack2>();
			for(int i = 0; i < removalFaces.Count; i++)
			{
				IntPack3 face = polytope.Faces[removalFaces[i]];
				IntPack2 edgeAB = new IntPack2(face.a, face.b);
				IntPack2 edgeAC = new IntPack2(face.c, face.a);
				IntPack2 edgeBC = new IntPack2(face.b, face.c);

				int k = EdgeInEdges(edges, edgeAB);
				if(k != -1)
					edges.RemoveAt(k);
				else
					edges.Add(edgeAB);

				k = EdgeInEdges(edges, edgeBC);
				if(k != -1)
					edges.RemoveAt(k);
				else
					edges.Add(edgeBC);
				k = EdgeInEdges(edges, edgeAC);
				if(k != -1)
					edges.RemoveAt(k);
				else
					edges.Add(edgeAC);
			}

			//remove the faces from the polytope
			for(int i = removalFaces.Count - 1; i >= 0; i--)
				polytope.Faces.RemoveAt(removalFaces[i]);

			//form new faces with the edges and new point
			for(int i = 0; i < edges.Count; i++)
				polytope.Faces.Add(new IntPack3(edges[i].a, edges[i].b, polytope.Vertices.Count - 1));
		}

		private int EdgeInEdges (List<IntPack2> edges, IntPack2 edge)
		{
			for(int i = 0; i < edges.Count; i++)
			{
				if (edges[i].IsReverse(edge))
					return i;
			}

			return -1;
		}

		private void FindClosestFace(ref Polytope3D polytope, out Fix64 distance, out FixVector3 normal)
		{
			distance = Fix64.MaxValue;
			normal = FixVector3.Zero;

			for (int i = 0; i < polytope.Faces.Count; i++)
			{
				IntPack3 face = polytope.Faces[i];

				FixVector3 norm = FixVector3.GetPlaneNormal(
					polytope.Vertices[face.a].Point(), 
					polytope.Vertices[face.b].Point(), 
					polytope.Vertices[face.c].Point()
				);

				FixVector3 a0 = polytope.Vertices[face.a].Point() * Fix64.NegativeOne;
				
				if(FixVector3.IsSameDirection(a0, norm))
					norm *= Fix64.NegativeOne;

				Fix64 dist = FixVector3.Dot(polytope.Vertices[face.a].Point(), norm);

				if(dist < distance)
				{
					distance = dist;
					normal = norm;
					polytope.ClosestFace = i;
				}
			}
		}

		private void EPA(Simplex3D simplex, SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out Fix64 Depth, out FixVector3 Contact)
		{
			int maxIterations = 0;

			List<IntPack3> faces = new List<IntPack3>()
			{
				new IntPack3(0, 1, 2),
				new IntPack3(0, 3, 1),
				new IntPack3(0, 2, 3),
				new IntPack3(1, 3, 2)
			};

			Polytope = new Polytope3D(simplex.Points, faces, 0);

			Normal = FixVector3.Zero;
			Depth = Fix64.MaxValue;
			Contact = FixVector3.Zero;

			while (true)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_EPA_ITERATIONS) break;

				FindClosestFace(ref Polytope, out Fix64 fDistance, out FixVector3 fNormal);
				
				SupportPoint3D support = SupportFunction(colliderA, colliderB, fNormal);
				Fix64 dist = FixVector3.Dot(support.Point(), fNormal);

				if (dist - fDistance < Fix64.Epsilon)
				{
					Normal = FixVector3.Normalize(fNormal);
					Depth = Fix64.Abs(fDistance) + Fix64.Epsilon;
					Contact = GetContactPoint(Polytope, Normal);
					break;
				}

				Polytope.Vertices.Add(support);
				Reconstruct(ref Polytope, support);
			}
		}

		//Contact point math solved thanks to GJKEPA
		//https://github.com/exatb/GJKEPA
		public FixVector3 GetContactPoint(Polytope3D polytope, FixVector3 normal)
		{
			Fix64 distance = FixVector3.Dot(polytope.Vertices[polytope.GetClosestFace().a].Point(), normal);
        	FixVector3 projectedPoint = -distance * normal;

			FixVector3 barCoord = SharpCollider3D.GetBarycentricCoordinates(
				projectedPoint, 
				polytope.Vertices[polytope.GetClosestFace().a].Point(), 
				polytope.Vertices[polytope.GetClosestFace().b].Point(), 
				polytope.Vertices[polytope.GetClosestFace().c].Point()
			);

			// In case a degenerate triangle is found, create a dummy point so it doesn't return 0
			if (barCoord == FixVector3.Zero)
			{
				barCoord = FixVector3.FindTriangleCentroid(
					polytope.Vertices[polytope.GetClosestFace().a].Point(), 
					polytope.Vertices[polytope.GetClosestFace().b].Point(), 
					polytope.Vertices[polytope.GetClosestFace().c].Point()
				);
			}

			FixVector3 contactPointA = (barCoord.x * polytope.Vertices[polytope.GetClosestFace().a].pointA) + 
										(barCoord.y * polytope.Vertices[polytope.GetClosestFace().b].pointA) + 
										(barCoord.z * polytope.Vertices[polytope.GetClosestFace().c].pointA);
			
			FixVector3 contactPointB = (barCoord.x * polytope.Vertices[polytope.GetClosestFace().a].pointB) + 
										(barCoord.y * polytope.Vertices[polytope.GetClosestFace().b].pointB) + 
										(barCoord.z * polytope.Vertices[polytope.GetClosestFace().c].pointB);

			return (contactPointA + contactPointB) / Fix64.Two;
		}
    }
}

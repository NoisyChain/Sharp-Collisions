using Godot;
using System.Collections.Generic;
using FixMath.NET;

namespace SharpCollisions.Sharp3D.GJK
{
    public partial class GJK3D
    {
		private bool AllowDraw = false;
        public const int MAX_GJK_ITERATIONS = 32;
		public const int MAX_EPA_ITERATIONS = 32;

		public GJK3D() {}
        public GJK3D(bool draw) { AllowDraw = draw; }

        private FixVector3 SupportFunction(SharpCollider3D colliderA, SharpCollider3D colliderB, FixVector3 direction)
		{
            FixVector3 SupportPointA = colliderA.Support(direction);
            FixVector3 SupportPointB = colliderB.Support(-direction);

			return SupportPointA - SupportPointB;
		}

        public bool PolygonCollision(SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out FixVector3 Depth, out FixVector3 ContactPoint)
		{
			int maxIterations = 0;

			Normal = FixVector3.Zero;
			Depth = FixVector3.Zero;
			ContactPoint = FixVector3.Zero;

			Simplex3D Simplex = new Simplex3D(
				new List<FixVector3>() { FixVector3.Zero, FixVector3.Zero, FixVector3.Zero, FixVector3.Zero }
			);

			FixVector3 supportDirection = colliderB.Center - colliderA.Center;
			FixVector3 SupportPoint;

			while (true)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_GJK_ITERATIONS) return false;

				SupportPoint = SupportFunction(colliderA, colliderB, supportDirection);
				if (!FixVector3.IsSameDirection(SupportPoint, supportDirection))
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
			FixVector3 a = Simplex.Points[0];
			FixVector3 b = Simplex.Points[1];

			FixVector3 ab = b - a;
			FixVector3 ao = a * Fix64.NegativeOne;

        	supportDirection = FixVector3.TripleProduct(ab, ao, ab);

			return false;
		}

		private bool TriangleSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			FixVector3 a = Simplex.Points[0];
			FixVector3 b = Simplex.Points[1];
			FixVector3 c = Simplex.Points[2];

			FixVector3 ab = b - a;
			FixVector3 ac = c - a;
			FixVector3 ao = a * Fix64.NegativeOne;
		
			FixVector3 abc = FixVector3.Cross(ab, ac);
		
			if (FixVector3.IsSameDirection(FixVector3.Cross(abc, ac), ao))
			{
				if (FixVector3.IsSameDirection(ac, ao))
				{
					Simplex.Reset(new List<FixVector3>(){a, c});
					supportDirection = FixVector3.TripleProduct(ac, ao, ac);
				}

				else
				{
					Simplex.Reset(new List<FixVector3>(){a, b});
					return LineSimplex(ref Simplex, ref supportDirection);
				}
			}
			else
			{
				if (FixVector3.IsSameDirection(FixVector3.Cross(ab, abc), ao))
				{
					Simplex.Reset(new List<FixVector3>(){a, b});
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
						Simplex.Reset(new List<FixVector3>(){a, c, b});
						supportDirection = -abc;
					}
				}
			}

			return false;
		}
		bool TetrahedronSimplex(ref Simplex3D Simplex, ref FixVector3 supportDirection)
		{
			FixVector3 a = Simplex.Points[0];
			FixVector3 b = Simplex.Points[1];
			FixVector3 c = Simplex.Points[2];
			FixVector3 d = Simplex.Points[3];

			FixVector3 ab = b - a;
			FixVector3 ac = c - a;
			FixVector3 ad = d - a;
			FixVector3 ao = a * Fix64.NegativeOne;
		
			FixVector3 abc = FixVector3.Cross(ab, ac);
			FixVector3 acd = FixVector3.Cross(ac, ad);
			FixVector3 adb = FixVector3.Cross(ad, ab);
		
			if (FixVector3.IsSameDirection(abc, ao))
			{
				Simplex.Reset(new List<FixVector3>(){a, b, c});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
				
			if (FixVector3.IsSameDirection(acd, ao))
			{
				Simplex.Reset(new List<FixVector3>(){a, c, d});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
		
			if (FixVector3.IsSameDirection(adb, ao))
			{
				Simplex.Reset(new List<FixVector3>(){a, b, d});
				return TriangleSimplex(ref Simplex, ref supportDirection);
			}
		
			return true;
		}

		private void Reconstruct(ref List<FixVector3> polytope, ref List<IntPack3> faces, FixVector3 extendPoint)
		{
			//I do realize that this function can be done more efficietly
			List<int> removalFaces = new List<int>();
			for(var i = 0; i < faces.Count; i++)
			{
				var face = faces[i];

				FixVector3 ab = polytope[face.b] - polytope[face.a];
				FixVector3 ac = polytope[face.c] - polytope[face.a];
				FixVector3 norm = FixVector3.Normalize(FixVector3.Cross(ab, ac));

				FixVector3 a0 = polytope[face.a] * Fix64.NegativeOne;
				if(FixVector3.IsSameDirection(a0, norm))
					norm *= Fix64.NegativeOne;

				if(FixVector3.IsSameDirection(norm, extendPoint - polytope[face.a]))
					removalFaces.Add(i);
			}

			//get the edges that are not shared between the faces that should be removed
			List<IntPack2> edges = new List<IntPack2>();
			for(int i = 0; i < removalFaces.Count; i++)
			{
				IntPack3 face = faces[removalFaces[i]];
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
			for(var i = removalFaces.Count - 1; i >= 0; i--)
				faces.RemoveAt(removalFaces[i]);

			//form new faces with the edges and new point
			for(int i = 0; i < edges.Count; i++)
				faces.Add(new IntPack3(edges[i].a, edges[i].b, polytope.Count - 1));
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

		private void FindClosestFace(ref List<FixVector3> polytope, ref List<IntPack3> faces, out Fix64 distance, out FixVector3 normal)
		{
			distance = Fix64.MaxValue;
			normal = FixVector3.Zero;

			for (int i = 0; i < faces.Count; i++)
			{
				IntPack3 face = faces[i];

				FixVector3 ab = polytope[face.b] - polytope[face.a];
				FixVector3 ac = polytope[face.c] - polytope[face.a];
				FixVector3 norm = FixVector3.Normalize(FixVector3.Cross(ab, ac));

				FixVector3 a0 = polytope[face.a] * Fix64.NegativeOne;
				
				if(FixVector3.IsSameDirection(a0, norm))
					norm *= Fix64.NegativeOne;

				Fix64 dist = FixVector3.Dot(polytope[face.a], norm);

				if(dist < distance)
				{
					distance = dist;
					normal = norm;
				}
			}
		}

		private void EPA(Simplex3D simplex, SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out Fix64 Depth, out FixVector3 Contact)
		{
			int maxIterations = 0;
			
			List<FixVector3> polytope = simplex.Points;
			List<IntPack3> simplexFaces  = new List<IntPack3>()
			{
				new IntPack3(0, 1, 2),
				new IntPack3(0, 3, 1),
				new IntPack3(0, 2, 3),
				new IntPack3(1, 3, 2)
			};

			Normal = FixVector3.Zero;
			Depth = Fix64.MaxValue;
			Contact = FixVector3.Zero;

			while (true)
			{
				//Break the loop after a while to avoid infinite loops
				//It should never happen, but better safe than sorry
				maxIterations++;
				if (maxIterations == MAX_EPA_ITERATIONS) break;

				FindClosestFace(ref polytope, ref simplexFaces, out Fix64 fDistance, out FixVector3 fNormal);
				
				FixVector3 support = SupportFunction(colliderA, colliderB, fNormal);
				Fix64 dist = FixVector3.Dot(support, fNormal);

				if (dist - fDistance < Fix64.ETA)
				{
					DrawPolytope(polytope, simplexFaces);
					Normal = FixVector3.Normalize(fNormal);
					Depth = Fix64.Abs(fDistance) + Fix64.ETA;
					Contact = GetContactPoint(colliderA, colliderB); //Not functional yet
					break;
				}

				polytope.Add(support);
				Reconstruct(ref polytope, ref simplexFaces, support);
			}
		}

        public FixVector3 GetContactPoint(SharpCollider3D colliderA, SharpCollider3D colliderB)
		{
			FixVector3 contacts = FixVector3.Zero;

			return contacts;
		}

		private void DrawPolytope(List<FixVector3> polytope, List<IntPack3> simplexFaces)
		{
			if (!AllowDraw) return;

			DebugDraw3D.DrawSphere(Vector3.Zero, 0.03f, new Color(0f, 0f, 0f));
			
			for (int i = 0; i < simplexFaces.Count; i++)
			{
				Vector3 a = (Vector3)polytope[simplexFaces[i].a];
				Vector3 b = (Vector3)polytope[simplexFaces[i].b];
				Vector3 c = (Vector3)polytope[simplexFaces[i].c];
				DebugDraw3D.DrawLine(a, b);
				DebugDraw3D.DrawLine(b, c);
				DebugDraw3D.DrawLine(c, a);
			}
		}
    }
}

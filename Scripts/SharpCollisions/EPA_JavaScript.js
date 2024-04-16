var EPA = function(aWorldVerts, bWorldVerts, simplex)
{
    var simplexFaces = [{a: 0, b: 1, c: 2},
                        {a: 0, b: 1, c: 3},
                        {a: 0, b: 2, c: 3},
                        {a: 1, b: 2, c: 3}];

    var ret = null;

    while(true)
    {
        var face = findClosestFace(simplex, simplexFaces);
        var point = support(aWorldVerts, bWorldVerts, face.norm);
        var dist = point.clone().dot(face.norm);

        if(dist - face.dist < 0.00001)
        {
            ret = {axis: face.norm, dist: dist};
            break;
        }

        simplex.push(point);
        reconstruct(simplex, simplexFaces, point);
    }

    return ret;
}

var reconstruct = function(simplex, simplexFaces, extendPoint)
{
    //I do realize that this function can be done more efficietly
    var removalFaces = [];
    for(var i = 0; i < simplexFaces.length; i++)
    {
        var face = simplexFaces[i];

        var ab = simplex[face.b].clone().sub(simplex[face.a]);
        var ac = simplex[face.c].clone().sub(simplex[face.a]);
        var norm = ab.cross(ac).normalize();

        var a0 = new THREE.Vector3().sub(simplex[face.a]);
        if(a0.dot(norm) > 0)
            norm.negate();

        if(norm.clone().dot(extendPoint.clone().sub(simplex[face.a])) > 0)
        {
            removalFaces.push(i);
        }
    }

    //get the edges that are not shared between the faces that should be removed
    var edges = [];
    for(var i = 0; i < removalFaces.length; i++)
    {
        var face = simplexFaces[removalFaces[i]];
        var edgeAB = {a: face.a, b: face.b};
        var edgeAC = {a: face.a, b: face.c};
        var edgeBC = {a: face.b, b: face.c};

        var k = edgeInEdges(edges, edgeAB);
        if(k != -1)
            edges.splice(k, 1);
        else
            edges.push(edgeAB);

        k = edgeInEdges(edges, edgeAC);
        if(k != -1)
            edges.splice(k, 1);
        else
            edges.push(edgeAC);

        k = edgeInEdges(edges, edgeBC);
        if(k != -1)
            edges.splice(k, 1);
        else
            edges.push(edgeBC);
    }

    //remove the faces from the polytope
    for(var i = removalFaces.length - 1; i >= 0; i--)
    {
        simplexFaces.splice(removalFaces[i], 1);
    }

    //form new faces with the edges and new point
    for(var i = 0; i < edges.length; i++)
    {
        simplexFaces.push({a: edges[i].a, b: edges[i].b, c: simplex.length - 1});
    }
}

var edgeInEdges = function(edges, edge)
{
    for(var i = 0; i < edges.length; i++)
    {
        if(edges[i].a == edge.a && edges[i].b == edge.b)
            return i;
    }

    return -1;
}

var findClosestFace = function(simplex, simplexFaces)
{
    var closest = {dist: Infinity};

    for(var i = 0; i < simplexFaces.length; i++)
    {
        var face = simplexFaces[i];

        var ab = simplex[face.b].clone().sub(simplex[face.a]);
        var ac = simplex[face.c].clone().sub(simplex[face.a]);
        var norm = ab.cross(ac).normalize();

        var a0 = new THREE.Vector3().sub(simplex[face.a]);
        if(a0.dot(norm) > 0)
            norm.negate();

        var dist = simplex[face.a].clone().dot(norm);

        if(dist < closest.dist)
        {
            closest = {index: i, dist: dist, norm: norm, a: face.a, b: face.b, c: face.c};
        }
    }

    return closest;
}

var support = function(aVerts, bVerts, dir)
{
    a = getFurthestPointInDirection(aVerts, dir);
    b = getFurthestPointInDirection(bVerts, dir.clone().negate());
    return a.clone().sub(b);
}

var getFurthestPointInDirection = function(verts, dir)
{
    var index = 0;
    var maxDot = verts[index].clone().dot(dir.clone().normalize());

    for(var i = 1; i < verts.length; i++)
    {
        var dot = verts[i].clone().dot(dir.clone().normalize());

        if(dot > maxDot) {
            maxDot = dot;
            index = i;
        }
    }

    return verts[index];
}

//The other EPA algorithm
/*private void GetFaceNormals(ref List<FixVector3> polytope, ref List<int> faces, out List<FixVector3> normals, out List<Fix64> distances, out int minFace)
		{
			normals = new List<FixVector3>();
			distances = new List<Fix64>();
			minFace = 0;
			Fix64 minDistance = Fix64.MaxValue;

			for (int i = 0; i < faces.Count; i += 3)
			{
				FixVector3 a = polytope[faces[i]];
				FixVector3 b = polytope[faces[i + 1]];
				FixVector3 c = polytope[faces[i + 2]];

				FixVector3 normal = FixVector3.Normalize(FixVector3.Cross(b - a, c - a));
				FixVector3 a0 = a * Fix64.NegativeOne;

				if (FixVector3.IsSameDirection(a0, normal))
					normal *= Fix64.NegativeOne;

				Fix64 distance = FixVector3.Dot(normal, a);

				normals.Add(normal);
				distances.Add(distance);

				if (distance < minDistance)
				{
					minFace = i / 3;
					minDistance = distance;
				}
			}
		}

		private void AddIfUniqueEdge(ref List<(int, int)> edges, ref List<int> faces, int a, int b)
		{
			//      0--<--3
			//     / \ B /   A: 2-0
			//    / A \ /    B: 0-2
			//   1-->--2
			var faceA = faces[a];
			var faceB = faces[b];
			int remove = -1;

			for(int i = 0; i < edges.Count; i++)
			{
				if (edges[i] == (faceB, faceA))
					remove = i;
			}

			if (remove != -1)
				edges.RemoveAt(remove);
			else
				edges.Add((faceA, faceB));
		}

		private void EPA(Simplex3D simplex, SharpCollider3D colliderA, SharpCollider3D colliderB, out FixVector3 Normal, out Fix64 Depth, out FixVector3 Contact)
		{
			List<FixVector3> polytope = simplex.Points;
			List<int> faces  = new List<int>()
			{
				0, 1, 2,
				0, 3, 1,
				0, 2, 3,
				1, 3, 2
			};

			GetFaceNormals(ref polytope, ref faces, out List<FixVector3> normals, out List<Fix64> distances, out int minFace);

			Fix64 minDistance = Fix64.MaxValue;
			FixVector3 minNormal = FixVector3.Zero;
			int limitIterations = 0;

			while (minDistance == Fix64.MaxValue)
			{
				limitIterations++;
				minNormal = normals[minFace];
				minDistance = distances[minFace];
		
				FixVector3 support = SupportFunction(colliderA, colliderB, minNormal);
				Fix64 sDistance = FixVector3.Dot(minNormal, support);
		
				if (sDistance - minDistance > Fix64.ETA)
				{
					//Doing this just to avoid infinite loops
					if (limitIterations == 64)
					{
						//DrawPolytope(polytope);
						//GD.Print(Fix64.Abs(sDistance - minDistance));
						minDistance = Fix64.Zero;
						break;
					}

					minDistance = Fix64.MaxValue;
					//std::vector<std::pair<size_t, size_t>> uniqueEdges;
					List<(int, int)> uniqueEdges = new List<(int, int)>();

					for (int i = 0; i < normals.Count; i++)
					{
						if (FixVector3.Dot(normals[i], support) > FixVector3.Dot(normals[i], polytope[faces[i * 3]]))
						//if (FixVector3.IsSameDirection(normals[i], support))
						{
							int f = i * 3;

							AddIfUniqueEdge(ref uniqueEdges, ref faces, f, f + 1);
							AddIfUniqueEdge(ref uniqueEdges, ref faces, f + 1, f + 2);
							AddIfUniqueEdge(ref uniqueEdges, ref faces, f + 2, f);

							faces[f + 2] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);
							faces[f + 1] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);
							faces[f    ] = faces[faces.Count - 1]; faces.RemoveAt(faces.Count - 1);

							normals[i] = normals[normals.Count - 1];
							normals.RemoveAt(normals.Count - 1);
							distances[i] = distances[distances.Count - 1];
							distances.RemoveAt(distances.Count - 1);

							i--;
						}
					}

					//std::vector<size_t> newFaces;
					List<int> newFaces = new List<int>();
					foreach ((int, int) edges in uniqueEdges)
					{
						newFaces.AddRange(new List<int>() { edges.Item1, edges.Item2, polytope.Count});
					}
				
					polytope.Add(support);

					GetFaceNormals(ref polytope, ref newFaces, out List<FixVector3> newNormals, out List<Fix64> newDistances, out int newMinFace);

					Fix64 oldMinDistance = Fix64.MaxValue;
					for (int i = 0; i < distances.Count; i++)
					{
						if (distances[i] < oldMinDistance)
						{
							oldMinDistance = distances[i];
							minFace = i;
						}
					}

					if (newDistances[newMinFace] < oldMinDistance)
					{
						minFace = newMinFace + distances.Count;
					}

					faces.AddRange(newFaces);
					normals.AddRange(newNormals);
					distances.AddRange(newDistances);
				}
			}
			
			Normal = FixVector3.Normalize(minNormal);
			Depth = Fix64.Abs(minDistance) + Fix64.ETA;
			Contact = GJKGetContactPoint(colliderA, colliderB);
			DrawPolytope(polytope, faces);
		}

		private void DrawPolytope(List<FixVector3> polytope, List<int> simplexFaces)
		{
			if (!DrawDebug) return;
			
			for (int i = 0; i < polytope.Count; i++)
			{
				int f = i * 3;
				Vector3 a = (Vector3)polytope[simplexFaces[f]];
				Vector3 b = (Vector3)polytope[simplexFaces[f + 1]];
				Vector3 c = (Vector3)polytope[simplexFaces[f + 2]];
				DebugDraw.Line(a, b);
				DebugDraw.Line(b, c);
				DebugDraw.Line(c, a);
				DebugDraw.Sphere((Vector3)polytope[i], 0.1f, new Color(0f, 1f, 0f));
			}
		}*/
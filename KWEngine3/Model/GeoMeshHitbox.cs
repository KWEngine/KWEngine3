using Assimp;
using OpenTK.Mathematics;

namespace KWEngine3.Model
{
    internal class GeoMeshHitbox
    {
        public override string ToString()
        {
            return Name;
        }
        internal Vector2i indexLeftRightMostVertex = new Vector2i(0);
        internal Vector2i indexBackFrontMostVertex = new Vector2i(0);
        internal Vector2i indexBottomTopMostVertex = new Vector2i(0);

        internal GeoMeshFace[] Faces { get; private set; }

        internal bool IsExtended { get; private set; } = false;

        public string Name { get; internal set; }
        internal float maxX, maxY, maxZ;
        internal float minX, minY, minZ;

        internal float width, height, depth;

        internal Vector3[] Vertices = new Vector3[8];
        internal Vector3[] Normals = new Vector3[3];
        internal Vector3 Center = new Vector3(0, 0, 0);

        internal Matrix4 Transform = Matrix4.Identity;

        public GeoModel Model { get; internal set; } = null;

        internal ColliderType _colliderType = ColliderType.ConvexHull;

        public GeoMeshHitbox(float maxX, float maxY, float maxZ, float minX, float minY, float minZ, Mesh meshData = null)
        {
            IsExtended = meshData != null;

            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;

            Center.X = minX + ((maxX - minX) / 2f);
            Center.Y = minY + ((maxY - minY) / 2f);
            Center.Z = minZ + ((maxZ - minZ) / 2f);

            width = maxX - minX;
            height = maxY - minY;
            depth = maxZ - minZ;

            if(IsExtended)
            {
                
                List<Vector3> tmpVertices = new List<Vector3>();
                List<Vector3> tmpNormals = new List<Vector3>();

                for(int i = 0; i < meshData.VertexCount; i++)
                {
                    Vector3 tmpVertex = new Vector3(meshData.Vertices[i].X, meshData.Vertices[i].Y, meshData.Vertices[i].Z);
                    if(!tmpVertices.Contains(tmpVertex))
                    {
                        tmpVertices.Add(tmpVertex);
                    }
                }
                Vertices = new Vector3[tmpVertices.Count];

                // Analyse normals and skip those who are identical or negated:
                for (int i = 0; i < meshData.Normals.Count; i++)
                {
                    Vector3 normalToBeAdded = new Vector3(meshData.Normals[i].X, meshData.Normals[i].Y, meshData.Normals[i].Z);
                    int identicalVectorIndex = -1;
                    for (int j = 0; j < tmpNormals.Count; j++)
                    {
                        if ((tmpNormals[j].X == normalToBeAdded.X && tmpNormals[j].Y == normalToBeAdded.Y && tmpNormals[j].Z == normalToBeAdded.Z)
                            || (tmpNormals[j].X == -normalToBeAdded.X && tmpNormals[j].Y == -normalToBeAdded.Y && tmpNormals[j].Z == -normalToBeAdded.Z))
                        {
                            identicalVectorIndex = j;
                            break;
                        }
                    }
                    if(identicalVectorIndex < 0)
                    {
                        tmpNormals.Add(normalToBeAdded);
                    }
                }
                Normals = tmpNormals.ToArray();

                float left = float.MaxValue;
                float bottom = float.MaxValue;
                float back = float.MaxValue;
                float right = float.MinValue;
                float top = float.MinValue;
                float front = float.MinValue;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    if (Vertices[i].X < left)
                    {
                        left = Vertices[i].X;
                        indexLeftRightMostVertex.X = i;
                    }
                    if (Vertices[i].X > right)
                    {
                        right = Vertices[i].X;
                        indexLeftRightMostVertex.Y = i;
                    }
                    if (Vertices[i].Y < bottom)
                    {
                        bottom = Vertices[i].Y;
                        indexBottomTopMostVertex.X = i;
                    }
                    if (Vertices[i].Y > top)
                    {
                        top = Vertices[i].Y;
                        indexBottomTopMostVertex.Y = i;
                    }
                    if (Vertices[i].Z < back)
                    {
                        back = Vertices[i].Z;
                        indexBackFrontMostVertex.X = i;
                    }
                    if (Vertices[i].Z > front)
                    {
                        front = Vertices[i].Z;
                        indexBackFrontMostVertex.Y = i;
                    }
                }
                // TODO: Find the most opposite X/Y/Z values...
            }
            else
            {
                Faces = new GeoMeshFace[6];

                Vertices[0] = new Vector3(minX, minY, maxZ); // frontleftdown
                Vertices[1] = new Vector3(maxX, minY, maxZ); // frontrightdown
                Vertices[2] = new Vector3(maxX, minY, minZ); // backrightdown
                Vertices[3] = new Vector3(minX, minY, minZ); // backleftdown

                Vertices[4] = new Vector3(minX, maxY, maxZ); // frontleftup
                Vertices[5] = new Vector3(maxX, maxY, maxZ); // frontrightup
                Vertices[6] = new Vector3(maxX, maxY, minZ); // backrightup
                Vertices[7] = new Vector3(minX, maxY, minZ); // backleftup

                Normals[0] = new Vector3(1, 0, 0);
                Normals[1] = new Vector3(0, 1, 0);
                Normals[2] = new Vector3(0, 0, 1);

                // Create faces of hitbox for sutherland-hodgman clipping algorithm:
                Faces[0] = new GeoMeshFace(2, false, 0, 1, 5, 4); // front
                Faces[1] = new GeoMeshFace(2, true, 2, 3, 7, 6);  // back
                Faces[2] = new GeoMeshFace(0, true, 3, 0, 4, 7);  // left
                Faces[3] = new GeoMeshFace(0, false, 1, 2, 6, 5); // right
                Faces[4] = new GeoMeshFace(1, false, 4, 5, 6, 7); // top
                Faces[5] = new GeoMeshFace(1, true, 0, 1, 2, 3);  // bottom

                indexLeftRightMostVertex.X = 0;
                indexLeftRightMostVertex.Y = 1;
                indexBottomTopMostVertex.X = 3;
                indexBottomTopMostVertex.Y = 7;
                indexBackFrontMostVertex.X = 3;
                indexBackFrontMostVertex.Y = 0;
            } 
        }

        public GeoMeshHitbox(float maxX, float maxY, float maxZ, float minX, float minY, float minZ, List<Vector3> uniqueNormals = null, List<Vector3> uniqueVertices = null, List<GeoMeshFace> uniqueFaces = null)
        {
            IsExtended = uniqueNormals != null && uniqueVertices != null && uniqueFaces != null;

            this.maxX = maxX;
            this.maxY = maxY;
            this.maxZ = maxZ;
            this.minX = minX;
            this.minY = minY;
            this.minZ = minZ;

            Center.X = minX + ((maxX - minX) / 2f);
            Center.Y = minY + ((maxY - minY) / 2f);
            Center.Z = minZ + ((maxZ - minZ) / 2f);

            width = maxX - minX;
            height = maxY - minY;
            depth = maxZ - minZ;

            if (IsExtended)
            {
                Vertices = uniqueVertices.ToArray();
                Normals = uniqueNormals.ToArray();

                Faces = new GeoMeshFace[uniqueFaces.Count];
                for(int i = 0; i < Faces.Length; i++)
                {
                    Faces[i] = uniqueFaces[i];
                }

                float left = float.MaxValue;
                float bottom = float.MaxValue;
                float back = float.MaxValue;
                float right = float.MinValue;
                float top = float.MinValue;
                float front = float.MinValue;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    if (Vertices[i].X < left)
                    {
                        left = Vertices[i].X;
                        indexLeftRightMostVertex.X = i;
                    }
                    if (Vertices[i].X > right)
                    {
                        right = Vertices[i].X;
                        indexLeftRightMostVertex.Y = i;
                    }
                    if (Vertices[i].Y < bottom)
                    {
                        bottom = Vertices[i].Y;
                        indexBottomTopMostVertex.X = i;
                    }
                    if (Vertices[i].Y > top)
                    {
                        top = Vertices[i].Y;
                        indexBottomTopMostVertex.Y = i;
                    }
                    if (Vertices[i].Z < back)
                    {
                        back = Vertices[i].Z;
                        indexBackFrontMostVertex.X = i;
                    }
                    if (Vertices[i].Z > front)
                    {
                        front = Vertices[i].Z;
                        indexBackFrontMostVertex.Y = i;
                    }
                }
                // TODO: Find the most opposite X/Y/Z values...
            }
            else
            {
                Vertices[0] = new Vector3(minX, minY, maxZ); // frontleftdown
                Vertices[1] = new Vector3(maxX, minY, maxZ); // frontrightdown
                Vertices[2] = new Vector3(maxX, minY, minZ); // backrightdown
                Vertices[3] = new Vector3(minX, minY, minZ); // backleftdown

                Vertices[4] = new Vector3(minX, maxY, maxZ); // frontleftup
                Vertices[5] = new Vector3(maxX, maxY, maxZ); // frontrightup
                Vertices[6] = new Vector3(maxX, maxY, minZ); // backrightup
                Vertices[7] = new Vector3(minX, maxY, minZ); // backleftup

                Normals[0] = new Vector3(1, 0, 0);
                Normals[1] = new Vector3(0, 1, 0);
                Normals[2] = new Vector3(0, 0, 1);

                // Create faces of hitbox for sutherland-hodgman clipping algorithm:
                Faces = new GeoMeshFace[6];
                Faces[0] = new GeoMeshFace(2, false, 0, 1, 5, 4); // front
                Faces[1] = new GeoMeshFace(2, true, 2, 3, 7, 6);  // back
                Faces[2] = new GeoMeshFace(0, true, 3, 0, 4, 7);  // left
                Faces[3] = new GeoMeshFace(0, false, 1, 2, 6, 5); // right
                Faces[4] = new GeoMeshFace(1, false, 4, 5, 6, 7); // top
                Faces[5] = new GeoMeshFace(1, true, 3, 2, 1, 0);  // bottom

                indexLeftRightMostVertex.X = 0;
                indexLeftRightMostVertex.Y = 1;
                indexBottomTopMostVertex.X = 3;
                indexBottomTopMostVertex.Y = 7;
                indexBackFrontMostVertex.X = 3;
                indexBackFrontMostVertex.Y = 0;
            }
        }
    }
}

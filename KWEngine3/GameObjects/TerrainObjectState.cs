using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.GameObjects
{
    internal struct TerrainObjectState
    {
        internal Vector3 _dimensions = Vector3.Zero;
        internal Vector3 _center = Vector3.Zero;
        internal TerrainObject _terrainObject = null;
        internal Matrix4 _modelMatrix = Matrix4.Identity;
        internal Matrix4 _normalMatrix = Matrix4.Identity;
        internal Vector4 _colorEmissive = new Vector4(1, 1, 1, 0);
        internal Vector3 _colorTint = Vector3.One;
        internal Vector4 _uvTransform = new Vector4(1, 1, 0, 0); //zw = offset.xy
        internal Vector3 _position;

        public TerrainObjectState():this(null)
        {
        }

        public TerrainObjectState(TerrainObject terrainObject)
        {
            if(terrainObject == null)
            {
                throw new ArgumentNullException();
            }
            _position = Vector3.Zero;
            this._terrainObject = terrainObject;
        }
    }
}

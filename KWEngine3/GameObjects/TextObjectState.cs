using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3.GameObjects
{
    internal struct TextObjectState
    {
        public TextObject _parent;
        public Vector3 _position;
        public Quaternion _rotation;
        public float _scale;
        public float _width;
        public float _spreadFactor;
        public Vector4 _color;
        public Vector4 _colorEmissive;
        public Matrix4 _modelMatrix;

        public TextObjectState(TextObject parent)
        {
            _parent = parent;
            _position = Vector3.Zero;
            _rotation = Quaternion.Identity;
            _scale = 1f;
            _spreadFactor = 1f;
            _color = new Vector4(1, 1, 1, 1);
            _colorEmissive = new Vector4(0, 0, 0, 0);
            _modelMatrix = Matrix4.Identity;
            _width = 1f;
        }
    }
}

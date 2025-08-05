using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldEQ
{
    internal class EQBlip : GameObject
    {
        private float _currentEmissive = 0f;
        private bool _isActivated = false;
        private const float REDUCE = 0.2f;
        private const float MAXEMISSIVE = 2.25f;
        private const float R = 0.2f;
        private const float G = 0.6f;
        private const float B = 1.0f;
        private bool _isTop = false;

        public override void Act()
        {
            if(_isActivated == false && _currentEmissive > 0f)
            {
                _currentEmissive = MathF.Max(0, _currentEmissive - (_isTop ? REDUCE * 0.05f : REDUCE));
                SetColorEmissive(R, G, B, _currentEmissive);
                if (_currentEmissive == 0)
                    _isTop = false;
            }
        }

        public void Activate()
        {
            SetColorEmissive(R, G, B, MAXEMISSIVE);
            _isActivated = true;
            _currentEmissive = MAXEMISSIVE;
        }

        public void Deactivate()
        {
            _isActivated = false;
        }

        public void SetAsTop(bool t)
        {
            _isTop = t;
        }

        public bool IsTop()
        {
            return _isTop && _currentEmissive > 0f;
        }
    }
}

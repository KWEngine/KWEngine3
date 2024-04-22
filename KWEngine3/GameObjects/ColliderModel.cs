using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.GameObjects
{
    internal class ColliderModel
    {
        public List<GameObjectHitbox> _hitboxes = new();
        public List<GameObjectHitbox> _hitboxesNew = new();
        //public string _customName = null;
    }
}

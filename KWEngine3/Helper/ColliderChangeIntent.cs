using KWEngine3.GameObjects;

namespace KWEngine3.Helper
{
    internal class ColliderChangeIntent
    {
        public GameObject _objectToChange = null;
        public AddRemoveHitboxMode _mode = AddRemoveHitboxMode.None;
        public List<GameObjectHitbox> _hitboxesNew = new();
        public string _customColliderName = ""; // "" = nochange, null = remove
        public string _customColliderFilename = "";
    }
}

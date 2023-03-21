using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFirstPersonView;
using OpenTK.Mathematics;
using System.Drawing;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldFirstPersonViewImport : World
    {
        public override void Act()
        {
          
        }

        public override void Prepare()
        {
            LoadJSON(@".\Worlds\2023-03-20_world-export.json");

            GameObject p = GetGameObjectByName("Player #1");
            if (p != null)
            {
                MouseCursorGrab();
                SetCameraToFirstPersonGameObject(p, 0.5f);
            }
        }
    }
}

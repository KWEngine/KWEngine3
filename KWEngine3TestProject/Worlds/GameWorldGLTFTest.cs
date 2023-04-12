using KWEngine3;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldGLTFTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(50, 50, 50);


            KWEngine.LoadModel("Barn", "./Models/Barn/barn.gltf");

            Immovable i01 = new Immovable();
            i01.Name = "Barn";
            i01.SetModel("Barn");
            AddGameObject(i01);
        }
    }
}

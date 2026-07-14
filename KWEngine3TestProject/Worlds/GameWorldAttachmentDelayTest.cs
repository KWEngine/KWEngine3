using KWEngine3;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldAttachmentDelayTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldAttachmentDelayTest : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Player", "./Models/AttachmentTest/Character.glb");
            KWEngine.LoadModel("Item", "./Models/AttachmentTest/Weapon.gltf");

            SetCameraPosition(0, 4, 10);
            SetCameraTarget(0, 4, 0);

            


            Item i = new Item();
            i.Name = "Weapon";
            i.SetModel("Item");
            AddGameObject(i);

            Player p = new Player();
            p.Name = "Player";
            p.SetModel("Player");
            p.SetAnimationID(3);
            p.SetRotation(0, 180, 0);
            AddGameObject(p);
            p.StartJump();


            // attach item:
            p.AttachGameObjectToBone(i, "Torso");
            HelperGameObjectAttachment.SetRotationForAttachment(i, 0, 90, 0);
            HelperGameObjectAttachment.SetPositionOffsetForAttachment(i, 0.5f, -0.1f, -0.2f);
        }
    }
}

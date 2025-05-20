using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject
{
    public class GameWorldEmpty : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Barn", "./Models/Barn/barntest.gltf");

            RenderObjectDefault ro = new RenderObjectDefault();
            ro.SetModel("Barn");
            ro.SetPosition(0, 5, -20);
            //ro.SetScale(20, 10, 40);
            //ro.SetColor(1, 1, 0);
            AddRenderObject(ro);

            Player p = new Player();
            p.SetPosition(0, 5, 25);
            p.SetScale(2);
            p.SetOpacity(0);
            p.SetRotation(0, 180, 0);
            AddGameObject(p);
            SetCameraToFirstPersonGameObject(p);

            MouseCursorGrab();


        }
    }
}

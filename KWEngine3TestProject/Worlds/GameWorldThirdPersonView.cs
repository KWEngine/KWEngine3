using KWEngine3;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldThirdPersonView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldThirdPersonView : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            Immovable f01 = new Immovable();
            f01.Name = "Floor";
            f01.SetModel("KWCube");
            f01.SetTexture(@"./textures/tiles_albedo.jpg", TextureType.Albedo);       // regular texture file
            f01.SetTexture(@"./textures/tiles_normal.jpg", TextureType.Normal);       // (optional) normal map for custom light reflections
            f01.SetTexture(@"./textures/tiles_roughness.png", TextureType.Roughness); // (optional) roughness map for specular highlights
            f01.SetTextureRepeat(3, 3);                                               // how many times the texture is tiled across the object?
            f01.SetScale(15, 0.2f, 15);
            f01.SetPosition(0, -0.1f, 0);
            f01.IsShadowCaster = true;                                                // does the object cast and receive shadows? (default: false)
            AddGameObject(f01);

            PlayerThirdPerson p01 = new PlayerThirdPerson();
            p01.SetModel("KWCube");
            p01.SetPosition(0, 1, 0);
            p01.SetScale(1, 2, 1);
            p01.IsShadowCaster = true;
            p01.SetRotation(0, 180, 0);
            AddGameObject(p01);

            MouseCursorGrab();
        }
    }
}

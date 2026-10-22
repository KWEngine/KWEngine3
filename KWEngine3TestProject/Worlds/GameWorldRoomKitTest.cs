using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldRoomKitTest : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            //LoadJSON("./JSON/GameWorldRoomKitTest.json");

            SetCameraPosition(0, 2, 10);
            SetCameraTarget(0, 2, 0);
            SetCameraFOV(90);

            KWEngine.LoadModel("CorridorEnd", "./Models/RoomKit/SciFi/Corridor_SciFi_End.glb");
            KWEngine.LoadCollider("CorridorEnd_HB", "./Models/RoomKit/SciFi/Corridor_SciFi_End_Hitboxes.glb", ColliderType.ConvexHull);

            Immovable test = new Immovable();
            test.SetModel("CorridorEnd");

            test.SetTexture("./Textures/RoomKit/scifi_tile_02_albedo.dds", TextureType.Albedo, 0);
            test.SetTexture("./Textures/RoomKit/scifi_tile_02_metallic.dds", TextureType.Metallic, 0);
            test.SetTexture("./Textures/RoomKit/scifi_tile_02_roughness.dds", TextureType.Roughness, 0);
            test.SetTexture("./Textures/RoomKit/scifi_tile_02_normal.dds", TextureType.Normal, 0);
            test.SetTextureRepeat(0.5f, 0.5f, 0);

            test.SetTexture("./Textures/RoomKit/modern-metal-wall_albedo.dds", TextureType.Albedo, 1);
            test.SetTexture("./Textures/RoomKit/modern-metal-wall_height.dds", TextureType.Height, 1);
            test.SetTexture("./Textures/RoomKit/modern-metal-wall_roughness.dds", TextureType.Roughness, 1);
            test.SetTexture("./Textures/RoomKit/modern-metal-wall_normal.dds", TextureType.Normal, 1);
            test.SetMetallic(0.9f, 1);

            test.SetTexture("./Textures/RoomKit/scifi_tile_02_albedo.dds", TextureType.Albedo, 2);
            test.SetTexture("./Textures/RoomKit/scifi_tile_02_metallic.dds", TextureType.Metallic, 2);
            test.SetTexture("./Textures/RoomKit/scifi_tile_02_roughness.dds", TextureType.Roughness, 2);
            test.SetTexture("./Textures/RoomKit/scifi_tile_02_normal.dds", TextureType.Normal, 2);
            test.SetTextureRepeat(0.5f, 0.5f, 2);

            test.SetParallaxOcclusionMappingScale(0.5f);
            test.IsCollisionObject = true;
            test.SetColliderModel("CorridorEnd_HB");
            test.SetRotation(0, 180, 0);
            AddGameObject(test);

            LightObjectPoint l = new LightObjectPoint(ShadowQuality.NoShadow);
            l.SetPosition(0, 3, 0);
            l.SetColor(1, 1, 1, 1.5f);
            l.SetNearFar(0, 3.5f);
            AddLightObject(l);

        }
    }
}

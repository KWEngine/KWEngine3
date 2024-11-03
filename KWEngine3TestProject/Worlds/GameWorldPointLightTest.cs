using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldPointLightTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldPointLightTest : World
    {
        public override void Act()
        {

        }

        // Prepare() wird einmalig beim Laden der Welt-Instanz (also bei Anzeige im Fenster) geladen:
        public override void Prepare()
        {
            KWEngine.LoadModel("Toon", "./Models/PlatformerPack/Toon.glb");
            Floor f = new Floor();              // Erstelle eine neue Instanz der Floor-Klasse
            f.Name = "FloorCube";               // Benenne die Instanz
            f.SetScale(10.0f, 2.0f, 202.0f);     // Skaliere die Instanz (X, Y, Z)
            f.SetPosition(0.0f, -1.0f, -100.0f);   // Positioniere den Boden (bei Würfeln ist die Position die Würfelmitte!)
            f.IsCollisionObject = true;         // Markiere die Instanz als Objekt für Kollisionsprüfungen
            f.IsShadowCaster = true;            // Markiere die Instanz als schattenwerfend
            f.SetColor(1.0f, 0.0f, 0.0f);
            AddGameObject(f);                   // Füge die Instanz der Welt hinzu


            Player p = new Player("Link", -2.5f, 0.0f, -5.0f);            // Gleiches gilt für die Player-Instanz (andere Klasse, ähnliche Konfiguration)
            AddGameObject(p);

            HUDObject GOS = new HUDObjectImage("./Models/GameOverScreen.png");
            GOS.Name = "GameOverScreen";
            GOS.SetPosition(640f, 360f);
            GOS.SetOpacity(0.0f);
            GOS.SetScale(1280f, 720f);
            AddHUDObject(GOS);
            
            Obstacle ob1 = new Obstacle();
            ob1.Name = "ob1";
            ob1.SetScale(5.0f, 5.0f, 2.0f);
            ob1.SetPosition(4f, 2.5f, -20.0f);
            ob1.IsCollisionObject = true;
            ob1.IsShadowCaster = true;
            ob1.SetColor(0.0f, 1.0f, 0.0f);
            AddGameObject(ob1);

            Obstacle ob2 = new Obstacle();
            ob2.Name = "ob2";
            ob2.SetScale(2.0f, 7.0f, 0.5f);
            ob2.SetPosition(2.0f, 3.5f, -60.0f);
            ob2.IsCollisionObject = true;
            ob2.IsShadowCaster = true;
            ob2.SetColor(0.0f, 1.0f, 0.0f);
            AddGameObject(ob2);

            Obstacle ob3 = new Obstacle();
            ob3.Name = "ob3";
            ob3.SetScale(4.0f, 5.0f, 0.5f);
            ob3.SetPosition(4.0f, 2.5f, -90.0f);
            ob3.IsCollisionObject = true;
            ob3.IsShadowCaster = true;
            ob3.SetColor(0.0f, 1.0f, 0.5f);
            AddGameObject(ob3);

            Obstacle ob4 = new Obstacle();
            ob4.Name = "ob4";
            ob4.SetScale(5.0f, 5.0f, 2.0f);
            ob4.SetPosition(2.5f, 2.5f, -120.0f);
            ob4.IsCollisionObject = true;
            ob4.IsShadowCaster = true;
            ob4.SetColor(0.8f, 0.0f, 0.8f);
            AddGameObject(ob4);

            Obstacle ob5 = new Obstacle();
            ob5.Name = "ob5";
            ob5.SetScale(27.0f, 2.0f, 17.0f);
            ob5.SetPosition(16.0f, 1.0f, -150.0f);
            ob5.IsCollisionObject = true;
            ob5.IsShadowCaster = true;
            ob5.SetColor(0.7f, 0.9f, 0.0f);
            AddGameObject(ob5);

            Obstacle ob6 = new Obstacle();
            ob6.Name = "ob6";
            ob6.SetScale(0.5f, 0.1f, 202.0f);
            ob6.SetPosition(5.0f, 0.05f, -100.0f);
            ob6.IsCollisionObject = true;
            ob6.IsShadowCaster = true;
            ob6.SetColor(0.1f, 0.1f, 0.1f);
            AddGameObject(ob6);

            Obstacle ob7 = new Obstacle();
            ob7.Name = "ob7";
            ob7.SetScale(5.0f, 5.0f, 5.0f);
            ob7.SetPosition(4.0f, 2.5f, -180.0f);
            ob7.IsCollisionObject = true;
            ob7.IsShadowCaster = true;
            ob7.SetColor(0.0f, 0.0f, 0.2f);
            AddGameObject(ob7);

            SetCameraPosition(0.0f, 10f, 15f);
            SetColorAmbient(0.3f, 0.3f, 0.3f);
        }
    }
}

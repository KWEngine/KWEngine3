using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldPointLightTest
{
    internal class Player : GameObject
    {
        private LightObject _torch;

        public Player(string Name, float posX, float posY, float posZ)
        {
            SetModel("Toon");
            this.Name = "Link";
            SetScale(1.0f, 1.0f, 1.0f);
            //SetScale(0.01f, 0.01f, 0.01f);
            SetPosition(posX, posY, posZ);
            //SetPosition(posX, 0.5f, posZ);
            //SetRotation(180.0f, 0.0f, -180.0f);
            SetRotation(0, 180, 0);
            this.IsCollisionObject = true;
            this.IsShadowCaster = true;
            

            
            
            _torch = new LightObject(LightType.Point, ShadowQuality.Medium);
            _torch.Name = "Kopffackel";
            _torch.SetPosition(0.0f, 3.0f, 0.0f);
            _torch.SetNearFar(0.01f, 20f);                  // 0,5m ab licht entstehen Schatten, licht reicht 37,1m weit
            _torch.SetColor(1.0f, 0.05f, 1.0f, 5f);
            
            /*
            _torch = new LightObject(LightType.Directional, ShadowQuality.Medium);
            _torch.Name = "Kopffackel";
            _torch.SetPosition(0.0f, 3.0f, 0.0f);
            _torch.SetNearFar(0.01f, 20f);                  // 0,5m ab licht entstehen Schatten, licht reicht 37,1m weit
            _torch.SetColor(1.0f, 0.05f, 1.0f, 5f);
            */
            CurrentWorld.AddLightObject(_torch);
        }

        private bool moving;
        private float IdleAnPercent;
        private float MoveAnPercent;
        private bool collided = false;

        // Act() wird von der Engine mit einer festen Aktualisierungsrate von 240Hz aufgerufen
        public override void Act()
        {
            this.moving = true;

            if (this.Position.Z > -200 && collided != true)
            {
                MoveOffset(0.0f, 0.0f, -0.02f);      // Bewege die Instanz um 0.01 Einheiten entlang der z-Achse nach vorne
            }

            if (Keyboard.IsKeyDown(Keys.A) == true && this.Position.X > -4.5 && collided != true)  // Ist die Taste 'A' gedrückt?
            {
                MoveOffset(-0.01f, 0.0f, 0.0f);      // Wenn ja, bewege die Instanz um 0.01 Einheiten entlang der x-Achse nach links
            }

            if (Keyboard.IsKeyDown(Keys.D) == true && this.Position.X < 4.5 && collided != true)  // Ist die Taste 'D' gedrückt?
            {
                MoveOffset(0.01f, 0.0f, 0.0f);      // Wenn ja, bewege die Instanz um 0.01 Einheiten entlang der x-Achse nach rechts
            }

            if (MoveAnPercent >= 1.0f)
            {
                this.MoveAnPercent = 0.0f;
            }


            if (IdleAnPercent >= 1.0f)
            {
                this.IdleAnPercent = 0.0f;
            }

            if (moving && collided == false)
            {
                SetAnimationID(11);
                this.MoveAnPercent += 0.005f;
                SetAnimationPercentage(MoveAnPercent);
                this.IdleAnPercent = 0.0f;
            }

            if (collided == true)
            {
                SetAnimationID(17);
                this.IdleAnPercent += 0.001f;
                SetAnimationPercentage(IdleAnPercent);
            }

            List<Intersection> collissions = GetIntersections();
            foreach (Intersection i in collissions)
            {
                if (i.Object is Floor || i.Object is Player)
                {
                    continue;
                }
                GameObject collider = i.Object;
                Console.WriteLine("Collider is: " + collider.Name);
                Vector3 mtv = i.MTV;
                MoveOffset(mtv);
                this.collided = true;
            }

            if (collided)
            {
                HUDObjectImage GOS = CurrentWorld.GetHUDObjectImageByName("GameOverScreen");
                if (GOS != null)
                {
                    GOS.SetOpacity(1.0f);
                }
            }

            CurrentWorld.SetCameraPosition(this.Position.X - 0, this.Position.Y + 7.5f * 2, this.Position.Z + 10.0f * 2);
            CurrentWorld.SetCameraTarget(this.Position);

            _torch.SetPosition(this.Position.X - 0.0f, this.Position.Y + 8.0f, this.Position.Z - 5f);
            _torch.SetTarget(this.Position);
        }
    }
}

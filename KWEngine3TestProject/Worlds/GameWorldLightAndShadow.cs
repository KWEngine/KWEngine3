using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldLightAndShadow;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldLightAndShadow : World
    {
        private LightObject _sun;
        private float _sunDegrees = -45f;
        private LightObject _mouseLight;
        private Immovable _mouseLightSphere;

        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.F1))
            {
                Window.SetWorld(new GameWorldJumpAndRunPhysics());
            }
            else if (Keyboard.IsKeyDown(Keys.F2))
            {
                Window.SetWorld(new GameWorldLightAndShadow());
            }
            else if (Keyboard.IsKeyDown(Keys.F3))
            {
                Window.SetWorld(new GameWorldPlatformerPack());
            }

            // SUN CONTROL:
            // Move sun around the scene:
            Vector3 newSunPosition = HelperRotation.CalculatePositionAfterRotationAroundPointOnAxis(
                                                        new Vector3(0, 20, 0),      // the point to rotate around (aka pivot)
                                                        20,                         // the radius to that point
                                                        _sunDegrees,                // the degrees of rotation
                                                        Axis.Y                      // the axis/plane to rotate around
                                                        );
            _sun.SetPosition(newSunPosition);
            _sunDegrees = (_sunDegrees - 0.25f) % 360f;   // this will cycle from 0 to 359 and then back to 0


            // RED LIGHT CONTROL:
            Vector3 mouseCursorPos = HelperIntersection.GetMouseIntersectionPointOnPlane(Plane.XZ, 3);
            _mouseLight.SetPosition(mouseCursorPos);
            _mouseLightSphere.SetPosition(mouseCursorPos);
        }

        public override void Prepare()
        {
            // Load custom model from model folder as "Nightshade":
            KWEngine.LoadModel("Nightshade", @"./Models/WorldLightAndShadow/Nightshade.fbx");
            SetCameraFOV(45);

            // Initialize _sun attribute with a new LightObject instance of type "sun":
            _sun = new LightObjectSun(ShadowQuality.High);
            _sun.SetPosition(20, 20, 20);
            _sun.SetFOV(25);                            // sun's field of view (the higher, the wider)
            _sun.SetColor(1f, 1f, 1f, 0.25f);   // sunlight color and intensity 
            _sun.SetNearFar(10, 50);                    // define the distance range (z-depth) that the sun 'sees'
            AddLightObject(_sun);

            // Initialize another light object that follows the mouse cursor:
            _mouseLight = new LightObjectPoint(ShadowQuality.High);
            _mouseLight.SetPosition(0, 5, 0);
            _mouseLight.SetColor(1, 0, 0, 5);
            _mouseLight.SetNearFar(0.1f, 5);
            AddLightObject(_mouseLight);

            // Initialize sphere object that shows the mouse cursor's position (not needed but a nice gimmick):
            _mouseLightSphere = new Immovable();
            _mouseLightSphere.SetModel("KWSphere");
            _mouseLightSphere.SetColorEmissive(1, 0.25f, 0.25f, 4.5f);
            _mouseLightSphere.SetColor(1, 0.25f, 0.125f);
            _mouseLightSphere.SetScale(0.25f);
            AddGameObject(_mouseLightSphere);

            SetColorAmbient(0.05f, 0.10f, 0.15f);        // determine the ambient light color (for pixels that are not seen by the sun)
            SetCameraPosition(0, 25, 25);

            Immovable f01 = new Immovable();
            f01.SetModel("KWCube");
            f01.SetTexture(@"./textures/tiles_albedo.jpg", TextureType.Albedo);       // regular texture file
            f01.SetTexture(@"./textures/tiles_normal.jpg", TextureType.Normal);       // (optional) normal map for custom light reflections
            f01.SetTexture(@"./textures/tiles_roughness.png", TextureType.Roughness); // (optional) roughness map for specular highlights
            f01.SetTextureRepeat(3, 3);                                               // how many times the texture is tiled across the object?
            f01.SetScale(15, 0.2f, 15);
            f01.SetPosition(0, -0.1f, 0);
            f01.IsShadowCaster = true;                                                // does the object cast and receive shadows? (default: false)
            AddGameObject(f01);

            SphereRotating i01 = new SphereRotating();
            i01.SetModel("KWSphere");
            i01.SetTexture(@"./textures/iron_panel_albedo.dds", TextureType.Albedo);
            i01.SetTexture(@"./textures/iron_panel_normal.dds", TextureType.Normal);
            i01.SetTexture(@"./textures/iron_panel_roughness.dds", TextureType.Roughness);
            i01.SetTexture(@"./textures/iron_panel_metal.dds", TextureType.Metallic);
            i01.SetPosition(5, 2.5f, 5);
            i01.SetScale(5);
            i01.IsShadowCaster = true;                                                // does the object cast and receive shadows? (default: false)
            AddGameObject(i01);

            PlayerNightshade i02 = new PlayerNightshade();
            i02.SetModel("Nightshade");
            i02.Name = "Nightshade";
            i02.SetPosition(-2.5f, 0, -1.25f);
            i02.SetScale(2f);
            i02.IsShadowCaster = true;                                                // does the object cast and receive shadows? (default: false)
            i02.SetAnimationID(0);                                                    // set the object's animation id (for animated models)
            AddGameObject(i02);
        }
    }
}

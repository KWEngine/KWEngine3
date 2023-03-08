using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.Collections.Generic;

namespace KWEngine3.Helper
{
    internal class SerializedWorld
    {
        // CAMERA
        public float[] CamPosition { get; set; }
        public float[] CamTarget { get; set; }
        public float CamFOV { get; set; }
        public string Type { get; set; }


        // COLOR & BACKGROUND
        public float[] ColorAmbient { get; set; }
        public float ColorBackgroundBoost { get; set; }
        public string BackgroundSkyBox { get; set; }
        public float BackgroundSkyboxRotation { get; set; }
        public string Background2D { get; set; }
        public float[] Background2DRepeat { get; set; }
        public float[] Background2DOffset { get; set; }
        public float[] Background2DClip { get; set; }

        // IMPORTED MODELS
        // ???

        // GAMEOBJECTS
        public List<SerializedGameObject> GameObjects { get; set; }
        public List<SerializedLightObject> LightObjects { get; set; }
        public List<SerializedTerrainObject> TerrainObjects { get; set; }
        public List<SerializedHUDObject> HUDObjects { get; set; }

        public static SerializedWorld GenerateWorldExportFor(World w)
        {
            // Prerequisites:
            Quaternion.ToEulerAngles(w._background._rotation.ExtractRotation(true), out Vector3 r);

            SerializedWorld wj = new SerializedWorld();
            wj.Type = w.GetType().FullName;

            //CAMERA
            wj.CamPosition = new float[] { w._cameraGame._stateCurrent._position.X, w._cameraGame._stateCurrent._position.Y, w._cameraGame._stateCurrent._position.Z };
            wj.CamTarget = new float[] { w._cameraGame._stateCurrent._target.X, w._cameraGame._stateCurrent._target.Y, w._cameraGame._stateCurrent._target.Z };
            wj.CamFOV = (float)Math.Round(w._cameraGame._stateCurrent._fov * 2);

            // COLOR & BACKGROUND
            wj.ColorAmbient = new float[] { w._colorAmbient.X, w._colorAmbient.Y, w._colorAmbient.Z };
            wj.ColorBackgroundBoost = w._background._brightnessMultiplier;
            wj.BackgroundSkyBox = w._background.Type == BackgroundType.Skybox ? w._background._filename : "";
            wj.BackgroundSkyboxRotation = w._background.Type == BackgroundType.Skybox ? MathHelper.RadiansToDegrees(r.Y) : 0f;

            wj.Background2D = w._background.Type == BackgroundType.Standard ? w._background._filename : "";
            wj.Background2DClip = new float[] { w._background.Clip.X, w._background.Clip.Y };
            wj.Background2DOffset = new float[] { w._background.Offset.X, w._background.Offset.Y };
            wj.Background2DRepeat = new float[] { w._background.Scale.X, w._background.Scale.Y };

            wj.GameObjects = GenerateGameObjects(w);
            wj.TerrainObjects = GenerateTerrainObjects(w);
            wj.LightObjects = GenerateLightObjects(w);
            wj.HUDObjects = GenerateHUDObjects(w);

            return wj;
        }

        public static List<SerializedHUDObject> GenerateHUDObjects(World w)
        {
            List<SerializedHUDObject> hudObjects = new List<SerializedHUDObject>();
            foreach (HUDObject h in w._hudObjects)
            {
                hudObjects.Add(SerializedHUDObject.GenerateSerializedHUDObject(h));
            }
            return hudObjects;
        }
        public static List<SerializedGameObject> GenerateGameObjects(World w)
        {
            List<SerializedGameObject> gameObjects = new List<SerializedGameObject>();
            foreach (GameObject g in w._gameObjects)
            {
                gameObjects.Add(SerializedGameObject.GenerateSerializedGameObject(g));
            }
            return gameObjects;
        }
        public static List<SerializedLightObject> GenerateLightObjects(World w)
        {
            List<SerializedLightObject> lightObjects = new List<SerializedLightObject>();

            foreach (LightObject l in w._lightObjects)
            {
                lightObjects.Add(SerializedLightObject.GenerateSerializedLightObject(l));
            }

            return lightObjects;
        }
        public static List<SerializedTerrainObject> GenerateTerrainObjects(World w)
        {
            List<SerializedTerrainObject> terrainObjects = new List<SerializedTerrainObject>();

            foreach (TerrainObject t in w._terrainObjects)
            {
                terrainObjects.Add(SerializedTerrainObject.GenerateSerializedTerrainObject(t));
            }

            return terrainObjects;
        }
    }
}

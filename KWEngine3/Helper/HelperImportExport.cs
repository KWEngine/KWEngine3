using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using System.Reflection;
using System.Text.Json;

namespace KWEngine3.Helper
{
    internal static class HelperImportExport
    {
        public static bool ExportWorld(World w)
        {
            SerializedWorld wex = SerializedWorld.GenerateWorldExportFor(w);

            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.General);
            options.MaxDepth = 8;
            options.WriteIndented = true;

            string json = "";
            try
            {
                json = JsonSerializer.Serialize(wex, options);
            }
            catch(Exception ex)
            {
                KWEngine.LogWriteLine("[Export] Error: " + ex.Message);
            }
            if(json.Length > 0)
            {
                string filename = DateTime.Now.ToString("yyyy-MM-dd") + "_world-export.json";
                bool ok = false;
                try
                {
                    File.WriteAllText(filename, json);
                    ok = true;
                    
                }
                catch(Exception ex)
                {
                    KWEngine.LogWriteLine("[Export] Error: " + ex.Message);
                }
                if(ok)
                {
                    KWEngine.LogWriteLine("[Export] " + filename + " written.");
                    return true;
                }
                return false;
            }
            return false;
        }

        public static void ImportWorld(World w, string filename)
        {
            if (filename == null)
                filename = "";
            if(File.Exists(filename.Trim()))
            {
                string json = File.ReadAllText(filename.Trim());
                try 
                {
                    SerializedWorld sw = JsonSerializer.Deserialize<SerializedWorld>(json);
                    BuildWorld(w, sw);
                }
                catch(Exception)
                {
                    KWEngine.LogWriteLine("[Import] Error reading JSON");
                }
            }
            else
            {
                KWEngine.LogWriteLine("[Import] " + filename + " not found");
            }
        }

        public static void BuildWorld(World w, SerializedWorld sw)
        {
            w.SetCameraPosition(sw.CamPosition[0], sw.CamPosition[1], sw.CamPosition[2]);
            w.SetCameraTarget(sw.CamTarget[0], sw.CamTarget[1], sw.CamTarget[2]);
            w.SetCameraFOV((int)sw.CamFOV);

            w.SetColorAmbient(sw.ColorAmbient[0], sw.ColorAmbient[1], sw.ColorAmbient[2]);
            w.SetBackgroundBrightnessMultiplier(sw.ColorBackgroundBoost);
            if(sw.Background2D.Length != 0)
            {
                w.SetBackground2D(sw.Background2D);
                w.SetBackground2DClip(sw.Background2DClip[0], sw.Background2DClip[1]);
                w.SetBackground2DOffset(sw.Background2DOffset[0], sw.Background2DOffset[1]);
                w.SetBackground2DRepeat(sw.Background2DRepeat[0], sw.Background2DRepeat[1]);
            }
            else if(sw.BackgroundSkyBox.Length != 0)
            {
                w.SetBackgroundSkybox(sw.BackgroundSkyBox, sw.BackgroundSkyboxRotation);
            }

            // Build and add game object instances:
            foreach(SerializedGameObject sg in sw.GameObjects)
            {
                if(!IsBuiltInModel(sg.ModelName))
                {
                    KWEngine.LoadModel(sg.ModelName, sg.ModelPath);
                }
                w.AddGameObject(BuildGameObject(sg));
            }

            // Build and add light object instances:
            foreach(SerializedLightObject sl in sw.LightObjects)
            {
                w.AddLightObject(BuildLightObject(sl));
            }

            // Load terrain heightmaps:
            List<string> usedterrainNames = new List<string>();
            foreach (SerializedTerrainObject st in sw.TerrainObjects)
            {
                if(!usedterrainNames.Contains(st.ModelName))
                {
                    KWEngine.BuildTerrainModel(st.ModelName, st.ModelPath, st.TextureAlbedo, st.Width, st.Height, st.Depth);
                    usedterrainNames.Add(st.ModelName);
                }
            }

            // Build and add terrain object instances:
            foreach (SerializedTerrainObject st in sw.TerrainObjects)
            {
                w.AddTerrainObject(BuildTerrainObject(st));
            }
            
            // Build and add hud object instances:
            foreach(SerializedHUDObject sh in sw.HUDObjects)
            {
                w.AddHUDObject(BuildHUDObject(sh));
            }

            KWEngine.CurrentWorld.AddRemoveGameObjects();
            KWEngine.CurrentWorld.AddRemoveTerrainObjects();
            KWEngine.CurrentWorld.AddRemoveLightObjects();
            KWEngine.CurrentWorld.AddRemoveHUDObjects();
        }

        private static HUDObject BuildHUDObject(SerializedHUDObject sh)
        {
            HUDObject h = new HUDObject(sh.Type, 0, 0);
            h.Name = sh.Name;
            h.IsVisible = sh.IsVisible;
            if(sh.Type == HUDObjectType.Image)
            {
                h.SetTexture(sh.Texture);
            }
            else
            {
                h.SetFont(sh.Font);
                h.SetText(sh.Text);
            }
            h.SetScale(sh.Scale[0], sh.Scale[1]);
            h.SetPosition(sh.Position[0], sh.Position[1]);

            return h;
        }

        private static TerrainObject BuildTerrainObject(SerializedTerrainObject st)
        {
            TerrainObject t = new TerrainObject(st.ModelName);

            t.Name = st.Name;
            t.IsShadowCaster = st.IsShadowCaster;
            t.IsCollisionObject = st.IsCollisionObject;

            t.SetPosition(st.Position[0], st.Position[1], st.Position[2]);
            t.SetColor(st.Color[0], st.Color[1], st.Color[2]);
            t.SetColorEmissive(st.ColorEmissive[0], st.ColorEmissive[1], st.ColorEmissive[2], st.ColorEmissive[3]);
            t.SetMetallic(st.Metallic);
            t.SetRoughness(st.Roughness);

            if (IsBuiltInModel(st.ModelName) && IsTextureSet(st.TextureAlbedo))
                t.SetTexture(st.TextureAlbedo);

            if (IsBuiltInModel(st.ModelName) && IsTextureSet(st.TextureNormal))
                t.SetTexture(st.TextureNormal, TextureType.Normal);

            if (IsBuiltInModel(st.ModelName) && IsTextureSet(st.TextureRoughness))
                t.SetTexture(st.TextureRoughness, TextureType.Roughness);

            if (IsBuiltInModel(st.ModelName) && IsTextureSet(st.TextureMetallic))
                t.SetTexture(st.TextureMetallic, TextureType.Metallic);

            if (IsBuiltInModel(st.ModelName) && IsTextureSet(st.TextureEmissive))
                t.SetTexture(st.TextureEmissive, TextureType.Emissive);

            return t;
        }

        private static LightObject BuildLightObject(SerializedLightObject sl)
        {
            LightObject l = new LightObject(sl.LightType, sl.ShadowCasterType);

            l.Name = sl.Name;
            l._shadowBias = sl.ShadowBias;
            l.SetPosition(sl.Position[0], sl.Position[1], sl.Position[2]);
            if (l.Type != LightType.Point)
                l.SetTarget(sl.Target[0], sl.Target[1], sl.Target[2]);
            l.SetNearFar(sl.Near, sl.Far);
            l.SetFOV(sl.FOV);
            l.SetColor(sl.Color[0], sl.Color[1], sl.Color[2], sl.Color[3]);

            return l;
        }

        private static GameObject BuildGameObject(SerializedGameObject sg)
        {
            GameObject g = (GameObject)Assembly.GetEntryAssembly().CreateInstance(sg.Type);
            g.SetModel(sg.ModelName);
            g.IsShadowCaster = sg.IsShadowCaster;
            g.IsCollisionObject = sg.IsCollisionObject;
            g.UpdateLast = sg.UpdateLast;
            g.Name = sg.Name;

            g.SetPosition(sg.Position[0], sg.Position[1], sg.Position[2]);
            g.SetScale(sg.Scale[0], sg.Scale[1], sg.Scale[2]);
            g.SetRotation(new Quaternion(sg.Rotation[0], sg.Rotation[1], sg.Rotation[2], sg.Rotation[3]));
            g.SetHitboxScale(sg.ScaleHitbox[0], sg.ScaleHitbox[1], sg.ScaleHitbox[2]);

            g.SetColor(sg.Color[0], sg.Color[1], sg.Color[2]);
            g.SetColorEmissive(sg.ColorEmissive[0], sg.ColorEmissive[1], sg.ColorEmissive[2], sg.ColorEmissive[3]);
            g.SetMetallic(sg.Metallic);
            g.SetMetallicType(sg.MetallicType);
            g.SetRoughness(sg.Roughness);

            g.SetTextureOffset(sg.TextureOffset[0], sg.TextureOffset[1]);
            g.SetTextureRepeat(sg.TextureRepeat[0], sg.TextureRepeat[1]);

            if (IsBuiltInModel(sg.ModelName) && IsTextureSet(sg.TextureAlbedo))
                g.SetTexture(sg.TextureAlbedo);

            if (IsBuiltInModel(sg.ModelName) && IsTextureSet(sg.TextureNormal))
                g.SetTexture(sg.TextureNormal, TextureType.Normal);

            if (IsBuiltInModel(sg.ModelName) && IsTextureSet(sg.TextureRoughness))
                g.SetTexture(sg.TextureRoughness, TextureType.Roughness);

            if (IsBuiltInModel(sg.ModelName) && IsTextureSet(sg.TextureMetallic))
                g.SetTexture(sg.TextureMetallic, TextureType.Metallic);

            if (IsBuiltInModel(sg.ModelName) && IsTextureSet(sg.TextureEmissive))
                g.SetTexture(sg.TextureEmissive, TextureType.Emissive);



            return g;
        }

        private static bool IsBuiltInModel(string name)
        {
            return name == "KWCube" || name == "KWSphere" || name == "KWQuad";
        }

        private static bool IsTextureSet(string texname)
        {
            return texname != null && texname.Trim().Length > 0;
        }
    }
}

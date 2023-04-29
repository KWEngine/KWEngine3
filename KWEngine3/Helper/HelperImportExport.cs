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
                string filename = wex.Type + ".json";
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
            List<string[]> attachmentList = new List<string[]>();
            foreach(SerializedGameObject sg in sw.GameObjects)
            {
                if(!IsBuiltInModel(sg.ModelName))
                {
                    KWEngine.LoadModel(sg.ModelName, sg.ModelPath);
                }
                w.AddGameObject(BuildGameObject(sg, sw, attachmentList));
            }
            KWEngine.CurrentWorld.AddRemoveGameObjects();

            if (sw.ViewSpaceGameObject != null)
                BuildAndAddViewSpaceGameObject(sw.ViewSpaceGameObject, w);

            // Rebuild attachments:
            foreach (string[] attachment in attachmentList)
            {
                // 0: attachedObjectID, 1: parentBoneName, 2: parentID
                int attachedObjectID = Convert.ToInt32(attachment[0]);
                string parentBoneName = attachment[1];
                int parentID = Convert.ToInt32(attachment[2]);

                GameObject attachmentObject = w._gameObjects.Find(g => g._importedID == attachedObjectID);
                if(attachmentObject != null)
                {
                    GameObject parentObject = w._gameObjects.Find(g => g._importedID == parentID);
                    if(parentObject != null)
                    {
                        parentObject.AttachGameObjectToBone(attachmentObject, parentBoneName);

                        SerializedGameObject sg = sw.GameObjects.Find(g => g.ID == attachedObjectID);
                        if(sg != null)
                        {
                            HelperGameObjectAttachment.SetPositionOffsetForAttachment(attachmentObject, new Vector3(sg.PositionOffset[0], sg.PositionOffset[1], sg.PositionOffset[2]));
                            Quaternion q = new Quaternion(sg.RotationOffset[0], sg.RotationOffset[1], sg.RotationOffset[2], sg.RotationOffset[3]);
                            Vector3 qEuler = q.ToEulerAngles();
                            HelperGameObjectAttachment.SetRotationForAttachment(attachmentObject, qEuler.X, qEuler.Y, qEuler.Z);
                            HelperGameObjectAttachment.SetScaleForAttachment(attachmentObject, new Vector3(sg.ScaleOffset[0], sg.ScaleOffset[1], sg.ScaleOffset[2]));
                        }
                        else
                        {
                            HelperGameObjectAttachment.SetPositionOffsetForAttachment(attachmentObject, Vector3.Zero);
                            HelperGameObjectAttachment.SetRotationForAttachment(attachmentObject, 0, 0, 0);
                            HelperGameObjectAttachment.SetScaleForAttachment(attachmentObject, Vector3.One);
                        }
                    }
                    else
                    {
                        if (sw.ViewSpaceGameObject != null)
                        {
                            // attached to the view space game object:
                            w._viewSpaceGameObject.AttachGameObjectToBone(attachmentObject, parentBoneName);
                            HelperGameObjectAttachment.SetPositionOffsetForAttachment(attachmentObject, attachmentObject._positionOffsetForAttachment);
                            Vector3 qEuler = attachmentObject.Rotation.ToEulerAngles();
                            HelperGameObjectAttachment.SetRotationForAttachment(attachmentObject, qEuler.X, qEuler.Y, qEuler.Z);
                            HelperGameObjectAttachment.SetScaleForAttachment(attachmentObject, attachmentObject._scaleOffsetForAttachment);
                        }
                    }
                }
            }

            // Build and add light object instances:
            foreach (SerializedLightObject sl in sw.LightObjects)
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

            
            KWEngine.CurrentWorld.AddRemoveTerrainObjects();
            KWEngine.CurrentWorld.AddRemoveLightObjects();
            KWEngine.CurrentWorld.AddRemoveHUDObjects();
        }

        private static void BuildAndAddViewSpaceGameObject(SerializedViewSpaceGameObject svsgs, World w)
        {
            ViewSpaceGameObject vsg = (ViewSpaceGameObject)Assembly.GetEntryAssembly().CreateInstance(svsgs.Type);
            if (!IsBuiltInModel(svsgs.ModelName))
            {
                KWEngine.LoadModel(svsgs.ModelName, svsgs.ModelPath);
            }
            vsg.SetModel(svsgs.ModelName);

            w.SetViewSpaceGameObject(vsg);
            vsg.SetOffset(svsgs.Position[0], svsgs.Position[1], svsgs.Position[2]);
            Quaternion q = new Quaternion(svsgs.Rotation[0], svsgs.Rotation[1], svsgs.Rotation[2], svsgs.Rotation[3]);
            Vector3 qEuler = q.ToEulerAngles();
            vsg.SetRotation(qEuler.X, qEuler.Y, qEuler.Z);
            vsg.SetScale(svsgs.Scale[0]);
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

            if (IsTextureSetTerrain(st.TextureAlbedo))
                t.SetTexture(st.TextureAlbedo);

            if (IsTextureSetTerrain(st.TextureNormal))
                t.SetTexture(st.TextureNormal, TextureType.Normal);

            if (IsTextureSetTerrain(st.TextureRoughness))
                t.SetTexture(st.TextureRoughness, TextureType.Roughness);

            if (IsTextureSetTerrain(st.TextureMetallic))
                t.SetTexture(st.TextureMetallic, TextureType.Metallic);

            if (IsTextureSetTerrain(st.TextureEmissive))
                t.SetTexture(st.TextureEmissive, TextureType.Emissive);

            if (st.TextureTransform != null && st.TextureTransform.Length >= 2)
            {
                t.SetTextureRepeat(st.TextureTransform[0], st.TextureTransform[1]);
                if (st.TextureTransform.Length == 4)
                    t.SetTextureOffset(st.TextureTransform[2], st.TextureTransform[3]);
            }

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

        private static GameObject BuildGameObject(SerializedGameObject sg, SerializedWorld sw, List<string[]> attachmentList)
        {
            GameObject g = (GameObject)Assembly.GetEntryAssembly().CreateInstance(sg.Type);
            g.SetModel(sg.ModelName);
            g.IsShadowCaster = sg.IsShadowCaster;
            g.IsCollisionObject = sg.IsCollisionObject;
            g.UpdateLast = sg.UpdateLast;
            g.Name = sg.Name;
            g._importedID = sg.ID;

            g.SetPosition(sg.Position[0], sg.Position[1], sg.Position[2]);
            g.SetScale(sg.Scale[0], sg.Scale[1], sg.Scale[2]);
            g.SetRotation(new Quaternion(sg.Rotation[0], sg.Rotation[1], sg.Rotation[2], sg.Rotation[3]));
            g.SetHitboxScale(sg.ScaleHitbox[0], sg.ScaleHitbox[1], sg.ScaleHitbox[2]);

            g.SetColor(sg.Color[0], sg.Color[1], sg.Color[2]);
            g.SetColorEmissive(sg.ColorEmissive[0], sg.ColorEmissive[1], sg.ColorEmissive[2], sg.ColorEmissive[3]);
            for(int i = 0; i < sg.Metallic.Count; i++)
            {
                g.SetMetallic(sg.Metallic[i], i);
                g.SetRoughness(sg.Roughness[i], i);
            }
            g.SetMetallicType(sg.MetallicType);

            for(int i = 0; i < sg.TextureAlbedo.Length; i++)
            {
                if (IsTextureSet(sg.TextureAlbedo[i], g._gModel.Material[i].TextureAlbedo.Filename))
                    g.SetTexture(sg.TextureAlbedo[i], TextureType.Albedo, i);

                if (IsTextureSet(sg.TextureNormal[i], g._gModel.Material[i].TextureNormal.Filename))
                    g.SetTexture(sg.TextureNormal[i], TextureType.Normal, i);

                if (IsTextureSet(sg.TextureRoughness[i], g._gModel.Material[i].TextureRoughness.Filename))
                    g.SetTexture(sg.TextureRoughness[i], TextureType.Roughness, i);

                if (IsTextureSet(sg.TextureMetallic[i], g._gModel.Material[i].TextureMetallic.Filename))
                    g.SetTexture(sg.TextureMetallic[i], TextureType.Metallic, i);

                if (IsTextureSet(sg.TextureEmissive[i], g._gModel.Material[i].TextureEmissive.Filename))
                    g.SetTexture(sg.TextureEmissive[i], TextureType.Emissive, i);

            }

            for (int i = 0; i < sg.TextureRepeat.Count; i++)
            {
                float[] repeat = sg.TextureRepeat[i];
                float[] offset = sg.TextureOffset[i];
                g.SetTextureRepeat(repeat[0], repeat[1], i);
                g.SetTextureOffset(offset[0], offset[1], i);
            }

            for(int i = 0; i < sg.TextureRoughnessInMetallic.Length; i++)
            {
                g._gModel.Material[i].TextureRoughnessInMetallic = sg.TextureRoughnessInMetallic[i];
            }

            g.SetTextureRepeat(sg.TextureTransform[0], sg.TextureTransform[1]);
            if(sg.TextureTransform.Length == 4)
                g.SetTextureOffset(sg.TextureTransform[2], sg.TextureTransform[3]);


            if(sg.AttachedToID > 0 && sg.AttachedToParentBone.Length > 0)
            {
                // find parent in sw:
                SerializedGameObject parent = sw.GameObjects.Find(swgo => swgo.ID == sg.AttachedToID);
                if(parent != null)
                {
                    // 0: attachedObjectID, 1: parentBoneName, 2: parentID
                    attachmentList.Add(new string[] {sg.ID.ToString(), sg.AttachedToParentBone, sg.AttachedToID.ToString()}); 
                }
            }
            else if(sg.AttachedToID == -1 && sg.AttachedToParentBone.Length > 0)
            {
                // must be attached to viewspacegameobject:
                attachmentList.Add(new string[] { sg.ID.ToString(), sg.AttachedToParentBone, sg.AttachedToID.ToString() });
            }

            return g;
        }

        private static bool IsBuiltInModel(string name)
        {
            return name == "KWCube" || name == "KWSphere" || name == "KWQuad" || name == "KWPlatform";
        }

        
        private static bool IsTextureSet(string texname, string currentTex)
        {
            if (texname != null)
            {
                bool texesAreEqual = false;
                if(currentTex != null)
                {
                    string currentTexFileOnly = Path.GetFileNameWithoutExtension(currentTex).ToLower().Trim();
                    string texnameFileOnly = Path.GetFileNameWithoutExtension(texname).ToLower().Trim();
                    texesAreEqual = currentTexFileOnly == texnameFileOnly;
                }

                return texname != null && texname.Trim().Length > 0 && !texesAreEqual;
            }
            else
            {
                return false;
            }
        }

        private static bool IsTextureSetTerrain(string texname)
        {
            return texname != null && texname.Trim().Length > 0;
        }
    }
}

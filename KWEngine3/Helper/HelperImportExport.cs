using KWEngine3.Exceptions;
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
            w.SetBackgroundFillColor(sw.BackgroundFillColor[0], sw.BackgroundFillColor[1], sw.BackgroundFillColor[2]);

            // Build and add game object instances:
            List<string[]> attachmentList = new List<string[]>();
            foreach(SerializedGameObject sg in sw.GameObjects)
            {
                if(!IsBuiltInModel(sg.ModelName))
                {
                    KWEngine.LoadModel(sg.ModelName, sg.ModelPath);
                }
                if(sg.CustomColliderFile != null && sg.CustomColliderFile.Length > 0)
                {
                    KWEngine.LoadCollider(sg.CustomColliderName, sg.CustomColliderFile, sg.CustomColliderType);
                }


                w.AddGameObject(BuildGameObject(sg, sw, attachmentList, sg.CustomColliderName));
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

            // Build and add render object instances:
            foreach (SerializedRenderObject sr in sw.RenderObjects)
            {
                if (!IsBuiltInModel(sr.ModelName))
                {
                    KWEngine.LoadModel(sr.ModelName, sr.ModelPath);
                }
                w.AddRenderObject(BuildRenderObject(sr));
            }
            KWEngine.CurrentWorld.AddRemoveRenderObjects();

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
                    KWEngine.BuildTerrainModel(st.ModelName, st.ModelPath, st.Height);
                    usedterrainNames.Add(st.ModelName);
                }
            }

            // Build and add terrain object instances:
            foreach (SerializedTerrainObject st in sw.TerrainObjects)
            {
                w.AddTerrainObject(BuildTerrainObject(st));
            }

            // Build and add foliage object instances:
            foreach (SerializedFoliageObject sf in sw.FoliageObjects)
            {
                w.AddFoliageObject(BuildFoliageObject(sf));
            }


            // Build and add hud object instances:
            foreach (SerializedHUDObject sh in sw.HUDObjects)
            {
                w.AddHUDObject(BuildHUDObject(sh));
            }

            // Build and add text object instances:
            foreach (SerializedTextObject st in sw.TextObjects)
            {
                w.AddTextObject(BuildTextObject(st));
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
            if(sh.Type == HUDObjectType.Image)
            {
                HUDObjectImage h = new HUDObjectImage(sh.Texture);
                h.Name = sh.Name;
                h.SetPosition(sh.Position[0], sh.Position[1]);
                h.SetScale(sh.Scale[0], sh.Scale[1]);
                h.IsVisible = sh.IsVisible;
                h.SetColor(sh.Color[0], sh.Color[1], sh.Color[2]);
                h.SetOpacity(sh.Color[3]);
                h.SetColorEmissive(sh.Glow[0], sh.Glow[1], sh.Glow[2]);
                h.SetColorEmissiveIntensity(sh.Glow[3]);

                return h;
            }
            else
            {
                HUDObjectText h = new HUDObjectText(sh.Text);
                h.Name = sh.Name;
                h.SetPosition(sh.Position[0], sh.Position[1]);
                h.SetScale(sh.Scale[0]);
                h.IsVisible = sh.IsVisible;
                h.SetColor(sh.Color[0], sh.Color[1], sh.Color[2]);
                h.SetOpacity(sh.Color[3]);
                h.SetColorEmissive(sh.Glow[0], sh.Glow[1], sh.Glow[2]);
                h.SetColorEmissiveIntensity(sh.Glow[3]);

                h.SetTextAlignment(sh.Alignment);
                h.SetFont(sh.Font);
                h.SetCharacterDistanceFactor(sh.CharacterDistanceFactor);

                return h;
            }
        }

        private static TextObject BuildTextObject(SerializedTextObject st)
        {
            TextObject t = (TextObject)Assembly.GetEntryAssembly().CreateInstance(st.Type);
            t.SetText(st.Text);
            t.Name = st.Name;
            t.SetScale(st.Scale);
            t.IsShadowCaster = st.IsShadowCaster;
            t.IsAffectedByLight = st.IsAffectedByLight;
            t.SetCharacterDistanceFactor(st.Spread);
            t.SetText(st.Text);

            t.SetOpacity(st.Color[3]);
            t.SetColor(st.Color[0], st.Color[1], st.Color[2]);
            t.SetColorEmissive(st.ColorEmissive[0], st.ColorEmissive[1], st.ColorEmissive[2], st.ColorEmissive[3]);
            t.SetPosition(st.Position[0], st.Position[1], st.Position[2]);

            Vector3 euler = HelperRotation.ConvertQuaternionToEulerAngles(new Quaternion(st.Rotation[0], st.Rotation[1], st.Rotation[2], st.Rotation[3]));
            t.SetRotation(euler.X, euler.Y, euler.Z);

            t.SetFont(st.Font);

            return t;
        }

        private static FoliageObject BuildFoliageObject(SerializedFoliageObject sf)
        {
            FoliageObject f = new FoliageObject(sf.FoliageType);
            f.IsAffectedByLight = sf.IsAffectedByLight;
            f.IsShadowReceiver = sf.IsShadowReceiver;
            f.IsSizeReducedAtCorners = sf.IsSizeReducedAtCorners;

            f.Name = sf.Name;
            f.SetColor(sf.Color[0], sf.Color[1], sf.Color[2], sf.Color[3]);
            f.SetInstanceCount(sf.InstanceCount);
            f.SetPatchSize(sf.PatchSize[0], sf.PatchSize[1]);
            f.SetPosition(sf.Position[0], sf.Position[1], sf.Position[2]);
            f.SetScale(sf.Scale[0], sf.Scale[1], sf.Scale[2]);
            f.SetSwayFactor(sf.SwayFactor);
            f.SetRoughness(sf.Roughness);

            if(sf.AttachedToID > 0)
            {
                foreach(TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
                {
                    if(t._idFromImport == sf.AttachedToID)
                    {
                        f.AttachToTerrain(t);
                        break;
                    }
                }
            }

            return f;
        }

        private static TerrainObject BuildTerrainObject(SerializedTerrainObject st)
        {
            TerrainObject t = new TerrainObject(st.ModelName);
            t._idFromImport = st.ID;
            t.Name = st.Name;
            t.IsShadowCaster = st.IsShadowCaster;
            t.IsCollisionObject = st.IsCollisionObject;
            t.IsVisible = st.IsVisible;

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
            l.SetShadowOffset(sl.ShadowOffset);
            return l;
        }

        private static GameObject BuildGameObject(SerializedGameObject sg, SerializedWorld sw, List<string[]> attachmentList, string customColliderName = "")
        {
            GameObject g = (GameObject)Assembly.GetEntryAssembly().CreateInstance(sg.Type);
            g.SetModel(sg.ModelName);
            if(customColliderName.Length > 0)
            {
                g.SetColliderModel(customColliderName);
            }
            g.SetOpacity(sg.Opacity);
            g.IsAffectedByLight = sg.IsAffectedByLight;
            g.IsDepthTesting = sg.IsDepthTesting;
            g.IsShadowCaster = sg.IsShadowCaster;
            g.IsCollisionObject = sg.IsCollisionObject;
            g.UpdateLast = sg.UpdateLast;
            g.Name = sg.Name;
            g.BlendTextureStates = sg.BlendTextureStates;
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
                if (IsTextureSet(sg.TextureAlbedo[i], g._model.Material[i].TextureAlbedo.Filename))
                    g.SetTexture(sg.TextureAlbedo[i], TextureType.Albedo, i);

                if (IsTextureSet(sg.TextureNormal[i], g._model.Material[i].TextureNormal.Filename))
                    g.SetTexture(sg.TextureNormal[i], TextureType.Normal, i);

                if (IsTextureSet(sg.TextureRoughness[i], g._model.Material[i].TextureRoughness.Filename))
                    g.SetTexture(sg.TextureRoughness[i], TextureType.Roughness, i);

                if (IsTextureSet(sg.TextureMetallic[i], g._model.Material[i].TextureMetallic.Filename))
                    g.SetTexture(sg.TextureMetallic[i], TextureType.Metallic, i);

                if (IsTextureSet(sg.TextureEmissive[i], g._model.Material[i].TextureEmissive.Filename))
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
                g._model.Material[i].TextureRoughnessInMetallic = sg.TextureRoughnessInMetallic[i];
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

        private static RenderObject BuildRenderObject(SerializedRenderObject sr)
        {
            RenderObject r = (RenderObject)Assembly.GetEntryAssembly().CreateInstance(sr.Type);
            r.SetModel(sr.ModelName);
            r.SetOpacity(sr.Opacity);
            r.IsAffectedByLight = sr.IsAffectedByLight;
            r.IsDepthTesting = sr.IsDepthTesting;
            r.IsShadowCaster = sr.IsShadowCaster;
            r.Name = sr.Name;
            r._importedID = sr.ID;

            r.SetPosition(sr.Position[0], sr.Position[1], sr.Position[2]);
            r.SetScale(sr.Scale[0], sr.Scale[1], sr.Scale[2]);
            r.SetRotation(new Quaternion(sr.Rotation[0], sr.Rotation[1], sr.Rotation[2], sr.Rotation[3]));

            r.SetColor(sr.Color[0], sr.Color[1], sr.Color[2]);
            r.SetColorEmissive(sr.ColorEmissive[0], sr.ColorEmissive[1], sr.ColorEmissive[2], sr.ColorEmissive[3]);
            for (int i = 0; i < sr.Metallic.Count; i++)
            {
                r.SetMetallic(sr.Metallic[i], i);
                r.SetRoughness(sr.Roughness[i], i);
            }
            r.SetMetallicType(sr.MetallicType);

            for (int i = 0; i < sr.TextureAlbedo.Length; i++)
            {
                if (IsTextureSet(sr.TextureAlbedo[i], r._model.Material[i].TextureAlbedo.Filename))
                    r.SetTexture(sr.TextureAlbedo[i], TextureType.Albedo, i);

                if (IsTextureSet(sr.TextureNormal[i], r._model.Material[i].TextureNormal.Filename))
                    r.SetTexture(sr.TextureNormal[i], TextureType.Normal, i);

                if (IsTextureSet(sr.TextureRoughness[i], r._model.Material[i].TextureRoughness.Filename))
                    r.SetTexture(sr.TextureRoughness[i], TextureType.Roughness, i);

                if (IsTextureSet(sr.TextureMetallic[i], r._model.Material[i].TextureMetallic.Filename))
                    r.SetTexture(sr.TextureMetallic[i], TextureType.Metallic, i);

                if (IsTextureSet(sr.TextureEmissive[i], r._model.Material[i].TextureEmissive.Filename))
                    r.SetTexture(sr.TextureEmissive[i], TextureType.Emissive, i);

            }

            for (int i = 0; i < sr.TextureRepeat.Count; i++)
            {
                float[] repeat = sr.TextureRepeat[i];
                float[] offset = sr.TextureOffset[i];
                r.SetTextureRepeat(repeat[0], repeat[1], i);
                r.SetTextureOffset(offset[0], offset[1], i);
            }

            for (int i = 0; i < sr.TextureRoughnessInMetallic.Length; i++)
            {
                r._model.Material[i].TextureRoughnessInMetallic = sr.TextureRoughnessInMetallic[i];
            }

            r.SetTextureRepeat(sr.TextureTransform[0], sr.TextureTransform[1]);
            if (sr.TextureTransform.Length == 4)
                r.SetTextureOffset(sr.TextureTransform[2], sr.TextureTransform[3]);

            r.SetAdditionalInstanceCount(sr.InstanceCount - 1, sr.Mode);
            for(int i = 16; i < sr.InstanceMatrices.Length; i += 16)
            {
                Matrix4 modelMatrix = new Matrix4(
                    sr.InstanceMatrices[i + 0],
                    sr.InstanceMatrices[i + 1],
                    sr.InstanceMatrices[i + 2],
                    sr.InstanceMatrices[i + 3],
                    sr.InstanceMatrices[i + 4],
                    sr.InstanceMatrices[i + 5],
                    sr.InstanceMatrices[i + 6],
                    sr.InstanceMatrices[i + 7],
                    sr.InstanceMatrices[i + 8],
                    sr.InstanceMatrices[i + 9],
                    sr.InstanceMatrices[i + 10],
                    sr.InstanceMatrices[i + 11],
                    sr.InstanceMatrices[i + 12],
                    sr.InstanceMatrices[i + 13],
                    sr.InstanceMatrices[i + 14],
                    sr.InstanceMatrices[i + 15]
                    );

                Quaternion rotation = modelMatrix.ExtractRotation();
                Vector3 position = modelMatrix.ExtractTranslation();
                Vector3 scale = modelMatrix.ExtractScale();

                r.SetPositionRotationScaleForInstance(i / 16, position, rotation, scale);
            }

            return r;
        }

        private static bool IsBuiltInModel(string name)
        {
            return name == "KWCube" || name == "KWSphere" || name == "KWQuad" || name == "KWQuad2D" || name == "KWPlatform";
        }

        
        private static bool IsTextureSet(string texname, string currentTex)
        {
            if (texname != null)
            {
                bool texesAreEqual = false;
                if(currentTex != null)
                {
                    string currentTexFileOnly = Path.GetFileNameWithoutExtension(currentTex).Trim();
                    string texnameFileOnly = Path.GetFileNameWithoutExtension(texname).Trim();
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

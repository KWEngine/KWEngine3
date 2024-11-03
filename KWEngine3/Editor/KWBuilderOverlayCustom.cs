using ImGuiNET;
using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Renderer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Reflection;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace KWEngine3.Editor
{
    internal partial class KWBuilderOverlay
    {
        private const int MAXFRAMETIMES = 240;
        private const int WINDOW_RIGHT_WIDTH = 460;
        private const int ROUNDDIGITCOUNT = 2;
        private const float KEYBOARDCOOLDOWN = 0.05f;
        private const float POSITIONSTEP = 0.05f;
        private static readonly Queue<float> _frameTimes = new(MAXFRAMETIMES);
        private static float _frameTimeSum = 0f;
        private static int _fpsCounter = 0;
        private static int _fpsValue = 0;
        private static float _lastKeyboardTimeInSeconds = 0;
        private static readonly Dictionary<MouseButton, KWMouseButtonState> _mouseButtonsActive = new() {
            { MouseButton.Left, new KWMouseButtonState() },
            { MouseButton.Right, new KWMouseButtonState() },
            { MouseButton.Middle, new KWMouseButtonState() },
            };
        private static bool _isSkybox = false;
        private static double _lastUpdateCycleTime = 0;
        private static double _lastRenderCallsTime = 0;

        public static bool IsButtonActive(MouseButton btn)
        {
            return _mouseButtonsActive[btn].Pressed;
        }

        public static void InitFrameTimeQueue()
        {
            for (int i = 0; i < MAXFRAMETIMES; i++)
            {
                AddFrameTime(0f);
            }
        }

        public static void UpdateLastRenderCallsTime(double time)
        {
            _lastRenderCallsTime = time;
        }

        public static void UpdateLastUpdateTime(double time)
        {
            _lastUpdateCycleTime = time;
        }

        public static void AddFrameTime(float t)
        {
            _frameTimeSum += t;
            _fpsCounter++;
            if (_frameTimeSum >= 1000)
            {
                _fpsValue = (int)(_fpsCounter / _frameTimeSum * 1000);
                _fpsCounter = 0;
                _frameTimeSum = 0f;
            }

            if (_frameTimes.Count == MAXFRAMETIMES)
            {
                _frameTimes.Dequeue();
            }
            _frameTimes.Enqueue(t);
        }

        private static void DrawGridAndBoundingBox()
        {
            if (KWEngine.DebugMode != DebugMode.None && !RenderManager.IsCurrentDebugMapACubeMap()) return;

            GL.Disable(EnableCap.DepthTest);
            RendererGrid.Bind();
            RendererGrid.SetGlobals();
            RendererGrid.Draw();

            if (SelectedGameObject != null)
            {
                RendererEditor.Bind();
                RendererEditor.SetGlobals();
                RendererEditor.Draw(SelectedGameObject);

                RendererEditorHitboxes.Bind();
                RendererEditorHitboxes.SetGlobals();
                RendererEditorHitboxes.Draw(SelectedGameObject);
            }
            else if(SelectedTerrainObject != null)
            {
                RendererEditor.Bind();
                RendererEditor.SetGlobals();
                RendererEditor.Draw(SelectedTerrainObject);
            }
            GL.Enable(EnableCap.DepthTest);
        }

        public static bool IsCursorPressedOnAnyControl(MouseButton btn)
        {
            return _mouseButtonsActive[btn].PressedOnOverlay;
        }

        public static bool IsCursorOnAnyControl()
        {
            ImGuiIOPtr ptr =  ImGui.GetIO();
            return ptr.WantCaptureMouse;
        }

        public static bool DiscardKeyboardNavigation()
        {
            ImGuiIOPtr ptr = ImGui.GetIO();
            return ptr.WantCaptureKeyboard;
        }

        private static bool IsCursorInsideRectangle(Vector2 mousePosition, int left, int right, int top, int bottom)
        {
            return mousePosition.X >= left && mousePosition.X <= right && mousePosition.Y >= top && mousePosition.Y <= bottom;
        }

        private static bool IsCursorOutsideOfWindow(Vector2 mousePosition)
        {
            return mousePosition.X <= 0 || mousePosition.X >= KWEngine.Window.ClientSize.X - 1 ||
                mousePosition.Y <= 0 || mousePosition.Y >= KWEngine.Window.ClientSize.Y - 1;
        }

        private static bool rWorldSpace = true;
        private static string _modelNameNew = null;
        private static readonly string[] _metallicTypes = new string[] { "Default", "Plastic or glass (low)", "Plastic or glass (high)", "Diamond", "Iron", "Copper", "Gold", "Aluminium", "Silver" };
        private static int _metallicTypesIndex = 0;

        private static void DrawObjectDetails()
        {
            if (SelectedGameObject != null)
            {
                System.Numerics.Vector3 pNew = new(SelectedGameObject.Position.X, SelectedGameObject.Position.Y, SelectedGameObject.Position.Z);
                System.Numerics.Vector3 sNew = new(SelectedGameObject.Scale.X, SelectedGameObject.Scale.Y, SelectedGameObject.Scale.Z);
                System.Numerics.Vector3 rNew = new();
                System.Numerics.Vector3 colorTintNew = new(SelectedGameObject._stateCurrent._colorTint.X, SelectedGameObject._stateCurrent._colorTint.Y, SelectedGameObject._stateCurrent._colorTint.Z);
                System.Numerics.Vector3 colorEmissiveNew = new(SelectedGameObject._stateCurrent._colorEmissive.X, SelectedGameObject._stateCurrent._colorEmissive.Y, SelectedGameObject._stateCurrent._colorEmissive.Z);
                float colorEmissiveIntensityNew = SelectedGameObject._stateCurrent._colorEmissive.W;
                string gName = SelectedGameObject.Name;
                string modelName = SelectedGameObject._model.ModelOriginal.Name;
                if (modelName == "kwcube.obj")
                    modelName = "KWCube";
                else if (modelName == "kwsphere.obj")
                    modelName = "KWSphere";
                else if (modelName == "kwquad.obj")
                    modelName = "KWQuad";
                else if (modelName == "kwquad2.obj")
                    modelName = "KWQuad2D";
                else if (modelName == "kwplatform.obj")
                    modelName = "KWPlatform";

                ImGui.Begin("GameObject properties", ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
                ImGui.SetWindowSize(new System.Numerics.Vector2(WINDOW_RIGHT_WIDTH, 720 - 32), ImGuiCond.Once);
                ImGui.SetWindowPos(new System.Numerics.Vector2(KWEngine.Window.ClientSize.X - WINDOW_RIGHT_WIDTH, 20), ImGuiCond.Once);
                ImGui.Text("ID: " + SelectedGameObject.ID.ToString().PadLeft(8, '0'));
                ImGui.SameLine();
                if (ImGui.InputText("Name", ref gName, 64))
                {
                    SelectedGameObject.Name = gName;
                }
                ImGui.Separator();

                // Model
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Change Model (current: " + modelName + ")");
                _modelNames = GetModelNamesForCurrentWorld();
                if (_modelNamesIndex >= _modelNames.Length)
                {
                    _modelNamesIndex = -1;
                }
                else
                {
                    if (_modelNameNew == null)
                    {
                        for (int i = 0; i < _modelNames.Length; i++)
                        {
                            if (_modelNames[i] == modelName)
                            {
                                _modelNamesIndex = i;
                            }
                        }
                    }
                }
                ImGui.PushItemWidth(200);
                if (ImGui.Combo("Model", ref _modelNamesIndex, _modelNames, _modelNames.Length))
                {
                    _modelNameNew = _modelNames[_modelNamesIndex];
                }
                ImGui.PopItemWidth();
                ImGui.SameLine();
                if (ImGui.Button("Apply"))
                {
                    if (_modelNamesIndex >= 0)
                    {
                        SelectedGameObject.SetModel(_modelNames[_modelNamesIndex]);
                        _modelNameNew = null;
                    }
                }

                ImGui.Separator();
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Misc. properties:");
                /*
                bool isCollObj = SelectedGameObject._colliderType != ColliderType.None;
                if(ImGui.Checkbox("Collider?", ref isCollObj))
                {
                    if(_)
                    SelectedGameObject.IsCollisionObject = isCollObj;
                }
                ImGui.SameLine();
                */
                ImGui.Checkbox("Shadow caster/receiver?", ref SelectedGameObject._isShadowCaster);
                ImGui.SameLine();
                ImGui.Checkbox("Affected by light?", ref SelectedGameObject._isAffectedByLight);
                //ImGui.SameLine();
                //ImGui.Text("In view: " + (SelectedGameObject.IsInsideScreenSpace ? "YES" : "NO"));

                // Position/Rotation/Scale
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Position, Rotation & Scale:");
                if (ImGui.InputFloat3("Position (X,Y,Z)", ref pNew, "%.2f"))
                {
                    SelectedGameObject.SetPosition(new Vector3(pNew.X, pNew.Y, pNew.Z));
                }
                ImGui.Separator();
                if (ImGui.SliderFloat3("Rotation (X,Y,Z)", ref rNew, -5, 5))
                {
                    SelectedGameObject.AddRotationZFromEditor(rNew.Z * KWEngine.DeltaTimeFactorForOverlay, rWorldSpace);
                    SelectedGameObject.AddRotationYFromEditor(rNew.Y * KWEngine.DeltaTimeFactorForOverlay, rWorldSpace);
                    SelectedGameObject.AddRotationXFromEditor(rNew.X * KWEngine.DeltaTimeFactorForOverlay, rWorldSpace);
                }
                if (ImGui.Button("Reset"))
                {
                    SelectedGameObject.SetRotation(0, 0, 0);
                }
                ImGui.SameLine();
                if (ImGui.Button("X +45"))
                {
                    SelectedGameObject.AddRotationXFromEditor(-45, rWorldSpace);
                }
                ImGui.SameLine();
                if (ImGui.Button("Y +45"))
                {
                    SelectedGameObject.AddRotationYFromEditor(-45, rWorldSpace);
                }
                ImGui.SameLine();
                if (ImGui.Button("Z +45"))
                {
                    SelectedGameObject.AddRotationZFromEditor(-45, rWorldSpace);
                }
                ImGui.SameLine();
                ImGui.Checkbox(" ", ref rWorldSpace);
                ImGui.SameLine();
                ImGui.Text("Rotation around world axes?");

                OpenTK.Mathematics.Vector3 angles = HelperRotation.ConvertQuaternionToEulerAngles(SelectedGameObject.Rotation);
                ImGui.Text("Computed rotation angles: " + Math.Round(angles.X, 0) + " | " + Math.Round(angles.Y, 0) + " | " + Math.Round(angles.Z, 0));

                ImGui.Separator();
                if (ImGui.InputFloat3("Scale (X,Y,Z)", ref sNew, "%.2f"))
                {
                    SelectedGameObject.SetScale(sNew.X, sNew.Y, sNew.Z);
                }
                ImGui.Separator();

                // Coloring:
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Color settings:");
                if (ImGui.ColorEdit3("Color tint", ref colorTintNew, ImGuiColorEditFlags.Float))
                {
                    SelectedGameObject.SetColor(colorTintNew.X, colorTintNew.Y, colorTintNew.Z);
                }
                if (!SelectedGameObject.HasEmissiveTexture)
                {
                    if (ImGui.ColorEdit3("Color emissive", ref colorEmissiveNew, ImGuiColorEditFlags.Float))
                    {
                        SelectedGameObject.SetColorEmissive(colorEmissiveNew.X, colorEmissiveNew.Y, colorEmissiveNew.Z, SelectedGameObject._stateCurrent._colorEmissive.W);
                    }
                    if (ImGui.SliderFloat("", ref colorEmissiveIntensityNew, 0, 10, "%.2f", ImGuiSliderFlags.AlwaysClamp))
                    {
                        SelectedGameObject.SetColorEmissive(colorEmissiveNew.X, colorEmissiveNew.Y, colorEmissiveNew.Z, colorEmissiveIntensityNew);
                    }
                    ImGui.SameLine();
                    ImGui.Text("Emissive intensity");
                }
                

                ImGui.Separator();

                ImGui.SliderFloat("Opacity", ref SelectedGameObject._stateCurrent._opacity, 0f, 1f);

                ImGui.Separator();

                if (SelectedGameObject._model.ModelOriginal.IsPrimitive)
                {
                    float roughness = SelectedGameObject._model.Material[0].Roughness;
                    float metallic = SelectedGameObject._model.Material[0].Metallic;
                    if (SelectedGameObject._model.Material[0].TextureRoughness.IsTextureSet == false)
                    {
                        if (ImGui.SliderFloat("Global roughness", ref roughness, 0f, 1f))
                        {
                            SelectedGameObject.SetRoughness(roughness);
                        }
                    }
                    if (SelectedGameObject._model.Material[0].TextureMetallic.IsTextureSet == false)
                    {
                        if (ImGui.SliderFloat("Global metallic", ref metallic, 0f, 1f))
                        {
                            SelectedGameObject.SetMetallic(metallic);
                        }
                    }

                    _metallicTypesIndex = (int)SelectedGameObject._model._metallicType;
                    if (ImGui.Combo("Metallic type", ref _metallicTypesIndex, _metallicTypes, _metallicTypes.Length))
                    {
                        SelectedGameObject.SetMetallicType(_metallicTypesIndex);
                    }


                    ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Textures:");
                    string albedoFilename = SelectedGameObject._model.Material[0].TextureAlbedo.IsTextureSet ? SelectedGameObject._model.Material[0].TextureAlbedo.Filename : "";
                    string normalFilename = SelectedGameObject._model.Material[0].TextureNormal.IsTextureSet ? SelectedGameObject._model.Material[0].TextureNormal.Filename : "";
                    string emissiveFilename = SelectedGameObject._model.Material[0].TextureEmissive.IsTextureSet ? SelectedGameObject._model.Material[0].TextureEmissive.Filename : "";
                    string roughnessFilename = SelectedGameObject._model.Material[0].TextureRoughness.IsTextureSet ? SelectedGameObject._model.Material[0].TextureRoughness.Filename : "";
                    string metallicFilename = SelectedGameObject._model.Material[0].TextureMetallic.IsTextureSet ? SelectedGameObject._model.Material[0].TextureMetallic.Filename : "";

                    float uvX = SelectedGameObject._stateCurrent._uvTransform.X; // SelectedGameObject._gModel.Material[0].TextureAlbedo.IsTextureSet ? SelectedGameObject._gModel.Material[0].TextureAlbedo.UVTransform.X : 1f;
                    float uvY = SelectedGameObject._stateCurrent._uvTransform.Y; // SelectedGameObject._gModel.Material[0].TextureAlbedo.IsTextureSet ? SelectedGameObject._gModel.Material[0].TextureAlbedo.UVTransform.Y : 1f;
                    System.Numerics.Vector2 uvTransform = new(uvX, uvY);

                    if (ImGui.InputText("Albedo", ref albedoFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoUndoRedo))
                    {
                        if (FileNameEmpty(albedoFilename))
                            SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Albedo);
                        else if (File.Exists(albedoFilename))
                            SelectedGameObject.SetTexture(albedoFilename.Trim(), TextureType.Albedo);
                    }

                    if (ImGui.InputText("Normal", ref normalFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoUndoRedo))
                    {
                        if (FileNameEmpty(normalFilename))
                            SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Normal);
                        else if (File.Exists(normalFilename))
                            SelectedGameObject.SetTexture(normalFilename.Trim(), TextureType.Normal);
                    }

                    if (ImGui.InputText("Emissive", ref emissiveFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoUndoRedo))
                    {
                        if (FileNameEmpty(emissiveFilename))
                            SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Emissive);
                        else if (File.Exists(emissiveFilename))
                            SelectedGameObject.SetTexture(emissiveFilename.Trim(), TextureType.Emissive);
                    }

                    if (ImGui.InputText("Metallic", ref metallicFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoUndoRedo))
                    {
                        if (FileNameEmpty(metallicFilename))
                            SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Metallic);
                        else if (File.Exists(metallicFilename))
                            SelectedGameObject.SetTexture(metallicFilename.Trim(), TextureType.Metallic);
                    }

                    if (ImGui.InputText("Roughness", ref roughnessFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoUndoRedo))
                    {
                        if (FileNameEmpty(roughnessFilename))
                            SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Roughness);
                        else if (File.Exists(roughnessFilename))
                            SelectedGameObject.SetTexture(roughnessFilename.Trim(), TextureType.Roughness);
                    }

                    if (SelectedGameObject._model.Material[0].TextureAlbedo.IsTextureSet)
                    {
                        if (ImGui.InputFloat2("Texture repeat", ref uvTransform, "%.2f", ImGuiInputTextFlags.NoUndoRedo))
                        {
                            SelectedGameObject.SetTextureRepeat(uvTransform.X, uvTransform.Y);
                        }
                    }

                    
                }
                else if(SelectedGameObject._modelNameInDB == "KWPlatform")
                {
                    float[] roughness = new float[SelectedGameObject._model.Material.Length];
                    float[] metallic = new float[SelectedGameObject._model.Material.Length];
                    ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Metallic/Roughness:");
                    ImGui.PushItemWidth(100);
                    for (int i = 0; i < SelectedGameObject._model.Material.Length; i++)
                    {
                        if (SelectedGameObject._model.Material[i].ColorAlbedo.W <= 0)
                            continue;
                        if(i > 0)
                            ImGui.SameLine();
                        roughness[i] = SelectedGameObject._model.Material[i].Roughness;
                        if (SelectedGameObject._model.Material[i].TextureRoughness.IsTextureSet == false)
                        {
                            if (ImGui.SliderFloat("R #" + i, ref roughness[i], 0f, 1f))
                            {
                                SelectedGameObject.SetRoughness(roughness[i], i);
                            }
                            
                        }
                        else
                        {
                            // TODO
                            ImGui.TextColored(new System.Numerics.Vector4(0, 0, 0, 1), "-------------------");
                        }
                        
                    }
                    
                    for (int i = 0; i < SelectedGameObject._model.Material.Length; i++)
                    {
                        if (SelectedGameObject._model.Material[i].ColorAlbedo.W <= 0)
                            continue;
                        if (i > 0 )
                            ImGui.SameLine();
                        metallic[i] = SelectedGameObject._model.Material[i].Metallic;

                        if (SelectedGameObject._model.Material[i].TextureMetallic.IsTextureSet == false)
                        {
                            if (ImGui.SliderFloat("M #" + i, ref metallic[i], 0f, 1f))
                            {
                                SelectedGameObject.SetMetallic(metallic[i], i);
                            }
                        }
                        else
                        {
                            //TODO
                            ImGui.TextColored(new System.Numerics.Vector4(0, 0, 0, 1), "-------------------");
                        }

                    }
                    ImGui.PopItemWidth();

                    _metallicTypesIndex = (int)SelectedGameObject._model._metallicType;
                    if (ImGui.Combo("Metallic type", ref _metallicTypesIndex, _metallicTypes, _metallicTypes.Length))
                    {
                        SelectedGameObject.SetMetallicType(_metallicTypesIndex);
                    }

                    ImGui.Separator();
                    ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Textures (0=Top/Bottom, 1=Left/Right, 2=Front/Back):");
                    string[] albedo_t = new string[SelectedGameObject._model.Material.Length];
                    string[] normal_t = new string[SelectedGameObject._model.Material.Length];
                    string[] metallic_t = new string[SelectedGameObject._model.Material.Length];
                    string[] roughness_t = new string[SelectedGameObject._model.Material.Length];
                    string[] emissive_t = new string[SelectedGameObject._model.Material.Length];

                    ImGui.PushItemWidth(150);
                    for (int i = 0; i < SelectedGameObject._model.Material.Length; i++)
                    {
                        if (SelectedGameObject._model.Material[i].ColorAlbedo.W <= 0)
                            continue;

                        if(i > 0)
                            ImGui.Separator();

                        albedo_t[i] = SelectedGameObject._model.Material[i].TextureAlbedo.IsTextureSet ? SelectedGameObject._model.Material[i].TextureAlbedo.Filename : "";
                        normal_t[i] = SelectedGameObject._model.Material[i].TextureNormal.IsTextureSet ? SelectedGameObject._model.Material[i].TextureNormal.Filename : "";
                        metallic_t[i] = SelectedGameObject._model.Material[i].TextureMetallic.IsTextureSet ? SelectedGameObject._model.Material[i].TextureMetallic.Filename : "";
                        roughness_t[i] = SelectedGameObject._model.Material[i].TextureRoughness.IsTextureSet ? SelectedGameObject._model.Material[i].TextureRoughness.Filename : "";
                        emissive_t[i] = SelectedGameObject._model.Material[i].TextureEmissive.IsTextureSet ? SelectedGameObject._model.Material[i].TextureEmissive.Filename : "";

                        

                        if (ImGui.InputText("Albedo " + i, ref albedo_t[i], 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoUndoRedo))
                        {
                            if (FileNameEmpty(albedo_t[i]))
                                SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Albedo, i);
                            else if (File.Exists(albedo_t[i]))
                                SelectedGameObject.SetTexture(albedo_t[i].Trim(), TextureType.Albedo, i);
                            

                        }
                        ImGui.SameLine();
                        if (ImGui.InputText("Normal " + i, ref normal_t[i], 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoUndoRedo))
                        {
                            if (FileNameEmpty(normal_t[i]))
                                SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Normal, i);
                            else if (File.Exists(normal_t[i]))
                                SelectedGameObject.SetTexture(normal_t[i].Trim(), TextureType.Normal, i);

                        }
                        
                        if (ImGui.InputText("Roughn." + i, ref roughness_t[i], 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoUndoRedo))
                        {
                            if (FileNameEmpty(roughness_t[i]))
                                SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Roughness, i);
                            else if (File.Exists(roughness_t[i]))
                                SelectedGameObject.SetTexture(roughness_t[i].Trim(), TextureType.Roughness, i);


                        }
                        ImGui.SameLine();
                        if (ImGui.InputText("Metal  " + i, ref metallic_t[i], 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoUndoRedo))
                        {
                            if (FileNameEmpty(metallic_t[i]))
                                SelectedGameObject._model.UnsetTextureForPrimitive(TextureType.Metallic, i);
                            else if (File.Exists(metallic_t[i]))
                                SelectedGameObject.SetTexture(metallic_t[i].Trim(), TextureType.Metallic, i);

                        }
                        
                    }
                    ImGui.PopItemWidth();

                    // Texture transform:
                    ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "UV transform:");
                    ImGui.PushItemWidth(96);
                    for (int i = 0; i < SelectedGameObject._model.Material.Length; i++)
                    {
                        if(SelectedGameObject._model.Material[i].TextureAlbedo.IsTextureSet)
                        {
                            System.Numerics.Vector2 uvTransform = new(SelectedGameObject._model.Material[i].TextureAlbedo.UVTransform.X, SelectedGameObject._model.Material[i].TextureAlbedo.UVTransform.Y);
                            if (ImGui.InputFloat2("XY #" + i, ref uvTransform, "%.2f", ImGuiInputTextFlags.NoUndoRedo))
                            {
                                SelectedGameObject.SetTextureRepeat(uvTransform.X, uvTransform.Y, i);
                            }
                        }
                        ImGui.SameLine();
                    }


                    ImGui.PopItemWidth();
                }
                else if(SelectedGameObject.HasArmatureAndAnimations)
                {
                    string[] animations = new string[SelectedGameObject._model.ModelOriginal.Animations.Count + 1];
                    int selectedId = SelectedGameObject._stateCurrent._animationID + 1;
                    animations[0] = "no animation";
                    for(int i = 0; i < SelectedGameObject._model.ModelOriginal.Animations.Count; i++)
                    {
                        animations[i + 1] = "#" + i.ToString().PadLeft(3, '0');
                    }
                    ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Animation:");
                    if(ImGui.Combo("Animation ID", ref selectedId, animations, animations.Length))
                    {
                        int tmpId = selectedId - 1;
                        SelectedGameObject.SetAnimationID(tmpId);
                    }

                    ImGui.SliderFloat("Animation %", ref SelectedGameObject._stateCurrent._animationPercentage, 0f, 1f);
                }
                
                ImGui.End();
            }
            else if (SelectedLightObject != null)
            {
                string lName = SelectedLightObject.Name;
                System.Numerics.Vector3 pNew = new(
                    SelectedLightObject._stateCurrent._position.X,
                    SelectedLightObject._stateCurrent._position.Y,
                    SelectedLightObject._stateCurrent._position.Z);
                System.Numerics.Vector3 tNew = new(
                   SelectedLightObject._stateCurrent._target.X,
                   SelectedLightObject._stateCurrent._target.Y,
                   SelectedLightObject._stateCurrent._target.Z);
                float nearNew = SelectedLightObject._stateCurrent._nearFarFOVType.X;
                float farNew = SelectedLightObject._stateCurrent._nearFarFOVType.Y;
                float fovNew = SelectedLightObject._stateCurrent._nearFarFOVType.Z;
                System.Numerics.Vector3 colorNew = new(
                    SelectedLightObject._stateCurrent._color.X,
                    SelectedLightObject._stateCurrent._color.Y,
                    SelectedLightObject._stateCurrent._color.Z
                    );
                float colorIntensityNew = SelectedLightObject._stateCurrent._color.W;

                ImGui.Begin("LightObject properties", ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
                ImGui.SetWindowSize(new System.Numerics.Vector2(WINDOW_RIGHT_WIDTH, 600), ImGuiCond.Once);
                ImGui.SetWindowPos(new System.Numerics.Vector2(KWEngine.Window.ClientSize.X - WINDOW_RIGHT_WIDTH, 20), ImGuiCond.Once);
                //LightType.Point ? 0 : l._type == LightType.Sun ? -1 : 1
                string lighttype = SelectedLightObject._stateCurrent._nearFarFOVType.W == 0f ? "Point" : SelectedLightObject._stateCurrent._nearFarFOVType.W > 0f ? "Directional" : "Sun";
                ImGui.Text(lighttype + " light ID: " + Math.Abs(SelectedLightObject.ID).ToString().PadLeft(8, '0'));
                ImGui.SameLine();
                if (ImGui.InputText("Name", ref lName, 64))
                {
                    SelectedLightObject.Name = lName;
                }
                ImGui.Separator();
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Position & Target:");
                if (ImGui.InputFloat3("Position (X,Y,Z)", ref pNew, "%.2f"))
                {
                    SelectedLightObject.SetPosition(new OpenTK.Mathematics.Vector3(pNew.X, pNew.Y, pNew.Z));
                }
                if (SelectedLightObject.Type != LightType.Point)
                {
                    if (ImGui.InputFloat3("Target (X,Y,Z)", ref tNew, "%.2f"))
                    {
                        SelectedLightObject.SetTarget(new OpenTK.Mathematics.Vector3(tNew.X, tNew.Y, tNew.Z));
                    }
                }

                ImGui.Separator();

                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Color settings:");
                if (ImGui.ColorEdit3("Light color", ref colorNew, ImGuiColorEditFlags.Float | ImGuiColorEditFlags.NoLabel))
                {
                    SelectedLightObject.SetColor(colorNew.X, colorNew.Y, colorNew.Z, colorIntensityNew);
                }
                if (ImGui.SliderFloat("Light intensity", ref colorIntensityNew, 0f, 100f))
                {
                    SelectedLightObject.SetColor(colorNew.X, colorNew.Y, colorNew.Z, colorIntensityNew);
                }

                ImGui.Separator();

                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "(Near &) Far bounds:");
                bool updateNearFar = false;
                if (SelectedLightObject.Type == LightType.Sun || SelectedLightObject.Type == LightType.Directional)
                {
                    if (SelectedLightObject.Type == LightType.Sun)
                    {
                        if (ImGui.SliderFloat("FOV", ref fovNew, 10, SelectedLightObject._shadowMapSize, "%.0f"))
                        {
                            SelectedLightObject.SetFOV(fovNew);
                        }
                    }
                    else
                    {
                        if (ImGui.SliderFloat("FOV (degrees)", ref fovNew, 10, 179, "%.0f"))
                        {
                            SelectedLightObject.SetFOV(fovNew);
                        }
                    }
                    if (SelectedLightObject.ShadowCasterType != ShadowQuality.NoShadow)
                    {
                        if (ImGui.SliderFloat("Near boundary", ref nearNew, 0.1f, 128f, "%.0f"))
                            updateNearFar = true;
                    }
                }
                if (ImGui.SliderFloat("Far  boundary", ref farNew, 1f, 512f, "%.0f"))
                    updateNearFar = true;

                if (updateNearFar)
                    SelectedLightObject.SetNearFar(nearNew, farNew);

                if (SelectedLightObject.ShadowCasterType != ShadowQuality.NoShadow)
                {
                    ImGui.SliderFloat("Shadow bias", ref SelectedLightObject._shadowBias, 0.0f, 0.001f, "%.6f");   
                }
                if (SelectedLightObject.ShadowCasterType != ShadowQuality.NoShadow)
                {
                    ImGui.SliderFloat("Shadow offset", ref SelectedLightObject._shadowOffset, 0.0f, 0.1f, "%.4f");
                }
            }
            else if(SelectedTerrainObject != null)
            {
                System.Numerics.Vector3 pNew = new(SelectedTerrainObject._stateCurrent._position.X, SelectedTerrainObject._stateCurrent._position.Y, SelectedTerrainObject._stateCurrent._position.Z);
                System.Numerics.Vector3 colorTintNew = new(SelectedTerrainObject._stateCurrent._colorTint.X, SelectedTerrainObject._stateCurrent._colorTint.Y, SelectedTerrainObject._stateCurrent._colorTint.Z);
                System.Numerics.Vector3 colorEmissiveNew = new(SelectedTerrainObject._stateCurrent._colorEmissive.X, SelectedTerrainObject._stateCurrent._colorEmissive.Y, SelectedTerrainObject._stateCurrent._colorEmissive.Z);
                float colorEmissiveIntensityNew = SelectedTerrainObject._stateCurrent._colorEmissive.W;
                string gName = SelectedTerrainObject.Name;

                ImGui.Begin("TerrainObject properties", ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
                ImGui.SetWindowSize(new System.Numerics.Vector2(WINDOW_RIGHT_WIDTH, 600), ImGuiCond.Once);
                ImGui.SetWindowPos(new System.Numerics.Vector2(KWEngine.Window.ClientSize.X - WINDOW_RIGHT_WIDTH, 20), ImGuiCond.Once);
                ImGui.Text("ID: " + SelectedTerrainObject.ID.ToString().PadLeft(8, '0'));
                ImGui.SameLine();
                if (ImGui.InputText("Name", ref gName, 64))
                {
                    SelectedTerrainObject.Name = gName;
                }
                ImGui.Separator();

                // Model
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Model: " + SelectedTerrainObject._gModel.ModelOriginal.Name);

                ImGui.Separator();

                // Position/Rotation/Scale
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Position:");
                if (ImGui.InputFloat3("Position (X,Y,Z)", ref pNew, "%.1f"))
                {
                    SelectedTerrainObject.SetPosition(new Vector3(pNew.X, pNew.Y, pNew.Z));
                }

                // Coloring:
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Color and emissive color settings:");
                if (ImGui.ColorEdit3("Color tint", ref colorTintNew, ImGuiColorEditFlags.Float))
                {
                    SelectedTerrainObject.SetColor(colorTintNew.X, colorTintNew.Y, colorTintNew.Z);
                }
                if (ImGui.ColorEdit3("Color emissive", ref colorEmissiveNew, ImGuiColorEditFlags.Float))
                {
                    SelectedTerrainObject.SetColorEmissive(colorEmissiveNew.X, colorEmissiveNew.Y, colorEmissiveNew.Z, SelectedTerrainObject._stateCurrent._colorEmissive.W);
                }
                //

                if (ImGui.SliderFloat("", ref colorEmissiveIntensityNew, 0, 10, "%.2f", ImGuiSliderFlags.AlwaysClamp))
                {
                    SelectedTerrainObject.SetColorEmissive(colorEmissiveNew.X, colorEmissiveNew.Y, colorEmissiveNew.Z, colorEmissiveIntensityNew);
                }
                ImGui.SameLine();
                ImGui.Text("Emissive intensity");

                ImGui.Separator();

                float roughness = SelectedTerrainObject._gModel._roughnessTerrain;
                float metallic = SelectedTerrainObject._gModel._metallicTerrain;
                if (SelectedTerrainObject._gModel.Material[0].TextureRoughness.IsTextureSet == false)
                {
                    if (ImGui.SliderFloat("Global roughness", ref roughness, 0f, 1f))
                    {
                        SelectedTerrainObject.SetRoughness(roughness);
                    }
                }
                if (SelectedTerrainObject._gModel.Material[0].TextureMetallic.IsTextureSet == false)
                {
                    if (ImGui.SliderFloat("Global metallic", ref metallic, 0f, 1f))
                    {
                        SelectedTerrainObject.SetMetallic(metallic);
                    }
                }

                _metallicTypesIndex = (int)SelectedTerrainObject._gModel._metallicType;
                if (ImGui.Combo("Metallic type", ref _metallicTypesIndex, _metallicTypes, _metallicTypes.Length))
                {
                    SelectedTerrainObject.SetMetallicType(_metallicTypesIndex);
                }


                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Textures:");
                string albedoFilename = SelectedTerrainObject._gModel.Material[0].TextureAlbedo.IsTextureSet ? SelectedTerrainObject._gModel.Material[0].TextureAlbedo.Filename : "";
                string normalFilename = SelectedTerrainObject._gModel.Material[0].TextureNormal.IsTextureSet ? SelectedTerrainObject._gModel.Material[0].TextureNormal.Filename : "";
                string emissiveFilename = SelectedTerrainObject._gModel.Material[0].TextureEmissive.IsTextureSet ? SelectedTerrainObject._gModel.Material[0].TextureEmissive.Filename : "";
                string roughnessFilename = SelectedTerrainObject._gModel.Material[0].TextureRoughness.IsTextureSet ? SelectedTerrainObject._gModel.Material[0].TextureRoughness.Filename : "";
                string metallicFilename = SelectedTerrainObject._gModel.Material[0].TextureMetallic.IsTextureSet ? SelectedTerrainObject._gModel.Material[0].TextureMetallic.Filename : "";

                float uvX = SelectedTerrainObject._stateCurrent._uvTransform.X;
                float uvY = SelectedTerrainObject._stateCurrent._uvTransform.Y;
                System.Numerics.Vector2 uvTransform = new(uvX, uvY);

                if (ImGui.InputText("Albedo", ref albedoFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoUndoRedo))
                {
                    if (FileNameEmpty(albedoFilename))
                        SelectedTerrainObject._gModel.UnsetTextureForPrimitive(TextureType.Albedo);
                    else if (File.Exists(albedoFilename))
                        SelectedTerrainObject.SetTexture(albedoFilename.Trim(), TextureType.Albedo);
                }

                if (ImGui.InputText("Normal", ref normalFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoUndoRedo))
                {
                    if (FileNameEmpty(normalFilename))
                        SelectedTerrainObject._gModel.UnsetTextureForPrimitive(TextureType.Normal);
                    else if (File.Exists(normalFilename))
                        SelectedTerrainObject.SetTexture(normalFilename.Trim(), TextureType.Normal);
                }

                if (ImGui.InputText("Emissive", ref emissiveFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoUndoRedo))
                {
                    if (FileNameEmpty(emissiveFilename))
                        SelectedTerrainObject._gModel.UnsetTextureForPrimitive(TextureType.Emissive);
                    else if (File.Exists(emissiveFilename))
                        SelectedTerrainObject.SetTexture(emissiveFilename.Trim(), TextureType.Emissive);
                }

                if (ImGui.InputText("Metallic", ref metallicFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoUndoRedo))
                {
                    if (FileNameEmpty(metallicFilename))
                        SelectedTerrainObject._gModel.UnsetTextureForPrimitive(TextureType.Metallic);
                    else if (File.Exists(metallicFilename))
                        SelectedTerrainObject.SetTexture(metallicFilename.Trim(), TextureType.Metallic);
                }

                if (ImGui.InputText("Roughness", ref roughnessFilename, 256, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.NoUndoRedo))
                {
                    if (FileNameEmpty(roughnessFilename))
                        SelectedTerrainObject._gModel.UnsetTextureForPrimitive(TextureType.Roughness);
                    else if (File.Exists(roughnessFilename))
                        SelectedTerrainObject.SetTexture(roughnessFilename.Trim(), TextureType.Roughness);
                }

                if (SelectedTerrainObject._gModel.Material[0].TextureAlbedo.IsTextureSet)
                {
                    if (ImGui.InputFloat2("Texture repeat", ref uvTransform, "%.2f", ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.NoUndoRedo))
                    {
                        SelectedTerrainObject.SetTextureRepeat(uvTransform.X, uvTransform.Y);
                    }
                }                
                ImGui.End();
            }
        }

        internal static bool FileNameEmpty(string filename)
        {
            return filename == null || filename.Length == 0;
        }

        internal static EditorAddObjectType _addMenuActive = EditorAddObjectType.None;
        internal static bool _worldMenuActive = false;
        internal static string _addGameObjectClassName = "";
        internal static ShadowQuality _newLightShadowCasterQuality = ShadowQuality.NoShadow;
        internal static string[] _shadowCasterQualities = new string[] { "no shadow", "low", "medium", "high" };
        internal static int _newLightShadowCasterQualityIndex = 0;

        public static void DrawLightFrustum()
        {
            if (SelectedLightObject != null && KWEngine.DebugMode == DebugMode.None)
            {
                GL.Disable(EnableCap.DepthTest);
                RendererLightFrustum.Bind();
                RendererLightFrustum.SetGlobals();
                RendererLightFrustum.Draw(SelectedLightObject);
                GL.Enable(EnableCap.DepthTest);
            }
        }

        public static void Draw()
        {
            DrawGridAndBoundingBox();
            DrawLightFrustum();

            ImGui.BeginMainMenuBar();
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Quit"))
                {
                    KWEngine.Window.Close();
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Objects"))
            {
                if (ImGui.MenuItem("Add GameObject"))
                {
                    _addMenuActive = EditorAddObjectType.GameObject;
                    _worldMenuActive = false;
                }
                if (ImGui.MenuItem("Add LightObject"))
                {
                    _addMenuActive = EditorAddObjectType.LightObject;
                    _worldMenuActive = false;
                }
                if (ImGui.MenuItem("Remove selected"))
                {
                    _addMenuActive = EditorAddObjectType.None;

                    if (SelectedGameObject != null)
                    {
                        KWEngine.CurrentWorld.RemoveGameObject(SelectedGameObject);
                        SelectedGameObject = null;
                    }
                    else if (SelectedLightObject != null)
                    {
                        KWEngine.CurrentWorld.RemoveLightObject(SelectedLightObject);
                        SelectedLightObject = null;
                        
                    }
                    else if(SelectedTerrainObject != null)
                    {
                        KWEngine.CurrentWorld.RemoveTerrainObject(SelectedTerrainObject);
                        SelectedTerrainObject = null;
                    }
                    else
                    {
                        KWEngine.LogWriteLine("Cannot remove object - nothing selected.");
                    }
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("World"))
            {
                if (ImGui.MenuItem("Settings"))
                    _worldMenuActive = true;

                ImGui.EndMenu();
            }
            if(_worldMenuActive)
            {
                System.Numerics.Vector3 colorAmbientNew = new(KWEngine.CurrentWorld._colorAmbient.X, KWEngine.CurrentWorld._colorAmbient.Y, KWEngine.CurrentWorld._colorAmbient.Z);
                string bgTexture = GetBackgroundTextureName();
                float fov = (float)Math.Round(KWEngine.CurrentWorld._cameraEditor._stateCurrent._fov * 2);
                
                ImGui.Begin("World settings", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
                ImGui.SetWindowSize(new System.Numerics.Vector2(640, 256));
                ImGui.SetWindowPos(new System.Numerics.Vector2(0, KWEngine.Window.ClientSize.Y - 256), ImGuiCond.Once);

                ImGui.LabelText(KWEngine.CurrentWorld._gameObjects.Count.ToString() + " / " + KWEngine.CurrentWorld._lightObjects.Count.ToString(), "GameObject/LightObject instances:");
                ImGui.Separator();
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Background:");
                ImGui.Checkbox("Skybox", ref _isSkybox);
                ImGui.SameLine();
                if(ImGui.InputText("Texture", ref bgTexture, 128, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.AutoSelectAll))
                {
                    if (bgTexture == null || !File.Exists(bgTexture))
                    {
                        bgTexture = "";
                    }
                    else
                    {
                        if (_isSkybox)
                        {
                            KWEngine.CurrentWorld.SetBackgroundSkybox(bgTexture);
                        }
                        else
                        {
                            KWEngine.CurrentWorld.SetBackground2D(bgTexture);
                        }
                    }
                }
                if (ImGui.ColorEdit3("Color ambient", ref colorAmbientNew, ImGuiColorEditFlags.Float))
                {
                    KWEngine.CurrentWorld.SetColorAmbient(colorAmbientNew.X, colorAmbientNew.Y, colorAmbientNew.Z);
                }
                ImGui.SliderFloat("Background brightness", ref KWEngine.CurrentWorld._background._brightnessMultiplier, 0f, 10f);
                
                ImGui.Separator();
                
                /*ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Collision detection debugging:");
                ImGui.Checkbox("Octree", ref KWEngine._octreeVisible);
                ImGui.SameLine();
                ImGui.SliderFloat("Safety Zone", ref KWEngine._octreeSafetyZone, 0f, 10f);
                ImGui.Separator();
                */
                ImGui.PushItemWidth(96);
                if (ImGui.SliderFloat("Camera FOV", ref fov, 20f, 180f))
                {
                    KWEngine.CurrentWorld._cameraEditor.SetFOVForPerspectiveProjection(fov);
                }
                ImGui.PopItemWidth();
                ImGui.PushItemWidth(72);
                ImGui.SameLine();
                ImGui.SliderFloat("Glow Size", ref KWEngine._glowRadius, 0f, 1f);
                ImGui.SameLine();
                ImGui.SliderFloat("Glow #1", ref KWEngine._glowUpsampleF1, 0.01f, 1f);
                ImGui.SameLine();
                ImGui.SliderFloat("Glow #2", ref KWEngine._glowUpsampleF2, 0.01f, 1f);

                //SSAO
                ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Screen-Space Ambient Occlusion (SSAO):");
                ImGui.Checkbox("Enabled?", ref KWEngine._ssaoEnabled);
                ImGui.SameLine();
                ImGui.SliderFloat("Radius", ref KWEngine._ssaoRadius, 0.01f, 1.0f);
                ImGui.SameLine();
                ImGui.SliderFloat("Bias", ref KWEngine._ssaoBias, 0.00f, 0.5f);
                
                ImGui.PopItemWidth();

                /*ImGui.SameLine();
                ImGui.Indent(312);
                if (ImGui.Button("Apply perspective to game"))
                {
                    KWEngine.CurrentWorld._cameraGame = KWEngine.CurrentWorld._cameraEditor;
                    KWEngine.CurrentWorld._cameraGame._statePrevious = KWEngine.CurrentWorld._cameraGame._stateCurrent;
                }
                */
                ImGui.NewLine();
                if(ImGui.Button("Export world & objects"))
                {
                    World.Export();
                }
                ImGui.SameLine();
                ImGui.Indent(578);
                if (ImGui.Button("Close"))
                {
                    _worldMenuActive = false;
                }
                ImGui.End();
            }

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Reset"))
                {
                    KWEngine.CurrentWorld._cameraEditor = KWEngine.CurrentWorld._cameraGame;
                    KWEngine.CurrentWorld._cameraEditor.UpdatePitchYaw(true);
                }
                if(ImGui.MenuItem("Apply current view to game camera"))
                {
                    KWEngine.CurrentWorld._cameraGame = KWEngine.CurrentWorld._cameraEditor;
                    KWEngine.CurrentWorld._cameraGame._statePrevious = KWEngine.CurrentWorld._cameraGame._stateCurrent;
                }
                ImGui.EndMenu();
            }

            if(ImGui.BeginMenu("Debug Mode"))
            {
                // Debug Modes:
                // 1 = Depth
                // 2 = Color
                // 3 = Normals
                // 4 = SSAO
                // 5 = Bloom
                // 6 = MetallicRoughness
                // 7 = SM1
                // 8 = SM2
                // 9 = SM3
                if (ImGui.MenuItem("Deactivate", "", KWEngine.DebugMode == DebugMode.None))
                {
                    KWEngine.DebugMode = DebugMode.None;
                }
                if (ImGui.MenuItem("Albedo shader", "", KWEngine.DebugMode == DebugMode.Colors))
                {
                    KWEngine.DebugMode = DebugMode.Colors;
                }
                if (ImGui.MenuItem("Depth buffer", "", KWEngine.DebugMode == DebugMode.DepthBufferLinearized))
                {
                    KWEngine.DebugMode = DebugMode.DepthBufferLinearized;
                }
                if (ImGui.MenuItem("Surface normals", "", KWEngine.DebugMode == DebugMode.SurfaceNormals))
                {
                    KWEngine.DebugMode = DebugMode.SurfaceNormals;
                }
                if (ImGui.MenuItem("Ambient occlusion" + (!KWEngine.SSAO_Enabled ? " (currently disabled)" : ""), "", KWEngine.DebugMode == DebugMode.ScreenSpaceAmbientOcclusion))
                {
                    KWEngine.DebugMode = DebugMode.ScreenSpaceAmbientOcclusion;
                }
                if (ImGui.MenuItem("Glow", "", KWEngine.DebugMode == DebugMode.Glow))
                {
                    KWEngine.DebugMode = DebugMode.Glow;
                }
                if (ImGui.MenuItem("Metallic/Roughness", "", KWEngine.DebugMode == DebugMode.MetallicRoughness))
                {
                    KWEngine.DebugMode = DebugMode.MetallicRoughness;
                }


                // shadow maps
                List<FramebufferShadowMap> maps = new();
                foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
                {
                    if (l._fbShadowMap != null)
                    {
                        maps.Add(l._fbShadowMap);
                    }
                }
                if (maps.Count >= 1 && ImGui.MenuItem("Shadow map #1", "", KWEngine.DebugMode == DebugMode.DepthBufferShadowMap1))
                {
                    KWEngine.DebugMode = DebugMode.DepthBufferShadowMap1;
                }
                if (ImGui.MenuItem("Shadow map #2", "", KWEngine.DebugMode == DebugMode.DepthBufferShadowMap2))
                {
                    KWEngine.DebugMode = DebugMode.DepthBufferShadowMap2;
                }
                if (ImGui.MenuItem("Shadow map #3", "", KWEngine.DebugMode == DebugMode.DepthBufferShadowMap3))
                {
                    KWEngine.DebugMode = DebugMode.DepthBufferShadowMap3;
                }
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();

            if (_addMenuActive != EditorAddObjectType.None)
            {
                if (_addMenuActive == EditorAddObjectType.GameObject)
                {
                    ImGui.Begin("Add GameObject instance", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
                }
                else
                {
                    ImGui.Begin("Add LightObject instance", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
                }
                ImGui.SetWindowSize(new System.Numerics.Vector2(512, 256));
                ImGui.SetWindowPos(new System.Numerics.Vector2(0, KWEngine.Window.ClientSize.Y - 256), ImGuiCond.Once);
                if (_addMenuActive == EditorAddObjectType.GameObject)
                {
                    ImGui.Combo("Class name", ref _classNamesIndex, _classNames, _classNames.Length);

                    if (ImGui.Button("Add instance now!"))
                    {
                        if (_classNamesIndex >= 0)
                        {
                            SelectedLightObject = null;
                            SelectedGameObject = KWEngine.CurrentWorld.BuildAndAddDefaultGameObjectForEditor(_classNames[_classNamesIndex]);
                            _addMenuActive = EditorAddObjectType.None;
                        }
                    }
                }
                else
                {
                    ImGui.Combo("Light type", ref _lightTypeIndex, _lightTypes, _lightTypes.Length);
                    if (Framebuffer.ShadowMapCount < 3)
                    {
                        if (ImGui.Combo("Shadow caster quality", ref _newLightShadowCasterQualityIndex, _shadowCasterQualities, _shadowCasterQualities.Length))
                        {
                            string q = _shadowCasterQualities[_newLightShadowCasterQualityIndex];
                            _newLightShadowCasterQuality = q == "no shadow" ? ShadowQuality.NoShadow : q == "low" ? ShadowQuality.Low : q == "medium" ? ShadowQuality.Medium : ShadowQuality.High;
                        }
                    }
                    if (ImGui.Button("Add instance now!"))
                    {
                        if (_lightTypeIndex >= 0)
                        {
                            SelectedGameObject = null;
                            SelectedLightObject = KWEngine.CurrentWorld.BuildAndAddDefaultLightObjectForEditor(_lightTypes[_lightTypeIndex], _newLightShadowCasterQuality);
                            _addMenuActive = EditorAddObjectType.None;
                            _newLightShadowCasterQuality = ShadowQuality.NoShadow;
                            _newLightShadowCasterQualityIndex = 0;
                        }
                    }
                }
                ImGui.NewLine(); ImGui.NewLine(); ImGui.NewLine(); ImGui.NewLine(); ImGui.NewLine(); ImGui.NewLine(); ImGui.NewLine(); ImGui.NewLine();
                ImGui.Indent(512 - 64);
                if(ImGui.Button("Close"))
                {
                    _addMenuActive = EditorAddObjectType.None;
                }
                ImGui.End();
            }

            ImGui.Begin("Information", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            ImGui.SetWindowSize(new System.Numerics.Vector2(316, KWEngine.Window.ClientSize.Y - 32));
            ImGui.SetWindowPos(new System.Numerics.Vector2(0, 20), ImGuiCond.Once);
            ImGui.BeginChild("LOG", new System.Numerics.Vector2(300, 128-32), true, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.HorizontalScrollbar);
            ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Log messages:");
            ImGui.Separator();
            foreach (string logMessage in EngineLog._messages.ToArray())
            {
                ImGui.TextUnformatted(logMessage);
            }
            ImGui.SetScrollHereY(1.0f);
            ImGui.EndChild();

            // Frame times
            ImGui.BeginChild("PERFORMANCE", new System.Numerics.Vector2(300, 144+32), true, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
            ImGui.TextColored(new System.Numerics.Vector4(0, 1, 1, 1), "Performance analysis:");
            float[] ftimes = _frameTimes.ToArray();
            ImGui.PlotLines("Frame times", ref ftimes[0], _frameTimes.Count, 0, "", 1f, 100f);
            ImGui.LabelText(KWEngine.LastFrameTime.ToString("000.##") + " ms", "Last render cycle time:");
            ImGui.LabelText(_fpsValue.ToString("D5") + " fps", "Average fps:");
            //ImGui.LabelText(KWEngine.LastSimulationUpdateCycleCount.ToString(), "Last update cycle count:");
            ImGui.LabelText(_lastUpdateCycleTime.ToString("000.##") + " ms", "Last update cycle time:");
            //ImGui.LabelText(_lastRenderCallsTime.ToString("000.##") + " ms", "Last render calls time:");
            ImGui.LabelText(KWEngine._texMemUsed.ToString("00000") + " MB", "Texture memory:");
            ImGui.LabelText(KWEngine._geometryMemUsed.ToString("00000") + " MB", "Geometry memory:");
            ImGui.EndChild();

            // Object list:
            int y = KWEngine.Window.ClientSize.Y - 128 - 144 - 96;
            ImGui.BeginChild("OBJECTTREE", new System.Numerics.Vector2(300, y), true, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);            
            if (ImGui.TreeNodeEx("GameObject instances", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (GameObject g in KWEngine.CurrentWorld.GetGameObjectsSortedByType())
                {
                    if(g == SelectedGameObject)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.25f, 0.5f, 1f, 0.5f));
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new System.Numerics.Vector4(0.25f, 0.5f, 1f, 0.5f));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.5f, 0.75f, 1f, 0.5f));
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0, 0, 0, 0));
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, new System.Numerics.Vector4(0, 0, 0, 0));
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new System.Numerics.Vector4(0.25f, 0.5f, 1.0f, 0.25f));
                    }
                    
                    if (ImGui.SmallButton("[" + g.GetType().Name + "] " + g.ID + ": " + g.Name))
                    {
                        DeselectAll();
                        SelectedGameObject = g;
                    }
                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                }
            }
            ImGui.NewLine();
            if (ImGui.TreeNodeEx("LightObject instances", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.CollapsingHeader | ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
                {
                    if (l == SelectedLightObject)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.25f, 0.5f, 1f, 0.5f));
                    }
                    else
                    {
                        ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0, 0, 0, 0));
                    }
                    if (ImGui.SmallButton("[" + l.Type.ToString() + "] " + Math.Abs(l.ID) + ": " + l.Name))
                    {
                        DeselectAll();
                        SelectedLightObject = l;
                    }
                    ImGui.PopStyleColor();
                }
            }
            ImGui.PopStyleColor();

            ImGui.EndChild();

            ImGui.End();

            DrawObjectDetails();

            HandleKeyboardNavigation();
        }
        /*
        private static readonly float[] pixelColor = new float[4];
        internal static int FramebufferPicking(Vector2 mousePosition)
        {
            GL.GetInteger(GetPName.FramebufferBinding, out int previousFBID);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RenderManager.FramebufferDeferred.ID);
            GL.PixelStore(PixelStoreParameter.PackAlignment, 1);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment2);
            GL.ReadPixels((int)mousePosition.X, KWEngine.Window.ClientSize.Y - (int)mousePosition.Y, 1, 1, PixelFormat.Rgba, PixelType.Float, pixelColor);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, previousFBID);
            return (int)pixelColor[3];
        }
        */

        public static GameObject SelectedGameObject { get; internal set; } = null;
        public static TerrainObject SelectedTerrainObject { get; internal set; } = null;
        public static LightObject SelectedLightObject { get; internal set; } = null;

        public static void HandleKeyboardNavigation()
        {
            
            if (KWEngine.Mode == EngineMode.Edit && DiscardKeyboardNavigation() == false)
            {
                float rate = KWEngine.LastFrameTime / (1f/60f * 1000f);

                if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.A) || KWEngine.Window.KeyboardState.IsKeyDown(Keys.D))
                {
                    KWEngine.CurrentWorld._cameraEditor.Strafe(0.25f * rate * (KWEngine.Window.KeyboardState.IsKeyDown(Keys.A) ? -1 : 1));
                }
                if(KWEngine.Window.KeyboardState.IsKeyDown(Keys.W) || KWEngine.Window.KeyboardState.IsKeyDown(Keys.S))
                {
                    KWEngine.CurrentWorld._cameraEditor.Move(0.25f * rate * (KWEngine.Window.KeyboardState.IsKeyDown(Keys.S) ? -1 : 1));
                }
                if(KWEngine.Window.KeyboardState.IsKeyDown(Keys.E) || KWEngine.Window.KeyboardState.IsKeyDown(Keys.Q))
                {
                    KWEngine.CurrentWorld._cameraEditor.MoveUpDown(0.25f * rate * (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Q) ? -1 : 1));
                }

                if (KWEngine.CurrentWorld.ApplicationTime - _lastKeyboardTimeInSeconds >= KEYBOARDCOOLDOWN)
                {
                    _lastKeyboardTimeInSeconds = KWEngine.CurrentWorld.ApplicationTime;

                    if (SelectedGameObject != null)
                    {
                        if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.PageUp))
                        {
                            Vector3 p = SelectedGameObject.Position;
                            SelectedGameObject.SetPosition(p.X, (float)Math.Round(p.Y + POSITIONSTEP, ROUNDDIGITCOUNT), p.Z);
                        }
                        else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.PageDown))
                        {
                            Vector3 p = SelectedGameObject.Position;
                            SelectedGameObject.SetPosition(p.X, (float)Math.Round(p.Y - POSITIONSTEP, ROUNDDIGITCOUNT), p.Z);
                        }
                        else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Left))
                        {
                            Vector3 p = SelectedGameObject.Position;
                            SelectedGameObject.SetPosition((float)Math.Round(p.X - POSITIONSTEP, ROUNDDIGITCOUNT), p.Y, p.Z);
                        }
                        else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Right))
                        {
                            Vector3 p = SelectedGameObject.Position;
                            SelectedGameObject.SetPosition((float)Math.Round(p.X + POSITIONSTEP, ROUNDDIGITCOUNT), p.Y, p.Z);
                        }
                        else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Up))
                        {
                            Vector3 p = SelectedGameObject.Position;
                            SelectedGameObject.SetPosition(p.X, p.Y, (float)Math.Round(p.Z + POSITIONSTEP, ROUNDDIGITCOUNT));
                        }
                        else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Down))
                        {
                            Vector3 p = SelectedGameObject.Position;
                            SelectedGameObject.SetPosition(p.X, p.Y, (float)Math.Round(p.Z - POSITIONSTEP, ROUNDDIGITCOUNT));
                        }

                    }
                    else if (SelectedLightObject != null)
                    {
                        if(KWEngine.Window.KeyboardState.IsKeyDown(Keys.LeftControl) || KWEngine.Window.KeyboardState.IsKeyDown(Keys.RightControl))
                        {
                            // TARGETS:
                            if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.PageUp))
                            {
                                Vector3 p = SelectedLightObject.Target;
                                SelectedLightObject.SetTarget(p.X, (float)Math.Round(p.Y + POSITIONSTEP, ROUNDDIGITCOUNT), p.Z);
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.PageDown))
                            {
                                Vector3 p = SelectedLightObject.Target;
                                SelectedLightObject.SetTarget(p.X, (float)Math.Round(p.Y - POSITIONSTEP, ROUNDDIGITCOUNT), p.Z);
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Left))
                            {
                                Vector3 p = SelectedLightObject.Target;
                                SelectedLightObject.SetTarget((float)Math.Round(p.X - POSITIONSTEP, ROUNDDIGITCOUNT), p.Y, p.Z);
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Right))
                            {
                                Vector3 p = SelectedLightObject.Target;
                                SelectedLightObject.SetTarget((float)Math.Round(p.X + POSITIONSTEP, ROUNDDIGITCOUNT), p.Y, p.Z);
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Up))
                            {
                                Vector3 p = SelectedLightObject.Target;
                                SelectedLightObject.SetTarget(p.X, p.Y, (float)Math.Round(p.Z + POSITIONSTEP, ROUNDDIGITCOUNT));
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Down))
                            {
                                Vector3 p = SelectedLightObject.Target;
                                SelectedLightObject.SetTarget(p.X, p.Y, (float)Math.Round(p.Z - POSITIONSTEP, ROUNDDIGITCOUNT));
                            }
                        }
                        else
                        {
                            if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.PageUp))
                            {
                                Vector3 p = SelectedLightObject.Position;
                                SelectedLightObject.SetPosition(p.X, (float)Math.Round(p.Y + POSITIONSTEP, ROUNDDIGITCOUNT), p.Z);
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.PageDown))
                            {
                                Vector3 p = SelectedLightObject.Position;
                                SelectedLightObject.SetPosition(p.X, (float)Math.Round(p.Y - POSITIONSTEP, ROUNDDIGITCOUNT), p.Z);
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Left))
                            {
                                Vector3 p = SelectedLightObject.Position;
                                SelectedLightObject.SetPosition((float)Math.Round(p.X - POSITIONSTEP, ROUNDDIGITCOUNT), p.Y, p.Z);
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Right))
                            {
                                Vector3 p = SelectedLightObject.Position;
                                SelectedLightObject.SetPosition((float)Math.Round(p.X + POSITIONSTEP, ROUNDDIGITCOUNT), p.Y, p.Z);
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Up))
                            {
                                Vector3 p = SelectedLightObject.Position;
                                SelectedLightObject.SetPosition(p.X, p.Y, (float)Math.Round(p.Z + POSITIONSTEP, ROUNDDIGITCOUNT));
                            }
                            else if (KWEngine.Window.KeyboardState.IsKeyDown(Keys.Down))
                            {
                                Vector3 p = SelectedLightObject.Position;
                                SelectedLightObject.SetPosition(p.X, p.Y, (float)Math.Round(p.Z - POSITIONSTEP, ROUNDDIGITCOUNT));
                            }
                        }
                    }
                }
            }
        }

        public static void HandleMouseSelection(Vector2 mousePosition)
        {
            // scan for transparent objects first:
            List<GameObject> transparentObjects = KWEngine.CurrentWorld.GetTransparentGameObjectsForEditor();
            Vector3 origin = KWEngine.CurrentWorld._cameraEditor._stateCurrent._position;
            Vector3 lav = KWEngine.CurrentWorld._cameraEditor.Get3DMouseCoords();
            foreach (GameObject g in transparentObjects)
            {
                if (!g.IsInvisible)
                {
                    bool hit = HelperIntersection.GetRayIntersectionPointOnGameObject(g, origin, lav, out Vector3 intersectionPoint, out string hitboxname);
                    if (hit)
                    {
                        SelectedGameObject = g;
                        SelectedLightObject = null;
                        SelectedTerrainObject = null;
                        return;
                    }
                }
            }

            int id = HelperIntersection.FramebufferPicking(mousePosition);
            if (id != 0)
            {
                if (id > 0)
                {
                    if (id < ushort.MaxValue)
                    {
                        SelectedLightObject = null;
                        GameObject pickedGameObject = GetGameObjectForId(id);
                        if (pickedGameObject != null)
                        {
                            SelectedGameObject = pickedGameObject;
                            SelectedLightObject = null;
                            SelectedTerrainObject = null;
                        }
                        else
                        {
                            // maybe terrain?
                            SelectedTerrainObject = GetTerrainObjectForId(id);
                            if (SelectedTerrainObject != null)
                            {
                                SelectedGameObject = null;
                                SelectedLightObject = null;
                            }
                        }
                    }
                    else
                    {
                        // deselect
                        DeselectAll();
                    }
                }
                else
                {
                    // LightObject
                    SelectedGameObject = null;
                    SelectedTerrainObject = null;
                    SelectedLightObject = null;
                    LightObject pickedLightObject = GetLightObjectForId(id);
                    if (pickedLightObject != null)
                    {
                        SelectedLightObject = pickedLightObject;
                    }
                }
            }
            else
            {
                // deselect
                DeselectAll();
            }
        }

        internal static void DeselectAll()
        {
            SelectedGameObject = null;
            SelectedLightObject = null;
            SelectedTerrainObject = null;
        }

        public static void HandleMouseButtonStatus(MouseButton btn, bool press)
        {
            if (_mouseButtonsActive.ContainsKey(btn))
            {
                _mouseButtonsActive[btn].Pressed = press;
                if (press)
                {
                    if (IsCursorOnAnyControl())
                    {
                        _mouseButtonsActive[btn].PressedOnOverlay = true;
                    }
                }
                else
                {
                    _mouseButtonsActive[btn].PressedOnOverlay = false;
                }
            }
        }

        private static GameObject GetGameObjectForId(int id)
        {
            foreach (GameObject g in KWEngine.CurrentWorld._gameObjects)
            {
                if (g.ID == id)
                {
                    return g;
                }
            }
            return null;
        }

        private static TerrainObject GetTerrainObjectForId(int id)
        {
            foreach (TerrainObject t in KWEngine.CurrentWorld._terrainObjects)
            {
                if (t.ID == id)
                {
                    return t;
                }
            }
            return null;
        }

        private static LightObject GetLightObjectForId(int id)
        {
            foreach (LightObject l in KWEngine.CurrentWorld._lightObjects)
            {
                if (l.ID == id)
                {
                    return l;
                }
            }
            return null;
        }

        private static readonly string[] _classNames = GetClassNamesForExecutingAssembly();
        private static string[] _modelNames;
        private static readonly string[] _lightTypes = new string[] { "Point light", "Directional light", "Sun light" };
        private static int _classNamesIndex = -1;
        private static int _lightTypeIndex = 0;
        private static int _modelNamesIndex = 0;

        private static string GetBackgroundTextureName()
        {
            return KWEngine.CurrentWorld._background._filename;
        }
        private static string[] GetModelNamesForCurrentWorld()
        {
            List<string> names = new();
            if (KWEngine.CurrentWorld != null)
            {
                foreach (string model in KWEngine.Models.Keys)
                {
                    if (!KWEngine.Models[model].IsTerrain)
                        names.Add(model);
                }
            }
            return names.ToArray();
        }
        private static string[] GetClassNamesForExecutingAssembly()
        {
            List<string> names = new();
            Assembly a = Assembly.GetEntryAssembly();
            if (a != null)
            {

                foreach (Type t in a.ExportedTypes)
                {
                    if (t.IsSubclassOf(typeof(GameObject)))
                    {
                        names.Add(t.FullName);
                    }
                }
            }
            return names.ToArray();
        }
    }
}

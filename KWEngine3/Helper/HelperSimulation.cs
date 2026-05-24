using KWEngine3.EngineCamera;
using KWEngine3.GameObjects;
using KWEngine3.Model;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;
using System.Collections.Generic;

namespace KWEngine3.Helper
{
    internal static class HelperSimulation
    {
        internal static readonly GeoAnimation[]    _activeAnims      = new GeoAnimation[EngineObjectState.MAX_ANIMATION_LAYERS];
        internal static readonly float[]           _activeTimestamps = new float[EngineObjectState.MAX_ANIMATION_LAYERS];
        internal static readonly float[]           _activeWeights    = new float[EngineObjectState.MAX_ANIMATION_LAYERS];
        internal static readonly HashSet<string>[] _activeMasks      = new HashSet<string>[EngineObjectState.MAX_ANIMATION_LAYERS];

        public static void BlendWorldBackgroundStates(float alpha)
        {
            if(KWEngine.CurrentWorld._background.Type == BackgroundType.Standard)
            {
                KWEngine.CurrentWorld._background._stateRender.Scale = Vector2.Lerp(
                    KWEngine.CurrentWorld._background._statePrevious.Scale,
                    KWEngine.CurrentWorld._background._stateCurrent.Scale,
                    alpha);

                KWEngine.CurrentWorld._background._stateRender.Clip = Vector2.Lerp(
                    KWEngine.CurrentWorld._background._statePrevious.Clip,
                    KWEngine.CurrentWorld._background._stateCurrent.Clip,
                    alpha);

                KWEngine.CurrentWorld._background._stateRender.Offset = Vector2.Lerp(
                    KWEngine.CurrentWorld._background._statePrevious.Offset,
                    KWEngine.CurrentWorld._background._stateCurrent.Offset,
                    alpha);
            }
        }

        public static void BlendLightObjectStates(LightObject l, float alpha)
        {
            l._stateRender._nearFarFOVType = new Vector4(
                l._statePrevious._nearFarFOVType.X * alpha + l._stateCurrent._nearFarFOVType.X * (1f - alpha),
                l._statePrevious._nearFarFOVType.Y * alpha + l._stateCurrent._nearFarFOVType.Y * (1f - alpha),
                l._statePrevious._nearFarFOVType.Z * alpha + l._stateCurrent._nearFarFOVType.Z * (1f - alpha),
                l._stateCurrent._nearFarFOVType.W);
            l._stateRender._position = Vector3.Lerp(l._statePrevious._position, l._stateCurrent._position, alpha);
            l._stateRender._target = Vector3.Lerp(l._statePrevious._target, l._stateCurrent._target, alpha);
            l._stateRender._lookAtVector = Vector3.NormalizeFast(Vector3.Lerp(l._statePrevious._lookAtVector, l._stateCurrent._lookAtVector, alpha));
            l._stateRender._color = Vector4.Lerp(l._statePrevious._color, l._stateCurrent._color, alpha);
            if (l._stateRender._nearFarFOVType.W == 0f) // point light
            {
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                    MathHelper.DegreesToRadians(l._stateCurrent._nearFarFOVType.Z),
                    1f,
                    l._stateCurrent._nearFarFOVType.X,
                    l._stateCurrent._nearFarFOVType.Y);

                l._stateRender._viewProjectionMatrix[0] = Matrix4.LookAt(l._stateRender._position, l._stateRender._position + new Vector3(1, 0, 0), new Vector3(0, -1, 0)) * projection;
                l._stateRender._viewProjectionMatrix[1] = Matrix4.LookAt(l._stateRender._position, l._stateRender._position + new Vector3(-1, 0, 0), new Vector3(0, -1, 0)) * projection;

                l._stateRender._viewProjectionMatrix[2] = Matrix4.LookAt(l._stateRender._position, l._stateRender._position + new Vector3(0, 1, 0), new Vector3(0, 0, 1)) * projection;
                l._stateRender._viewProjectionMatrix[3] = Matrix4.LookAt(l._stateRender._position, l._stateRender._position + new Vector3(0, -1, 0), new Vector3(0, 0, -1)) * projection;

                l._stateRender._viewProjectionMatrix[4] = Matrix4.LookAt(l._stateRender._position, l._stateRender._position + new Vector3(0, 0, 1), new Vector3(0, -1, 0)) * projection;
                l._stateRender._viewProjectionMatrix[5] = Matrix4.LookAt(l._stateRender._position, l._stateRender._position + new Vector3(0, 0, -1), new Vector3(0, -1, 0)) * projection;
            }
            else if(l._stateRender._nearFarFOVType.W > 0f) {

                l._stateRender._viewProjectionMatrix[0] = 
                    Matrix4.LookAt(l._stateRender._position, l._stateRender._target, KWEngine.WorldUp) * 
                    Matrix4.CreatePerspectiveFieldOfView(
                        MathHelper.DegreesToRadians(l._stateRender._nearFarFOVType.Z / 2f), 
                        1f, // 1:1 because shadow maps are 1:1
                        l._stateRender._nearFarFOVType.X, 
                        l._stateRender._nearFarFOVType.Y
                        );
            }
            else
            {
                l._stateRender._viewProjectionMatrix[0] =
                    Matrix4.LookAt(l._stateRender._position, l._stateRender._target, KWEngine.WorldUp) *
                    Matrix4.CreateOrthographic(
                        l._stateRender._nearFarFOVType.Z,
                        l._stateRender._nearFarFOVType.Z,
                        l._stateRender._nearFarFOVType.X,
                        l._stateRender._nearFarFOVType.Y
                        );

                if(l.ShadowType == SunShadowType.CascadedShadowMap)
                {
                    l._stateRender._viewProjectionMatrix[1] =
                    Matrix4.LookAt(l._stateRender._position, l._stateRender._target, KWEngine.WorldUp) *
                    Matrix4.CreateOrthographic(
                        l._stateRender._nearFarFOVType.Z * (int)(l as LightObjectSun)._csmFactor,
                        l._stateRender._nearFarFOVType.Z * (int)(l as LightObjectSun)._csmFactor,
                        l._stateRender._nearFarFOVType.X,
                        l._stateRender._nearFarFOVType.Y
                        );
                }
            }
            l.UpdateFrustum();
        }

        public static void BlendTextObjectStates(TextObject t, float alpha)
        {
            t._stateRender._width = Vector3.Lerp(new Vector3(t._statePrevious._width), new Vector3(t._stateCurrent._width), alpha).X;
            t._stateRender._position = Vector3.Lerp(t._statePrevious._position, t._stateCurrent._position, alpha);
            t._stateRender._color = Vector4.Lerp(t._statePrevious._color, t._stateCurrent._color, alpha);
            t._stateRender._colorEmissive = Vector4.Lerp(t._statePrevious._colorEmissive, t._stateCurrent._colorEmissive, alpha);
            t._stateRender._scale = Vector3.Lerp(t._statePrevious._scale, t._stateCurrent._scale, alpha);
            t._stateRender._rotation = Quaternion.Slerp(t._statePrevious._rotation, t._stateCurrent._rotation, alpha);
            t._stateRender._spreadFactor = t._statePrevious._spreadFactor * (1f - alpha) + t._stateCurrent._spreadFactor * alpha;
        }

        public static void BlendTerrainObjectStates(TerrainObject t, float alpha)
        {
            t._stateRender._position = Vector3.Lerp(t._statePrevious._position, t._stateCurrent._position, alpha);
            t._stateRender._dimensions = t._stateCurrent._dimensions;
            t._stateRender._center = Vector3.Lerp(t._statePrevious._center, t._stateCurrent._center, alpha);
            t._stateRender._colorTint = Vector3.Lerp(t._statePrevious._colorTint, t._stateCurrent._colorTint, alpha);
            t._stateRender._colorEmissive = Vector4.Lerp(t._statePrevious._colorEmissive, t._stateCurrent._colorEmissive, alpha);
            t._stateRender._uvTransform = Vector4.Lerp(t._statePrevious._uvTransform, t._stateCurrent._uvTransform, alpha);
            t._stateRender._uvTransformSlope = Vector4.Lerp(t._statePrevious._uvTransformSlope, t._stateCurrent._uvTransformSlope, alpha);
            t._stateRender._modelMatrix = Matrix4.CreateTranslation(t._stateRender._position);
            t._stateRender._normalMatrix = Matrix4.Transpose(Matrix4.Invert(t._stateRender._modelMatrix));
        }

        public static void BlendGameObjectStates(GameObject g, float alpha, bool isVSG = false)
        {
            g._stateRender._scale = Vector3.Lerp(g._statePrevious._scale, g._stateCurrent._scale, alpha);
            g._stateRender._position = Vector3.Lerp(g._statePrevious._position, g._stateCurrent._position, alpha);
            g._stateRender._rotation = Quaternion.Slerp(g._statePrevious._rotation, g._stateCurrent._rotation, alpha);
            g._stateRender._modelMatrix = HelperMatrix.CreateModelMatrix(g._stateRender);
            g._stateRender._normalMatrix = Matrix4.Transpose(Matrix4.Invert(g._stateRender._modelMatrix));
            g._stateRender._lookAtVector = Vector3.Lerp(g._statePrevious._lookAtVector, g._stateCurrent._lookAtVector, alpha);

            // Animations-Layer von _stateCurrent in _stateRender übernehmen und Percentage interpolieren
            for (int li = 0; li < EngineObjectState.MAX_ANIMATION_LAYERS; li++)
            {
                AnimationLayer lc = g._stateCurrent.GetAnimationLayer(li);
                AnimationLayer lp = g._statePrevious.GetAnimationLayer(li);
                AnimationLayer lr = lc;
                lr.Percentage = lp.Percentage * alpha + lc.Percentage * (1f - alpha);
                g._stateRender.SetAnimationLayer(li, lr);
            }
            g._stateRender._opacity = g._statePrevious._opacity* alpha +g._stateCurrent._opacity * (1f - alpha);
            g._stateRender._colorTint = Vector3.Lerp(g._statePrevious._colorTint, g._stateCurrent._colorTint, alpha);
            //g._stateRender._colorEmissive = Vector4.Lerp(g._statePrevious._colorEmissive, g._stateCurrent._colorEmissive, alpha);
            if(g.BlendTextureStates)
                g._stateRender._uvTransform = Vector4.Lerp(g._statePrevious._uvTransform, g._stateCurrent._uvTransform, alpha);
            else
                g._stateRender._uvTransform = g._stateCurrent._uvTransform;
            g._stateRender._uvClip = Vector2.Lerp(g._statePrevious._uvClip, g._stateCurrent._uvClip, alpha);
            g._stateRender._center = Vector3.Lerp(g._statePrevious._center, g._stateCurrent._center, alpha);
            g._stateRender._dimensions = Vector3.Lerp(g._statePrevious._dimensions, g._stateCurrent._dimensions, alpha);

            foreach(int meshId in g._stateCurrent._rotationPre.Keys)
            {
                Vector3 lerped;
                if(g._statePrevious._rotationPre.ContainsKey(meshId))
                {
                    lerped = Vector3.Lerp(g._statePrevious._rotationPre[meshId], g._stateCurrent._rotationPre[meshId], alpha);
                }
                else
                {
                    lerped = g._stateCurrent._rotationPre[meshId];
                }
                
                lerped = new Vector3(MathHelper.DegreesToRadians(lerped.X), MathHelper.DegreesToRadians(lerped.Y), MathHelper.DegreesToRadians(lerped.Z));
                Quaternion q = Quaternion.FromAxisAngle(Vector3.UnitY, lerped.Y) * Quaternion.FromAxisAngle(Vector3.UnitZ, lerped.Z) * Quaternion.FromAxisAngle(Vector3.UnitX, lerped.X);
                g._stateRender._rotationPre[meshId] = q;
            }

            UpdateBoneTransforms(g);
            UpdateModelMatricesForRenderPass(g);
        }

        public static void BlendRenderObjectStates(RenderObject r, float alpha)
        {
            r._stateRender._scale = Vector3.Lerp(r._statePrevious._scale, r._stateCurrent._scale, alpha);
            r._stateRender._position = Vector3.Lerp(r._statePrevious._position, r._stateCurrent._position, alpha);
            r._stateRender._rotation = Quaternion.Slerp(r._statePrevious._rotation, r._stateCurrent._rotation, alpha);
            r._stateRender._modelMatrix = HelperMatrix.CreateModelMatrix(r._stateRender);
            r._stateRender._normalMatrix = Matrix4.Transpose(Matrix4.Invert(r._stateRender._modelMatrix));
            r._stateRender._lookAtVector = Vector3.Lerp(r._statePrevious._lookAtVector, r._stateCurrent._lookAtVector, alpha);

            // Animations-Layer von _stateCurrent in _stateRender übernehmen und Percentage interpolieren
            for (int li = 0; li < EngineObjectState.MAX_ANIMATION_LAYERS; li++)
            {
                AnimationLayer lc = r._stateCurrent.GetAnimationLayer(li);
                AnimationLayer lp = r._statePrevious.GetAnimationLayer(li);
                AnimationLayer lr = lc;
                lr.Percentage = lp.Percentage * alpha + lc.Percentage * (1f - alpha);
                r._stateRender.SetAnimationLayer(li, lr);
            }
            r._stateRender._opacity = r._statePrevious._opacity * alpha + r._stateCurrent._opacity * (1f - alpha);
            r._stateRender._colorTint = Vector3.Lerp(r._statePrevious._colorTint, r._stateCurrent._colorTint, alpha);

            if (r.BlendTextureStates)
                r._stateRender._uvTransform = Vector4.Lerp(r._statePrevious._uvTransform, r._stateCurrent._uvTransform, alpha);
            else
                r._stateRender._uvTransform = r._stateCurrent._uvTransform;

            r._stateRender._uvClip = Vector2.Lerp(r._statePrevious._uvClip, r._stateCurrent._uvClip, alpha);
            r._stateRender._center = Vector3.Lerp(r._statePrevious._center, r._stateCurrent._center, alpha);
            r._stateRender._dimensions = Vector3.Lerp(r._statePrevious._dimensions, r._stateCurrent._dimensions, alpha);

            foreach (int meshId in r._stateRender._rotationPre.Keys)
            {
                Vector3 lerped;
                if (r._statePrevious._rotationPre.ContainsKey(meshId))
                {
                    lerped = Vector3.Lerp(r._statePrevious._rotationPre[meshId], r._stateCurrent._rotationPre[meshId], alpha);
                }
                else
                {
                    lerped = r._stateCurrent._rotationPre[meshId];
                }
                lerped = new Vector3(MathHelper.DegreesToRadians(lerped.X), MathHelper.DegreesToRadians(lerped.Y), MathHelper.DegreesToRadians(lerped.Z));

                Quaternion q = Quaternion.FromAxisAngle(Vector3.UnitY, lerped.Y) * Quaternion.FromAxisAngle(Vector3.UnitZ, lerped.Z) * Quaternion.FromAxisAngle(Vector3.UnitX, lerped.X);
                r._stateRender._rotationPre[meshId] = q;
            }

            UpdateBoneTransforms(r);
            UpdateModelMatricesForRenderPass(r);
        }


        private static void UpdateModelMatricesForRenderPass(EngineObject g)
        {
            for (int index = 0; index < g._model.ModelOriginal.Meshes.Count; index++)
            {
                GeoMesh mesh = g._model.ModelOriginal.Meshes.Values.ElementAt(index);
                bool useMeshTransform = mesh.BoneNames.Count == 0 || !(g._stateRender._animationID >= 0 && g._model.ModelOriginal.Animations != null && g._model.ModelOriginal.Animations.Count > 0);
                if (useMeshTransform)
                {
                    Matrix4 meshTransform = mesh.Transform;
                    if(g._stateRender._rotationPre.ContainsKey(index))
                    {
                        meshTransform = Matrix4.CreateFromQuaternion(g._stateRender._rotationPre[index]) * meshTransform;
                    }
                    Matrix4.Mult(meshTransform, g._stateRender._modelMatrix, out g._stateRender._modelMatrices[g._model.ModelOriginal.IsKWCube6 ? 0 : index]);
                    g._stateRender._normalMatrices[g._model.ModelOriginal.IsKWCube6 ? 0 : index] = Matrix4.Transpose(Matrix4.Invert(g._stateRender._modelMatrices[g._model.ModelOriginal.IsKWCube6 ? 0 : index]));
                }
                else
                {
                    g._stateRender._modelMatrices[g._model.ModelOriginal.IsKWCube6 ? 0 : index] = g._stateRender._modelMatrix;
                    g._stateRender._normalMatrices[g._model.ModelOriginal.IsKWCube6 ? 0 : index] = g._stateRender._normalMatrix;
                }
            }
        }

        public static void BlendCameraStates(ref Camera camGame, ref Camera camEditor, float alpha)
        {
            camGame._stateRender._fov = camGame._statePrevious._fov * alpha + camGame._stateCurrent._fov * (1f - alpha);
            camGame._stateRender._position = Vector3.Lerp(camGame._statePrevious._position, camGame._stateCurrent._position, alpha);
            camGame._stateRender._target = Vector3.Lerp(camGame._statePrevious._target, camGame._stateCurrent._target, alpha);
            camGame._stateRender._rotation = Quaternion.Slerp(camGame._statePrevious._rotation, camGame._stateCurrent._rotation, alpha);
            camGame._stateRender._shakeOffset = camGame._stateCurrent._shakeOffset;
            camGame._stateRender._shakeDuration = camGame._stateCurrent._shakeDuration;
            camGame._stateRender._shakeTimestamp = camGame._stateCurrent._shakeTimestamp;

            camGame._stateRender.UpdateViewMatrixAndLookAtVectorRenderPass();
            camGame._stateRender.UpdateViewProjectionMatrix(camGame._zNear, camGame._zFar, true);

            camEditor._stateRender._fov = camEditor._statePrevious._fov * alpha + camEditor._stateCurrent._fov * (1f - alpha);
            camEditor._stateRender._position = Vector3.Lerp(camEditor._statePrevious._position, camEditor._stateCurrent._position, alpha);
            camEditor._stateRender._target = Vector3.Lerp(camEditor._statePrevious._target, camEditor._stateCurrent._target, alpha);
            camEditor._stateRender._rotation = Quaternion.Slerp(camEditor._statePrevious._rotation, camEditor._stateCurrent._rotation, alpha);
            camEditor._stateRender.UpdateViewMatrixAndLookAtVector();
            camEditor._stateRender.UpdateViewProjectionMatrix(camEditor._zNear, camEditor._zFar);
        }

        internal static void UpdateBoneTransformsForViewSpaceGameObject(ViewSpaceGameObject vsg)
        {
            if (vsg._gameObject.IsAnimated)
            {
                vsg._gameObject._attachBoneNodes.Clear();
                for (int i = 0; i < vsg._gameObject._gameObjectsAttached.Keys.Count; i++)
                {
                    GeoNode boneNode = vsg._gameObject._gameObjectsAttached.Keys.ElementAt(i);
                    vsg._gameObject._attachBoneNodes.Add(boneNode);
                }
                // Layer aus _stateCurrent nehmen (VSG nutzt den Simulations-Zustand)
                BlendAndApplyAnimationLayers(vsg._gameObject, ref vsg._gameObject._stateCurrent, vsg._gameObject._attachBoneNodes, isVSG: true);
            }
        }

        internal static void UpdateBoneTransforms(EngineObject g)
        {
            if (g.IsAnimated && g.IsInsideScreenSpaceForRenderPass)
            {
                List<GeoNode> attachBones = null;
                if (g is GameObject go)
                {
                    go._attachBoneNodes.Clear();
                    for (int i = 0; i < go._gameObjectsAttached.Keys.Count; i++)
                        go._attachBoneNodes.Add(go._gameObjectsAttached.Keys.ElementAt(i));
                    attachBones = go._attachBoneNodes;
                }
                BlendAndApplyAnimationLayers(g, ref g._stateRender, attachBones, isVSG: false);
            }
        }

        private static void BlendAndApplyAnimationLayers(EngineObject g, ref EngineObjectState state, List<GeoNode> attachBones, bool isVSG)
        {
            int activeCount = 0;
            for (int li = 0; li < EngineObjectState.MAX_ANIMATION_LAYERS; li++)
            {
                AnimationLayer layer = state.GetAnimationLayer(li);
                if (!layer.Active || layer.Weight <= 0f) continue;
                GeoAnimation anim = g._model.ModelOriginal.Animations[layer.AnimationID];
                _activeAnims[activeCount]      = anim;
                _activeTimestamps[activeCount] = layer.Percentage * anim.DurationInTicks;
                _activeWeights[activeCount]    = layer.Weight;
                _activeMasks[activeCount]      = layer.BoneMask;
                activeCount++;
            }
            if (activeCount == 0) return;
            Matrix4 identity = Matrix4.Identity;
            ReadNodeHierarchyBlended(g, _activeAnims, _activeTimestamps, _activeWeights, _activeMasks, activeCount,
                                     g._model.ModelOriginal.Root, ref identity, attachBones, isVSG);
        }

        internal static void BlendAndApplyAnimationLayers(EngineObject g, ref EngineObjectRenderState state, List<GeoNode> attachBones, bool isVSG)
        {
            int activeCount = 0;
            for (int li = 0; li < EngineObjectState.MAX_ANIMATION_LAYERS; li++)
            {
                AnimationLayer layer = state.GetAnimationLayer(li);
                if (!layer.Active || layer.Weight <= 0f) continue;
                GeoAnimation anim = g._model.ModelOriginal.Animations[layer.AnimationID];
                _activeAnims[activeCount]      = anim;
                _activeTimestamps[activeCount] = layer.Percentage * anim.DurationInTicks;
                _activeWeights[activeCount]    = layer.Weight;
                _activeMasks[activeCount]      = layer.BoneMask;
                activeCount++;
            }
            if (activeCount == 0) return;
            Matrix4 identity = Matrix4.Identity;
            ReadNodeHierarchyBlended(g, _activeAnims, _activeTimestamps, _activeWeights, _activeMasks, activeCount,
                                     g._model.ModelOriginal.Root, ref identity, attachBones, isVSG);
        }

        /// <summary>
        /// Traversiert die Knochen-Hierarchie einmalig. Pro Knoten werden alle aktiven Layer
        /// auf TRS-Ebene gemischt, bevor der globalTransform aufgebaut und die Bone-Matrix
        /// berechnet wird. Entspricht strukturell genau dem originalen ReadNodeHierarchy,
        /// erweitert um den Layer-Blend-Schritt.
        /// </summary>
        internal static void ReadNodeHierarchyBlended(
            EngineObject g,
            GeoAnimation[] anims,
            float[] timestamps,
            float[] weights,
            HashSet<string>[] masks,
            int count,
            GeoNode node,
            ref Matrix4 parentTransform,
            List<GeoNode> attachBones,
            bool isVSG)
        {
            // Mix TRS from all layers:
            Vector3    blendedS = Vector3.Zero;
            Vector3    blendedT = Vector3.Zero;
            float      blendedRx = 0f, blendedRy = 0f, blendedRz = 0f, blendedRw = 0f;
            float      totalWeight = 0f;
            bool       anyLayerContributed = false;

            // special case: only one layer and no masking active
            if (count == 1 && (masks[0] == null || masks[0].Count == 0))
            {
                Matrix4 nodeTransformation = node.Transform;
                anims[0].AnimationChannels.TryGetValue(node.Name, out GeoNodeAnimationChannel channel);
                if (channel != null)
                {
                    Vector3    s = channel.ScaleKeys       == null ? nodeTransformation.ExtractScale()         : CalcInterpolatedScaling(timestamps[0], ref channel);
                    Quaternion r = channel.RotationKeys    == null ? nodeTransformation.ExtractRotation(false) : CalcInterpolatedRotation(timestamps[0], ref channel);
                    Vector3    t = channel.TranslationKeys == null ? nodeTransformation.ExtractTranslation()   : CalcInterpolatedTranslation(timestamps[0], ref channel);
                    nodeTransformation = HelperMatrix.CreateModelMatrix(ref s, ref r, ref t);
                }
                Matrix4 globalTransformSingle = nodeTransformation * parentTransform;

                string boneLookupSingle = node.NameWithoutFBXSuffix;
                foreach (GeoMesh mesh in g._model.ModelOriginal.Meshes.Values)
                {
                    int index = mesh.BoneNames.IndexOf(boneLookupSingle);
                    if (index >= 0)
                    {
                        Matrix4 boneMatrix = mesh.BoneOffset[index] * globalTransformSingle * g._model.ModelOriginal.TransformGlobalInverse;
                        g._stateRender._boneTranslationMatrices[mesh.Name][index] = boneMatrix;

                        int tempIndex = attachBones != null ? attachBones.IndexOf(node) : -1;
                        if (tempIndex >= 0 && g is GameObject goSingle)
                        {
                            GameObject attachedObject = goSingle._gameObjectsAttached[node];
                            if (attachedObject != null)
                            {
                                Matrix4 attachmentMatrix = isVSG
                                    ? mesh.BoneOffsetInverse[index] * boneMatrix * g._stateCurrent._modelMatrix
                                    : mesh.BoneOffsetInverse[index] * boneMatrix * g._stateRender._modelMatrix;
                                Vector3 tmpUp      = attachedObject.LookAtVectorLocalUp    * attachedObject._positionOffsetForAttachment.Y;
                                Vector3 tmpRight   = attachedObject.LookAtVectorLocalRight * attachedObject._positionOffsetForAttachment.X;
                                Vector3 tmpForward = attachedObject.LookAtVector           * attachedObject._positionOffsetForAttachment.Z;
                                attachedObject.SetScaleRotationAndTranslation(
                                    attachmentMatrix.ExtractScale()        * attachedObject._scaleOffsetForAttachment,
                                    attachmentMatrix.ExtractRotation(true) * attachedObject._rotationOffsetForAttachment,
                                    attachmentMatrix.ExtractTranslation()  + tmpUp + tmpRight + tmpForward,
                                    false);
                            }
                        }
                    }
                }
                for (int i = 0; i < node.Children.Count; i++)
                    ReadNodeHierarchyBlended(g, anims, timestamps, weights, masks, count,
                                             node.Children[i], ref globalTransformSingle, attachBones, isVSG);
                return;
            }

            // Mehrere Layer oder BoneMask aktiv: TRS-Blend
            for (int li = 0; li < count; li++)
            {
                // Bone-Masken-Check: Gilt dieser Layer für diesen Knoten?
                HashSet<string> mask = masks[li];
                bool inMask = mask == null || mask.Count == 0
                              || mask.Contains(node.Name)
                              || (node.NameWithoutFBXSuffix != null && mask.Contains(node.NameWithoutFBXSuffix));
                if (!inMask)
                    continue;

                Matrix4 nodeTransformation = node.Transform;
                anims[li].AnimationChannels.TryGetValue(node.Name, out GeoNodeAnimationChannel channelMulti);

                Vector3    s;
                Quaternion r;
                Vector3    t;

                if (channelMulti != null)
                {
                    s = channelMulti.ScaleKeys       == null ? nodeTransformation.ExtractScale()         : CalcInterpolatedScaling(timestamps[li], ref channelMulti);
                    r = channelMulti.RotationKeys    == null ? nodeTransformation.ExtractRotation(false) : CalcInterpolatedRotation(timestamps[li], ref channelMulti);
                    t = channelMulti.TranslationKeys == null ? nodeTransformation.ExtractTranslation()   : CalcInterpolatedTranslation(timestamps[li], ref channelMulti);
                }
                else
                {
                    s = nodeTransformation.ExtractScale();
                    r = nodeTransformation.ExtractRotation(false);
                    t = nodeTransformation.ExtractTranslation();
                }

                float w = weights[li];
                blendedS += s * w;
                blendedT += t * w;

                // Quaternion-Akkumulation: beim ersten Layer direkt setzen, danach Halbkugel prüfen
                if (!anyLayerContributed)
                {
                    blendedRx = r.X * w;
                    blendedRy = r.Y * w;
                    blendedRz = r.Z * w;
                    blendedRw = r.W * w;
                }
                else
                {
                    Quaternion acc = new Quaternion(blendedRx, blendedRy, blendedRz, blendedRw);
                    if (HelperRotation.Dot(acc, r) < 0f)
                        r = new Quaternion(-r.X, -r.Y, -r.Z, -r.W);
                    blendedRx += r.X * w;
                    blendedRy += r.Y * w;
                    blendedRz += r.Z * w;
                    blendedRw += r.W * w;
                }

                totalWeight        += w;
                anyLayerContributed = true;
            }

            // globalTransform aufbauen
            Matrix4 globalTransform;
            if (anyLayerContributed)
            {
                blendedS /= totalWeight;
                blendedT /= totalWeight;
                Quaternion blendedR = Quaternion.Normalize(new Quaternion(blendedRx, blendedRy, blendedRz, blendedRw));
                Matrix4 nodeTransformation = HelperMatrix.CreateModelMatrix(ref blendedS, ref blendedR, ref blendedT);
                globalTransform = nodeTransformation * parentTransform;
            }
            else
            {
                // Kein Layer gilt für diesen Knoten (BoneMask): unveränderter Node-Transform
                Matrix4 nodeTransformation = node.Transform;
                globalTransform = nodeTransformation * parentTransform;
            }

            // Bone-Matrix berechnen und speichern (wie vor dem Blending)
            string boneLookup = node.NameWithoutFBXSuffix;
            foreach (GeoMesh mesh in g._model.ModelOriginal.Meshes.Values)
            {
                int index = mesh.BoneNames.IndexOf(boneLookup);
                if (index >= 0)
                {
                    Matrix4 boneMatrix = mesh.BoneOffset[index] * globalTransform * g._model.ModelOriginal.TransformGlobalInverse;
                    g._stateRender._boneTranslationMatrices[mesh.Name][index] = boneMatrix;

                    int tempIndex = attachBones != null ? attachBones.IndexOf(node) : -1;
                    if (tempIndex >= 0 && g is GameObject go2)
                    {
                        GameObject attachedObject = go2._gameObjectsAttached[node];
                        if (attachedObject != null)
                        {
                            Matrix4 attachmentMatrix;
                            if (isVSG)
                                attachmentMatrix = mesh.BoneOffsetInverse[index] * boneMatrix * g._stateCurrent._modelMatrix;
                            else
                                attachmentMatrix = mesh.BoneOffsetInverse[index] * boneMatrix * g._stateRender._modelMatrix;

                            Vector3 tmpUp      = attachedObject.LookAtVectorLocalUp    * attachedObject._positionOffsetForAttachment.Y;
                            Vector3 tmpRight   = attachedObject.LookAtVectorLocalRight * attachedObject._positionOffsetForAttachment.X;
                            Vector3 tmpForward = attachedObject.LookAtVector           * attachedObject._positionOffsetForAttachment.Z;
                            attachedObject.SetScaleRotationAndTranslation(
                                attachmentMatrix.ExtractScale()        * attachedObject._scaleOffsetForAttachment,
                                attachmentMatrix.ExtractRotation(true) * attachedObject._rotationOffsetForAttachment,
                                attachmentMatrix.ExtractTranslation()  + tmpUp + tmpRight + tmpForward,
                                false);
                        }
                    }
                }
            }

            // Kindknoten rekursiv verarbeiten:
            for (int i = 0; i < node.Children.Count; i++)
                ReadNodeHierarchyBlended(g, anims, timestamps, weights, masks, count,
                                         node.Children[i], ref globalTransform, attachBones, isVSG);
        }

        private static Vector3 CalcInterpolatedScaling(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.ScaleKeys == null)
                return new Vector3(1, 1, 1);

            List<GeoAnimationKeyframe> keys = channel.ScaleKeys;
            int[] disabled = channel.DisabledScaleKeyAxes;

            GeoAnimationKeyframe firstKey = keys[0];
            if (timestamp < firstKey.Time)
            {
                float factor = timestamp / firstKey.Time;
                if (disabled == null)
                {
                    Vector3.Lerp(keys[keys.Count - 1].Scale, firstKey.Scale, factor, out Vector3 s);
                    return s;
                }
                Vector3 loS = GetEffectiveScale(keys, disabled, keys.Count - 1);
                Vector3 hiS = GetEffectiveScale(keys, disabled, 0);
                Vector3.Lerp(loS, hiS, factor, out Vector3 sDisabled);
                return sDisabled;
            }
            if (timestamp > keys[keys.Count - 1].Time)
            {
                if (disabled == null)
                    return keys[keys.Count - 1].Scale;
                return GetEffectiveScale(keys, disabled, keys.Count - 1);
            }

            int lo = 0, hi = keys.Count - 2;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (keys[mid].Time <= timestamp)
                    lo = mid;
                else
                    hi = mid - 1;
            }
            {
                GeoAnimationKeyframe key  = keys[lo];
                GeoAnimationKeyframe key2 = keys[lo + 1];
                float factor = (timestamp - key.Time) / (key2.Time - key.Time);
                if (disabled == null)
                {
                    Vector3.Lerp(key.Scale, key2.Scale, factor, out Vector3 scaling);
                    return scaling;
                }
                Vector3 loScale = GetEffectiveScale(keys, disabled, lo);
                Vector3 hiScale = GetEffectiveScale(keys, disabled, lo + 1);
                Vector3.Lerp(loScale, hiScale, factor, out Vector3 scalingDisabled);
                return scalingDisabled;
            }
        }

        private static Vector3 CalcInterpolatedTranslation(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.TranslationKeys == null)
                return Vector3.Zero;

            List<GeoAnimationKeyframe> keys = channel.TranslationKeys;
            int[] disabled = channel.DisabledTranslationKeyAxes;

            GeoAnimationKeyframe firstKey = keys[0];
            if (timestamp < firstKey.Time)
            {
                float factor = timestamp / firstKey.Time;
                if (disabled == null)
                {
                    Vector3.Lerp(keys[keys.Count - 1].Translation, firstKey.Translation, factor, out Vector3 t);
                    return t;
                }
                Vector3 loT = GetEffectiveTranslation(keys, disabled, keys.Count - 1);
                Vector3 hiT = GetEffectiveTranslation(keys, disabled, 0);
                Vector3.Lerp(loT, hiT, factor, out Vector3 tDisabled);
                return tDisabled;
            }
            if (timestamp > keys[keys.Count - 1].Time)
            {
                if (disabled == null)
                    return keys[keys.Count - 1].Translation;
                return GetEffectiveTranslation(keys, disabled, keys.Count - 1);
            }

            int lo = 0, hi = keys.Count - 2;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (keys[mid].Time <= timestamp)
                    lo = mid;
                else
                    hi = mid - 1;
            }
            {
                GeoAnimationKeyframe key  = keys[lo];
                GeoAnimationKeyframe key2 = keys[lo + 1];
                float factor = (timestamp - key.Time) / (key2.Time - key.Time);
                if (disabled == null)
                {
                    Vector3.Lerp(key.Translation, key2.Translation, factor, out Vector3 trans);
                    return trans;
                }
                Vector3 loTrans = GetEffectiveTranslation(keys, disabled, lo);
                Vector3 hiTrans = GetEffectiveTranslation(keys, disabled, lo + 1);
                Vector3.Lerp(loTrans, hiTrans, factor, out Vector3 transDisabled);
                return transDisabled;
            }
        }

        private static Quaternion CalcInterpolatedRotation(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.RotationKeys == null)
                return Quaternion.Identity;

            List<GeoAnimationKeyframe> keys = channel.RotationKeys;
            int[] disabled = channel.DisabledRotationKeyAxes;

            GeoAnimationKeyframe firstKey = keys[0];
            if (timestamp < firstKey.Time)
            {
                float factor = timestamp / firstKey.Time;
                if (disabled == null)
                    return Quaternion.Slerp(keys[keys.Count - 1].Rotation, firstKey.Rotation, factor);
                Quaternion loR = GetEffectiveRotation(keys, disabled, keys.Count - 1);
                Quaternion hiR = GetEffectiveRotation(keys, disabled, 0);
                return Quaternion.Slerp(loR, hiR, factor);
            }
            if (timestamp > keys[keys.Count - 1].Time)
            {
                if (disabled == null)
                    return keys[keys.Count - 1].Rotation;
                return GetEffectiveRotation(keys, disabled, keys.Count - 1);
            }

            int lo = 0, hi = keys.Count - 2;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) / 2;
                if (keys[mid].Time <= timestamp)
                    lo = mid;
                else
                    hi = mid - 1;
            }
            {
                GeoAnimationKeyframe key  = keys[lo];
                GeoAnimationKeyframe key2 = keys[lo + 1];
                float factor = (timestamp - key.Time) / (key2.Time - key.Time);
                if (disabled == null)
                    return Quaternion.Slerp(key.Rotation, key2.Rotation, factor);
                Quaternion loRot = GetEffectiveRotation(keys, disabled, lo);
                Quaternion hiRot = GetEffectiveRotation(keys, disabled, lo + 1);
                return Quaternion.Slerp(loRot, hiRot, factor);
            }
        }

        // Gibt für jeden Translation-Kanal die effektive Komponente zurück:
        // Für jede Achse (X/Y/Z) wird rückwärts ab 'index' der letzte nicht-disabled Keyframe gesucht.
        // Fallback wenn keiner gefunden: 0f.
        private static Vector3 GetEffectiveTranslation(List<GeoAnimationKeyframe> keys, int[] disabled, int index)
        {
            const int maskX = (int)AnimationKeyframeType.PositionX;
            const int maskY = (int)AnimationKeyframeType.PositionY;
            const int maskZ = (int)AnimationKeyframeType.PositionZ;

            float x = 0f, y = 0f, z = 0f;
            bool foundX = false, foundY = false, foundZ = false;

            for (int i = index; i >= 0 && !(foundX && foundY && foundZ); i--)
            {
                int d = disabled[i];
                if (!foundX && (d & maskX) == 0) { x = keys[i].Translation.X; foundX = true; }
                if (!foundY && (d & maskY) == 0) { y = keys[i].Translation.Y; foundY = true; }
                if (!foundZ && (d & maskZ) == 0) { z = keys[i].Translation.Z; foundZ = true; }
            }
            return new Vector3(x, y, z);
        }

        // Rückwärts ab 'index' den letzten nicht-disabled Scale-Keyframe suchen.
        // Fallback: Vector3.One.
        private static Vector3 GetEffectiveScale(List<GeoAnimationKeyframe> keys, int[] disabled, int index)
        {
            for (int i = index; i >= 0; i--)
            {
                if (disabled[i] == 0)
                    return keys[i].Scale;
            }
            return Vector3.One;
        }

        // Rückwärts ab 'index' den letzten nicht-disabled Rotation-Keyframe suchen.
        // Fallback: Quaternion.Identity.
        private static Quaternion GetEffectiveRotation(List<GeoAnimationKeyframe> keys, int[] disabled, int index)
        {
            for (int i = index; i >= 0; i--)
            {
                if (disabled[i] == 0)
                    return keys[i].Rotation;
            }
            return Quaternion.Identity;
        }
    }
}

﻿using KWEngine3.EngineCamera;
using KWEngine3.GameObjects;
using KWEngine3.Model;
using OpenTK.Mathematics;
using OpenTK.Platform.Windows;

namespace KWEngine3.Helper
{
    internal static class HelperSimulation
    {
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
            t._stateRender._position = Vector3.Lerp(t._statePrevious._position, t._stateCurrent._position, alpha);
            t._stateRender._position.X -= t._width * 0.5f;
            t._stateRender._color = Vector4.Lerp(t._statePrevious._color, t._stateCurrent._color, alpha);
            t._stateRender._colorEmissive = Vector4.Lerp(t._statePrevious._colorEmissive, t._stateCurrent._colorEmissive, alpha);
            t._stateRender._scale = Vector3.Lerp(new Vector3(t._statePrevious._scale), new Vector3(t._stateCurrent._scale), alpha).X;
            t._stateRender._rotation = Quaternion.Slerp(t._statePrevious._rotation, t._stateCurrent._rotation, alpha);
            t._stateRender._width = Vector3.Lerp(new Vector3(t._statePrevious._width), new Vector3(t._stateCurrent._width), alpha).X;
            t._stateRender._spreadFactor = t._statePrevious._spreadFactor * (1f - alpha) + t._stateCurrent._spreadFactor * alpha;

            Vector3 tmpScale = new(t._stateRender._scale);
            t._stateRender._modelMatrix = HelperMatrix.CreateModelMatrix(ref tmpScale, ref t._stateRender._rotation, ref t._stateRender._position);
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

            g._stateRender._animationID = g._stateCurrent._animationID;
            g._stateRender._animationPercentage = g._statePrevious._animationPercentage * alpha + g._stateCurrent._animationPercentage * (1f - alpha);
            g._stateRender._opacity = g._statePrevious._opacity* alpha +g._stateCurrent._opacity * (1f - alpha);
            g._stateRender._colorTint = Vector3.Lerp(g._statePrevious._colorTint, g._stateCurrent._colorTint, alpha);
            g._stateRender._colorEmissive = Vector4.Lerp(g._statePrevious._colorEmissive, g._stateCurrent._colorEmissive, alpha);
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

            r._stateRender._animationID = r._stateCurrent._animationID;
            r._stateRender._animationPercentage = r._statePrevious._animationPercentage * alpha + r._stateCurrent._animationPercentage * (1f - alpha);
            r._stateRender._opacity = r._statePrevious._opacity * alpha + r._stateCurrent._opacity * (1f - alpha);
            r._stateRender._colorTint = Vector3.Lerp(r._statePrevious._colorTint, r._stateCurrent._colorTint, alpha);
            r._stateRender._colorEmissive = Vector4.Lerp(r._statePrevious._colorEmissive, r._stateCurrent._colorEmissive, alpha);
            r._stateRender._uvTransform = Vector4.Lerp(r._statePrevious._uvTransform, r._stateCurrent._uvTransform, alpha);
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
                GeoAnimation a = vsg._gameObject._model.ModelOriginal.Animations[vsg._gameObject._stateCurrent._animationID];
                Matrix4 identity = Matrix4.Identity;
                vsg._gameObject._attachBoneNodes.Clear();
                for (int i = 0; i < vsg._gameObject._gameObjectsAttached.Keys.Count; i++)
                {
                    GeoNode boneNode = vsg._gameObject._gameObjectsAttached.Keys.ElementAt(i);
                    vsg._gameObject._attachBoneNodes.Add(boneNode);
                }
                ReadNodeHierarchy(vsg._gameObject, a.DurationInTicks * vsg._gameObject._stateCurrent._animationPercentage, ref a, vsg._gameObject._model.ModelOriginal.Root, ref identity, vsg._gameObject._attachBoneNodes, true);
            }
        }

        internal static void UpdateBoneTransforms(EngineObject g)
        {
            if (g.IsAnimated)
            {
                if (g.IsInsideScreenSpaceForRenderPass)
                {
                    GeoAnimation a = g._model.ModelOriginal.Animations[g._stateRender._animationID];
                    Matrix4 identity = Matrix4.Identity;
                    if (g is GameObject)
                    {
                        GameObject go = g as GameObject;
                        go._attachBoneNodes.Clear();
                        for (int i = 0; i < go._gameObjectsAttached.Keys.Count; i++)
                        {
                            GeoNode boneNode = go._gameObjectsAttached.Keys.ElementAt(i);
                            go._attachBoneNodes.Add(boneNode);
                        }
                        ReadNodeHierarchy(g, a.DurationInTicks * g._stateRender._animationPercentage, ref a, g._model.ModelOriginal.Root, ref identity, go._attachBoneNodes);
                    }
                    else
                    {
                        ReadNodeHierarchy(g, a.DurationInTicks * g._stateRender._animationPercentage, ref a, g._model.ModelOriginal.Root, ref identity, null);
                    }
                }
            }
        }

        private static void ReadNodeHierarchy(EngineObject g, float timestamp, ref GeoAnimation animation, GeoNode node, ref Matrix4 parentTransform, List<GeoNode> attachBones, bool isVSG = false)
        {
            Matrix4 nodeTransformation = node.Transform;
            string channelLookup = node.Name;
            animation.AnimationChannels.TryGetValue(channelLookup, out GeoNodeAnimationChannel channel);

            if (channel != null)
            {
                Vector3 s = channel.ScaleKeys == null ? nodeTransformation.ExtractScale() : CalcInterpolatedScaling(timestamp, ref channel);
                Quaternion r = channel.RotationKeys == null ? nodeTransformation.ExtractRotation(false) : CalcInterpolatedRotation(timestamp, ref channel);
                Vector3 t = channel.TranslationKeys == null ? nodeTransformation.ExtractTranslation() : CalcInterpolatedTranslation(timestamp, ref channel);
                nodeTransformation = HelperMatrix.CreateModelMatrix(ref s, ref r, ref t);
            }
            Matrix4 globalTransform = nodeTransformation * parentTransform;

            foreach (GeoMesh mesh in g._model.ModelOriginal.Meshes.Values)
            {
                int index = mesh.BoneNames.IndexOf(node.NameWithoutFBXSuffix);
                if (index >= 0)
                {
                    Matrix4 boneMatrix = mesh.BoneOffset[index] * globalTransform * g._model.ModelOriginal.TransformGlobalInverse;
                    g._stateRender._boneTranslationMatrices[mesh.Name][index] = boneMatrix;
                    int tempIndex = attachBones != null ? attachBones.IndexOf(node) : -1;
                    if (tempIndex >= 0 && g is GameObject)
                    {
                        GameObject attachedObject = (g as GameObject)._gameObjectsAttached[node];
                        if (attachedObject != null)
                        {
                        Matrix4 attachmentMatrix;
                            if (isVSG)
                            {
                                attachmentMatrix = mesh.BoneOffsetInverse[index] * boneMatrix * g._stateCurrent._modelMatrix;
                            }
                            else
                            {
                                attachmentMatrix = mesh.BoneOffsetInverse[index] * boneMatrix * g._stateRender._modelMatrix;
                            }

                            Vector3 tmpUp = attachedObject.LookAtVectorLocalUp * attachedObject._positionOffsetForAttachment.Y;
                            Vector3 tmpRight = attachedObject.LookAtVectorLocalRight * attachedObject._positionOffsetForAttachment.X;
                            Vector3 tmpForward = attachedObject.LookAtVector * attachedObject._positionOffsetForAttachment.Z;
                            attachedObject.SetScaleRotationAndTranslation(
                                attachmentMatrix.ExtractScale() * attachedObject._scaleOffsetForAttachment,
                                attachmentMatrix.ExtractRotation(true) * attachedObject._rotationOffsetForAttachment,
                                attachmentMatrix.ExtractTranslation() + tmpUp + tmpRight + tmpForward
                                );
                        }
                    }
                }
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                ReadNodeHierarchy(g, timestamp, ref animation, node.Children[i], ref globalTransform, attachBones);
            }
        }

        private static Vector3 CalcInterpolatedScaling(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.ScaleKeys == null)
            {
                return new Vector3(1, 1, 1);
            }

            GeoAnimationKeyframe firstKey = channel.ScaleKeys[0];
            if (timestamp < firstKey.Time)
            {
                GeoAnimationKeyframe lastKey = channel.ScaleKeys[channel.ScaleKeys.Count - 1];
                float deltaTime = firstKey.Time;
                float factor = timestamp / deltaTime;
                Vector3 scalingStart = lastKey.Scale;
                Vector3 scalingEnd = firstKey.Scale;
                Vector3.Lerp(scalingStart, scalingEnd, factor, out Vector3 scaling);
                return scaling;
            }
            if(timestamp > channel.ScaleKeys[channel.ScaleKeys.Count - 1].Time)
            {
                return channel.ScaleKeys[channel.ScaleKeys.Count - 1].Scale;
            }

            for (int i = 0; i < channel.ScaleKeys.Count - 1; i++)
            {
                GeoAnimationKeyframe key = channel.ScaleKeys[i];
                if (timestamp >= key.Time && timestamp <= channel.ScaleKeys[i + 1].Time)
                {
                    GeoAnimationKeyframe key2 = channel.ScaleKeys[i + 1];

                    float deltaTime = (key2.Time - key.Time);
                    float factor = (timestamp - key.Time) / deltaTime;
                    Vector3 scalingStart = key.Scale;
                    Vector3 scalingEnd = key2.Scale;
                    Vector3.Lerp(scalingStart, scalingEnd, factor, out Vector3 scaling);
                    return scaling;
                }
            }
            return new Vector3(1, 1, 1);
        }

        private static Vector3 CalcInterpolatedTranslation(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.TranslationKeys == null)
            {
                return new Vector3(0, 0, 0);
            }

            GeoAnimationKeyframe firstKey = channel.TranslationKeys[0];
            if (timestamp < firstKey.Time)
            {
                GeoAnimationKeyframe lastKey = channel.TranslationKeys[channel.TranslationKeys.Count - 1];
                float deltaTime = firstKey.Time;
                float factor = timestamp / deltaTime;
                Vector3 transStart = lastKey.Translation;
                Vector3 transEnd = firstKey.Translation;
                Vector3.Lerp(transStart, transEnd, factor, out Vector3 translation);
                return translation;
            }
            if (timestamp > channel.TranslationKeys[channel.TranslationKeys.Count - 1].Time)
            {
                return channel.TranslationKeys[channel.TranslationKeys.Count - 1].Translation;
            }

            for (int i = 0; i < channel.TranslationKeys.Count - 1; i++)
            {
                GeoAnimationKeyframe key = channel.TranslationKeys[i];
                
                if (timestamp >= key.Time && timestamp <= channel.TranslationKeys[i + 1].Time)
                {
                    GeoAnimationKeyframe key2 = channel.TranslationKeys[i + 1];

                    float deltaTime = (key2.Time - key.Time);
                    float factor = (timestamp - key.Time) / deltaTime;
                    Vector3 transStart = key.Translation;
                    Vector3 transEnd = key2.Translation;
                    Vector3.Lerp(transStart, transEnd, factor, out Vector3 trans);
                    return trans;
                }
            }
            return new Vector3(0, 0, 0);
        }

        private static Quaternion CalcInterpolatedRotation(float timestamp, ref GeoNodeAnimationChannel channel)
        {
            if (channel.RotationKeys == null)
            {
                return Quaternion.Identity;
            }

            GeoAnimationKeyframe firstKey = channel.RotationKeys[0];
            if (timestamp < firstKey.Time)
            {
                GeoAnimationKeyframe lastKey = channel.RotationKeys[channel.RotationKeys.Count - 1];
                float deltaTime = firstKey.Time;
                float factor = timestamp / deltaTime;
                Quaternion transStart = lastKey.Rotation;
                Quaternion transEnd = firstKey.Rotation;
                return Quaternion.Slerp(transStart, transEnd, factor);
            }

            if (timestamp > channel.RotationKeys[channel.RotationKeys.Count - 1].Time)
            {
                return channel.RotationKeys[channel.RotationKeys.Count - 1].Rotation;
            }

            for (int i = 0; i < channel.RotationKeys.Count - 1; i++)
            {
                if (timestamp >= channel.RotationKeys[i].Time && timestamp <= channel.RotationKeys[i + 1].Time)
                {
                    GeoAnimationKeyframe key2 = channel.RotationKeys[i + 1];

                    float deltaTime = key2.Time - channel.RotationKeys[i].Time;
                    float factor = (timestamp - channel.RotationKeys[i].Time) / deltaTime;
                    Quaternion rotationStart = channel.RotationKeys[i].Rotation;
                    Quaternion rotationEnd = key2.Rotation;
                    Quaternion rotation = Quaternion.Slerp(rotationStart, rotationEnd, factor);
                    return rotation;
                }
            }
            return new Quaternion(0, 0, 0, 1);
        }
    }
}

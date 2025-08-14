using KWEngine3.Framebuffers;
using KWEngine3.GameObjects;

namespace KWEngine3.Renderer
{
    internal interface IRenderer
    {
        public void Init();
        public void Bind();
        public void SetGlobals();
        public void Draw();
        public void Draw(Framebuffer fbSource);
        public void Draw(GameObject g, bool isVSG = false);
        public void RenderScene();
        public void RenderScene(List<GameObject> transparentObjects, List<GameObject> stencilObjects);
        public void RenderScene(List<RenderObject> transparentObjects);
        public void RenderSceneForLight(LightObject l);
        public void Draw(GameObject g);
        public void Draw(TerrainObject t);
        public void Draw(RenderObject r);
        public void Draw(ViewSpaceGameObject vsgo);
        public void UnbindUBO(int ubo);

    }
}

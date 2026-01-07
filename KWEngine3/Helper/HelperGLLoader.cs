using OpenTK.Graphics.OpenGL4;

namespace KWEngine3.Helper
{
    internal static class HelperGLLoader
    {

        internal static List<LoadPackage> LoadList = new List<LoadPackage>();

        internal static void AddCall(object receiver, Action a)
        {

            LoadPackage lp = new LoadPackage();
            lp.Receiver = receiver;
            lp.Action = a;
            lp.ReceiverType = receiver.GetType();
            lock (LoadList)
            {
                LoadList.Add(lp);
            }
        }

        internal static DepthFunction GetDepthFunction(int v)
        {
            switch (v)
            {
                case 0x0200:
                    return DepthFunction.Never;
                case 0x0201:
                    return DepthFunction.Less;
                case 0x0202:
                    return DepthFunction.Equal;
                case 0x0203:
                    return DepthFunction.Lequal;
                case 0x0204:
                    return DepthFunction.Greater;
                case 0x0205:
                    return DepthFunction.Notequal;
                case 0x0206:
                    return DepthFunction.Gequal;
                case 0x0207:
                    return DepthFunction.Always;
                default:
                    return DepthFunction.Less;
            }
        }

    }
}

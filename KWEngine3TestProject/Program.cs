using KWEngine3;

namespace KWEngine3TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            KWEngine.GBufferLighting = GBufferLightingMode.Default;

            using (GameWindow gw = new GameWindow())
            {
                gw.Run();
            }
        }
    }
}
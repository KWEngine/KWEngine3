﻿using KWEngine3;

namespace KWEngine3TestProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            KWEngine.GBufferLighting = GBufferLightingMode.Default;
            KWEngine.AudioBuffersPerChannel = 2;

            using (GameWindow gw = new GameWindow())
            {
                gw.Run();
            }
        }
    }
}
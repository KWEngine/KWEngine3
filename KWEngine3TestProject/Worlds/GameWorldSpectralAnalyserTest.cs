using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Audio;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3TestProject.Classes.WorldEQ;
using KWEngine3TestProject.Classes;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldSpectralAnalyserTest : World
    {
        private int _channelId = -1;

        private EQBlip[,] _eqBlips;
        private float _timestampLastEQUpdate = 0f;
        private float[] _eqDbs;
        private const float REDUCE = 0.4f;
        private const float OFFSET = 100.0f;

        public override void Act()
        {
            for (int i = 0; i < _eqDbs.Length; i++)
            {
                _eqDbs[i] = MathF.Max(_eqDbs[i] - REDUCE, 0f);
            }

            if(_channelId >= 0 && Audio.IsChannelPlaying(_channelId))
            {
                AudioAnalysis a = Audio.GetAudioAnalysisForChannel(_channelId);
                if(a.IsValid && a.TimestampWorld > _timestampLastEQUpdate)
                {
                    UpdateEQ(ref a);
                    _timestampLastEQUpdate = a.TimestampWorld;

                    for (int column = 0; column < _eqBlips.GetLength(0); column++)
                    {
                        // rows per column:
                        for (int row = 0; row < _eqBlips.GetLength(1); row++)
                        {
                            if (_eqDbs[column] > row)
                            {
                                if (row + 1 < _eqBlips.GetLength(1) && _eqDbs[column] < row + 1 && CheckAbove(column, row) == false)
                                {
                                    _eqBlips[column, row].SetAsTop(true);
                                }
                                else
                                {
                                    _eqBlips[column, row].SetAsTop(false);
                                }
                            }
                        }
                    }
                }
            }

            for (int column = 0; column < _eqBlips.GetLength(0); column++)
            {
                // rows per column:
                for (int row = 0; row < _eqBlips.GetLength(1); row++)
                {
                    if (_eqDbs[column] > row)
                    {
                        _eqBlips[column, row].Activate();
                    }
                    else
                    {
                        _eqBlips[column, row].Deactivate();
                    }
                }
            }
        }

        private bool CheckAbove(int column, int row)
        {
            for (int checkRow = row + 1; checkRow < _eqBlips.GetLength(1); checkRow++)
            {
                if (_eqBlips[column, checkRow].IsTop())
                {
                    return true;
                }
            }
            return false;
        }

        private void UpdateEQ(ref AudioAnalysis a)
        {
            if (_eqDbs[0]  < a.Band01.Decibel + OFFSET) _eqDbs[0]  = MathF.Max(0, a.Band01.Decibel + OFFSET);
            if (_eqDbs[1]  < a.Band02.Decibel + OFFSET) _eqDbs[1]  = MathF.Max(0, a.Band02.Decibel + OFFSET);
            if (_eqDbs[2]  < a.Band03.Decibel + OFFSET) _eqDbs[2]  = MathF.Max(0, a.Band03.Decibel + OFFSET);
            if (_eqDbs[3]  < a.Band04.Decibel + OFFSET) _eqDbs[3]  = MathF.Max(0, a.Band04.Decibel + OFFSET);
            if (_eqDbs[4]  < a.Band05.Decibel + OFFSET) _eqDbs[4]  = MathF.Max(0, a.Band05.Decibel + OFFSET);
            if (_eqDbs[5]  < a.Band06.Decibel + OFFSET) _eqDbs[5]  = MathF.Max(0, a.Band06.Decibel + OFFSET);
            if (_eqDbs[6]  < a.Band07.Decibel + OFFSET) _eqDbs[6]  = MathF.Max(0, a.Band07.Decibel + OFFSET);
            if (_eqDbs[7]  < a.Band08.Decibel + OFFSET) _eqDbs[7]  = MathF.Max(0, a.Band08.Decibel + OFFSET);
            if (_eqDbs[8]  < a.Band09.Decibel + OFFSET) _eqDbs[8]  = MathF.Max(0, a.Band09.Decibel + OFFSET);
            if (_eqDbs[9]  < a.Band10.Decibel + OFFSET) _eqDbs[9]  = MathF.Max(0, a.Band10.Decibel + OFFSET);
            if (_eqDbs[10] < a.Band11.Decibel + OFFSET) _eqDbs[10] = MathF.Max(0, a.Band11.Decibel + OFFSET);
            if (_eqDbs[11] < a.Band12.Decibel + OFFSET) _eqDbs[11] = MathF.Max(0, a.Band12.Decibel + OFFSET);
            if (_eqDbs[12] < a.Band13.Decibel + OFFSET) _eqDbs[12] = MathF.Max(0, a.Band13.Decibel + OFFSET);
            if (_eqDbs[13] < a.Band14.Decibel + OFFSET) _eqDbs[13] = MathF.Max(0, a.Band14.Decibel + OFFSET);
            if (_eqDbs[14] < a.Band15.Decibel + OFFSET) _eqDbs[14] = MathF.Max(0, a.Band15.Decibel + OFFSET);
            if (_eqDbs[15] < a.Band16.Decibel + OFFSET) _eqDbs[15] = MathF.Max(0, a.Band16.Decibel + OFFSET);
            if (_eqDbs[16] < a.Band17.Decibel + OFFSET) _eqDbs[16] = MathF.Max(0, a.Band17.Decibel + OFFSET);
            if (_eqDbs[17] < a.Band18.Decibel + OFFSET) _eqDbs[17] = MathF.Max(0, a.Band18.Decibel + OFFSET);
            if (_eqDbs[18] < a.Band19.Decibel + OFFSET) _eqDbs[18] = MathF.Max(0, a.Band19.Decibel + OFFSET);
            if (_eqDbs[19] < a.Band20.Decibel + OFFSET) _eqDbs[19] = MathF.Max(0, a.Band20.Decibel + OFFSET);

            
        }


        public override void Prepare()
        {
            KWEngine.LoadModel("Sony", "./Models/sony.obj");
            Audio.BufferSizeMs = 50;
            KWEngine.GlowRadius = 0.5f;
            KWEngine.GlowStyleFactor1 = 0.2f;
            KWEngine.GlowStyleFactor2 = 0.75f;

            SetCameraPosition(-3, 0, 7.5f);
            SetCameraTarget(-3, 0, 0);
            SetCameraFOV(10);
            SetColorAmbient(0.25f, 0.25f, 0.25f);
            SetCameraRenderDistance(50);
            
            MetalFront front = new MetalFront();
            front.SetModel("KWQuad");
            front.SetTexture("./Textures/EQ/Metal009_2K-PNG_Color2.png");
            front.SetTexture("./Textures/EQ/Metal009_2K-PNG_NormalGL.png", TextureType.Normal);
            front.SetTexture("./Textures/EQ/Metal009_2K-PNG_Metalness.png", TextureType.Metallic);
            front.SetTexture("./Textures/EQ/Metal009_2K-PNG_Roughness.png", TextureType.Roughness);
            front.SetScale(10, 10, 0.00001f);
            front.SetPosition(0, 0, -0.1f);
            front.SetTextureRepeat(6, 6);
            front.SetColor(0.5f, 0.5f, 0.5f);
            AddGameObject(front);
            /*
            Sony s = new Sony();
            s.SetModel("Sony");
            s.SetPosition(-3.95f, 0.55f, -0.09f);
            s.SetScale(0.15f, 0.1f, 0.5f);
            s.SetColor(0.19f, 0.19f, 0.19f);
            s.SetMetallic(0.5f);
            s.SetRoughness(0.6f);
            AddGameObject(s);
            */

            LightObject light = new LightObjectDirectional(ShadowQuality.NoShadow);
            light.SetPosition(5, 10, 10);
            light.SetTarget(-8, 0, 0);
            light.SetColor(1, 1, 1, 32);
            AddLightObject(light);

            GlassFront backG = new GlassFront();
            backG.SetModel("KWQuad");
            backG.SetTexture("./Textures/EQ/glass.png");
            backG.HasTransparencyTexture = true;
            backG.SetScale(2, 1, 0.000001f);
            backG.SetPosition(-3, 0, -0.05f);
            backG.SetMetallic(1.0f);
            backG.SetRoughness(0.25f);
            backG.SetColor(0.025f, 0.025f, 0.025f);
            AddGameObject(backG);

            GlassFront frontG = new GlassFront();
            frontG.SetModel("KWQuad");
            frontG.SetScale(2, 1, 0.000001f);
            frontG.SetPosition(-3, 0, 0.05f);
            frontG.SetMetallic(1.0f);
            frontG.SetRoughness(0.005f);
            frontG.SetOpacity(0.50f);
            frontG.SetTexture("./Textures/EQ/glass.png");
            frontG.HasTransparencyTexture = true;
            AddGameObject(frontG);

            GenerateBlips();

            _channelId = Audio.PlaySound("./SFX/maninthemirror.wav", true, 1.0f);
        }

        private void GenerateBlips()
        {
            _eqDbs = new float[20];

            _eqBlips = new EQBlip[20,60];
            const float XBASE = -3.775f;
            const float XMARGIN = 0.08f;
            const float YBASE = -0.35f;
            const float YMARGIN = 0.0125f;

            const float XSIZE = 0.125f * 0.6f;
            const float YSIZE = 0.05f * 0.25f;

            const float ZPOSROD = 0.055f;
            /*
            RenderObjectDefault rod1 = new RenderObjectDefault();
            rod1.SetAdditionalInstanceCount(599);
            rod1.SetModel("KWQuad");
            rod1.IsAffectedByLight = false;
            rod1.SetTexture("./Textures/EQ/dotmatrixpattern.png");
            rod1.SetOpacity(0.5f);
            rod1.SetTextureRepeat(XSIZE * 3, YSIZE * 5);
            rod1.SetScale(XSIZE, YSIZE, 0.000001f);
            RenderObjectDefault rod2 = new RenderObjectDefault();
            rod2.SetAdditionalInstanceCount(599);
            rod2.SetModel("KWQuad");
            rod2.IsAffectedByLight = false;
            rod2.SetTexture("./Textures/EQ/dotmatrixpattern.png");
            rod2.SetOpacity(0.5f);
            rod2.SetTextureRepeat(XSIZE * 3, YSIZE * 5);
            rod2.SetScale(XSIZE, YSIZE, 0.000001f);
            */

            int counter = 0;
            // columns:
            for (int column = 0; column < _eqBlips.GetLength(0); column++)
            {
                // rows per column:
                for (int row = 0; row < _eqBlips.GetLength(1); row++)
                {
                    
                    EQBlip e = new EQBlip();
                    e.SetModel("KWQuad");
                    e.SetScale(XSIZE, YSIZE, 0.000001f);
                    e.SetPosition(XBASE + column * XMARGIN, YBASE + row * YMARGIN, 0.0025f);
                    e.SetTexture("./Textures/EQ/eqblip2.png");
                    e.SetColor(0, 0, 0);
                    e.SetColorEmissive(0, 0.5f, 1.0f, 0.0f);
                    e.IsAffectedByLight = false;
                    e.HasTransparencyTexture = true;
                    AddGameObject(e);

                    RenderObjectDefault rod = new RenderObjectDefault();
                    rod.SetModel("KWQuad");
                    rod.IsAffectedByLight = false;
                    rod.SetTexture("./Textures/EQ/dotmatrixpattern2.png");
                    rod.SetOpacity(HelperRandom.GetRandomNumber(0.45f, 0.55f));
                    //rod.SetTextureRepeat(XSIZE * 3, YSIZE * 5);
                    //rod.SetTextureOffset(HelperRandom.GetRandomNumber(0f, 1f), HelperRandom.GetRandomNumber(0f, 1f));
                    rod.SetScale(XSIZE, YSIZE, 0.000001f);
                    rod.SetPosition(XBASE + column * XMARGIN, YBASE + row * YMARGIN, ZPOSROD);
                    AddRenderObject(rod);

                    /*
                    if (counter < 600)
                    {
                        if (counter == 0)
                            rod1.SetPosition(XBASE + column * XMARGIN, YBASE + row * YMARGIN, ZPOSROD);
                        else
                        {
                            rod1.SetPositionRotationScaleForInstance(
                                counter,
                                new Vector3(XBASE + column * XMARGIN, YBASE + row * YMARGIN, ZPOSROD), Quaternion.Identity, new Vector3(XSIZE, YSIZE, 0.000001f)
                                );
                        }
                    }
                    else
                    {
                        if (counter == 600)
                            rod2.SetPosition(XBASE + column * XMARGIN, YBASE + row * YMARGIN, ZPOSROD);
                        else
                            rod2.SetPositionRotationScaleForInstance(
                                counter - 600,
                                new Vector3(XBASE + column * XMARGIN, YBASE + row * YMARGIN, ZPOSROD), Quaternion.Identity, new Vector3(XSIZE, YSIZE, 0.000001f)
                                );
                    }
                    */
                    counter++;

                    _eqBlips[column, row] = e;
                }
            }

            //AddRenderObject(rod1);
            //AddRenderObject(rod2);
        }
    }
}

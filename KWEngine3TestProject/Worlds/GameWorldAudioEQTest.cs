using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.Audio;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldAudioEQTest : World
    {
        private int _channel = -1;
        private float _gain = 1;
        private float _timestampData = 0;
        private float _timestampShot = 0;
        private GameObject[] _bands;

        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.Space) && !Audio.IsChannelPlayingOrPaused(_channel))
            {
                Console.WriteLine("play...");
                _channel = Audio.PlaySound("./SFX/maninthemirror.wav", true, 1);
            }
            else if (Keyboard.IsKeyPressed(Keys.Space) && Audio.IsChannelPlaying(_channel))
            {
                Console.WriteLine("stop...");
                Audio.StopSound(_channel);
            }
            else if (Keyboard.IsKeyPressed(Keys.Space) && Audio.IsChannelPaused(_channel))
            {
                Console.WriteLine("continue...");
                Audio.ContinueSound(_channel);
            }
            
            if (Audio.IsChannelPlaying(_channel))
            {
                if(Keyboard.IsKeyDown(Keys.Up))
                {
                    _gain = MathHelper.Clamp(_gain + 0.01f, 0, 1);
                    Audio.ChangeSoundGain(_channel, _gain);
                    Console.WriteLine(_gain);
                }
                else if(Keyboard.IsKeyDown(Keys.Down))
                {
                    _gain = MathHelper.Clamp(_gain - 0.01f, 0, 1);
                    Audio.ChangeSoundGain(_channel, _gain);
                    Console.WriteLine(_gain);
                }

                if (Audio.IsNewAudioAnalysisDataAvailable(_channel, _timestampData))
                {
                    AudioAnalysis aa = Audio.GetAudioAnalysisForChannel(_channel);
                    if (aa.IsValid)
                    {
                        _timestampData = WorldTime;

                        _bands[0].SetPositionY((MathHelper.Clamp(aa.Band01.Decibel, -60, 0) + 60) / 2);
                        _bands[0].SetScale(1, MathHelper.Clamp(aa.Band01.Decibel, -60, 0) + 60, 1);

                        _bands[1].SetPositionY((MathHelper.Clamp(aa.Band02.Decibel, -60, 0) + 60) / 2);
                        _bands[1].SetScale(1, MathHelper.Clamp(aa.Band02.Decibel, -60, 0) + 60, 1);

                        _bands[2].SetPositionY((MathHelper.Clamp(aa.Band03.Decibel, -60, 0) + 60) / 2);
                        _bands[2].SetScale(1, MathHelper.Clamp(aa.Band03.Decibel, -60, 0) + 60, 1);

                        _bands[3].SetPositionY((MathHelper.Clamp(aa.Band04.Decibel, -60, 0) + 60) / 2);
                        _bands[3].SetScale(1, MathHelper.Clamp(aa.Band04.Decibel, -60, 0) + 60, 1);

                        _bands[4].SetPositionY((MathHelper.Clamp(aa.Band05.Decibel, -60, 0) + 60) / 2);
                        _bands[4].SetScale(1, MathHelper.Clamp(aa.Band05.Decibel, -60, 0) + 60, 1);

                        _bands[5].SetPositionY((MathHelper.Clamp(aa.Band06.Decibel, -60, 0) + 60) / 2);
                        _bands[5].SetScale(1, MathHelper.Clamp(aa.Band06.Decibel, -60, 0) + 60, 1);

                        _bands[6].SetPositionY((MathHelper.Clamp(aa.Band07.Decibel, -60, 0) + 60) / 2);
                        _bands[6].SetScale(1, MathHelper.Clamp(aa.Band07.Decibel, -60, 0) + 60, 1);

                        _bands[7].SetPositionY((MathHelper.Clamp(aa.Band08.Decibel, -60, 0) + 60) / 2);
                        _bands[7].SetScale(1, MathHelper.Clamp(aa.Band08.Decibel, -60, 0) + 60, 1);

                        _bands[8].SetPositionY((MathHelper.Clamp(aa.Band09.Decibel, -60, 0) + 60) / 2);
                        _bands[8].SetScale(1, MathHelper.Clamp(aa.Band09.Decibel, -60, 0) + 60, 1);

                        _bands[9].SetPositionY((MathHelper.Clamp(aa.Band10.Decibel, -60, 0) + 60) / 2);
                        _bands[9].SetScale(1, MathHelper.Clamp(aa.Band10.Decibel, -60, 0) + 60, 1);

                        _bands[10].SetPositionY((MathHelper.Clamp(aa.Band11.Decibel, -60, 0) + 60) / 2);
                        _bands[10].SetScale(1, MathHelper.Clamp(aa.Band11.Decibel, -60, 0) + 60, 1);

                        _bands[11].SetPositionY((MathHelper.Clamp(aa.Band12.Decibel, -60, 0) + 60) / 2);
                        _bands[11].SetScale(1, MathHelper.Clamp(aa.Band12.Decibel, -60, 0) + 60, 1);

                        _bands[12].SetPositionY((MathHelper.Clamp(aa.Band13.Decibel, -60, 0) + 60) / 2);
                        _bands[12].SetScale(1, MathHelper.Clamp(aa.Band13.Decibel, -60, 0) + 60, 1);

                        _bands[13].SetPositionY((MathHelper.Clamp(aa.Band14.Decibel, -60, 0) + 60) / 2);
                        _bands[13].SetScale(1, MathHelper.Clamp(aa.Band14.Decibel, -60, 0) + 60, 1);

                        _bands[14].SetPositionY((MathHelper.Clamp(aa.Band15.Decibel, -60, 0) + 60) / 2);
                        _bands[14].SetScale(1, MathHelper.Clamp(aa.Band15.Decibel, -60, 0) + 60, 1);

                        _bands[15].SetPositionY((MathHelper.Clamp(aa.Band16.Decibel, -60, 0) + 60) / 2);
                        _bands[15].SetScale(1, MathHelper.Clamp(aa.Band16.Decibel, -60, 0) + 60, 1);

                        _bands[16].SetPositionY((MathHelper.Clamp(aa.Band17.Decibel, -60, 0) + 60) / 2);
                        _bands[16].SetScale(1, MathHelper.Clamp(aa.Band17.Decibel, -60, 0) + 60, 1);

                        _bands[17].SetPositionY((MathHelper.Clamp(aa.Band18.Decibel, -60, 0) + 60) / 2);
                        _bands[17].SetScale(1, MathHelper.Clamp(aa.Band18.Decibel, -60, 0) + 60, 1);

                        _bands[18].SetPositionY((MathHelper.Clamp(aa.Band19.Decibel, -60, 0) + 60) / 2);
                        _bands[18].SetScale(1, MathHelper.Clamp(aa.Band19.Decibel, -60, 0) + 60, 1);

                        _bands[19].SetPositionY((MathHelper.Clamp(aa.Band20.Decibel, -60, 0) + 60) / 2);
                        _bands[19].SetScale(1, MathHelper.Clamp(aa.Band20.Decibel, -60, 0) + 60, 1);
                    }
                }
            }

            if(Keyboard.IsKeyPressed(Keys.A))
            {
                Audio.PlaySound("./SFX/target01_comeup.ogg", false, 1.0f);
            }
            if (Keyboard.IsKeyPressed(Keys.S))
            {
                Audio.PlaySound("./SFX/cymbal_crash.ogg", false, 1.0f);
            }
            if (Keyboard.IsKeyPressed(Keys.D))
            {
                Audio.PlaySound("./SFX/shot01.ogg", false, 1.0f);
            }

            if(Keyboard.IsKeyDown(Keys.T))
            {
                if (WorldTime > (_timestampShot + 0.08f))
                {
                    Audio.PlaySound("./SFX/tf4/shot07.ogg", false, 1.0f);
                    _timestampShot = WorldTime;
                }
            }
        }

        public override void Prepare()
        {
            Audio.AnalyserActive = true;
            Audio.BufferSizeMs = 50;

            SetColorAmbient(1, 1, 1);
            SetCameraPosition(0, 30, 80);
            SetCameraTarget(0, 30, 0);

            _bands = new GameObject[20];
            for (int i = 0; i < _bands.Length; i++)
            {
                _bands[i] = new Immovable();
                _bands[i].SetPosition(i - _bands.Length / 2, 0, 0);
                _bands[i].SetScale(1, 10, 1);
                AddGameObject(_bands[i]);
            }

            Immovable top = new Immovable();
            top.SetScale(20, 0.1f, 1);
            top.SetPosition(0, 60, 0);
            top.SetColor(1, 0, 0);
            AddGameObject(top);
        }
    }
}

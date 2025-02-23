using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class Sound
    {
        public static async Task Play(string fileName)
        {
            try
            {
                string filePath = LoadFile.Load("Sounds", fileName);
                await Task.Run(() =>
                {
                    using (var audioFile = new AudioFileReader(filePath))
                    using (var outputDevice = new WaveOutEvent())
                    {
                        audioFile.Volume = Math.Clamp(1.0f, 0.0f, 1.0f);
                        outputDevice.Init(audioFile);
                        outputDevice.Play();

                        Task.Run(async () =>
                        {
                            while (outputDevice.PlaybackState == PlaybackState.Playing)
                            {
                                await Task.Delay(10);
                            }
                        }).Wait();
                    }
                });
            }
            catch(Exception ex)
            {
                Logger.Error("Sound.Play", ex.ToString());
            }
        }
    }
}

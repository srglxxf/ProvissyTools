using Grabacr07.KanColleViewer.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using NAudio.Wave;
using Settings = Grabacr07.KanColleViewer.Models.Settings;
using StatusService = Grabacr07.KanColleViewer.Models.StatusService;
using Volume = Grabacr07.KanColleViewer.Models.Volume;

namespace ProvissyTools
{
    class CustomSound
    {
        private BlockAlignReductionStream BlockStream = null;
        private DirectSoundOut SoundOut = null;
        string Main_folder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

        public void SoundOutput(string header, bool IsWin8)
        {

            DisposeWave();

            try
            {

                var Audiofile = GetRandomSound(header);

                if (!IsWin8 && Audiofile == null)
                {
                    SystemSounds.Beep.Play();
                    return;
                }
                else if (IsWin8 && Audiofile == null)
                    return;

                float Volume = (float)99;

                if (Path.GetExtension(Audiofile).ToLower() == ".wav")
                {
                    WaveStream pcm = new WaveChannel32(new WaveFileReader(Audiofile), Volume, 0);
                    BlockStream = new BlockAlignReductionStream(pcm);
                }
                else if (Path.GetExtension(Audiofile).ToLower() == ".mp3")
                {
                    WaveStream pcm = new WaveChannel32(new Mp3FileReader(Audiofile), Volume, 0);
                    BlockStream = new BlockAlignReductionStream(pcm);
                }
                else
                    return;

                SoundOut = new DirectSoundOut();
                SoundOut.Init(BlockStream);
                SoundOut.Play();
            }
            catch (Exception ex)
            {
                StatusService.Current.Notify("播放音频失败！: " + ex.Message);
            }
        }


        public string GetRandomSound(string header)
        {
            try
            {
                if (!Directory.Exists("Sounds\\"))
                {
                    Directory.CreateDirectory("Sounds");
                }

                if (!Directory.Exists("Sounds\\" + header))
                {
                    Directory.CreateDirectory("Sounds\\" + header);
                    return null;
                }

                List<string> FileList = Directory.GetFiles("Sounds\\" + header, "*.wav", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles("Sounds\\" + header, "*.mp3", SearchOption.AllDirectories)).ToList();

                if (FileList.Count > 0)
                {
                    Random Rnd = new Random();
                    return FileList[Rnd.Next(0, FileList.Count)];
                }
            }
            catch (Exception ex)
            {
                StatusService.Current.Notify("找不到音频文件！: " + ex.Message);
            }

            return null;
        }

        public string FileCheck(string header)
        {
            string SelFolder = "";

            if (header == Resources.Updater_Notification_Title)
                return null;

            var checkV = Volume.GetInstance();

            if (header == Resources.Expedition_NotificationMessage_Title) SelFolder = "\\expedition";
            else if (header == Resources.Repairyard_NotificationMessage_Title) SelFolder = "\\repair";
            else if (header == Resources.ReSortie_NotificationMessage_Title) SelFolder = "\\resortie";
            else if (header == Resources.ReSortie_CriticalConditionMessage_Title) SelFolder = "\\Critical";
            else SelFolder = "";

            string MP3path = Main_folder + SelFolder + "\\notify.mp3";
            string Wavpath = Main_folder + SelFolder + "\\notify.wav";

            if (checkV.IsMute == false)
            {
                if (File.Exists(MP3path) == true)
                    return MP3path;
                else if (File.Exists(Wavpath) == true)
                    return Wavpath;
                else
                {
                    if (File.Exists(Main_folder + "\\notify.mp3") == true)
                        return Main_folder + "\\notify.mp3";
                    else if (File.Exists(Main_folder + "\\notify.wav") == true)
                        return Main_folder + "\\notify.wav";
                    else
                        return null;
                }
            }

            return null;
        }

        private void DisposeWave()
        {
            try
            {
                if (SoundOut != null)
                {
                    if (SoundOut.PlaybackState == PlaybackState.Playing)
                        SoundOut.Stop();
                    SoundOut.Dispose();
                    SoundOut = null;
                }
                if (BlockStream != null)
                {
                    BlockStream.Dispose();
                    BlockStream = null;
                }
            }
            catch { }
        }

    }
}
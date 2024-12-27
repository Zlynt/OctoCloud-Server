using System.Diagnostics;
using OctoCloud.Settings;

namespace OctoCloud.Server.Music
{
    public class AudioFingerprint
    {
        public int Duration;
        public string Fingerprint;

        public AudioFingerprint(int duration, string fingerprint)
        {
            this.Duration = duration;
            this.Fingerprint = fingerprint;
        }

        public string ToString(){
            return $"Duration={this.Duration}\nFingerprint={this.Fingerprint}";
        }
    }
    public class Fingerprint
    {

        private static string _ChromaprintVersion = "1.5.1";
        private static string _ChromaprintOS = SystemInfo.GetPlatform().ToString();
        private static string _ChromaprintArch = SystemInfo.getCPUArch().ToString();

        public string _chromaPath;

        public Fingerprint(){
            string resourcesFolder = Path.Combine(AppContext.BaseDirectory, "Resources");
            _chromaPath = Path.Combine(resourcesFolder, "Chromaprint", _ChromaprintVersion, _ChromaprintOS, _ChromaprintArch, "fpcalc");
        }

        public AudioFingerprint GetFingerprint(string filePath){
            ProcessStartInfo startInfo = new ProcessStartInfo{
                FileName                = _chromaPath,
                Arguments               = filePath,
                RedirectStandardOutput  = true,
                UseShellExecute         = false,
                CreateNoWindow          = true
            };
            using (Process process = Process.Start(startInfo))
            {
                if(process == null) throw new Exception("Cannot run fingerprint");
                
                using (StreamReader streamReader = process.StandardOutput)
                {
                    string[] result = streamReader.ReadToEnd().Split("\n");
                    AudioFingerprint audioFingerprint = new AudioFingerprint(
                        int.Parse(result[0].Split("=")[1]),
                        result[1].Split("=")[1]
                    );
                    
                    return audioFingerprint;
                }
            }
        }
    }
}

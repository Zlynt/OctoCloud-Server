using System.Runtime.InteropServices;

namespace OctoCloud.Settings
{
    public static class SystemInfo{

        public static OSPlatform GetPlatform() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
                return OSPlatform.Linux;
            }else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
                return OSPlatform.Windows;
            }else 
                throw new Exception("Cannot run on a incompatible platform");
        }

        public static PlatformID getOSID(){ return System.Environment.OSVersion.Platform; }
        public static OperatingSystem getOS() { return System.Environment.OSVersion; }

        public static Architecture getCPUArch(){ return System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture; }
    }
}
using System.IO;

namespace CryMediaAPI.Resources
{
    public static class Res 
    {
        public const string Directory = "Resources";
            
        public const string Video_Mp4 = "small.mp4";
        public const string Video_Webm = "small.webm";
        public const string Video_Flv = "small.flv";
        public const string Audio_Mp3 = "horse.mp3";
        public const string Audio_Ogg = "horse.ogg";

        public static string GetPath(string resourceName) => Path.Combine(Directory, resourceName);
    }
}

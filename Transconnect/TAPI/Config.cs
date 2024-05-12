using TransLib;

namespace TAPI
{
    internal static class Config
    {
        private static AppConfig? _cfg = null;
        
        public static AppConfig cfg { get {
            if (_cfg == null) throw new Exception("Config not loaded");
            return _cfg;
        } }

        public static void SetFromArgs(string[] args)
        {
            string path = args.Length > 0 ? args[0] : "data/credentials.json";
            _cfg = AppConfig.read_config(path);
        }
    }
}

using Terraria.ModLoader;

namespace WuDao.Content.Config
{
    public static class JuexueRuntime
    {
        public static bool Enabled { get; private set; } = true;

        public static void ApplyFromConfig(JuexueServerConfig cfg)
        {
            Enabled = cfg?.Enabled ?? true;
        }

        public static void TryRebuildFromConfig()
        {
            JuexueServerConfig cfg = null;
            try
            {
                cfg = ModContent.GetInstance<JuexueServerConfig>();
            }
            catch
            {
            }

            ApplyFromConfig(cfg);
        }

        public static void Clear()
        {
            Enabled = true;
        }
    }
}
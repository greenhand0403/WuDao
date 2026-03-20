using Terraria.ModLoader;
using WuDao.Content.Global;

namespace WuDao.Content.Systems
{
    public class TimeStopWorldSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            TimeStopSystem.Clear();
        }

        public override void OnWorldUnload()
        {
            TimeStopSystem.Clear();
        }

        public override void PostUpdateEverything()
        {
            TimeStopSystem.UpdateServerSide();
        }
    }
}
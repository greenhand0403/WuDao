using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Global.Projectiles;
using WuDao.Content.Players;

namespace WuDao.Content.Global.NPCs
{
    // 模仿者：记录最近是否被模仿者弹体命中过，并在击杀时给玩家结算进度
    public class MimickerGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        private int recentMimickerHitTimer;
        private int lastHitterPlayer = -1;

        public override void PostAI(NPC npc)
        {
            if (recentMimickerHitTimer > 0)
            {
                recentMimickerHitTimer--;
                if (recentMimickerHitTimer <= 0)
                    lastHitterPlayer = -1;
            }
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (projectile == null || !projectile.active)
                return;

            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            var gp = projectile.GetGlobalProjectile<MimickerGlobalProjectile>();
            if (!gp.fromMimicker)
                return;

            lastHitterPlayer = projectile.owner;
            recentMimickerHitTimer = 60;
        }

        public override void OnKill(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (recentMimickerHitTimer <= 0 || lastHitterPlayer < 0 || lastHitterPlayer >= Main.maxPlayers)
                return;

            Player player = Main.player[lastHitterPlayer];
            if (player == null || !player.active)
                return;

            player.GetModPlayer<MimickerPlayer>().RegisterKillFromServer(npc.type);

            recentMimickerHitTimer = 0;
            lastHitterPlayer = -1;
        }
    }
}
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Players;
using System;

namespace WuDao.Content.Global.Projectiles
{
    public class BeetleArrowGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            // 暴击属于真实战斗结果：多人里只让服务器判，避免客户端各自随机
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
                return;

            Player player = Main.player[projectile.owner];
            var modPlayer = player.GetModPlayer<BeetleArrowPlayer>();

            if (!modPlayer.beetleArrow)
                return;

            bool isSummon = projectile.CountsAsClass(DamageClass.Summon);
            bool isWhip = projectile.CountsAsClass(DamageClass.SummonMeleeSpeed);

            if (!isSummon && !isWhip)
                return;

            float critChance;

            if (isWhip)
                critChance = player.GetCritChance(DamageClass.SummonMeleeSpeed);
            else
                critChance = player.GetCritChance(DamageClass.Summon);

            critChance = Math.Max(0f, critChance);

            if (Main.rand.NextFloat(100f) < critChance)
            {
                modifiers.SetCrit();
            }
        }
    }
}
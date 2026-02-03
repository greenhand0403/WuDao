using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;
using WuDao.Content.Projectiles.Melee;

namespace WuDao.Content.Juexue.Active
{
    public class Kamehameha : JuexueItem
    {
        public override int QiCost => 1; // 蓄力期间每帧-1气
        public override int SpecialCooldownTicks => 60 * 120; // 2 分钟
        public const int KamehamehaFrameIndex = 4;
        // 龟派气功不在 TryActivate 中直接释放，由 QiPlayer.ProcessTriggers 处理按住/松开
        protected override bool OnActivate(Player player, QiPlayer qi)
        {
            return false;
        }

        public void ReleaseFire(Player player, QiPlayer qi, int spentQi)
        {
            if (spentQi <= 0) return; // 没有蓄到就不发射
            if (!qi.CanUseActiveNow(Item.type, SpecialCooldownTicks)) return;

            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float speed = 14f;

            int projType = ModContent.ProjectileType<KamehamehaProj>();
            int baseDmg = 60;
            float mult = 0.10f * spentQi + Helpers.BossProgressPower.GetUniqueBossCount();
            int damage = (int)(baseDmg * mult);

            int pid = Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                player.Center,
                dir * speed,
                projType,
                damage,
                3f,
                player.whoAmI,
                spentQi,
                0f
            );
            Main.projectile[pid].friendly = true;
            Main.projectile[pid].hostile = false;

            SoundEngine.PlaySound(SoundID.Item33, player.Center);

            // 结算专属冷却 + 公共 2s
            qi.StampActiveUse(Item.type, SpecialCooldownTicks);
            // if (!Main.dedServ)
            // {
            //     // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
            //     qi.TriggerJuexueGhost(KamehamehaFrameIndex, durationTick: 120, scale: 1.1f, offset: new Vector2(0, -20));
            // }
        }
    }
}

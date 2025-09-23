using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Common;

namespace WuDao.Content.Juexue.Active
{
    // TODO: 气功波贴图，绘制乌龟虚影
    public class Kamehameha : JuexueItem
    {
        public override int QiCost => 0; // 蓄力期间每帧-1气
        public override int SpecialCooldownTicks => 60 * 8; // 8 秒

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("绝学·龟派气功");
            // Tooltip.SetDefault("主动：按住以蓄力（每帧消耗1气，每点+10%伤害），松开向光标发出冲击波；8秒冷却。");
        }

        // 龟派气功不在 TryActivate 中直接释放，由 QiPlayer.ProcessTriggers 处理按住/松开
        protected override bool OnActivate(Player player, QiPlayer qi) => false;

        public void ReleaseFire(Player player, QiPlayer qi, int spentQi)
        {
            if (spentQi <= 0) return; // 没有蓄到就不发射
            if (!qi.CanUseActiveNow(Item.type, SpecialCooldownTicks)) return;

            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float speed = 24f;

            // 使用一条显眼的激光/冲击波占位：DeathLaser / LastPrismLaser等
            int projType = ProjectileID.DeathLaser;
            int baseDmg = 60;
            float mult = 1f + 0.10f * spentQi;
            int damage = (int)(baseDmg * mult);

            int pid = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, dir * speed, projType, damage, 3f, player.whoAmI);
            Main.projectile[pid].friendly = true;
            Main.projectile[pid].hostile = false;

            SoundEngine.PlaySound(SoundID.Item33, player.Center);

            // 结算专属冷却 + 公共 2s
            qi.StampActiveUse(Item.type, SpecialCooldownTicks);
        }
    }
}

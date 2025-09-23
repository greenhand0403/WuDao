using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using WuDao.Content.Projectiles.Magic;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Players
{
    public class WrathLotusPlayer : ModPlayer
    {
        public bool hasLotus;
        public int cooldowns = 0;
        public override void ResetEffects()
        {
            hasLotus = false;
        }
        public override void PostUpdate()
        {
            if (cooldowns > 0)
            {
                cooldowns--;
            }
        }
        // WrathLotusPlayer.cs
        public override void OnHurt(Player.HurtInfo info)
        {
            if (hasLotus && cooldowns == 0)
            {
                Player.immune = true;
                Player.immuneTime += 30;
                cooldowns += 120;

                if (Player.whoAmI == Main.myPlayer)
                {
                    // —— 基础参数 —— //
                    const int baseDamage = 30;     // 你的“基础伤害”
                    const float baseKnockback = 3f;

                    // —— 套用玩家魔法加成 —— //
                    int finalDamage = (int)Player.GetTotalDamage(DamageClass.Magic).ApplyTo(baseDamage);
                    float finalKB = Player.GetTotalKnockback(DamageClass.Magic).ApplyTo(baseKnockback);

                    // —— 在光标位置生成 —— //
                    int p1 = Projectile.NewProjectile(
                        Player.GetSource_Accessory(Player.HeldItem),
                        Main.MouseWorld,
                        Vector2.Zero,
                        ModContent.ProjectileType<WrathLotusProj>(),
                        finalDamage,
                        finalKB,
                        Player.whoAmI
                    );
                    // —— 在玩家位置生成 —— //
                    int p2 = Projectile.NewProjectile(
                        Player.GetSource_Accessory(Player.HeldItem),
                        Player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<WrathLotusProj>(),
                        finalDamage,
                        finalKB,
                        Player.whoAmI
                    );
                }
            }
        }

    }
}
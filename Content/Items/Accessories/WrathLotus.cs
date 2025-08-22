using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Common.Buffs;
using WuDao.Content.Projectiles.Magic;
using System.Collections.Generic;

namespace WuDao.Content.Items.Accessories
{
    // TODO ：贴图置换 佛怒火莲
    public class WrathLotus : BuffItem
    // public class WrathLotus : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.HellCake}";
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed; // 星星斗篷 一王前
            Item.value = Item.sellPrice(gold: 5);
            Item.defense = 2;
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            // 增加15%移动速度、免疫着火了、燃烧等减益
            rules.Add(new StatRule(BuffConditions.Always,
            // StatEffect.MoveSpeed(0.08f),
            // StatEffect.accRunSpeed(6.75f),
            // StatEffect.accRunSpeedAdd(3.75f),

            // StatEffect.RunAcceleration(0.75f),
            // StatEffect.AccRunSpeed(0.15f),
            // StatEffect.MaxRunSpeed(0.15f),
            // StatEffect.RunSlowdown(0.75f)

            // StatEffect.ControlJump(true),
            // StatEffect.JumpSpeedBoost(1.6f),
            // StatEffect.ExtraFall(10)
            // StatEffect.JumpBoost()

            // StatEffect.AttackSpeedAdd(0.15f),
            // StatEffect.MeleeCrit(-0.2f),
            // StatEffect.DamageAdd(0.1f)

            // StatEffect.KnockbackMulti(1f),//+100%击退
            // StatEffect.kbGlove(),//启用原版标志位 +100%击退
            // StatEffect.MeleeDamageAdd(0.12f),//+12%近战伤害
            // StatEffect.MeleeAttackSpeedAdd(0.12f),//+12%近战攻速
            // StatEffect.MeleeSizePercent(0.1f),//+10%近战武器尺寸
            // StatEffect.MeleeScaleGlove(),//启用原版标志位 +10%近战武器尺寸
            // StatEffect.AutoReuse()//启用原版标志位 自动重复攻击

            // StatEffect.ImmuneTo(BuffID.OnFire),
            // StatEffect.EnduranceAdd(0.1f)//+10%耐力

            // StatEffect.FireWalk(),
            // StatEffect.LavaMaxAdd(420)
            StatEffect.LavaImmune(),
            StatEffect.NoKnockback()
            ));
        }
    }

    public class WrathLotusPlayer : ModPlayer
    {
        public bool hasLotus;
        private const int baseDamage = 30;
        public override void ResetEffects()
        {
            hasLotus = false;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (hasLotus)
            {
                // 增加无敌帧 (比如额外 30 tick)
                Player.immune = true;
                Player.immuneTime += 30;

                if (Player.whoAmI == Main.myPlayer)
                {
                    // 在光标位置生成莲花射弹
                    Projectile.NewProjectile(
                        Player.GetSource_Accessory(Player.HeldItem),
                        Main.MouseWorld,
                        Vector2.Zero,
                        ModContent.ProjectileType<WrathLotusProj>(),
                        baseDamage,
                        3f,
                        Player.whoAmI
                    );

                    // 在玩家位置生成莲花射弹
                    Projectile.NewProjectile(
                        Player.GetSource_Accessory(Player.HeldItem),
                        Player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<WrathLotusProj>(),
                        baseDamage,
                        3f,
                        Player.whoAmI
                    );
                }
            }
        }
    }
}

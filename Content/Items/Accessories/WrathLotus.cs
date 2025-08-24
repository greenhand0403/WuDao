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
            // 免疫着火了、燃烧、岩浆
            rules.Add(new StatRule(BuffConditions.Always,
                StatEffect.ImmuneTo(BuffID.OnFire, BuffID.Burning),
                StatEffect.LavaImmune()
            ));
        }
    }

    public class WrathLotusPlayer : ModPlayer
    {
        public bool hasLotus;
        public int cooldowns = 0;
        private const int baseDamage = 30;
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
        public override void OnHurt(Player.HurtInfo info)
        {
            if (hasLotus && cooldowns == 0)
            {
                // 增加无敌帧 (比如额外 30 tick)
                Player.immune = true;
                Player.immuneTime += 30;
                cooldowns += 120;

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

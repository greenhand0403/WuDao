using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Common.Buffs;
using WuDao.Content.Projectiles.Magic;

namespace WuDao.Content.Items.Accessories
{
    // TODO ：贴图置换 佛怒火莲
    public class WrathLotus : BuffItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.HellCake}";
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("莲花护符");
        //     Tooltip.SetDefault("受击时生成莲花灵气\n额外提供短暂无敌时间");
        // }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed; // 星星斗篷 一王前
            Item.value = Item.sellPrice(gold: 5);
        }

        // public override void UpdateAccessory(Player player, bool hideVisual)
        // {
        //     // 这里可以以后拓展，例如：
        //     // player.lavaImmune = true;
        //     player.GetModPlayer<WrathLotusPlayer>().hasLotus = true;
        // }
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

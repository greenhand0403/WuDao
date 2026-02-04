using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles;

namespace WuDao.Content.Items.Pets
{
    // 丘比召唤物，召唤丘比作为宠物跟随玩家
    public class Kyubey : ModItem
    {
        public override string Texture => "WuDao/Content/Items/Pets/KyubeySoulStone";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ZephyrFish); // 复制微风鱼的物品属性
            Item.shoot = ModContent.ProjectileType<KyubeyPetProjectile>(); // “发射”宠物
            Item.buffType = ModContent.BuffType<KyubeyPetBuff>();          // 使用时添加的 Buff
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.AddBuff(Item.buffType, 3600); // 1 分钟；实际会被 Update 里维持住
            }
            return true;
        }
        // 如需合成，可在这里添加配方
        // public override void AddRecipes() { ... }
    }
}
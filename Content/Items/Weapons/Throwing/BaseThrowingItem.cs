
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace WuDao.Content.Items.Weapons.Throwing
{
    // 通用投掷物品基类 —— 方便以后复用
    public abstract class BaseThrowingItem : ModItem
    {
        // 子类必须指定默认要发射的射弹类型
        public abstract int BaseProjectileType { get; }
        // 子类可以覆盖要发送的 AI 模式（0 = 无重力无击退(针), 1 = 受重力有击退(石)）
        public virtual int ProjectileAIMode => 1;
        // public override string Texture => "Terraria/Images/MagicPixel";
        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 直接生成射弹并把 AI 模式写入 ai[0]
            var proj = Projectile.NewProjectileDirect(source, position, velocity, BaseProjectileType, damage, knockback, player.whoAmI);
            proj.ai[0] = ProjectileAIMode; // 0 或 1
            return false; // 我们已经手动生成了射弹
        }
    }
}
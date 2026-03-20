using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Buffs
{
    // 附魔皇家权杖
    public class EnchantedRoyalScepterBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff";
        private static Asset<Texture2D> RoyalScepterTexture;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false; // 允许右键移除
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.instance.LoadItem(ItemID.RoyalScepter);
            RoyalScepterTexture = TextureAssets.Item[ItemID.RoyalScepter];
        }
        public override void Unload()
        {
            RoyalScepterTexture = null;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            // 维持一个可见时长（即便不显示时间），避免自然掉光
            player.buffTime[buffIndex] = 18000;

            // 如果没有在场，就由本地玩家生成 1 把
            if (player.whoAmI == Main.myPlayer &&
                player.ownedProjectileCounts[ModContent.ProjectileType<RoyalScepterMinion>()] <= 0)
            {
                var pos = player.Center - new Vector2(0f, 50f);
                Projectile.NewProjectile(
                    player.GetSource_Buff(buffIndex),
                    pos, Vector2.Zero,
                    ModContent.ProjectileType<RoyalScepterMinion>(),
                    player.GetWeaponDamage(player.HeldItem),
                    player.GetWeaponKnockback(player.HeldItem),
                    player.whoAmI);
            }
        }

        // 可选：显式支持右键移除
        public override bool RightClick(int buffIndex) => true;
        public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams)
        {
            // 确保已加载纹理
            if (Main.dedServ || RoyalScepterTexture == null)
                return;

            Texture2D texture = RoyalScepterTexture.Value;

            Rectangle source = new Rectangle(0, 0, 14, 32);
            // 32-14 = 18, 18 / 2 = 9
            Vector2 origin = new Vector2(-9, 0);

            Main.EntitySpriteDraw(
                texture,
                drawParams.Position,
                source,
                drawParams.DrawColor, // 不要再用 Color.White
                0f,
                origin,
                1f,
                SpriteEffects.None,
                0
            );
        }
    }
}

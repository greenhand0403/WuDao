using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.ID;
using WuDao.Common; // 要用 ProjectileID.Sets

namespace WuDao.Content.Projectiles.Melee
{
    public class TenThousandSwordsProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // 不用本体贴图，PreDraw手画

        // 用 ai[0] 同步“选中的剑物品ID”，避免联机每端随机不一致
        private int chosenItemType
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetStaticDefaults()
        {
            // 放宽离屏裁剪，让刚出生在屏外的剑也能早一点被绘制
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.hide = false;
            Projectile.light = 0.5f;
            Projectile.MaxUpdates = 3;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 按原版流程：确保已加载 → 拿到帧矩形 → 用帧居中绘制
            Main.instance.LoadItem(chosenItemType); // 确保贴图已加载（原版也是这么干的）:contentReference[oaicite:2]{index=2}
            // 只在拥有者一侧决定随机结果，然后通过 ai[0] 同步给其他端
            if (Projectile.owner == Main.myPlayer)
            {
                chosenItemType = ItemSets.SwordItemSet.Get(SelectionMode.Random);
                Projectile.netUpdate = true;
            }
        }

        public override void AI()
        {
            // 若因为同步延迟 chosenItemType 还没到，就先等一等
            if (chosenItemType == 0)
                return;

            // 贴图朝向默认是右上45°，要抵消掉这45°，使“剑尖跟随速度方向”
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (chosenItemType == 0) return false;

            Texture2D tex = TextureAssets.Item[chosenItemType].Value;

            // 物品有效帧（比直接画整张更准）
            Rectangle frame = Item.GetDrawHitbox(chosenItemType, Main.LocalPlayer); // 会考虑动画帧/留白等
            Vector2 origin = frame.Size() / 2f;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(tex, drawPos, frame, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            return false; // 阻止默认绘制
        }
    }
}

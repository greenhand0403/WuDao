using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// 170x170 x4（竖排）刀光的“持有型近战”弹幕
    /// 贴图：SteelSlash.png（尺寸 170x680，默认朝右）
    /// </summary>
    public class SteelBroadSwordProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4; // 竖向4帧
        }

        // ai[0]：挥砍方向（1 = 顺时针 / -1 = 逆时针）
        // ai[1]：本次挥砍总时长（帧）
        public override void SetDefaults()
        {
            // 命中/碰撞用的矩形；不必等于贴图大小，但这里与帧同大更直观
            Projectile.width = 170;
            Projectile.height = 170;

            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 60;

            // 一次挥砍只打到同一目标一次的手感
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];

            // 本次挥砍时长
            Projectile.ai[1] = player.itemAnimationMax > 0 ? player.itemAnimationMax : 20;

            // 初始朝向：指向鼠标
            Vector2 aim = (Main.MouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX * player.direction);
            Projectile.rotation = aim.ToRotation();

            // 交替挥砍方向（也可以固定为 1 或 -1）
            Projectile.ai[0] = (player.itemAnimation % 2 == 0) ? 1f : -1f;

            SoundEngine.PlaySound(SoundID.Item1, player.Center);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            // 面向鼠标
            player.direction = (Main.MouseWorld.X >= player.Center.X) ? 1 : -1;
            player.heldProj = Projectile.whoAmI;

            if (player.itemAnimation <= 0 || player.CCed)
            {
                Projectile.Kill();
                return;
            }

            // 挥砍进度（0->1）
            float total = Projectile.ai[1] <= 0 ? 20f : Projectile.ai[1];
            float progress = 1f - (player.itemAnimation / total); // itemAnimation 从 Max 递减到 0

            // 扫弧约180°
            float swingRadians = MathHelper.Pi;
            float dir = Projectile.ai[0];

            float start = Projectile.rotation - dir * swingRadians * 0.5f;
            float current = start + dir * swingRadians * progress;

            // 刀尖到手的半径：170帧较大，这里设为 ~100，按手感再调
            float radius = 100f;

            Projectile.Center = player.MountedCenter + current.ToRotationVector2() * radius;
            Projectile.rotation = current + (player.direction == 1 ? 0f : MathHelper.Pi);
            Projectile.spriteDirection = player.direction;

            // 继承物品数值
            Projectile.damage = player.GetWeaponDamage(player.HeldItem);
            Projectile.knockBack = player.HeldItem.knockBack;
            Projectile.CritChance = player.GetWeaponCrit(player.HeldItem);

            // 维持攻击动画
            player.itemTime = 2;
            player.itemAnimation = (int)MathHelper.Clamp(player.itemAnimation, 1, (int)total);

            Projectile.velocity = Vector2.Zero;

            // 帧动画：按进度从 0→3
            int frames = Main.projFrames[Type];
            int frame = (int)(progress * frames);
            if (frame >= frames) frame = frames - 1;
            Projectile.frame = frame;
        }

        public override bool? CanHitNPC(NPC target) => null; // 使用默认近战碰撞 + ownerHitCheck

        public override void CutTiles()
        {
            // 砍草/罐子：线宽固定值，避免因为 width=170 导致过粗
            Terraria.Utils.PlotTileLine(
                Main.player[Projectile.owner].MountedCenter,
                Projectile.Center,
                24f,
                DelegateMethods.CutTiles
            );
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 命中线段（手->刀光），线宽固定值更可控
            Vector2 start = Main.player[Projectile.owner].MountedCenter;
            Vector2 end = Projectile.Center;
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(), targetHitbox.Size(),
                start, end, 24f, ref _
            );
        }

        // 自定义绘制：把“旋转原点”稍微向内侧（刀根）偏移，贴手一点
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            int frameHeight = tex.Height / Main.projFrames[Type];
            Rectangle src = new Rectangle(0, Projectile.frame * frameHeight, tex.Width, frameHeight);

            // 原点：X 取 40%（靠近刀根），Y 在中线
            Vector2 origin = new Vector2(tex.Width * 0.40f, frameHeight * 0.5f);

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                src,
                lightColor,
                Projectile.rotation,
                origin,
                1f,
                Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically,
                0
            );
            return false; // 已手动绘制
        }
    }
}

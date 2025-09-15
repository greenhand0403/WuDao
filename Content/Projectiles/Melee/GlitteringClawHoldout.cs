using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WuDao.Content.Projectiles.Melee
{
    public class GlitteringClawHoldout : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.FetidBaghnakhs}";
        // 可调参数
        private const float Reach = 26f;               // 手心到爪尖的中心距离（命中点前伸量）
        private const float ArcOffset = 0.08f;         // 轻微上扬角，正值略向上
        private const int Life = 11;                   // 弹幕寿命（与 useTime 接近或略大）
        private const float HitboxWidth = 26f;         // 贴脸矩形宽度（越大越宽容）
        private const float RecoilSpeed = 5.2f;        // 命中后玩家轻微后坐
        private const int LocalIFrames = 8;            // 对同一NPC的本地无敌帧
        private static Asset<Texture2D> TexAsset;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                TexAsset = ModContent.Request<Texture2D>("WuDao/Content/Items/Weapons/Melee/GlitteringClaw",AssetRequestMode.AsyncLoad);
            }
        }
        public override void Unload()
        {
            TexAsset = null;
        }
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Life;
            Projectile.tileCollide = false;            // 手上判定不与地形碰撞
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = LocalIFrames;

            Projectile.ownerHitCheck = true;           // 只在玩家能触及处命中（防穿墙
        }

        // public override void AI()
        // {
        //     Player player = Main.player[Projectile.owner];
        //     if (!player.active || player.dead)
        //     {
        //         Projectile.Kill();
        //         return;
        //     }

        //     // 把这个弹幕标记为玩家“手持中的投射物”以便tML摆胳膊等
        //     player.heldProj = Projectile.whoAmI;

        //     // 面向鼠标
        //     Vector2 aim = (Main.MouseWorld - player.MountedCenter);
        //     if (aim.LengthSquared() < 0.001f)
        //         aim = new Vector2(player.direction, 0f);
        //     float aimRot = aim.ToRotation();

        //     // 控制玩家前手复合关节（1.4+）
        //     // StretchAmount.Full 让前臂完全伸展，角度是相对X轴的弧度
        //     player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, aimRot + ArcOffset);

        //     // 计算“手心/握把”世界坐标
        //     Vector2 armOrigin = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, aimRot + ArcOffset);
        //     // GetFrontHandPosition 是 tML 在 1.4 分支提供的前手位置计算（需要 tModLoader 2022+）
        //     // 如果你的 tML 版本过旧没有该API，可改用 RotatedRelativePoint+经验偏移，见下方注释替代实现。

        //     // 将弹幕中心对齐到“爪尖附近”（手心向 aim 方向前探 Reach）
        //     Vector2 forward = aim.SafeNormalize(Vector2.UnitX);
        //     Projectile.Center = armOrigin + forward * Reach;

        //     // 朝向与贴图翻转
        //     Projectile.rotation = aimRot;
        //     Projectile.direction = player.direction = (Main.MouseWorld.X >= player.Center.X) ? 1 : -1;
        //     Projectile.spriteDirection = Projectile.direction;

        //     // 让玩家面朝攻击方向，期间禁止物品使用时间错乱
        //     player.itemRotation = MathHelper.WrapAngle(aimRot * Projectile.direction);
        //     player.itemTime = player.itemAnimation = 2; // 保持“出手中”姿态直到弹幕结束

        //     // 命中检测：在爪尖前方做一个细长AABB，贴脸/短距离
        //     // tML 的伤害结算会在 OnHitNPC 里调用，这里不用手写碰撞体；只需把 Projectile 的碰撞盒放到位。
        //     // 通过扩大 width/height 或者使用 ModifyDamageHitbox 精细化
        // }

        // public override void ModifyDamageHitbox(ref Rectangle hitbox)
        // {
        //     // 把命中盒改成沿着朝向的细长矩形（增加贴脸容错）
        //     Vector2 center = Projectile.Center;
        //     Vector2 dir = Projectile.rotation.ToRotationVector2();
        //     // 命中盒中心沿朝向再探一些
        //     center += dir * (HitboxWidth * 0.5f);

        //     // 构造一个宽 HitboxWidth，高 28 的盒子，并把它旋转贴到世界
        //     // 由于 ModifyDamageHitbox 只给 AABB，这里简单用横向拉长的AABB对齐即可（够用）
        //     int w = (int)HitboxWidth;
        //     int h = 28;
        //     hitbox = new Rectangle((int)(center.X - w / 2), (int)(center.Y - h / 2), w, h);
        // }

        // public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        // {
        //     // 轻微后坐：把玩家沿反方向弹回一点点，强调“打中手感”
        //     Player player = Main.player[Projectile.owner];
        //     Vector2 dir = Projectile.rotation.ToRotationVector2();
        //     player.velocity -= dir * RecoilSpeed * player.GetTotalArmorPenetration(DamageClass.Melee) * 0f;
        //     // 上面乘0f是给你一个开关：把0f改成1f启用后坐，或直接用常数 1f。
        //     // 也可以根据敌人是否为 Boss 来调整强度：
        //     // float scale = target.boss ? 0.5f : 1f; player.velocity -= dir * RecoilSpeed * scale;

        //     // 命中音效（可换成更尖锐的击打）
        //     SoundEngine.PlaySound(SoundID.DD2_SonicBoomBladeSlash, Projectile.Center);
        // }

        // public override bool PreDraw(ref Color lightColor)
        // {
        //     // 自绘：把爪套贴在手上
        //     Texture2D tex = TexAsset.Value;
        //     // 计算 origin：你的贴图请把“握把/手心”画在贴图的某个像素点，这里用(12, 24)举例
        //     Vector2 origin = new Vector2(12f, 24f);

        //     // 计算渲染位置/翻转
        //     SpriteEffects fx = (Projectile.spriteDirection == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        //     // 注意：当水平翻转时，origin.X 也需要对称，否则会偏位。
        //     Vector2 drawOrigin = origin;
        //     if (fx == SpriteEffects.FlipHorizontally)
        //     {
        //         drawOrigin.X = tex.Width - origin.X;
        //     }

        //     // 使用弹幕的 Center/rotation 作位姿（已在AI里对齐到手）
        //     Main.EntitySpriteDraw(
        //         tex,
        //         Projectile.Center - Main.screenPosition,
        //         null,
        //         Color.White,                  // 不做发光/染色，保持干净
        //         Projectile.rotation,
        //         drawOrigin,
        //         1f,
        //         fx,
        //         0
        //     );
        //     return false; // 我们已手动绘制
        // }

        // public override void OnKill(int timeLeft)
        // {
        //     // 结束时的收尾：这里不做尘效，保持清爽。如需手感可加一个轻微音/抖动。
        // }
    }
}

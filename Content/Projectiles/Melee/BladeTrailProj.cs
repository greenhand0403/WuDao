using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Rendering;   // 你的 BladeTrailRenderer
using WuDao.Common;
using Terraria.GameContent;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;   // 你的 ItemSets.BladeTrailSet

namespace WuDao.Content.Projectiles.Melee
{
    public class BladeTrailProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Star_4";
        Player player => Main.player[Projectile.owner];//获取玩家
        private static float SwordShaderExtraLen = 0.8f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 9;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 10;//弹幕 趋势 的时间
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.ownerHitCheck = true;     // 防止隔墙命中
        }
        public override bool PreAI()
        {
            // 预热拖尾 刀光三角带初始生成角度
            if (Projectile.localAI[0] == 0f)
            {
                for (int i = 0; i < Projectile.oldRot.Length; i++)
                    Projectile.oldRot[i] = player.itemRotation + MathHelper.PiOver4 * player.direction;
                Projectile.localAI[0] = 1f;
            }
            return true;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;

            Projectile.rotation = player.itemRotation + MathHelper.PiOver4 * player.direction;
            // 解决旧刀光带结束点与新刀光带初始点之间错误的生成刀光带
            // 角度跳变守卫：若当前角与最近历史差值过大，说明进入新一刀 —— 刷新 oldRot
            if (Projectile.oldRot != null && Projectile.oldRot.Length > 0)
            {
                float last = Projectile.oldRot[0];
                float diff = Math.Abs(MathHelper.WrapAngle(Projectile.rotation - last));
                if (diff > MathHelper.PiOver2)
                {
                    for (int i = 0; i < Projectile.oldRot.Length; i++)
                        Projectile.oldRot[i] = player.itemRotation + MathHelper.PiOver4 * player.direction;
                }
            }
            Projectile.velocity = new Vector2(0, -10).RotatedBy(Projectile.rotation);//给弹幕一个速度 仅仅用于击退方向
            player.heldProj = Projectile.whoAmI;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 调用你的 BladeTrailRenderer
            int len = ProjectileID.Sets.TrailCacheLength[Type];
            if (len < 2) return false;
            float reach = (player.itemWidth + player.itemHeight) * 0.5f;

            // —— 收藏武器 → 贴图与 UV —— //
            bool useWeaponTex = player.HeldItem.favorited;
            Texture2D tex;
            Func<int, Vector2> uvOuter, uvInner;

            if (useWeaponTex)
            {
                tex = TextureAssets.Item[player.HeldItem.type].Value;
                uvOuter = (i) => new Vector2(Math.Max(len - i, len * 0.5f) / (float)len,
                                             (0.5f + Math.Min(i, len * 0.5f)) / (float)len);
                uvInner = (i) => new Vector2((Math.Max(len - i, len * 0.5f) - 1.0f) / (float)len, 0);
            }
            else
            {
                tex = ScallionSwordProj.SwordTailTexAsset.Value;
                uvOuter = (i) => new Vector2(i / (float)(len - 1), 1f);
                uvInner = (i) => new Vector2(i / (float)(len - 1), 0f);
            }

            // —— 首个收藏染料 —— //
            int? dyeId = null;
            foreach (var inv in player.inventory)
            {
                if (inv != null && inv.favorited && inv.dye > 0)
                {
                    dyeId = inv.type;
                    break;
                }
            }

            BladeTrailParams p = new BladeTrailParams
            {
                WorldCenter = Projectile.Center,
                RotAt = (i) => Projectile.oldRot[i],
                TrailLen = len,
                // 半径太小会出现刀光带头尾瞬间绘制出错误三角带的情况
                OuterRadius = 1.68f * reach,
                InnerRadius = 0.42f * reach,
                // OuterRadius = 80f,
                // InnerRadius = 20f,
                HalfWidth = (i) =>
                {
                    float wf = 1f;
                    //  + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction * SwordShaderExtraLen
                    return wf;
                },
                ColorAt = (i) =>
                {
                    var c = Color.White;
                    c.A = (byte)(180 * (len - i) / (float)len);
                    return c;
                },

                UvOuter = uvOuter,
                UvInner = uvInner,
                Texture0 = tex,
                ArmorDyeShaderItemId = dyeId,
                Effect = null,
                Additive = true,
            };
            BladeTrailRenderer.Render(ref p, BladeTrailScratchBuffer.Verts);
            return false; // 自己绘制弹幕
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 用提取好的 BladeTrailCollision 工具检测
            int len = ProjectileID.Sets.TrailCacheLength[Type];
            float reach = (player.itemWidth + player.itemHeight) * 0.5f;

            return BladeTrailCollision.CheckCollision(
                Projectile.Center, Projectile.oldRot, len,
                1.9f * reach, 0.8f * reach, Main.player[Projectile.owner].direction, SwordShaderExtraLen,
                targetHitbox
            );
        }
    }
}
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Rendering;   // 你的 BladeTrailRenderer
using WuDao.Common;
using Terraria.GameContent;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.DataStructures;   // 你的 ItemSets.BladeTrailSet

namespace WuDao.Content.Projectiles.Melee
{
    public class BladeTrailProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Star_4";
        Player player => Main.player[Projectile.owner];//获取玩家
        private static float SwordShaderExtraLen = 0.8f;
        private HashSet<int> hitNPCs = new();
        public Color[] DiagColors;
        public Color[] RowWeightedColors;
        public override bool? CanHitNPC(NPC target)
        {
            return hitNPCs.Contains(target.whoAmI) ? false : null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitNPCs.Add(target.whoAmI);
        }
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

            Projectile.timeLeft = 2;//弹幕 趋势 的时间
            // Projectile.usesLocalNPCImmunity = true;
            // Projectile.localNPCHitCooldown = Projectile.timeLeft;
            Projectile.ownerHitCheck = true;     // 防止隔墙命中

            Projectile.usesLocalNPCImmunity = false;
            Projectile.usesIDStaticNPCImmunity = true;
            // 这个冷却要覆盖“同一把刀的两次生成间隔”
            // 建议 ≥ item.useAnimation 或根据你的刀光 timeLeft/生成频率来定
            Projectile.idStaticNPCHitCooldown = 16; // 例：16帧，可按手感调
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
            if (Projectile.ai[0] > 0)
                Projectile.timeLeft = (int)Projectile.ai[0];

            Texture2D texForColor = TextureAssets.Item[player.HeldItem.type].Value;
            int len = ProjectileID.Sets.TrailCacheLength[Type];

            // 取对角线色带（可配置方向、剔除规则、带宽）：
            // diagColors = TrailColorSampler.SampleDiagonalColors(
            //     texForColor,
            //     samples: len,// 9段
            //     dir: DiagDir.RightUp_to_LeftDown,          // ← 你可以切换成 LeftDown_to_RightUp
            //     exclude: ExcludeMode.ExcludeBlackOrTransparent, // ← 4 种模式任选
            //     bandWidth: 2                                // 抗锯齿带宽，1~3 都可
            // );

            // 取行加权色带（可配置方向、剔除规则、带宽）：
            RowWeightedColors = TrailColorSampler.SampleDiagonalRowWeightedColors(
                texForColor,
                samples: len,
                dir: DiagDir.RightUp_to_LeftDown, // 可切换方向
                exclude: ExcludeMode.ExcludeBlackOrTransparent, // 可选过滤
                rowRadius: 1,       // 行邻域（0~2）
                sigma: 6f,          // 横向带宽
                profile: TrailColorSampler.WeightProfile.Gaussian
            );

            // 可选：把接近黑的颜色做透明（再保险一次）
            // for (int i = 0; i < diagColors.Length; i++)
            // {
            //     var c = diagColors[i];
            //     if (c.R < 6 && c.G < 6 && c.B < 6) c.A = 0;
            //     diagColors[i] = c;
            // }
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

            player.GetModPlayer<BladeTrailPlayer>().TrailActive = true;

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
                // 直接从物品贴图中取对角线的顶点颜色 从 外圈(1,0.5+1/len) 内圈(1-1/len,0) 到 外圈(0.5,0.5+1/len) 内圈(0.5-1/len,0)
                tex = TextureAssets.Item[player.HeldItem.type].Value;
                // uvOuter = (i) => new Vector2(Math.Max(len - i, len * 0.5f) / (float)len,
                                            //  (0.5f + Math.Min(i, len * 0.5f)) / (float)len);
                // uvInner = (i) => new Vector2((Math.Max(len - i, len * 0.5f) - 1.0f) / (float)len, 0);

                uvOuter = (i) => new Vector2(Math.Max(len - i, len * 0.4f) / (float)len,
                                             (0.5f + Math.Min(i, len * 0.6f)) / (float)len);
                uvInner = (i) => new Vector2((Math.Max(len - i, len * 0.4f) - 1.0f) / (float)len, 0);
            }
            else
            {
                // 经典三角形刀光 UV 从 外圈(0,1) 内圈(0,0) 到 外圈(1,1) 内圈(1,0)
                tex = ScallionSwordProj.SwordTailTexAsset.Value;
                uvOuter = (i) => new Vector2(i / (float)(len - 1), 1f);
                uvInner = (i) => new Vector2(i / (float)(len - 1), 0f);
                // xy轴调换一下，UV方向测试
                // uvOuter = (i) => new Vector2(0f, 1 - i / (float)(len - 1));
                // uvInner = (i) => new Vector2(1f, 1 - i / (float)(len - 1));
                // 测试方向
                // uvOuter = (i) => new Vector2(1f, i / (float)(len - 1));
                // uvInner = (i) => new Vector2(0f, i / (float)(len - 1));
                // 再测试
                // uvOuter = (i) => new Vector2(0f, 1 - i / (float)(len - 1));
                // uvInner = (i) => new Vector2(1 - i / (float)(len - 1), 1 - i / (float)(len - 1));
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
            // 使用武器贴图的对角线颜色
            // 绑定到参数的 ColorAt（i 从尾到头/头到尾你都可选）
            Func<int, Color> colorAt = (i) =>
            {
                int idx = (int)MathHelper.Clamp(i, 0, DiagColors.Length - 1);
                return DiagColors[idx];
            };

            BladeTrailParams p = new BladeTrailParams
            {
                WorldCenter = Projectile.Center,
                RotAt = (i) => Projectile.oldRot[i],
                TrailLen = len,
                // 半径太小会出现刀光带头尾瞬间绘制出错误三角带的情况
                OuterRadius = 1.64f * reach,
                InnerRadius = 0.41f * reach,
                // OuterRadius = 80f,
                // InnerRadius = 20f,
                HalfWidth = (i) =>
                {
                    float wf = 1f;
                    //  + (float)Math.Cos(Projectile.oldRot[i] - MathHelper.PiOver2) * player.direction * SwordShaderExtraLen
                    return wf;
                },
                // 对角线采样
                // ColorAt = colorAt,
                // 行加权采样
                ColorAt = i => RowWeightedColors[Math.Clamp(i, 0, RowWeightedColors.Length - 1)],
                // ColorAt = (i) =>
                // {
                //     var c = Color.DarkGray;
                //     c.A = (byte)(200 * (len - i) / (float)len);
                //     return c;
                // },

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
                1.6f * reach, 0.8f * reach, Main.player[Projectile.owner].direction, SwordShaderExtraLen,
                targetHitbox
            );
        }
    }
    public class BladeTrailPlayer : ModPlayer
    {
        public bool TrailActive;

        public override void ResetEffects() => TrailActive = false;
    }

}
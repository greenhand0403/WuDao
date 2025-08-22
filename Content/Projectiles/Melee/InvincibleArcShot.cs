// Content/Projectiles/InvincibleArcShot.cs
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace WuDao.Content.Projectiles.Melee
{
    /// <summary>
    /// “无敌”剑形射弹：
    /// - 贴图来源：原版剑 Item 贴图（循环）
    /// - 剑尖朝飞行方向（支持每把剑单独偏移角修正）
    /// - 从鼠标周围淡入，可穿墙、可设定穿透，飞行一段时间后淡出消失
    /// - 命中造成固定伤害（敌MaxHP*1% + 我方MaxHP*10%），不吃暴击
    /// </summary>
    public class InvincibleArcShot : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FinalFractal}";
        // ========== 你可以自由编辑 ==========
        // 1) 循环贴图用的原版剑 Item 列表
        public static int[] SwordItemIDs = new int[]
        {
            ItemID.CopperBroadsword,
            ItemID.LightsBane,
            ItemID.Muramasa,
            ItemID.Terragrim,
            ItemID.BloodButcherer,
            ItemID.Starfury,
            ItemID.EnchantedSword,
            ItemID.BeeKeeper,
            ItemID.BladeofGrass,
            ItemID.FieryGreatsword,
            ItemID.NightsEdge,
            ItemID.TrueNightsEdge,
            ItemID.TrueExcalibur,
            ItemID.Excalibur,
            ItemID.Seedler,
            ItemID.TerraBlade,
            ItemID.TheHorsemansBlade,
            ItemID.StarWrath,
            ItemID.Meowmere,
            ItemID.InfluxWaver,
            ItemID.Zenith
        };

        // 2) 剑尖对齐角修正（度）。键：ItemID；值：让“贴图”绕中心旋转多少度后，剑尖指向“贴图的朝前方向”。
        //    正值=顺时针。你只要校一次即可（在地图里看看哪把不准，微调到对准）。
        public static Dictionary<int, float> SwordTipRotationOffsetDeg = new()
        {
            // 示例（以下角度仅示意，请按你包里的实际视觉调整）：
            [ItemID.CopperBroadsword] = 45,
            [ItemID.LightsBane] = 45,
            [ItemID.Muramasa] = 45,
            [ItemID.Terragrim] = 45,
            // ...
        };

        // 若某把剑不在上表里，使用这个默认偏移（先给个常用的 45° 起手，觉得不准再改）
        public const float DefaultTipOffsetDeg = 45f;

        // 飞行/视觉参数
        private const int LifeTime = 34;       // 总寿命（帧）
        private const int FadeInFrames = 6;    // 淡入
        private const int FadeOutFrames = 6;   // 淡出
        private const float Speed = 6f;       // 初速度（像素/帧）
        private const int LocalHitCooldown = 4;

        // AI 用途：
        // ai[0] = age（已存活帧数）
        // ai[1] = 贴图循环的随机起点

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;        // 无限穿透（如需要首撞即散，改为 1）
            Projectile.timeLeft = LifeTime;
            Projectile.tileCollide = false;   // 可穿墙
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = LocalHitCooldown;
            Projectile.hide = false;
            Projectile.light = 0.3f;
            Projectile.MaxUpdates = 3;
        }
        // TODO: 必有1个本体剑刺向光标位置
        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            Vector2 mouse = Main.MouseWorld;
            // 你已有的：在鼠标附近生成
            float radius = 52;
            float theta = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            Vector2 ran = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
            Vector2 spawn = mouse + Main.rand.NextFloat(0, radius) * ran;
            // ★ 改这里：初速度 = 从“生成点→鼠标位置”的方向
            // Player owner = Main.player[Projectile.owner];
            Vector2 dir = (mouse - spawn).SafeNormalize(Vector2.UnitX);
            // 轻微随机扰动
            dir = dir.RotateRandom(0.1f);
            // 一半冲向玩家，一半远离玩家
            // if (Main.rand.NextBool(2))
            // {
            //     spawn -= 2 * ran;
            //     dir = Vector2.Negate(dir);
            // }
            Projectile.velocity = dir * Speed;

            Projectile.Center = spawn - 0.5f * radius * Projectile.velocity;
            // 这颗射弹整生存期固定使用同一把剑
            Projectile.ai[1] = Main.rand.Next(SwordItemIDs.Length);
            Projectile.ai[2] = (Main.rand.NextBool(2) ? 0.4f : -0.4f) * (MathHelper.PiOver4 + Main.rand.NextFloat(MathHelper.PiOver4));
        }


        public override void AI()
        {
            // 年龄累加
            Projectile.ai[0]++;

            // 速度扰动
            Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[2] / LifeTime);
            Projectile.velocity *= 1.05f;
            // 贴图方向转动相同角度，对齐速度方向
            // Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // 固定伤害：敌怪生命上限 1% + 玩家生命上限 10%，不吃暴击
            Player owner = Main.player[Projectile.owner];
            int fixedDamage = (int)(target.lifeMax * 0.01f + owner.statLifeMax2 * 0.10f);

            modifiers.SourceDamage *= 0f;
            modifiers.FlatBonusDamage += fixedDamage;
            modifiers.DisableCrit();
            // 若希望完全无视防御，再加：modifiers.ArmorPenetration += 999999;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (SwordItemIDs == null || SwordItemIDs.Length == 0) return false;

            // 计算当前应显示的剑贴图
            int idx = (int)Projectile.ai[1] % SwordItemIDs.Length;
            int itemId = SwordItemIDs[idx];

            // ★ 确保贴图已加载（关键！否则很多原版道具没用到就不会预加载）
            if (!TextureAssets.Item[itemId].IsLoaded)
                Main.instance.LoadItem(itemId);
            Texture2D tex = TextureAssets.Item[itemId].Value;
            if (tex == null) return false;

            int age = (int)Projectile.ai[0];

            // 透明度（淡入/淡出）
            float alpha;
            if (age <= FadeInFrames)
                alpha = MathHelper.Clamp(age / (float)FadeInFrames, 0f, 1f);
            else if (age >= LifeTime - FadeOutFrames)
                alpha = MathHelper.Clamp((LifeTime - age) / (float)FadeOutFrames, 0f, 1f);
            else
                alpha = 1f;

            // 方向：速度方向 + 剑尖修正角
            float baseRot = Projectile.velocity.ToRotation();
            float tipOffsetDeg = DefaultTipOffsetDeg;
            if (SwordTipRotationOffsetDeg.TryGetValue(itemId, out float off)) tipOffsetDeg = off;
            float rotation = baseRot + MathHelper.ToRadians(tipOffsetDeg);

            // 推荐改成 tex.Size()，更稳（有些环境下 Bounds.Size() 不可用）
            Vector2 origin = tex.Size() * 0.5f;
            float scale = 1f;

            // ★ 应用淡入/淡出透明度
            Color c = Color.White * alpha;
            // 不要射弹拖尾
            // if (Projectile.timeLeft % 2 == 0)
            {
                Main.EntitySpriteDraw(
                    tex,
                    Projectile.Center - Main.screenPosition,
                    null,
                    c,
                    rotation,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0
                );
            }
            return false;

        }

        // private int FindClosestNPC(float maxDetect, out Vector2 pos)
        // {
        //     pos = Vector2.Zero;
        //     int best = -1;
        //     float min = maxDetect;

        //     for (int i = 0; i < Main.maxNPCs; i++)
        //     {
        //         NPC n = Main.npc[i];
        //         if (!n.active || n.friendly || n.life <= 0) continue;
        //         float d = Vector2.Distance(n.Center, Projectile.Center);
        //         if (d < min)
        //         {
        //             min = d;
        //             best = i;
        //             pos = n.Center;
        //         }
        //     }
        //     return best;
        // }
    }
}

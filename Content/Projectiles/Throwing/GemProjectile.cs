using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using WuDao.Common;
using Terraria.DataStructures;

namespace WuDao.Content.Projectiles.Throwing
{
    // 宝石射弹：继承基类，使用不同的贴图并在击中时加一些光效
    public class GemProjectile : BaseThrowingProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Diamond}";
        private int gemID;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 420;
            Projectile.MaxUpdates = 2;
            Projectile.light = 0.5f;
        }
        public override void OnSpawn(IEntitySource source)
        {
            // 随机选宝石，并加载对应贴图
            gemID = ItemSets.GemSet.Get(SelectionMode.Random);
            Main.instance.LoadItem(gemID);

            // —— 出生时一次性的被动调整（不依赖命中）—— //
            switch (gemID)
            {
                case ItemID.Emerald:
                    // 翡翠：额外穿透 1 个敌人
                    Projectile.penetrate = (Projectile.penetrate <= 0) ? 1 : Projectile.penetrate + 1;
                    break;

                case ItemID.Amber:
                    // 琥珀：体积（命中盒+绘制）增大 20%
                    Projectile.scale *= 1.2f;
                    Projectile.width = (int)(Projectile.width * 1.2f);
                    Projectile.height = (int)(Projectile.height * 1.2f);
                    break;

                case ItemID.Amethyst:
                    // 紫晶：初速度 +20%
                    Projectile.velocity *= 1.2f;
                    break;
            }
        }
        public override void ImpactEffects(Vector2 position, Vector2 velocity)
        {
            // 根据宝石类型挑选更贴合的尘粒，而不是完全随机
            int dustType = DustID.GemDiamond; // 兜底
            switch (gemID)
            {
                case ItemID.Diamond: dustType = DustID.GemDiamond; break;
                case ItemID.Topaz: dustType = DustID.GemTopaz; break;
                case ItemID.Ruby: dustType = DustID.GemRuby; break;
                case ItemID.Emerald: dustType = DustID.GemEmerald; break;
                case ItemID.Sapphire: dustType = DustID.GemSapphire; break;
                case ItemID.Amber: dustType = DustID.AmberBolt; break; // 琥珀可用更暖色的特效
                case ItemID.Amethyst: dustType = DustID.GemAmethyst; break;
            }

            for (int i = 0; i < 12; i++)
            {
                int d = Dust.NewDust(position, Projectile.width, Projectile.height, dustType);
                Main.dust[d].velocity *= 0.6f;
                Main.dust[d].noGravity = true;
            }
        }
        // 钻石增伤10%，黄玉击退1.5倍，红玉治疗2生命，翡翠额外穿透1个敌人（已在 OnSpawn 处理），
        // 蓝玉消耗5法力附加10点最终伤害，琥珀体积增大20%（已在 OnSpawn 处理），紫晶射弹速度增加20%（已在 OnSpawn 处理）
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            base.ModifyHitNPC(target, ref modifiers);
            Player owner = Main.player[Projectile.owner];

            switch (gemID)
            {
                case ItemID.Diamond:
                    // 钻石：增伤 10%（加在源伤害上，受后续计算影响，更自然）
                    modifiers.SourceDamage *= 1.10f;
                    break;

                case ItemID.Topaz:
                    // 黄玉：击退 1.5 倍
                    modifiers.Knockback *= 1.5f;
                    break;

                case ItemID.Ruby:
                    // 红玉：命中时治疗 2 生命（仅本地拥有者触发）
                    if (Main.myPlayer == owner.whoAmI && owner.statLife < owner.statLifeMax2)
                    {
                        int heal = 2;
                        owner.statLife = Math.Min(owner.statLife + heal, owner.statLifeMax2);
                        owner.HealEffect(heal, true);
                    }
                    break;

                case ItemID.Sapphire:
                    // 蓝玉：若法力 ≥5，则消耗 5 法力，给予 +10 点最终伤害
                    if (owner.statMana >= 5)
                    {
                        if (Main.myPlayer == owner.whoAmI)
                        {
                            owner.statMana -= 5;
                            owner.ManaEffect(-5); // 漂浮文字反馈
                        }
                        // 最终伤害加成：Flat 表示在最终值上再加固定值（贴合“最终伤害+10”的直觉）
                        modifiers.FinalDamage.Flat += 10f;
                    }
                    break;
            }
        }

        // 若你想让绘制也更“贴合宝石”，可保留你原来的 PreDraw：
        // （已经根据 gemID 切换了贴图）
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Item[gemID].Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Main.spriteBatch.Draw(tex, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
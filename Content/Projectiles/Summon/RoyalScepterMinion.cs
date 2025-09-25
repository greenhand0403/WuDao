using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using WuDao.Content.Buffs;

namespace WuDao.Content.Projectiles.Summon
{
    public class RoyalScepterMinion : ModProjectile
    {
        // ===== 可调参数 =====
        const float HoverYOffset = 50f;               // 在玩家头顶上方高度
        const float TargetRange = 600f;               // 索敌范围
        const int FireCD = 36;                        // 射击间隔（帧）
        const float DamagePerEmptySlot = 0.10f;       // 每个空闲召唤栏位 +10% 伤害
        public override string Texture => $"Terraria/Images/Item_{ItemID.RoyalScepter}";
        int fireTimer;
        private int _baseDamageFromItem;
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("附魔皇家权杖（随从）");
            Main.projPet[Projectile.type] = true; // 随从（非光宠）
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 18000;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.minionSlots = 1f; // 占用 1 个随从栏位
        }

        public override bool? CanCutTiles() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                player.ClearBuff(ModContent.BuffType<EnchantedRoyalScepterBuff>());
                return;
            }
            if (player.HasBuff(ModContent.BuffType<EnchantedRoyalScepterBuff>()))
                Projectile.timeLeft = 2;

            // ★ 完全静止锁定到玩家头顶，不用速度/惯性，彻底消抖
            Vector2 desired = player.MountedCenter + new Vector2(0f, -HoverYOffset);
            Projectile.velocity = Vector2.Zero;
            Projectile.Center = desired;

            // 面向
            Projectile.direction = player.direction;
            Projectile.spriteDirection = Projectile.direction;

            // 自动索敌并开火
            fireTimer++;
            if (fireTimer >= FireCD && Main.myPlayer == Projectile.owner)
            {
                int target = FindTarget(player.Center, TargetRange);
                if (target != -1)
                {
                    fireTimer = 0;
                    Vector2 dir = (Main.npc[target].Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                    
                    var src = Projectile.GetSource_FromThis();
                    float speedShoot = 1f; // 只要不是0，OnSpawn就不会强制改成竖直

                    // 1) 空闲随从栏位加成（例：每个空位 +10%）
                    float usedSlots = player.slotsMinions;
                    float maxSlots = Math.Max(1f, player.maxMinions);
                    float empty = Math.Max(0f, maxSlots - usedSlots);

                    // 2) 以“生成时记录的基准伤害”为底，套空位加成
                    int baseWithEmpty = (int)Math.Round(_baseDamageFromItem * (1f + empty * DamagePerEmptySlot));

                    // 3) 再套上玩家当前的召唤系数（饰品/药水/配点等）
                    int shotDmg = (int)player.GetTotalDamage(DamageClass.Summon).ApplyTo(baseWithEmpty);

                    // 发射光束时，damage 用 shotDmg（不要用 Projectile.damage）
                    Projectile.NewProjectile(src, Projectile.Center, dir * speedShoot,
                        ModContent.ProjectileType<RoyalShadowBeam>(),
                        shotDmg, 0f, Projectile.owner,
                        ai0: Projectile.whoAmI, ai1: 0f);

                    SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.8f }, Projectile.Center);
                }
            }
        }

        int GetBaseDamage()
        {
            // originalDamage 在生成时 = Item.damage；这里为了“空栏位增伤”更稳定，取当前持有武器也可
            return Math.Max(1, Projectile.originalDamage);
        }

        int FindTarget(Vector2 from, float range)
        {
            int target = -1;
            float best = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n.CanBeChasedBy(this))
                {
                    float d = Vector2.Distance(from, n.Center);
                    if (d <= best)
                    {
                        // 不需要视线：穿墙射线
                        best = d;
                        target = i;
                    }
                }
            }
            return target;
        }
        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            // 生成这一刻记录基准伤害（来自召唤杖+玩家当时的加成）
            _baseDamageFromItem = Math.Max(1, Projectile.originalDamage);
        }
        // 使用“原版物品贴图”渲染随从（放大/抖动一点点让它更像飘着的法杖）
        public override bool PreDraw(ref Color lightColor)
        {
            // 如果你不想动态取原版贴图，可以直接用本 Mod 的默认贴图：return true;
            Texture2D tex = TextureAssets.Item[ItemID.RoyalScepter].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(tex, drawPos, null, lightColor, 0f, origin, 0.9f, SpriteEffects.None, 0);
            return false; // 我们自己画，不用默认贴图
        }
    }
}

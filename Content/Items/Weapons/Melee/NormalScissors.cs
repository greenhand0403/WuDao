using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using WuDao.Content.Buffs;

// TODO: 重绘贴图 成长型武器，剪刀，改成了发射斩波 辅助类未拆分到目录
// ModPlayer ModItem ModProjectile 写在一个文件了
namespace WuDao.Content.Items.Weapons.Melee
{
    // —— 玩家状态：冲刺CD + 蓄力能量 —— //
    public class NormalScissorsPlayer : ModPlayer
    {
        public int dashCooldown;            // 右键冲刺冷却（帧）
        public int energy;                  // 左键能量（0~EnergyMax）
        public const int EnergyMax = 100;   // 满能量阈值
        public const int EnergyGainPerHit = 25; // 每次左键命中获得的能量
        public bool forceChargedOnNextSwing = false; // 冲刺后置位，下一次左键挥砍必定释放蓄力斩波

        public override void ResetEffects()
        {
            if (dashCooldown > 0) dashCooldown--;
        }

        public float EnergyPercent => Utils.Clamp(energy / (float)EnergyMax, 0f, 1f);

        public void GainEnergy(int amount)
        {
            energy += amount;
            if (energy > EnergyMax) energy = EnergyMax;
        }

        public void ClearEnergy() => energy = 0;
    }

    /// <summary>
    /// NormalScissors 三段成长 + 能量蓄力
    /// - 击败巨鹿（Deerclops）：开启“左键平A攒能量→满后自动释放蓄力斩”
    /// - 击败血肉墙（Wall of Flesh → Hardmode）：右键冲刺切割（CD 1s）
    /// - 击败史莱姆皇后（Queen Slime）：近战命中时额外发射 Enchanted Beam
    /// - 放大后持有方式后移对齐手臂
    /// </summary>
    public class NormalScissors : ModItem
    {
        // —— 可调参数 —— //
        private const float BaseDamage = 22f;

        // “蜂后增大尺寸”的老规则，可选保留（你之前提过“物品放大后应该后移对齐”）
        private const bool EnableQueenBeeScale = true;
        private const float ScaleAfterQueenBee = 1.35f;

        // 放大后持有时的后移像素（越大越靠后）
        private const float HoldBackOffset = 6f;

        // 蓄力斩数值（巨鹿后解锁）
        private const float WaveDamageMul = 1.45f; // 斩波伤害系数（相对近战本体）
        private const float WaveSpeed = 17f;   // 斩波飞行速度
        // 右键冲刺（WoF后）
        private const int DashCooldownFrames = 60;     // 1秒CD
        private const float DashSpeed = 11.5f;
        private const int DashSlashProjTime = 10;

        // Queen Slime 射弹
        private const int BeamProjectileType = ProjectileID.EnchantedBeam;
        private const float BeamSpeed = 12f;

        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 34;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(silver: 50);

            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useTurn = true;
            Item.autoReuse = true;      // 自动挥舞无冲突
            Item.UseSound = SoundID.Item1;

            Item.DamageType = DamageClass.Melee;
            Item.damage = (int)BaseDamage;
            Item.knockBack = 4.8f;
            Item.crit = 4;
            Item.scale = 1f;

            Item.shoot = ProjectileID.None;   // 默认不发射
            Item.shootSpeed = 0f;

            Item.channel = false; // 不使用按住蓄力
        }

        // 放大后持有对齐：近战使用时往反挥击方向拉一点点
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (player.itemAnimation > 0 && player.HeldItem == Item)
            {
                float scale = GetCurrentScale(player);
                if (scale > 1f)
                {
                    Vector2 dir = (player.itemRotation).ToRotationVector2();
                    player.itemLocation -= dir.SafeNormalize(Vector2.UnitX) * HoldBackOffset * (scale - 1f);
                }
            }
        }

        // Queen Bee 可选：让武器尺寸放大
        public override void ModifyItemScale(Player player, ref float scale)
        {
            if (EnableQueenBeeScale && NPC.downedQueenBee)
            {
                scale *= ScaleAfterQueenBee;
            }
        }

        // 动态伤害：这里不用做“蓄力时加成”，因为这代由“自动释放蓄力斩”单独结算
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // 可在此做其他阶段数值（若需要）
        }

        // 左键命中：攒能量 + Queen Slime 发射射弹
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            var mp = player.GetModPlayer<NormalScissorsPlayer>();

            // —— 攒能量：打过巨鹿才开始记录 —— //
            if (NPC.downedDeerclops)
            {
                mp.GainEnergy(NormalScissorsPlayer.EnergyGainPerHit);

                // 能量满 → 自动释放蓄力斩
                if (mp.energy >= NormalScissorsPlayer.EnergyMax && Main.myPlayer == player.whoAmI)
                {
                    FireChargedSlash(player);
                    mp.ClearEnergy();
                }
            }

            // —— Queen Slime：附加发射原版射弹 —— //
            // if (NPC.downedQueenSlime && Main.myPlayer == player.whoAmI)
            // {
            //     Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center);
            //     if (dir == Vector2.Zero) dir = new Vector2(player.direction, 0f);
            //     Projectile.NewProjectile(
            //         player.GetSource_ItemUse(Item),
            //         player.Center,
            //         dir * BeamSpeed,
            //         BeamProjectileType,
            //         (int)(Item.damage * 0.75f),
            //         Item.knockBack * 0.7f,
            //         player.whoAmI
            //     );
            // }
        }

        // 右键功能启用
        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            var mp = player.GetModPlayer<NormalScissorsPlayer>();

            if (player.altFunctionUse == 2)
            {
                // 右键冲刺（打完WoF → Hardmode）
                if (!Main.hardMode) return false;
                if (player.HasBuff(ModContent.BuffType<NormalScissorsDashBuff>())) return false;

                Item.useStyle = ItemUseStyleID.Thrust;
                Item.useAnimation = 12;
                Item.useTime = 12;
                Item.noUseGraphic = false;
                Item.noMelee = true;            // 冲刺伤害交给冲刺命中体
                Item.UseSound = SoundID.Item71;
            }
            else
            {
                // 左键常规挥击（自动挥舞无冲突）
                Item.useStyle = ItemUseStyleID.Swing;
                Item.useAnimation = 22;
                Item.useTime = 22;
                Item.noUseGraphic = false;
                Item.noMelee = false;
                Item.channel = false;
                // ★ 关键改动：Queen Slime 解锁后，给物品常驻发射 EnchantedBeam（像附魔剑）
                if (NPC.downedQueenSlime)
                {
                    Item.shoot = ProjectileID.EnchantedBeam;
                    Item.shootSpeed = BeamSpeed;          // 你之前定义的 BeamSpeed（例如 12f）
                                                          // （可选）如果想让斩波略弱于本体，可在 Shoot/ModifyShootStats 里调整 damage/knockback，见下方可选项
                }
                else
                {
                    Item.shoot = ProjectileID.None;
                    Item.shootSpeed = 0f;
                }
            }
            return true;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity,
                                              ref int type, ref int damage, ref float knockback)
        {
            if (type == ProjectileID.EnchantedBeam && NPC.downedQueenSlime)
            {
                damage = (int)(damage * 0.75f);  // 比本体伤害低一些，按需调整
                knockback *= 0.7f;
            }
        }

        public override bool? UseItem(Player player)
        {
            var mp = player.GetModPlayer<NormalScissorsPlayer>();

            if (player.altFunctionUse == 2 && Main.myPlayer == player.whoAmI)
            {
                // —— 冲刺 —— //
                Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center);
                if (dir.LengthSquared() < 0.001f) dir = new Vector2(player.direction, 0f);

                player.velocity = dir * DashSpeed;
                player.immune = true;
                player.immuneTime = 10;

                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center,
                    dir * 0.1f,
                    ModContent.ProjectileType<DashSlashProj>(),
                    (int)(Item.damage * 1.1f),
                    Item.knockBack + 1.0f,
                    player.whoAmI,
                    DashSlashProjTime
                );

                for (int i = 0; i < 10; i++)
                {
                    int dust = Dust.NewDust(player.position, player.width, player.height, DustID.Smoke,
                        dir.X * 2f, dir.Y * 2f, 150, default, 1.2f);
                    Main.dust[dust].noGravity = true;
                }

                mp.dashCooldown = DashCooldownFrames;
                player.AddBuff(ModContent.BuffType<NormalScissorsDashBuff>(), DashCooldownFrames);

                mp.energy = NormalScissorsPlayer.EnergyMax;   // 能量直接拉满
                mp.forceChargedOnNextSwing = true;            // 标记：下一次左键必放斩波
                if (Main.netMode != NetmodeID.Server)
                    CombatText.NewText(player.Hitbox, new Color(120, 255, 120), "能量充盈");
            }
            else
            {
                if (mp.forceChargedOnNextSwing)
                {
                    // 直接释放你之前实现的斩波（TerraBeam 版本）
                    FireChargedSlash(player);
                    mp.ClearEnergy();                 // 清能量，避免叠触发
                    mp.forceChargedOnNextSwing = false;

                    // （可选）如果你实现了斩波锁（waveLockout），这里顺手置个极短CD避免与同帧其他发射叠加
                    // mp.waveLockout = 6;
                }
            }
            return base.UseItem(player);
        }

        // ——— 自动释放“蓄力斩” ——— //
        private void FireChargedSlash(Player player)
        {
            // （可保留）短暂无敌，给点手感容错；不需要就删
            player.immune = true;
            player.immuneTime = 10;

            Vector2 dir = Vector2.Normalize(Main.MouseWorld - player.Center);
            if (dir.LengthSquared() < 0.001f) dir = new Vector2(player.direction, 0f);

            // ⭐ 改成发泰拉刃的射弹（TerraBeam）
            Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                player.Center,
                dir * WaveSpeed,
                ProjectileID.TerraBeam,                       // ← 关键：泰拉刃斩波
                (int)(Item.damage * WaveDamageMul),
                Item.knockBack + 1.5f,
                player.whoAmI
            );

            // （可选）音效/粒子，随你保留或精简
            SoundEngine.PlaySound(SoundID.Item71.WithPitchOffset(-0.1f), player.Center);
            for (int i = 0; i < 14; i++)
            {
                int d = Dust.NewDust(player.Center - new Vector2(12, 12), 24, 24, DustID.MagicMirror,
                    dir.X * Main.rand.NextFloat(2f, 4f), dir.Y * Main.rand.NextFloat(2f, 4f), 120, default, 1.2f);
                Main.dust[d].noGravity = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            int idx = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name == "Tooltip0");
            if (idx < 0) idx = tooltips.Count;

            bool deer = NPC.downedDeerclops;
            bool wof = Main.hardMode;
            bool qsl = NPC.downedQueenSlime;

            tooltips.Insert(idx++, new TooltipLine(Mod, "GrowthHeader", "成长解锁："));
            tooltips.Insert(idx++, new TooltipLine(Mod, "Step_Deer",
                $"{(deer ? "✔" : "✖")} 巨鹿：左键平A攒能量 → 满能自动释放“蓄力斩”"));
            tooltips.Insert(idx++, new TooltipLine(Mod, "Step_WoF",
                $"{(wof ? "✔" : "✖")} 血肉墙：右键冲刺切割（短CD）"));
            tooltips.Insert(idx++, new TooltipLine(Mod, "Step_QSlime",
                $"{(qsl ? "✔" : "✖")} 史莱姆皇后：近战命中时发射 Enchanted Beam"));

            // 显示当前能量（仅客户端）
            if (Main.LocalPlayer?.HeldItem == Item && deer)
            {
                float pct = Main.LocalPlayer.GetModPlayer<NormalScissorsPlayer>().EnergyPercent;
                int p = (int)(pct * 100f);
                tooltips.Insert(idx++, new TooltipLine(Mod, "EnergyNow", $"当前能量：{p}%"));
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 10)
                .AddIngredient(ItemID.Wood, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }

        private float GetCurrentScale(Player player)
        {
            float s = Item.scale;
            if (EnableQueenBeeScale && NPC.downedQueenBee) s *= ScaleAfterQueenBee;
            return s;
        }
    }

    // —— 右键冲刺：贴身命中体 —— //
    public class DashSlashProj : ModProjectile
    {
        public override string Texture => "WuDao/Content/Items/Weapons/Melee/NormalScissors";
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 10;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            if (Projectile.ai[0] > 0) Projectile.timeLeft = (int)Projectile.ai[0];
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead) { Projectile.Kill(); return; }

            Projectile.Center = owner.Center + owner.velocity * 0.5f;
            Projectile.direction = owner.direction;

            if (Main.rand.NextBool(3))
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 150, default, 1f);
                Main.dust[d].velocity *= 0.2f;
                Main.dust[d].noGravity = true;
            }
        }
    }
}
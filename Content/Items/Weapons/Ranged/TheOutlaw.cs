using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common.Players;
using WuDao.Content.Projectiles.Ranged;
/*
    ✅ 基础霰弹：useTime=30，扇形 3 发，子弹穿透 3，穿透每次 -10%（ModifyHitNPC 根据 localAI[0] 递减）
    ✅ 史莱姆王：4 发
    ✅ 蜂王：4~5 发
    ✅ 巨鹿：暴击后 标记 “下一次” 6 发且每颗 +30%（在 GritPlayer.nextShotEmpowered）
    ✅ 血肉墙：右键后跳（2分钟 CD），短暂无敌，暴击 -10秒 CD
    ✅ 任意机械：每次攻击额外发 1 枚 GritSplitShot，命中/撞墙/短时后分裂成左右 GritSplitBomblet，即刻爆炸（T字分裂）
    ✅ 世纪之花：分裂时生成 GritFirewall（2 秒）触碰伤害
    ✅ 拜月邪教徒：累计 4 次暴击 → 下一次发射 终极爆弹（基础伤害×5），首撞时对目标身后扇形90%溅射；若未命中，飞行到时限在前方扇形90%爆炸
    ✅ 烟雾手雷：爆炸生成 5 秒烟雾圈，对范围内敌怪每秒 5~10 固定伤害
*/
namespace WuDao.Content.Items.Weapons.Ranged
{
    // TODO: 重绘贴图 成长型散弹枪（法外狂徒风格）
    public class TheOutlaw : ModItem
    {
        // public override void SetStaticDefaults()
        // {
        //     DisplayName.SetDefault("法外碎星 · 散弹枪");
        //     Tooltip.SetDefault(
        //         "每秒2次射击，扇形霰弹，穿透3个目标并每穿透-10%伤害\n" +
        //         "成长：击败史莱姆王/蜂王/巨鹿/血肉墙/任意机械/世纪之花/拜月邪教徒 解锁不同效果\n" +
        //         "右键（血肉墙后）：后跳并获得短暂无敌（2分钟冷却，暴击-10秒冷却）"
        //     );
        // }
        // 试验一下，它到底会用同名的PNG还是自己选定的原版贴图
        public override string Texture => $"Terraria/Images/Item_{ItemID.QuadBarrelShotgun}";
        public override void SetDefaults()
        {
            Item.damage = 30;              // 基础伤害（会被成长与技能修改）
            Item.DamageType = DamageClass.Ranged;
            Item.width = 54;
            Item.height = 20;
            Item.useTime = 30;             // 每秒2次
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.value = Item.buyPrice(0, 5);
            Item.rare = ItemRarityID.Red;
            Item.UseSound = SoundID.Item36;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TheOutlawPellet>();
            Item.shootSpeed = 6f;
            Item.useAmmo = AmmoID.None;    // 内置子弹，不消耗弹药
            Item.crit = 90;//测试
        }

        public override bool AltFunctionUse(Player player) => Main.hardMode; // 血肉墙后可右键

        public override bool CanUseItem(Player player)
        {
            var gp = player.GetModPlayer<TheOutlawPlayer>();
            if (player.altFunctionUse == 2)
            {
                // 右键：后跳（仅在冷却结束时触发）
                if (!gp.CanDashBack()) return false;

                // 触发后跳：后撤一小段距离 + 短暂无敌帧
                // 以玩家面向的反方向位移
                float backSpeed = 8f;
                player.velocity = new Vector2(-player.direction * backSpeed, -3f);
                player.immune = true;
                player.immuneTime = 20; // ~1/3秒
                gp.TriggerDashBack();

                // 右键这帧不射击
                return false;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var gp = player.GetModPlayer<TheOutlawPlayer>();

            // 终极爆弹（拜月邪教徒后，攒4层暴击触发一次）
            if (NPC.downedAncientCultist && gp.ultimateReady)
            {
                gp.ultimateReady = false;
                // 终极爆弹：基础伤害*5
                int dmg = (int)(damage * 5f);
                int proj = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<TheOutlawUltimateBomb>(), dmg, knockback, player.whoAmI);
                // 不走普通扇形逻辑
                return false;
            }

            // 普通霰弹逻辑 + Deerclops 暴击增幅“下一次”
            int pelletCount = 3; // 基础 3
            if (NPC.downedSlimeKing) pelletCount = 4;                      // 史莱姆王后 4发
            if (NPC.downedQueenBee) pelletCount = Main.rand.Next(4, 6);    // 蜂王后 4~5发
            float dmgScale = 1f;
            if (NPC.downedDeerclops && gp.nextShotEmpowered)
            {
                pelletCount = 6;
                dmgScale *= 1.3f; // +30% 伤害
                gp.nextShotEmpowered = false; // 消耗
            }

            float spread = MathHelper.ToRadians(5f); // 扇形总展开角的大致基准（单发的左右偏移）15f 太大了
            for (int i = 0; i < pelletCount; i++)
            {
                float interp = pelletCount == 1 ? 0f : (i - (pelletCount - 1) / 2f) / (pelletCount - 1f); // [-0.5, 0.5]
                float angle = interp * spread * (pelletCount - 1);
                Vector2 perturbed = velocity.RotatedBy(angle);
                int proj = Projectile.NewProjectile(
                    source, position, perturbed,
                    ModContent.ProjectileType<TheOutlawPellet>(),
                    (int)(damage * dmgScale), knockback, player.whoAmI
                );
            }

            // 机械后：额外发射 1 枚“分裂射弹”（穿透1，命中/撞墙/短时后分裂成左右爆破弹）
            if (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3)
            {
                int split = Projectile.NewProjectile(
                    source, position, velocity,
                    ModContent.ProjectileType<TheOutlawSplitShot>(),
                    damage, knockback, player.whoAmI
                );
            }

            return false; // 我们手动发射了
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            var p = Main.LocalPlayer;
            // 逐项判定 Boss 击败状态
            bool slime = NPC.downedSlimeKing;
            bool bee = NPC.downedQueenBee;
            bool deer = NPC.downedDeerclops;
            bool wall = Main.hardMode;
            bool mech = NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3;
            bool plan = NPC.downedPlantBoss;
            bool cult = NPC.downedAncientCultist;

            // 第一行：基础说明（固定）
            tooltips.Add(new TooltipLine(Mod, "Outlaw_Base",
                "每秒2次射击，扇形霰弹，子弹穿透3个目标且每穿透-10%伤害。"));

            // 成长解锁标题
            tooltips.Add(new TooltipLine(Mod, "Outlaw_Title", "成长解锁："));

            // 逐条展示（✅/❌）
            void AddLine(string key, bool ok, string text)
            {
                string mark = ok ? "✅" : "❌";
                tooltips.Add(new TooltipLine(Mod, key, $"{mark} {text}"));
            }

            AddLine("SK", slime, "击败史莱姆王：每次射击 4 发扇形子弹");
            AddLine("QB", bee, "击败蜂王：每次射击 4~5 发扇形子弹（随机）");
            AddLine("DEER", deer, "击败巨鹿：暴击后，下一次射击 6 发且每颗 +30% 伤害");
            AddLine("WALL", wall, "击败血肉墙：右键后跳（2分钟CD），短暂无敌。暴击 -10秒CD");
            AddLine("MECH", mech, "击败任一机械Boss：额外发射 1 枚分裂射弹（命中/撞墙/短时后分裂为左右爆破弹）");
            AddLine("PLAN", plan, "击败世纪之花：分裂时必定生成火墙（2秒）");
            AddLine("CULT", cult, "击败拜月邪教徒：触发4次暴击后，下一次发射【终极爆弹】（基础伤害×5，扇形90%溅射）");
        }

    }
}

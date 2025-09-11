using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Juexue.Active;
using Terraria.ID;
using WuDao.Content.Systems;
using System;

namespace WuDao.Content.Players
{
    public class QiPlayer : ModPlayer
    {
        // —— 数值 —— //
        public int QiMaxBase = 0;               // 装备绝学后基础上限 100
        public int QiMaxFromItems = 0;          // 灵芝/仙草永久上限
        public float QiCurrent = 0f;            // 当前气力（float 便于逐帧蓄力/消耗）
        public float QiRegenStand = 2;
        public float QiRegenMove = 1;
        public int QiMax => QiMaxBase + QiMaxFromItems;

        // —— 绝学槽中的物品 —— //
        public Item JuexueSlot = new Item();

        // —— 上限道具使用次数 —— //
        public int Used_ReiShi = 0;            // 最多 1
        public int Used_PassionFruit = 0;            // 最多 5

        // —— 主动技能冷却 —— //
        public readonly Dictionary<int, uint> perSkillNextUseTick = new();   // key = item.type
        public uint nextGlobalActiveTick = 0;                                 // 公共冷却（2秒）
        public const int GlobalActiveCooldownTicks = 60 * 2;

        // —— 蓄力（龟派气功） —— //
        public bool Charging = false;
        public int ChargeQiSpent = 0;           // 本次按住期间消耗的气力点数
        // === 凌波微步（开关技） ===
        public bool LingboActive = false;
        private Vector2 _lingboLastWavePos;
        private float _lingboDistanceAcc = 0f;

        // === 天外飞仙（短时突进） ===
        public int FeixianTicks = 0; // >0 表示进行中
        public const int FeixianTotalTicks = 150; // ~2.5s
        public Vector2 FeixianTarget; // ★ 新增：飞仙的目标点

        // === 利刃华尔兹（简化 R） ===
        public int BladeWaltzTicks = 0;        // 整个流程剩余时间（8*54=432）
        public int BladeWaltzStepTimer = 0;    // 当前段计时（54 tick一段）
        public int BladeWaltzHitsLeft = 0;     // 剩余攻击次数
        public int BladeWaltzTarget = -1;      // 当前锁定 npc 索引

        // === 降龙十八掌 计数 ===
        public int XiangLongCount = 0;

        // —— UI 显示条件 —— //
        public bool ShouldShowQiBar()
        {
            // 与 ResetEffects 的 enableQi 一致：只要背包或槽位里有绝学，就显示
            if (!JuexueSlot.IsAir && JuexueSlot.ModItem is JuexueItem) return true;

            for (int i = 0; i < 58; i++)
            {
                var it = Player.inventory[i];
                if (it?.ModItem is JuexueItem) return true;
            }
            return false;
        }

        public override void Initialize()
        {
            JuexueSlot = new Item();
            JuexueSlot.TurnToAir();
        }

        public override void SaveData(TagCompound tag)
        {
            tag["QiMaxFromItems"] = QiMaxFromItems;
            tag["Used_ReiShi"] = Used_ReiShi;
            tag["Used_PassionFruit"] = Used_PassionFruit;

            // 保存绝学槽
            if (!JuexueSlot.IsAir)
                tag["JuexueSlot"] = ItemIO.Save(JuexueSlot);
        }

        public override void LoadData(TagCompound tag)
        {
            QiMaxFromItems = tag.GetInt("QiMaxFromItems");
            Used_ReiShi = tag.GetInt("Used_ReiShi");
            Used_PassionFruit = tag.GetInt("Used_PassionFruit");

            if (tag.ContainsKey("JuexueSlot"))
            {
                JuexueSlot = ItemIO.Load(tag.GetCompound("JuexueSlot"));
            }
            else
            {
                JuexueSlot = new Item();
                JuexueSlot.TurnToAir();
            }
        }

        public override void ResetEffects()
        {
            // 1) 是否“拥有气力系统”：背包或绝学槽存在绝学就开启
            bool hasJuexueInSlot = !JuexueSlot.IsAir && JuexueSlot.ModItem is JuexueItem;

            bool hasJuexueInInventory = false;
            for (int i = 0; i < 58; i++)
            { // 主背包 0..57
                var it = Player.inventory[i];
                if (it?.ModItem is JuexueItem) { hasJuexueInInventory = true; break; }
            }

            bool hasJuexueOnMouse = !Main.mouseItem.IsAir && Main.mouseItem.ModItem is JuexueItem;
            bool enableQi = hasJuexueInSlot || hasJuexueInInventory || hasJuexueOnMouse;

            // 2) 基础上限：满足条件就给 100，否则为 0
            QiMaxBase = enableQi ? 100 : 0;

            // 只有“完全没有绝学”时才做下压，避免拖拽瞬时把当前值压低
            if (!enableQi)
            {
                QiCurrent = MathHelper.Clamp(QiCurrent, 0, QiMax);
            }
            else if (QiCurrent < 0f)
            {
                QiCurrent = 0f;
            }

            // 凌波微步激活时：在 ModifyHitBy* 里给 10% 躲避，这里只做位置初始化
            if (!LingboActive)
            {
                _lingboDistanceAcc = 0f;
                _lingboLastWavePos = Player.Bottom;
            }
        }

        public override void PreUpdate()
        {
            // 气力回复：站立/不动 +4/s，移动或攻击 +2/s
            if (QiMax > 0)
            {
                float perTick = Common.Helpers.IsPlayerAttackingOrMoving(Player) ? (QiRegenMove / 60f) : (QiRegenStand / 60f);
                QiCurrent = MathHelper.Clamp(QiCurrent + perTick, 0, QiMax);
            }

            // 蓄力期间（龟派气功）：每帧 -1 气
            if (Charging)
            {
                if (QiCurrent >= 2f)
                {
                    QiCurrent -= 1f;
                    ChargeQiSpent++;
                    // 每消耗 20 点真气，播放短促提示音
                    if (ChargeQiSpent % 20 == 0)
                    {
                        Terraria.Audio.SoundEngine.PlaySound(
                        Terraria.ID.SoundID.MaxMana with { Volume = 0.6f, Pitch = 0.15f },
                        Player.Center
                        );
                    }
                }
                else
                {
                    // 没气了自动松手
                    Charging = false;
                    if (JuexueSlot.ModItem is Kamehameha k)
                    {
                        k.ReleaseFire(Player, this, ChargeQiSpent);
                        ChargeQiSpent = 0;
                    }
                }
            }

            // —— 凌波微步：每秒 15 气；无气自动关闭；移动出水波 —— 
            if (LingboActive)
            {
                if (QiCurrent <= 15f)
                {
                    LingboActive = false;
                }
                else
                {
                    // 扣气
                    QiCurrent -= 15f / 60f;
                    // 距离累计 & 水波（Dust 水波占位）
                    float moved = Vector2.Distance(Player.Bottom, _lingboLastWavePos);
                    _lingboDistanceAcc += moved;
                    _lingboLastWavePos = Player.Bottom;

                    if (_lingboDistanceAcc >= 32f)
                    { // 每移动 ~32px 出一圈水波
                        _lingboDistanceAcc = 0f;
                        for (int i = 0; i < 12; i++)
                        {
                            var d = Dust.NewDustPerfect(Player.Bottom + new Vector2(Main.rand.NextFloat(-12, 12), 0),
                                DustID.Water, new Vector2(Main.rand.NextFloat(-1, 1), -Main.rand.NextFloat(0.5f, 1.5f)));
                            d.noGravity = true;
                            d.scale = 1.5f;
                        }
                    }
                }
            }

            // —— 天外飞仙：逐帧推进 & 路径伤害 —— //
            if (FeixianTicks > 0)
            {
                // 基础无敌 & 隐身
                Player.immune = true;
                Player.immuneTime = 2;
                Player.invis = true;
                Player.noKnockback = true;

                // 朝向锁定
                Vector2 toTarget = (FeixianTarget - Player.Center);
                Vector2 dir = toTarget.SafeNormalize(Vector2.UnitX);
                Player.direction = (dir.X >= 0f) ? 1 : -1;

                // 推进速度（像素/帧）；到达目标或超时即结束
                float speed = 26f;
                if (toTarget.Length() <= speed || FeixianTicks == 1)
                {
                    Player.velocity = Vector2.Zero;
                    FeixianTicks = 0;
                    // 解冻全场（仅当当前冻结来自飞仙）
                    TimeStopSystem.StopIfFeixian();
                }
                else
                {
                    Player.velocity = dir * speed;
                    int damage = 120;                     // 伤害自行调

                    // 每 2 帧在当前位置生成极短命友方投射物（路径伤害；放行已在 TimeStopSystem 里处理）
                    if ((FeixianTicks % 2) == 0)
                    {
                        int projType = ProjectileID.FirstFractal;  // 天顶剑视觉；也可换成 EnchantedBeam
                        int proj = Projectile.NewProjectile(
                            Player.GetSource_Misc("FeixianTrail"),
                            Player.Center,
                            dir * 26f,
                            projType, damage, 4f, Player.whoAmI);
                        Main.projectile[proj].timeLeft = 30;
                        Main.projectile[proj].tileCollide = false;
                    }
                    // ★ 在飞行路径四周生成“花瓣环”：每 6 帧一圈，6~8 片
                    if ((FeixianTicks % 6) == 0)
                    {
                        int petals = 6;                 // 每圈花瓣数量
                        float radius = 32f;             // 出生半径（围绕玩家）
                        float forwardSpeed = 18f;       // 沿路径前进分量
                        float outwardMin = 3f;          // 向外发散速度范围
                        float outwardMax = 7f;

                        // 法线（与 dir 垂直），用于做环
                        Vector2 normal = new Vector2(-dir.Y, dir.X);
                        float baseAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);

                        for (int i = 0; i < petals; i++)
                        {
                            float ang = baseAngle + i * MathHelper.TwoPi / petals;
                            // 以 dir/normal 为正交基构造圆环点
                            Vector2 ringOffset = (float)Math.Cos(ang) * normal + (float)Math.Sin(ang) * dir;
                            Vector2 spawnPos = Player.Center + ringOffset * radius;

                            // 速度 = 沿路线前进 + 径向微外扩
                            Vector2 outward = (spawnPos - Player.Center).SafeNormalize(Vector2.UnitX);
                            Vector2 vel = dir * forwardSpeed + outward * Main.rand.NextFloat(outwardMin, outwardMax);

                            var p = Projectile.NewProjectileDirect(
                                Player.GetSource_Misc("FeixianPetals"),
                                spawnPos,
                                vel,
                                ProjectileID.FlowerPetal,        // 原版花瓣
                                damage, 3f, Player.whoAmI
                            );
                            if (p != null)
                            {
                                p.friendly = true;
                                p.hostile = false;
                                p.tileCollide = false;           // 防止被地形卡住（看喜好可改为 true）
                                p.timeLeft = 45;                 // 花瓣寿命
                                p.penetrate = 1;                 // 每片最多命中 1 次（按需调整）
                                p.usesLocalNPCImmunity = true;   // 本地免疫，避免一群花瓣同帧狂打
                                p.localNPCHitCooldown = 12;
                                // 可选：近战系数
                                p.DamageType = DamageClass.Melee; // 或 Generic
                            }
                        }
                    }
                }

                FeixianTicks--;
            }

            // —— 利刃华尔兹：简化 8 段，每段 54 tick，出现残影 + 伤害 —— 
            if (BladeWaltzTicks > 0)
            {
                BladeWaltzTicks--;
                BladeWaltzStepTimer--;

                // 全程无敌 + 隐身
                Player.immune = true;
                Player.immuneTime = 2;
                Player.immuneNoBlink = true;  // 不要免疫闪烁（避免“可见”闪动）
                Player.invis = true; // 结束时会复位

                // 禁止一切玩家输入与使用
                Player.controlUseItem = false;
                Player.controlUseTile = false;
                Player.controlHook = false;
                Player.controlMount = false;
                Player.controlJump = false;
                Player.controlLeft = Player.controlRight = Player.controlUp = Player.controlDown = false;
                Player.mount?.Dismount(Player);         // 退出坐骑（以免移动）
                Player.velocity = Vector2.Zero;  // 锁定原地

                Player.AddBuff(BuffID.Invisibility, 2, true); // ★ 每帧续 2tick，确保完全隐身

                if (BladeWaltzStepTimer <= 0 && BladeWaltzHitsLeft > 0)
                {
                    BladeWaltzStepTimer = 54;   // 每段 ~0.9s
                    BladeWaltzHitsLeft--;

                    // 目标：半屏范围随机一个可追踪敌人
                    int targetIndex = FindRandomWaltzTarget();
                    Microsoft.Xna.Framework.Vector2 targetPos;

                    if (targetIndex >= 0)
                    {
                        var npc = Main.npc[targetIndex];
                        targetPos = npc.Center;
                    }
                    else
                    {
                        // 无目标则朝鼠标方向抡一刀
                        targetPos = Main.MouseWorld;
                    }

                    // 斩击起点：玩家附近随机一个空位（稍有偏移，像“瞬身到敌近旁出刀”）
                    var spawn = Player.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(60f, 120f);

                    // 方向与速度
                    var dir = (targetPos - spawn).SafeNormalize(Microsoft.Xna.Framework.Vector2.UnitX);
                    float speed = 26f; // 速度越高，视觉越“快斩”
                    int damage = 120;  // 调整这里来平衡
                    float knockback = 3f;

                    var projEnt = Projectile.NewProjectileDirect(
                        Player.GetSource_Misc("BladeWaltz"),
                        spawn,
                        dir * speed,
                        ProjectileID.FirstFractal, // ★ 第一分形视觉 / 伤害体
                        damage,
                        knockback,
                        Player.whoAmI
                    );

                    if (projEnt != null)
                    {
                        projEnt.timeLeft = 30;
                        projEnt.tileCollide = false;
                        projEnt.friendly = true;    // ★ 强制友方，确保命中
                        projEnt.hostile = false;
                        projEnt.netUpdate = true;
                    }
                    
                    // 残影/特效（可选）
                for (int i = 0; i < 16; i++)
                {
                    var d = Dust.NewDustPerfect(
                        spawn + Main.rand.NextVector2Circular(18, 18),
                        DustID.SilverFlame,
                        Main.rand.NextVector2Circular(2, 2),
                        150, default, 1.1f
                    );
                    d.noGravity = true;
                }
                }

                if (BladeWaltzTicks <= 0)
                {
                    Player.invis = false; // 结束显示
                }
            }
        }

        // 进一步保险：把绘制信息设为隐身，确保任何残余层都不画
        public override void ModifyDrawInfo(ref Terraria.DataStructures.PlayerDrawSet drawInfo)
        {
            // 利刃华尔兹或天外飞仙期间都可隐藏（你也可以只对华尔兹隐藏）
            if (BladeWaltzTicks > 0 /* || FeixianTicks > 0 */)
            {
                drawInfo.hideEntirePlayer = true;   // ★ 彻底不画玩家
                drawInfo.drawPlayer.invis = true;   // 兼容部分层读取 invis
                drawInfo.colorArmorBody.A = 0;
                drawInfo.colorArmorHead.A = 0;
                drawInfo.colorArmorLegs.A = 0;
                drawInfo.colorEyeWhites.A = 0;
                drawInfo.colorEyes.A = 0;
                drawInfo.colorHair.A = 0;
            }
        }
        // 改成“随机选取”半屏半径内的敌人（原先是 nearest）
        private int FindRandomWaltzTarget()
        {
            float radius = System.Math.Min(Main.screenWidth, Main.screenHeight) * 0.5f;
            // 收集候选
            System.Span<int> buf = stackalloc int[200];
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var n = Main.npc[i];
                if (!n.active || n.friendly || !n.CanBeChasedBy()) continue;
                if (Microsoft.Xna.Framework.Vector2.Distance(n.Center, Player.Center) <= radius)
                {
                    buf[count++] = i;
                    if (count >= buf.Length) break;
                }
            }
            if (count == 0) return -1;

            int pick = buf[Main.rand.Next(count)];
            return pick;
        }
        private int FindWaltzTarget()
        {
            float radius = System.Math.Min(Main.screenWidth, Main.screenHeight) * 0.5f;
            int chosen = -1; float best = float.MaxValue;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var n = Main.npc[i];
                if (!n.active || n.friendly || !n.CanBeChasedBy()) continue;
                float d = Vector2.Distance(n.Center, Player.Center);
                if (d <= radius && d < best) { best = d; chosen = i; }
            }
            return chosen;
        }

        // —— 凌波 10% 躲避（对怪近战 & 投射物） —— 
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (LingboActive && Main.rand.NextFloat() < 0.10f)
            {
                modifiers.FinalDamage *= 0f;
                modifiers.Knockback *= 0f;
                // 可加一小段水波/闪避提示
            }
        }
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (LingboActive && Main.rand.NextFloat() < 0.10f && proj.hostile)
            {
                modifiers.FinalDamage *= 0f;
                modifiers.Knockback *= 0f;
            }
        }
        public bool TrySpendQi(int cost)
        {
            if (QiCurrent + 1e-3f >= cost)
            {
                QiCurrent -= cost;
                return true;
            }
            return false;
        }

        public bool CanUseActiveNow(int itemType, int extraCooldownTicks)
        {
            // 公共 2s 冷却
            if (Main.GameUpdateCount < nextGlobalActiveTick) return false;

            // 专属冷却
            if (perSkillNextUseTick.TryGetValue(itemType, out var t) && Main.GameUpdateCount < t) return false;

            return true;
        }

        public void StampActiveUse(int itemType, int extraCooldownTicks)
        {
            nextGlobalActiveTick = Main.GameUpdateCount + GlobalActiveCooldownTicks;
            perSkillNextUseTick[itemType] = Main.GameUpdateCount + (uint)extraCooldownTicks;
        }

        public override void ProcessTriggers(Terraria.GameInput.TriggersSet triggersSet)
        {
            if (QiKeybinds.CastSkillKey.JustPressed)
            {
                // 按下：如果槽里是龟派气功，进入蓄力，否则尝试施放主动技能
                if (JuexueSlot.ModItem is Kamehameha k)
                {
                    // 若冷却允许才进入蓄力，否则当作释放失败
                    if (CanUseActiveNow(JuexueSlot.type, k.SpecialCooldownTicks))
                    {
                        Charging = true;
                        ChargeQiSpent = 0;
                    }
                }
                else if (JuexueSlot.ModItem is JuexueItem ji && ji.IsActive)
                {
                    // ★ 华尔兹进行中时，忽略再次按键
                    if (BladeWaltzTicks > 0) return;

                    ji.TryActivate(Player, this); // 主动释放
                }

            }
            if (QiKeybinds.CastSkillKey.JustReleased)
            {
                // 松开：若是龟派气功则结算发射
                if (JuexueSlot.ModItem is Kamehameha k && Charging)
                {
                    Charging = false;
                    k.ReleaseFire(Player, this, ChargeQiSpent);
                    ChargeQiSpent = 0;
                }
            }
        }
    }
}

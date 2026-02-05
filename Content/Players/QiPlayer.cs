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
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Melee;
using WuDao.Common;
using Terraria.GameContent;

namespace WuDao.Content.Players
{
    // 绝学与气力系统
    public class QiPlayer : ModPlayer
    {
        // —— 数值 —— //
        public int QiMaxBase = 0;               // 装备绝学后基础上限 100
        public int QiMaxFromItems = 0;          // 灵芝/仙草永久上限
        public float QiCurrent = 0f;            // 当前气力（float 便于逐帧蓄力/消耗）
        public float QiRegenStand = 4;// 静止时气力再生速度
        public float QiRegenMove = 2;
        public int QiMax => QiMaxBase + QiMaxFromItems;
        public int JinggongUsed = 0;          // 静功秘籍已使用次数（可选限制）
        public int DonggongUsed = 0;          // 动功秘籍已使用次数

        public float QiRegenStandBonus = 0f;  // 静止额外回气（永久）
        public float QiRegenMoveBonus = 0f;   // 移动/攻击额外回气（永久）
        // —— 绝学槽中的物品 —— //
        public Item JuexueSlot = new Item();

        // —— 上限道具使用次数 —— //
        public int Used_ReiShi = 0;            // 最多 1
        public int Used_PassionFruit = 0;            // 最多 5

        // —— 主动技能冷却 —— //
        public readonly Dictionary<int, uint> perSkillNextUseTick = new();   // key = item.type
        public uint nextGlobalActiveTick = 0;
        // 公共冷却（1秒）
        public int GlobalActiveCooldownTicks = 60;
        // 被动触发绝学的冷却
        public readonly Dictionary<int, uint> perPassiveNextProcTick = new();
        // —— 蓄力（龟派气功） —— //
        public bool Charging = false;
        public int ChargeQiSpent = 0;           // 本次按住期间消耗的气力点数
        // 虚影射弹实例
        public int KamehamehaGhostProj = -1;
        // 绝学实例
        private Kamehameha kamehameha_juexueItem;
        // === 凌波微步（开关技） ===
        public bool LingboActive = false;
        private Vector2 _lingboLastWavePos;
        private float _lingboDistanceAcc = 0f;
        private float lingboRate = 0.5f;
        public int LingboQiCost = 0;// 每秒消耗气力
        // === 天外飞仙（短时突进） ===
        public int FeixianTicks = 0; // >0 表示进行中
        public const int FeixianTotalTicks = 30; // 0.5s
        public Vector2 FeixianTarget; // ★ 新增：飞仙的目标点

        // === 利刃华尔兹（简化 R） ===
        public int BladeWaltzTicks = 0;        // 整个流程剩余时间（8*54=432）
        public int BladeWaltzStepTimer = 0;    // 当前段计时（54 tick一段）
        public int BladeWaltzHitsLeft = 0;     // 剩余攻击次数
        public int BladeWaltzTarget = -1;      // 当前锁定 npc 索引

        // === 降龙十八掌 计数 ===
        public int XiangLongCount = 0;
        // —— 绝学虚影播放状态 —— //
        public Asset<Texture2D> JueXueTex;
        public struct JuexueGhostState
        {
            public Rectangle Src;       // 从竖直条里裁切的单帧
            public int TimeLeft;        // 剩余tick
            public int Duration;        // 总时长tick
            public float Scale;
            public Vector2 Offset;      // 相对玩家中心的偏移
            public SpriteEffects Fx;

            public bool IsCooldownIcon;
            public uint CooldownEndTick;
        }
        public const int Juexue1TotalFrames = 14; // 14个绝学虚影
        public JuexueGhostState Ghost; // 当前仅允许一个虚影，够用了
        // 乾坤大挪移残影
        // —— 弧线冲刺状态 —— //
        private bool shiftActive;
        private int shiftTimer;       // 已经过的tick
        private int shiftDuration;    // 总时长tick
        private Vector2 bezP0, bezC, bezP1;
        private Vector2 lastPos;      // 上一帧位置（算速度/残影用）
        private int invulnOnEnd = 15; // 短暂无敌（你已有ShortInvulnBuff也可用）

        // 轻量“残影轨迹”缓存（只用来撒尘，不做真正玩家贴图复制）
        private const int TrailCap = 8;
        private readonly Vector2[] trail = new Vector2[TrailCap];
        private int trailCount = 0;
        // —— 可选：暴露轨迹给绘制层（如果你要做真正的“玩家贴图残像”）——
        public bool ShiftActive => shiftActive;
        public int ShiftTrailCount => trailCount;
        public Vector2 GetShiftTrailPos(int i) => trail[i];
        // —— 被动触发冷却（按物品type区分） —— //

        public bool CanProcPassiveNow(int key, int cooldownTicks)
        {
            uint now = Main.GameUpdateCount;
            return !perPassiveNextProcTick.TryGetValue(key, out uint next) || now >= next;
        }

        public void StampPassiveProc(int key, int cooldownTicks)
        {
            perPassiveNextProcTick[key] = Main.GameUpdateCount + (uint)cooldownTicks;
        }
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
        public override void OnEnterWorld()
        {
            // 使用真名时气力的恢复速度加快
            if (Main.worldName == "wudao")
            {
                QiRegenStand = 180;
                QiRegenMove = 120;
                GlobalActiveCooldownTicks = 10;
            }
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

            tag["JinggongUsed"] = JinggongUsed;
            tag["DonggongUsed"] = DonggongUsed;
            tag["QiRegenStandBonus"] = QiRegenStandBonus;
            tag["QiRegenMoveBonus"] = QiRegenMoveBonus;

            // 保存绝学槽
            if (!JuexueSlot.IsAir)
                tag["JuexueSlot"] = ItemIO.Save(JuexueSlot);
        }

        public override void LoadData(TagCompound tag)
        {
            QiMaxFromItems = tag.GetInt("QiMaxFromItems");
            Used_ReiShi = tag.GetInt("Used_ReiShi");
            Used_PassionFruit = tag.GetInt("Used_PassionFruit");

            JinggongUsed = tag.GetInt("JinggongUsed");
            DonggongUsed = tag.GetInt("DonggongUsed");
            QiRegenStandBonus = tag.GetFloat("QiRegenStandBonus");
            QiRegenMoveBonus = tag.GetFloat("QiRegenMoveBonus");

            if (tag.ContainsKey("JuexueSlot"))
            {
                JuexueSlot = ItemIO.Load(tag.GetCompound("JuexueSlot"));
            }
            else
            {
                JuexueSlot = new Item();
                JuexueSlot.TurnToAir();
            }

            // 加载绝学贴图
            JueXueTex = ModContent.Request<Texture2D>("WuDao/Assets/JueXue1", AssetRequestMode.ImmediateLoad);
            // 获取龟派气功绝学实例
            kamehameha_juexueItem = ModContent.GetInstance<Kamehameha>();
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
                float perTick = Helpers.IsPlayerAttackingOrMoving(Player) ? (QiRegenMove + QiRegenMoveBonus) : (QiRegenStand + QiRegenStandBonus);

                QiCurrent = MathHelper.Clamp(QiCurrent + perTick / 60f, 0, QiMax);
            }

            // 蓄力期间（龟派气功）：每帧扣气
            if (Charging)
            {
                if (QiCurrent >= 5f)
                {
                    QiCurrent -= kamehameha_juexueItem.QiCost;
                    ChargeQiSpent++;
                    // ★ 确保龟派气功射弹虚影存在
                    EnsureKamehamehaGhost();
                    // 每消耗 20 点真气，播放短促提示音
                    if (ChargeQiSpent % 20 == 0)
                    {
                        SoundEngine.PlaySound(
                        SoundID.MaxMana with { Volume = 0.6f, Pitch = 0.15f },
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
                        // 若存在虚影射弹实例，也一并删除
                        if (KamehamehaGhostProj >= 0 && Main.projectile[KamehamehaGhostProj].active)
                        {
                            Main.projectile[KamehamehaGhostProj].Kill();
                            KamehamehaGhostProj = -1;
                        }
                        k.ReleaseFire(Player, this, ChargeQiSpent);
                        ChargeQiSpent = 0;
                    }
                }
            }

            // —— 凌波微步：每秒 15 气；无气自动关闭；移动出水波 —— 
            if (LingboActive)
            {
                if (QiCurrent <= LingboQiCost)
                {
                    LingboActive = false;
                }
                else
                {
                    // 扣气
                    QiCurrent -= LingboQiCost / 60f;
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
                // Player.immune = true;
                // Player.immuneTime = 2;
                // Player.invis = true;
                // Player.noKnockback = true;

                // 朝向锁定
                Vector2 toTarget = FeixianTarget - Player.Center;
                Vector2 dir = toTarget.SafeNormalize(Vector2.UnitX);
                Player.direction = (dir.X >= 0f) ? 1 : -1;

                // 推进速度（像素/帧）；到达目标或超时即结束
                float speed = 20f;
                if (toTarget.Length() <= speed || FeixianTicks == 1)
                {
                    Player.velocity = Vector2.Zero;
                    FeixianTicks = 0;
                    // 解冻全场（仅当当前冻结来自飞仙）
                    TimeStopSystem.StopIfFeixian();
                    // ★ 结束帧：立刻清理免疫/隐身
                    Player.immune = false;
                    Player.immuneTime = 0;
                    Player.invis = false;
                    Player.noKnockback = false;
                }
                else
                {
                    // ★ 仅在未结束时维持免疫
                    Player.immune = true;
                    Player.immuneTime = 2;
                    Player.invis = true;
                    Player.noKnockback = true;

                    Player.velocity = dir * speed;
                    int damage = 240;// * Helpers.BossProgressPower.GetUniqueBossCount();

                    // 每 2 帧在当前位置生成极短命友方投射物（路径伤害；放行已在 TimeStopSystem 里处理）
                    if ((FeixianTicks % 2) == 0)
                    {
                        // int projType = ModContent.ProjectileType<FirstFractalCloneProj>();
                        int proj = Projectile.NewProjectile(
                            Player.GetSource_Misc("FeixianTrail"),
                            Player.Center,
                            Player.velocity,
                            ProjectileID.FirstFractal,
                            damage,
                            4f,
                            Player.whoAmI);
                        Main.projectile[proj].timeLeft = 20;
                        Main.projectile[proj].tileCollide = false;
                    }
                    // ★ 在飞行路径四周生成“花瓣环”：每 6 帧一圈，6~8 片
                    if ((FeixianTicks % 6) == 0)
                    {
                        int petals = 6;                 // 每圈花瓣数量
                        float radius = 18f;             // 出生半径（围绕玩家）
                        float forwardSpeed = 8f;       // 沿路径前进分量
                        float outwardMin = 1f;          // 向外发散速度范围
                        float outwardMax = 5f;

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
                            Vector2 vel = dir.RotatedBy(MathHelper.TwoPi / petals * i) * forwardSpeed + outward * Main.rand.NextFloat(outwardMin, outwardMax);

                            var p = Projectile.NewProjectileDirect(
                                Player.GetSource_Misc("FeixianPetals"),
                                spawnPos,
                                vel,
                                ProjectileID.FlowerPetal,        // 原版花瓣
                                damage,
                                3f,
                                Player.whoAmI
                            );
                            if (p != null)
                            {
                                p.friendly = true;
                                p.hostile = false;
                                p.tileCollide = false;           // 防止被地形卡住（看喜好可改为 true）
                                p.timeLeft = 30;                 // 花瓣寿命
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
                    BladeWaltzStepTimer = 30;   // 每段 ~0.5s
                    BladeWaltzHitsLeft--;

                    // 目标：半屏范围随机一个可追踪敌人
                    int targetIndex = FindRandomWaltzTarget();
                    Vector2 targetPos;

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
                    var dir = (targetPos - spawn).SafeNormalize(Vector2.UnitX);
                    float speed = 14f; // 速度越高，视觉越“快斩”
                    int damage = 115;//80 * (1 + Helpers.BossProgressPower.GetUniqueBossCount());
                    float knockback = 3f;

                    if (Main.netMode != NetmodeID.MultiplayerClient) // 只在服务端
                    {
                        // int projType = ModContent.ProjectileType<FirstFractalCloneProj>();

                        int pid = Projectile.NewProjectile(
                            Player.GetSource_Misc("BladeWaltz"),
                            spawn,
                            dir * speed,
                            ProjectileID.FirstFractal,
                            damage,
                            knockback,
                            Player.whoAmI
                        );
                        if (pid >= 0 && pid < Main.maxProjectiles)
                        {
                            Projectile p = Main.projectile[pid];
                            p.tileCollide = false;
                            p.timeLeft = 30;
                            p.usesLocalNPCImmunity = true;
                            p.localNPCHitCooldown = 10;
                            p.netUpdate = true;
                            // p.width = 56;
                            // p.height = 56;
                        }
                    }
                }

                if (BladeWaltzTicks <= 0)
                {
                    // ★ 结束帧：立刻清理免疫/隐身
                    Player.immune = false;
                    Player.immuneTime = 0;
                    Player.invis = false;
                    Player.noKnockback = false;
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
            float radius = Math.Min(Main.screenWidth, Main.screenHeight) * 0.5f;
            // 收集候选
            Span<int> buf = stackalloc int[200];
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                var n = Main.npc[i];
                if (!n.active || n.friendly || !n.CanBeChasedBy()) continue;
                if (Vector2.Distance(n.Center, Player.Center) <= radius)
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
            float radius = Math.Min(Main.screenWidth, Main.screenHeight) * 0.5f;
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
            if (LingboActive && Main.rand.NextFloat() < lingboRate)
            {
                modifiers.Cancel();
                // modifiers.FinalDamage *= 0f;
                // modifiers.Knockback *= 0f;
                // 给无敌帧
                Player.immune = true;        // 开启无敌标记
                Player.immuneTime = 15;      // 0.25 秒
                // 可加一小段水波/闪避提示
                Main.NewText("凌波微步-闪避近战", Color.SkyBlue);
                // 闪避成功消耗2倍气力
                QiCurrent -= LingboQiCost * 2;
                // 气力不足退出状态
                if (QiCurrent <= 0)
                {
                    QiCurrent = 0;
                    LingboActive = false;
                }
            }
        }
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (LingboActive && Main.rand.NextFloat() < lingboRate && proj.hostile)
            {
                modifiers.Cancel();
                // modifiers.FinalDamage *= 0f;
                // modifiers.Knockback *= 0f;
                // 给无敌帧
                Player.immune = true;        // 开启无敌标记
                Player.immuneTime = 15;      // 0.25 秒
                // 可加一小段水波/闪避提示
                Main.NewText("凌波微步-闪避射弹", Color.SkyBlue);
                // 闪避成功消耗2倍气力
                QiCurrent -= LingboQiCost * 2;
                // 气力不足退出状态
                if (QiCurrent <= 0)
                {
                    QiCurrent = 0;
                    LingboActive = false;
                }
            }
        }
        public bool TrySpendQi(int cost)
        {
            if (QiCurrent >= cost)
            {
                QiCurrent -= cost;
                return true;
            }
            return false;
        }

        public bool CanUseActiveNow(int itemType, int extraCooldownTicks)
        {
            // 公共冷却时间
            if (Main.GameUpdateCount < nextGlobalActiveTick) return false;

            // 专属冷却
            if (perSkillNextUseTick.TryGetValue(itemType, out var t) && Main.GameUpdateCount < t) return false;

            return true;
        }

        public void StampActiveUse(int itemType, int extraCooldownTicks)
        {
            nextGlobalActiveTick = Main.GameUpdateCount + (uint)GlobalActiveCooldownTicks;
            perSkillNextUseTick[itemType] = Main.GameUpdateCount + (uint)extraCooldownTicks;
        }
        // 绝学冷却时，禁止更换装备新的绝学
        public bool IsJuexueSlotLockedByActiveCooldown()
        {
            // 只锁主动绝学（被动不需要锁）
            if (JuexueSlot?.ModItem is not JuexueItem ji || !ji.IsActive)
                return false;

            uint now = Main.GameUpdateCount;

            // 公共冷却
            if (now < nextGlobalActiveTick) return true;

            // 专属冷却（按当前槽位里的 item.type 判断）
            if (perSkillNextUseTick.TryGetValue(JuexueSlot.type, out uint t) && now < t)
                return true;

            return false;
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
                    else
                    {
                        Main.NewText("绝学尚未冷却。", Color.OrangeRed);
                    }
                }
                else if (JuexueSlot.ModItem is JuexueItem ji && ji.IsActive)
                {
                    // ★ 华尔兹进行中时，忽略再次按键
                    if (BladeWaltzTicks > 0) return;

                    ji.TryActivate(Player, this); // 尝试主动释放
                }

            }
            if (QiKeybinds.CastSkillKey.JustReleased)
            {
                // 松开：若是龟派气功则结算发射
                if (JuexueSlot.ModItem is Kamehameha k && Charging)
                {
                    Charging = false;
                    // 若存在虚影射弹实例，也一并删除
                    if (KamehamehaGhostProj >= 0 && Main.projectile[KamehamehaGhostProj].active)
                    {
                        Main.projectile[KamehamehaGhostProj].Kill();
                        KamehamehaGhostProj = -1;
                    }
                    k.ReleaseFire(Player, this, ChargeQiSpent);
                    ChargeQiSpent = 0;
                }
            }
        }

        /// <summary>
        /// 触发：在 durationTick 内淡入淡出绘制。
        /// </summary>
        public void TriggerJuexueGhost(int frameIndex, int durationTick = 120, float scale = 1f, Vector2? offset = null, SpriteEffects fx = SpriteEffects.None)
        {
            if (Main.dedServ) return;

            Ghost.Src = VerticalFrame(frameIndex);
            Ghost.Duration = durationTick;
            Ghost.TimeLeft = durationTick;
            Ghost.Scale = scale;
            Ghost.Offset = offset ?? Vector2.Zero;
            Ghost.Fx = fx;
        }
        public void TriggerJuexueCooldownIcon(int frameIndex, int itemType, int cooldownTicks, float scale = 1f, Vector2? offset = null, SpriteEffects fx = SpriteEffects.None)
        {
            if (Main.dedServ) return;

            Ghost.Src = VerticalFrame(frameIndex);
            Ghost.Duration = cooldownTicks;
            Ghost.Scale = scale;
            Ghost.Offset = offset ?? Vector2.Zero;
            Ghost.Fx = fx;

            Ghost.IsCooldownIcon = true;

            Ghost.CooldownEndTick = Main.GameUpdateCount + (uint)cooldownTicks;
            Ghost.Duration = cooldownTicks;
            Ghost.TimeLeft = cooldownTicks;
            Ghost.IsCooldownIcon = true;

            Ghost.TimeLeft = cooldownTicks; // 会在 PostUpdate 里刷新成真实剩余
        }

        public override void PostUpdate()
        {
            if (Ghost.IsCooldownIcon)
            {
                uint now = Main.GameUpdateCount;
                int left = (Ghost.CooldownEndTick > now) ? (int)(Ghost.CooldownEndTick - now) : 0;
                Ghost.TimeLeft = left;

                // 冷却完自动关闭显示
                if (left <= 0)
                    Ghost.IsCooldownIcon = false;
            }
            else
            {
                if (Ghost.TimeLeft > 0)
                    Ghost.TimeLeft--;
            }
        }

        /// <summary>启动乾坤大挪移的“二次贝塞尔弧线冲刺”。</summary>
        public void StartQiankunCurveDash(Vector2 p0, Vector2 c, Vector2 p1, int duration)
        {
            shiftActive = true;
            shiftTimer = 0;
            shiftDuration = duration;
            bezP0 = p0; bezC = c; bezP1 = p1;
            lastPos = Player.Center;

            // 起手：短无敌，防止被打断；禁用一些动作
            Player.immune = true;
            Player.immuneTime = Math.Max(Player.immuneTime, duration + 8);
            Player.noFallDmg = true;
            Player.controlUseItem = false;
            Player.controlJump = false;
            Player.controlLeft = Player.controlRight = Player.controlDown = Player.controlUp = false;

            trailCount = 0;
        }
        public override void PreUpdateMovement()
        {
            if (!shiftActive) return;

            // 归一化时间（缓动）：前半加速，后半减速
            float t = (shiftTimer + 1) / (float)shiftDuration;
            t = Utils.SmoothStep(0f, 1f, t); // t^2*(3-2t)

            // === 椭圆弧线 ===
            // 端点/中点/方向
            Vector2 p0 = bezP0;
            Vector2 p1 = bezP1;
            Vector2 mid = (p0 + p1) * 0.5f;
            Vector2 dir = p1 - p0;
            float L = dir.Length();
            Vector2 unit = dir.SafeNormalize(Vector2.UnitX);
            Vector2 normal = new Vector2(-unit.Y, unit.X);

            // 用 OnActivate 里传进来的 c（bezC）来决定弧高与左右侧：
            // 取 c 相对中点在法线方向上的投影作为弧高（可再做安全夹取）
            float H = Vector2.Dot(bezC - mid, normal);
            H = MathHelper.Clamp(H, -300f, 300f); // 防炸值，按需微调

            // 椭圆参数（半椭圆）：x 从 -L/2 到 L/2，y 从 0 到 H*sin(pi*t)
            float x = 0.5f * L * (2f * t - 1f);
            float y = H * (float)Math.Sin(Math.PI * t);

            // 局部 -> 世界
            Vector2 local = new Vector2(x, y);
            Vector2 pos = mid + local.X * unit + local.Y * normal;

            // 位移与朝向（保持你原先的两段式“速度+位移”）
            Vector2 vel = pos - Player.Center;
            Player.velocity = vel;
            Player.position += vel;

            if (vel.LengthSquared() > 0.001f)
                Player.direction = vel.X >= 0 ? 1 : -1;

            // 记录轨迹点（你的残影需求）
            PushTrail(Player.Center);

            lastPos = Player.Center;
            shiftTimer++;

            // 结束判定（保持原样）
            if (shiftTimer >= shiftDuration)
            {
                shiftActive = false;
                Player.velocity = Vector2.Zero;
                Player.noFallDmg = false;

                SoundEngine.PlaySound(SoundID.Item6, Player.Center);
                for (int i = 0; i < 24; i++)
                {
                    var v = Main.rand.NextVector2Circular(3f, 3f);
                    int d = Dust.NewDust(Player.position, Player.width, Player.height, DustID.Teleporter, v.X, v.Y, 150, default, Main.rand.NextFloat(1.2f, 1.6f));
                    Main.dust[d].noGravity = true;
                }
                Player.AddBuff(ModContent.BuffType<ShortInvulnBuff>(), invulnOnEnd);
            }
        }
        private void SpawnShiftDust(Vector2 at, Vector2 vel)
        {
            if (Main.netMode == NetmodeID.Server) return;

            // 主体尾迹电光
            for (int i = 0; i < 2; i++)
            {
                var d = Dust.NewDustPerfect(at + Main.rand.NextVector2Circular(12, 12), DustID.MagicMirror,
                    -vel * Main.rand.NextFloat(0.05f, 0.2f), 160, default, Main.rand.NextFloat(1.0f, 1.35f));
                d.noGravity = true;
            }
            // 侧向微粒
            if (Main.rand.NextBool(3))
            {
                var d2 = Dust.NewDustPerfect(at, DustID.GemDiamond, Main.rand.NextVector2Circular(1.5f, 1.5f), 100, default, 1.05f);
                d2.noGravity = true;
            }
        }

        private void PushTrail(Vector2 p)
        {
            if (trailCount < TrailCap) trail[trailCount++] = p;
            else
            {
                for (int i = 1; i < TrailCap; i++)
                    trail[i - 1] = trail[i];
                trail[TrailCap - 1] = p;
            }
        }
        public Rectangle VerticalFrame(int frameIndex)
        {
            var tex = JueXueTex.Value;
            int w = tex.Width;
            int h = tex.Height / Juexue1TotalFrames;
            return new Rectangle(0, h * frameIndex, w, h);
        }
        private void EnsureKamehamehaGhost()
        {
            if (Main.dedServ) return;

            int projType = ModContent.ProjectileType<KamehamehaProj>();
            int owner = Player.whoAmI;

            // 是否已有虚影（owner 自己的、ai[1]==1）
            bool exists = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner && p.type == projType && p.ai[1] == 1f)
                {
                    // 保鲜（最重要，不然会过期）
                    p.timeLeft = 2;
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                // 生成一个“虚影”实例：伤害 0、速度 0、ai1=1
                KamehamehaGhostProj = Projectile.NewProjectile(
                    Player.GetSource_Misc("Kamehameha_Ghost"),
                    Player.Center,
                    Vector2.Zero,
                    projType,
                    0, 0f, owner,
                    0f, // ai[0] 不用，实时读 ChargeQiSpent 更丝滑
                    1f  // ai[1] = 1f => 虚影模式
                );
                if (!Main.dedServ)
                {

                    // 触发 2 秒虚影，稍微放大 1.1 倍，向上偏移 16 像素（站位更好看）
                    TriggerJuexueGhost(Kamehameha.KamehamehaFrameIndex, durationTick: 120, scale: 1.1f, offset: new Vector2(0, -20));
                }
            }
        }
    }
}

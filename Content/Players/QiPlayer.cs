using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Juexue.Active;
using Terraria.ID;
using WuDao.Content.Global;
using System;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using WuDao.Content.Buffs;
using WuDao.Content.Projectiles.Melee;
using WuDao.Common;
using Terraria.Localization;
using WuDao.Content.Config;
using WuDao.Content.DamageClasses;
using WuDao.Content.Mounts;
using Terraria.DataStructures;
using System.IO;

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
        // === 御剑术 ===
        public bool YuJianActive = false;
        public int YuJianQiCostPerSecond = 0;

        public int YuJianSwordType = 0;
        public int YuJianSwordDamage = 0;
        public float YuJianSwordKnockback = 0f;
        // 保持初始透明度
        private float initialOpacity = 1f;
        // 防止一帧打很多次：对每个NPC做本地冷却
        private int[] _yuJianNpcHitCooldown;
        // === 月步 ===
        public bool SkyWalkingActive = false;

        // 配置
        public const int SkyWalkingJumpQiCost = 10;
        public const int SkyWalkingStandQiCostPerSecond = 1;

        // 运行时状态
        public bool SkyWalkingStandingOnAir = false;

        // 跳跃输入边沿锁，防止按住空格一帧扣很多次
        private bool _skyWalkingJumpPressedLastFrame = false;
        private bool _skyWalkingJumpConsumed = false;
        // 绝学招式特殊伤害类型
        private ChiEnergyDamageClass chi;
        private SupremeDamageClass sup;
        // —— 被动触发冷却（按物品type区分） —— //
        public bool CanProcPassiveNow(int key)
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

            chi = ModContent.GetInstance<ChiEnergyDamageClass>();
            sup = ModContent.GetInstance<SupremeDamageClass>();
        }
        public override void Initialize()
        {
            base.Initialize();
            _yuJianNpcHitCooldown = new int[Main.maxNPCs];
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
            bool enableSystem = JuexueRuntime.Enabled;
            bool enableQi = enableSystem && (hasJuexueInSlot || hasJuexueInInventory || hasJuexueOnMouse);

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
            // 御剑飞行
            if (YuJianActive)
            {
                Player.noFallDmg = true;
                Player.noKnockback = true;
            }
            // 月步
            if (SkyWalkingActive)
            {
                Player.noFallDmg = true;
            }
        }
        public override void PreUpdate()
        {
            UpdateQiRegen();
            UpdateKamehamehaCharging();
            UpdateLingbo();
            UpdateYuJian();
            UpdateSkyWalking();
            UpdateFeixian();
            UpdateBladeWaltz();
        }
        private void UpdateQiRegen()
        {
            // 气力回复：站立/不动 +4/s，移动或攻击 +2/s, 御剑飞行和月步时不再自动回气
            if (QiMax > 0 && !SkyWalkingActive && !YuJianActive)
            {
                float perTick = Helpers.IsPlayerAttackingOrMoving(Player)
                    ? (QiRegenMove + QiRegenMoveBonus)
                    : (QiRegenStand + QiRegenStandBonus);

                QiCurrent = MathHelper.Clamp(QiCurrent + perTick / 60f, 0, QiMax);
            }
        }
        private void UpdateKamehamehaCharging()
        {
            if (!Charging)
                return;

            if (QiCurrent >= 5f)
            {
                QiCurrent -= kamehameha_juexueItem.QiCost;
                ChargeQiSpent++;

                // 本机玩家维护虚影
                EnsureKamehamehaGhost();

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
                Charging = false;

                if (JuexueSlot.ModItem is Kamehameha k)
                {
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
        private void UpdateLingbo()
        {
            if (!LingboActive)
                return;

            if (QiCurrent <= LingboQiCost)
            {
                LingboActive = false;
                return;
            }

            QiCurrent -= LingboQiCost / 60f;

            float moved = Vector2.Distance(Player.Bottom, _lingboLastWavePos);
            _lingboDistanceAcc += moved;
            _lingboLastWavePos = Player.Bottom;

            if (_lingboDistanceAcc >= 32f)
            {
                _lingboDistanceAcc = 0f;

                for (int i = 0; i < 12; i++)
                {
                    var d = Dust.NewDustPerfect(
                        Player.Bottom + new Vector2(Main.rand.NextFloat(-12, 12), 0),
                        DustID.Water,
                        new Vector2(Main.rand.NextFloat(-1, 1), -Main.rand.NextFloat(0.5f, 1.5f))
                    );
                    d.noGravity = true;
                    d.scale = 1.5f;
                }
            }
        }
        private void UpdateYuJian()
        {
            if (!YuJianActive)
                return;

            if (Player.dead || Player.ghost || !Player.active)
            {
                EndYuJian(true);
                return;
            }

            if (Player.controlMount && Player.releaseMount)
            {
                EndYuJian();
                return;
            }

            if (Player.controlHook && Player.releaseHook)
            {
                EndYuJian();
                return;
            }

            if (Player.mount == null || !Player.mount.Active || Player.mount.Type != ModContent.MountType<YuJianMount>())
            {
                EndYuJian();
                return;
            }

            float perTick = YuJianQiCostPerSecond / 60f;
            if (QiCurrent <= perTick)
            {
                QiCurrent = 0;
                EndYuJian();
                return;
            }

            QiCurrent -= perTick;

            ApplyYuJianControlLock();

            if (Player.controlInv)
                Main.playerInventory = false;

            DoYuJianContactDamage();
        }
        private void UpdateSkyWalking()
        {
            if (!SkyWalkingActive)
                return;

            if (Player.dead || Player.ghost || !Player.active)
            {
                EndSkyWalking();
                return;
            }

            if (Player.mount != null && Player.mount.Active)
            {
                EndSkyWalking();
                return;
            }

            if (Player.controlMount && Player.releaseMount)
            {
                EndSkyWalking();
                return;
            }

            if (Player.controlHook && Player.releaseHook)
            {
                EndSkyWalking();
                return;
            }

            SpawnSkyWalkingDust();

            bool onGround = Player.velocity.Y == 0f && ModContent.GetInstance<BuffPlayer>().IsStandingOnGround(Player);
            bool canStartSkyWalkingJump = onGround || SkyWalkingStandingOnAir;

            if (!canStartSkyWalkingJump)
                _skyWalkingJumpConsumed = false;

            if (Player.controlJump && canStartSkyWalkingJump && !_skyWalkingJumpConsumed)
            {
                TrySkyWalkingJump();
                _skyWalkingJumpConsumed = true;
                return;
            }

            UpdateSkyWalkingAirStand();
        }
        private void SpawnSkyWalkingDust()
        {
            if (!Main.rand.NextBool(3))
                return;

            Vector2 legPos = Player.Bottom + new Vector2(0f, -10f * Player.gravDir);
            legPos.X += Main.rand.NextFloat(-10f, 10f);
            legPos.Y += Main.rand.NextFloat(-4f, 4f);

            int dust = Dust.NewDust(
                legPos,
                2,
                2,
                DustID.Cloud,
                0f,
                0f,
                0,
                default,
                2f
            );

            Main.dust[dust].velocity = new Vector2(
                Main.rand.NextFloat(-1.2f, 1.2f),
                Main.rand.NextFloat(-1.2f, 1.2f)
            );

            Main.dust[dust].noGravity = true;
        }
        private void UpdateFeixian()
        {
            if (FeixianTicks <= 0)
                return;

            Vector2 toTarget = FeixianTarget - Player.Center;
            Vector2 dir = toTarget.SafeNormalize(Vector2.UnitX);
            Player.direction = (dir.X >= 0f) ? 1 : -1;

            float speed = 20f;
            if (toTarget.Length() <= speed || FeixianTicks == 1)
            {
                EndFeixian();
                return;
            }

            ApplyFeixianState(dir, speed);
            SpawnFeixianProjectiles(dir);

            FeixianTicks--;
        }
        private void EndFeixian()
        {
            Player.velocity = Vector2.Zero;
            FeixianTicks = 0;

            TimeStopSystem.StopIfFeixian();

            Player.immune = false;
            Player.immuneTime = 0;
            Player.invis = false;
            Player.noKnockback = false;
        }
        private void ApplyFeixianState(Vector2 dir, float speed)
        {
            Player.immune = true;
            Player.immuneTime = 2;
            Player.invis = true;
            Player.noKnockback = true;
            Player.velocity = dir * speed;
        }
        private void SpawnFeixianProjectiles(Vector2 dir)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            int baseDamage = Feixian.Damage;

            if ((FeixianTicks % 2) == 0)
                SpawnFeixianTrailProjectile(baseDamage);

            if ((FeixianTicks % 6) == 0)
                SpawnFeixianPetals(dir, baseDamage);
        }
        private void SpawnFeixianTrailProjectile(int baseDamage)
        {
            int finalDamage = (int)Player.GetTotalDamage(chi).ApplyTo(baseDamage);

            int proj = Projectile.NewProjectile(
                Player.GetSource_Misc("FeixianTrail"),
                Player.Center,
                Player.velocity,
                ProjectileID.FirstFractal,
                finalDamage,
                4f,
                Player.whoAmI
            );

            Projectile p = Main.projectile[proj];
            p.DamageType = chi;
            p.originalDamage = finalDamage;
            p.timeLeft = 20;
            p.tileCollide = false;
        }
        private void SpawnFeixianPetals(Vector2 dir, int baseDamage)
        {
            int petals = 6;
            float radius = 18f;
            float forwardSpeed = 8f;
            float outwardMin = 1f;
            float outwardMax = 5f;

            Vector2 normal = new Vector2(-dir.Y, dir.X);
            float baseAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);

            int finalDamage = (int)Player.GetTotalDamage(chi).ApplyTo(baseDamage) / petals;

            for (int i = 0; i < petals; i++)
            {
                float ang = baseAngle + i * MathHelper.TwoPi / petals;
                Vector2 ringOffset = (float)Math.Cos(ang) * normal + (float)Math.Sin(ang) * dir;
                Vector2 spawnPos = Player.Center + ringOffset * radius;

                Vector2 outward = (spawnPos - Player.Center).SafeNormalize(Vector2.UnitX);
                Vector2 vel = dir.RotatedBy(MathHelper.TwoPi / petals * i) * forwardSpeed
                              + outward * Main.rand.NextFloat(outwardMin, outwardMax);

                var p = Projectile.NewProjectileDirect(
                    Player.GetSource_Misc("FeixianPetals"),
                    spawnPos,
                    vel,
                    ProjectileID.FlowerPetal,
                    finalDamage,
                    3f,
                    Player.whoAmI
                );

                if (p != null)
                {
                    p.friendly = true;
                    p.hostile = false;
                    p.tileCollide = false;
                    p.timeLeft = 30;
                    p.penetrate = 1;
                    p.usesLocalNPCImmunity = true;
                    p.localNPCHitCooldown = 12;
                    p.DamageType = chi;
                    p.originalDamage = finalDamage;
                }
            }
        }
        private void UpdateBladeWaltz()
        {
            if (BladeWaltzTicks <= 0)
                return;

            BladeWaltzTicks--;
            BladeWaltzStepTimer--;

            ApplyBladeWaltzState();

            if (BladeWaltzStepTimer <= 0 && BladeWaltzHitsLeft > 0)
                PerformBladeWaltzStep();

            if (BladeWaltzTicks <= 0)
                EndBladeWaltz();
        }
        private void ApplyBladeWaltzState()
        {
            Player.immune = true;
            Player.immuneTime = 2;
            Player.immuneNoBlink = true;
            Player.invis = true;

            Player.controlUseItem = false;
            Player.controlUseTile = false;
            Player.controlHook = false;
            Player.controlMount = false;
            Player.controlJump = false;
            Player.controlLeft = Player.controlRight = Player.controlUp = Player.controlDown = false;
            Player.mount?.Dismount(Player);
            Player.velocity = Vector2.Zero;

            Player.AddBuff(BuffID.Invisibility, 2, true);
        }
        private void EndBladeWaltz()
        {
            Player.immune = false;
            Player.immuneTime = 0;
            Player.invis = false;
            Player.noKnockback = false;
        }
        private void PerformBladeWaltzStep()
        {
            BladeWaltzStepTimer = 30;
            BladeWaltzHitsLeft--;

            int targetIndex = FindRandomWaltzTarget();
            Vector2 targetPos = targetIndex >= 0 ? Main.npc[targetIndex].Center : Main.MouseWorld;

            Vector2 spawn = Player.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(60f, 120f);
            Vector2 dir = (targetPos - spawn).SafeNormalize(Vector2.UnitX);
            float speed = 14f;

            int finalDamage = (int)Player.GetTotalDamage(chi).ApplyTo(BladeWaltz.baseDamage);
            float knockback = 3f;

            if (Player.whoAmI != Main.myPlayer)
                return;

            int pid = Projectile.NewProjectile(
                Player.GetSource_Misc("BladeWaltz"),
                spawn,
                dir * speed,
                ProjectileID.FirstFractal,
                finalDamage,
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
                p.DamageType = chi;
                p.originalDamage = finalDamage;
            }
        }
        // 进一步保险：把绘制信息设为隐身，确保任何残余层都不画
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
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
                Main.NewText(Language.GetTextValue("Mods.WuDao.Messages.JueXue.Immune"), Color.SkyBlue);
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
                Main.NewText(Language.GetTextValue("Mods.WuDao.Messages.JueXue.Immune"), Color.SkyBlue);
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
            if (!JuexueRuntime.Enabled)
                return false;

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
            if (!JuexueRuntime.Enabled)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            if (QiKeybinds.CastSkillKey.JustPressed)
            {
                if (JuexueSlot.ModItem is Kamehameha k)
                {
                    if (CanUseActiveNow(JuexueSlot.type, k.SpecialCooldownTicks))
                    {
                        Charging = true;
                        ChargeQiSpent = 0;
                    }
                    else
                    {
                        Main.NewText(Language.GetTextValue("Mods.WuDao.Messages.JueXue.Cooldown"), Color.OrangeRed);
                    }
                }
                else if (JuexueSlot.ModItem is JuexueItem ji && ji.IsActive)
                {
                    if (BladeWaltzTicks > 0)
                        return;

                    ji.TryActivate(Player, this);
                }
            }

            if (QiKeybinds.CastSkillKey.JustReleased)
            {
                if (JuexueSlot.ModItem is Kamehameha k && Charging)
                {
                    Charging = false;

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
            // 绝学冷却图标虚影
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
            if (Player.whoAmI != Main.myPlayer)
                return;

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
            if (Main.dedServ)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            int projType = ModContent.ProjectileType<KamehamehaProj>();
            int owner = Player.whoAmI;

            bool exists = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == owner && p.type == projType && p.ai[1] == 1f)
                {
                    p.timeLeft = 2;
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                KamehamehaGhostProj = Projectile.NewProjectile(
                    Player.GetSource_Misc("Kamehameha_Ghost"),
                    Player.Center,
                    Vector2.Zero,
                    projType,
                    0, 0f, owner,
                    0f,
                    1f
                );

                TriggerJuexueGhost(Kamehameha.KamehamehaFrameIndex, durationTick: 120, scale: 1.1f, offset: new Vector2(0, -20));
            }
        }
        // 御剑飞行
        public bool TryFindSwordFromBackpack(out Item sword)
        {
            // 非快捷栏：默认 10..57（0..9 是热键栏）
            for (int i = 10; i < 58; i++)
            {
                Item it = Player.inventory[i];
                if (ItemSets.IsYuJianSword(it))
                {
                    sword = it;
                    return true;
                }
            }
            sword = default;
            return false;
        }
        public void BeginYuJian(int swordType, int swordDamage, float swordKnockback, int qiCostPerSecond)
        {
            YuJianActive = true;

            YuJianSwordType = swordType;
            YuJianSwordDamage = swordDamage;
            YuJianSwordKnockback = swordKnockback;
            YuJianQiCostPerSecond = qiCostPerSecond;

            // 清理本地命中CD
            for (int i = 0; i < _yuJianNpcHitCooldown.Length; i++)
                _yuJianNpcHitCooldown[i] = 0;

            // 立刻关背包，避免“进入御剑还在拖拽物品”造成异常
            if (Main.playerInventory)
                Main.playerInventory = false;

            // 如果玩家正在抓钩/坐椅/睡觉/轨道车等，这里建议强制打断
            Player.RemoveAllGrapplingHooks();
            Player.pulley = false;
            Player.sleeping.isSleeping = false;
            Player.sitting.isSitting = false;

            // 保持初始透明度
            initialOpacity = Player.opacityForAnimation;
            Player.opacityForAnimation = 0.8f;
        }

        public void EndYuJian(bool fromDeath = false)
        {
            YuJianActive = false;
            YuJianQiCostPerSecond = 0;
            YuJianSwordType = 0;
            YuJianSwordDamage = 0;
            YuJianSwordKnockback = 0f;

            Player.velocity *= 0.35f;
            // 清接触伤害CD
            if (_yuJianNpcHitCooldown != null)
                Array.Clear(_yuJianNpcHitCooldown, 0, _yuJianNpcHitCooldown.Length);

            // 若还有钩爪，也建议收掉，避免“刚退出就被钩爪拽飞”
            Player.RemoveAllGrapplingHooks();

            // 只卸掉御剑坐骑，避免别的异常状态被误伤
            if (Player.mount != null && Player.mount.Active &&
                Player.mount.Type == ModContent.MountType<YuJianMount>())
            {
                Player.mount.Dismount(Player);
            }
            // 恢复玩家透明度
            Player.opacityForAnimation = initialOpacity;
        }
        private void ApplyYuJianControlLock()
        {
            // 禁止使用物品/攻击/放置/挥动
            Player.controlUseItem = false;
            Player.controlUseTile = false;
            Player.controlThrow = false;
            Player.controlHook = false;

            // 禁止快捷键药水等（按你需求可增删）
            Player.controlQuickHeal = false;
            Player.controlQuickMana = false;
            Player.controlTorch = false;

            // 最关键：彻底禁止“用物品”
            Player.noItems = true;

            // 禁止鼠标与UI交互（降低“背包拖拽/装备坐骑物品”等bug概率）
            Player.mouseInterface = true;

            // 你如果不想禁跳也可以保留 controlJump
            Player.controlJump = false;
        }

        private void DoYuJianContactDamage()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            for (int i = 0; i < _yuJianNpcHitCooldown.Length; i++)
                if (_yuJianNpcHitCooldown[i] > 0) _yuJianNpcHitCooldown[i]--;

            Rectangle hitbox = Player.Hitbox;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || n.friendly || n.dontTakeDamage || n.lifeMax <= 5)
                    continue;

                if (_yuJianNpcHitCooldown[i] > 0)
                    continue;

                if (!hitbox.Intersects(n.Hitbox))
                    continue;

                int dmg = YuJianSwordDamage;
                float kb = YuJianSwordKnockback;

                Player.ApplyDamageToNPC(n, dmg, kb, Player.direction, crit: false);
                _yuJianNpcHitCooldown[i] = 10;
            }
        }
        public override void UpdateDead()
        {
            if (YuJianActive)
                EndYuJian(true); // true = 死亡清理模式，可少做一些播放效果之类
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            if (YuJianActive)
                EndYuJian(true);
        }

        public override void OnRespawn()
        {
            if (YuJianActive)
                EndYuJian(true);
        }
        // 月步状态同步
        public override void CopyClientState(ModPlayer targetCopy)
        {
            QiPlayer clone = (QiPlayer)targetCopy;
            clone.SkyWalkingActive = SkyWalkingActive;
            clone.SkyWalkingStandingOnAir = SkyWalkingStandingOnAir;
            clone.QiCurrent = QiCurrent;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            QiPlayer old = (QiPlayer)clientPlayer;

            if (old.SkyWalkingActive != SkyWalkingActive ||
                old.SkyWalkingStandingOnAir != SkyWalkingStandingOnAir)
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncSkyWalkingState);
            packet.Write((byte)Player.whoAmI);
            packet.Write(SkyWalkingActive);
            packet.Write(SkyWalkingStandingOnAir);
            packet.Write(QiCurrent);
            packet.Send(toWho, fromWho);
        }
        // 月步
        public void BeginSkyWalking()
        {
            SkyWalkingActive = true;
            SkyWalkingStandingOnAir = false;
            _skyWalkingJumpPressedLastFrame = false;

            // 释放月步时松开钩爪
            Player.RemoveAllGrapplingHooks();

            if (Player.whoAmI == Main.myPlayer)
                SyncPlayer(-1, Main.myPlayer, false);
        }

        public void EndSkyWalking()
        {
            SkyWalkingActive = false;
            SkyWalkingStandingOnAir = false;
            _skyWalkingJumpPressedLastFrame = false;
            _skyWalkingJumpConsumed = false;

            if (Player.whoAmI == Main.myPlayer)
                SyncPlayer(-1, Main.myPlayer, false);
        }
        private void TrySkyWalkingJump()
        {
            if (QiCurrent < SkyWalkingJumpQiCost)
            {
                EndSkyWalking();
                return;
            }

            QiCurrent -= SkyWalkingJumpQiCost;
            SkyWalkingStandingOnAir = false;

            // 1. 起跳速度：吃原版 jumpSpeedBoost
            float jumpVelocity = Player.jumpSpeed + Player.jumpSpeedBoost;

            // 你如果想让月步本身比普通跳跃更强，可以额外加一点基础值
            jumpVelocity += 2.5f;

            Player.velocity.Y = -jumpVelocity * Player.gravDir;

            // 2. 跳跃持续时间：吃原版跳跃增强
            int jumpFrames = Player.jumpHeight;

            if (Player.jumpBoost)
                jumpFrames += 5;

            if (Player.frogLegJumpBoost)
                jumpFrames += 2;

            Player.jump = jumpFrames;

            // 3. 刷新摔落起点
            Player.fallStart = (int)(Player.position.Y / 16f);

            // 4. 标记本帧刚跳过，避免和别的逻辑打架
            Player.justJumped = true;

            if (Player.whoAmI == Main.myPlayer)
                SyncPlayer(-1, Main.myPlayer, false);
        }
        private void UpdateSkyWalkingAirStand()
        {
            // 判定是否站在地面上
            bool onGround = Player.velocity.Y == 0f && ModContent.GetInstance<BuffPlayer>().IsStandingOnGround(Player);

            bool rising = Player.velocity.Y * Player.gravDir < 0f; // 正在向“上”运动
            bool wantsToDrop = Player.controlDown;

            if (onGround)
            {
                SkyWalkingStandingOnAir = false;
                return;
            }

            // 空中、且不再上升、且不想主动下落 -> 踏空站立
            if (!rising && !wantsToDrop)
            {
                float costPerTick = SkyWalkingStandQiCostPerSecond / 60f;

                if (QiCurrent <= costPerTick)
                {
                    QiCurrent = 0;
                    EndSkyWalking();
                    return;
                }

                QiCurrent -= costPerTick;

                SkyWalkingStandingOnAir = true;

                // 关键：冻结下落
                Player.gravity = 0f;
                Player.maxFallSpeed = 0f;
                Player.velocity.Y = 0f;

                // 刷新 fallStart，避免退出月步后瞬间结算高额摔落伤害
                Player.fallStart = (int)(Player.position.Y / 16f);
            }
            else
            {
                SkyWalkingStandingOnAir = false;
            }
        }
        // 服务器同步相关代码
        public void RequestSyncJuexueSlot()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.SyncJuexueSlot);
            packet.Write((byte)Player.whoAmI);
            WriteSimpleItem(packet, JuexueSlot);
            packet.Send();
        }

        public static void WriteSimpleItem(ModPacket packet, Item item)
        {
            bool hasItem = item != null && !item.IsAir;
            packet.Write(hasItem);

            if (!hasItem)
                return;

            packet.Write(item.type);
            packet.Write(item.stack);
            packet.Write(item.prefix);
            packet.Write(item.favorited);
        }

        public static Item ReadSimpleItem(BinaryReader reader)
        {
            bool hasItem = reader.ReadBoolean();

            Item item = new Item();
            item.TurnToAir();

            if (!hasItem)
                return item;

            int type = reader.ReadInt32();
            int stack = reader.ReadInt32();
            byte prefix = reader.ReadByte();
            bool favorited = reader.ReadBoolean();

            item.SetDefaults(type);
            item.stack = stack;
            item.favorited = favorited;

            if (prefix > 0)
                item.Prefix(prefix);

            return item;
        }
    }
}

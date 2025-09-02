using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Systems
{
    // 静止怀表 冻结时间的辅助类
    public static class TimeStopSystem
    {
        public static bool IsFrozen = false;
        public static int Timer = 0;

        public static void StartFreeze(int duration = 300)
        {
            IsFrozen = true;
            Timer = duration;
        }

        public static void Update()
        {
            if (IsFrozen)
            {
                Timer--;
                if (Timer <= 0)
                {
                    IsFrozen = false;
                    Timer = 0;
                }
            }
        }
    }
    public class TimeStopPlayer : ModPlayer
    {
        public override bool CanUseItem(Item item)
        {
            if (TimeStopSystem.IsFrozen)
            {
                // 禁止近战、远程、魔法、召唤攻击
                if (item.damage > 0 || item.shoot > ProjectileID.None)
                    return false;
            }
            return true;
        }

        public override void PreUpdate()
        {
            TimeStopSystem.Update(); // 每帧刷新冻结计时器
        }
    }
    public class TimeStopNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // —— 冻结期的“钉住值”缓存 —— //
        private bool cacheValid;
        private int cachedDir;
        private int cachedSpriteDir;
        private float cachedRot;
        private Vector2 cachedPos; // 可选：钉住位置，避免微动
        private Rectangle cachedFrame; // 可选：钉住帧

        private void Capture(NPC npc)
        {
            if (cacheValid) return;
            cachedDir = npc.direction;
            cachedSpriteDir = npc.spriteDirection;
            cachedRot = npc.rotation;
            cachedPos = npc.position;
            cachedFrame = npc.frame;
            cacheValid = true;
        }

        private void Apply(NPC npc)
        {
            npc.position = cachedPos;
            npc.velocity = Vector2.Zero;

            npc.direction = cachedDir;
            npc.spriteDirection = cachedSpriteDir;
            npc.rotation = cachedRot;

            // 如果想连动画都暂停，就把帧也钉住（多数 Boss 的“转头”也会顺便被消掉）
            npc.frame = cachedFrame;

            // 禁掉贴身伤害（避免靠近时掉血）
            npc.damage = 0; // 可选，如果不想影响数值就改用 CanHitPlayer 钩子（你已经加过）
        }

        private void ClearCache()
        {
            cacheValid = false;
        }

        public override bool PreAI(NPC npc)
        {
            if (!TimeStopSystem.IsFrozen)
            {
                // 解冻：清缓存
                if (cacheValid) ClearCache();
                return true;
            }

            // 冻结：第一次冻结帧记录一下
            Capture(npc);

            // 停住（AI 不跑，最干净）
            Apply(npc);
            npc.netUpdate = false; // 不要每帧强更，避免抖动/带宽
            return false; // 阻断 AI
        }

        public override void PostAI(NPC npc)
        {
            if (TimeStopSystem.IsFrozen)
            {
                // 双保险：就算有别的地方改了，也把数值写回
                Apply(npc);
            }
        }

        // 关键：很多 Boss 在 FindFrame 里改朝向/旋转/帧，这里把它拦下来
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (!TimeStopSystem.IsFrozen) return;

            // 确保缓存存在
            Capture(npc);

            // 强行还原，禁止任何基于玩家位置的转向逻辑生效
            npc.direction = cachedDir;
            npc.spriteDirection = cachedSpriteDir;
            npc.rotation = cachedRot;
            npc.frame = cachedFrame;

            // 同时把帧计数冻结，避免自动走帧
            npc.frameCounter = 0d;

            // 特判（可选）：克苏鲁之眼（以及血肉之墙之眼）类会在 FindFrame 中做旋转，这里再钉一遍
            if (npc.type == NPCID.EyeofCthulhu || npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism)
            {
                npc.direction = cachedDir;
                npc.spriteDirection = cachedSpriteDir;
                npc.rotation = cachedRot;
            }
        }

        // 避免冻结时贴身伤害（如果你不想改 npc.damage）
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot) => !TimeStopSystem.IsFrozen;
    }
    public class TimeStopProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        // —— 冻结期的“钉住值”缓存 —— //
        private bool cacheValid;
        private Vector2 cachedVel;
        private float cachedRot;
        private int? cachedExtraUpdates;

        private void Capture(Projectile p)
        {
            if (cacheValid) return;
            cachedVel = p.velocity;
            cachedRot = p.rotation;
            
            cachedExtraUpdates = p.extraUpdates; // 少数弹幕依赖它
            cacheValid = true;
        }

        private void ClearCache()
        {
            cacheValid = false;
            cachedExtraUpdates = null;
        }

        // 冻结：阻断原生 AI，避免它在冻结中“自行播音/改状态”
        public override bool PreAI(Projectile p)
        {
            if (!TimeStopSystem.IsFrozen)
            {
                // 解冻：如果之前冻结过，恢复一次关键状态
                if (cacheValid)
                {
                    // 只恢复一次速度（多数情况已足够）
                    p.velocity = cachedVel;
                    if (cachedExtraUpdates.HasValue) p.extraUpdates = cachedExtraUpdates.Value;
                    // 其余（friendly/hostile/rotation）通常会在后续原生AI中自然更新，无需强行改回
                    ClearCache();
                }
                return true; // 允许正常 AI
            }

            // 冻结：第一次进入冻结时，抓备份
            Capture(p);

            // 维持原位置/朝向，但速度为0（画面“悬停”）
            p.velocity = Vector2.Zero;
            p.rotation = cachedRot;

            // 让寿命不减少（否则冻结时会提前消失）
            p.timeLeft++;

            // 如果你之前为“静音”做过声音 Hook，这里无需处理；否则可在全局 Hook 里拦
            // 关键：跳过本帧 AI，防止 AI 内部继续改状态/播音
            return false;
        }

        public override void PostAI(Projectile p)
        {
            // 冻结状态下，双保险再钉一次速度
            if (TimeStopSystem.IsFrozen && cacheValid)
            {
                p.velocity = Vector2.Zero;
                p.rotation = cachedRot;
            }
        }

        // 冻结时彻底禁用命中（玩家与NPC都不应被已存在弹幕打到）
        public override bool CanHitPlayer(Projectile p, Player t)
            => !TimeStopSystem.IsFrozen;
        public override bool? CanHitNPC(Projectile projectile, NPC target)
        {
            return !TimeStopSystem.IsFrozen;
        }

        public override bool? CanDamage(Projectile p)
            => TimeStopSystem.IsFrozen ? false : null;
    }

}

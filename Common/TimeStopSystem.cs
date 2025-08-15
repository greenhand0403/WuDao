using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace WuDao.Common
{
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
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
            => !TimeStopSystem.IsFrozen;
    }
    public class TimeStopProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public override void AI(Projectile projectile)
        {
            if (TimeStopSystem.IsFrozen)
            {
                // 玩家自己的投射物依然存在，但不移动
                projectile.velocity = Vector2.Zero;
                projectile.ai[0] = projectile.ai[1] = 0;
                projectile.timeLeft++; // 防止自然消失
            }
        }
    }

}

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

        public override void AI(NPC npc)
        {
            if (TimeStopSystem.IsFrozen)
            {
                npc.velocity = Vector2.Zero;
                npc.ai[0] = npc.ai[1] = npc.ai[2] = npc.ai[3] = 0;
                npc.netUpdate = true;
            }
        }

        public override void PostAI(NPC npc)
        {
            if (TimeStopSystem.IsFrozen)
            {
                npc.position = npc.oldPosition; // 防止抖动
            }
        }
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

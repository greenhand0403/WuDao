using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace WuDao.Content.Global
{
    public enum FreezeScope { None, Global, Feixian }
    // 静止游鱼和天外飞仙 冻结时间的辅助类
    public static class TimeStopSystem
    {
        public static bool IsFrozen = false;
        public static int Timer = 0;
        // ★ 新增：全局冷却
        public static int CooldownTimer = 0;         // 冷却计时器（帧）
        private static int _pendingCooldown = 0;     // 本次冻结结束后要应用的冷却（帧）
        public static bool IsOnCooldown => CooldownTimer > 0;
        // ★ 新增：冻结作用域 & 允许放行的玩家（飞仙施法者）
        public static FreezeScope Scope = FreezeScope.None;
        public static int AllowedPlayer = -1;
        // ★ 新增：仅用于天外飞仙的“定向冻结”，放行某位玩家的友方弹幕
        public static void StartFeixianFreeze(int playerWhoAmI, int duration)
        {
            IsFrozen = true;
            Timer = duration;
            Scope = FreezeScope.Feixian;
            AllowedPlayer = playerWhoAmI;
        }
        // ★ 新增：推荐用这个来启动冻结（含冷却），返回是否成功
        public static bool TryStartFreeze(int duration = 300, int cooldown = 900, FreezeScope scope = FreezeScope.Global, int allowedPlayer = -1)
        {
            if (IsFrozen || IsOnCooldown)
                return false;

            IsFrozen = true;
            Timer = duration;
            Scope = scope;
            AllowedPlayer = allowedPlayer;

            _pendingCooldown = cooldown; // 冻结结束后再开始走冷却
            return true;
        }
        // ★ 新增：如果当前是飞仙冻结，就结束它（避免误停在全局冻结状态）
        public static void StopIfFeixian()
        {
            if (Scope == FreezeScope.Feixian)
            {
                IsFrozen = false;
                Timer = 0;
                Scope = FreezeScope.None;
                AllowedPlayer = -1;
            }
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
                    Scope = FreezeScope.None;
                    AllowedPlayer = -1;
                }
            }
            // ★ 新增：冷却递减
            if (CooldownTimer > 0)
                CooldownTimer--;
        }
    }

    public class TimeStopGlobalProjectile : GlobalProjectile
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
                if (cacheValid)
                {
                    p.velocity = cachedVel;
                    if (cachedExtraUpdates.HasValue) p.extraUpdates = cachedExtraUpdates.Value;
                    ClearCache();
                }
                return true;
            }

            // ★ 飞仙定向冻结：放行施法者的友方弹幕
            if (TimeStopSystem.Scope == FreezeScope.Feixian
                && p.owner == TimeStopSystem.AllowedPlayer
                && p.friendly && !p.hostile)
            {
                return true; // 不冻结本人的友方弹幕
            }

            Capture(p);
            p.velocity = Vector2.Zero;
            p.rotation = cachedRot;
            p.timeLeft++;
            return false; // 冻结其余弹幕
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
            if (!TimeStopSystem.IsFrozen) return null;

            // ★ 飞仙定向冻结：仅放行施法者的友方弹幕命中
            if (TimeStopSystem.Scope == FreezeScope.Feixian
                && projectile.owner == TimeStopSystem.AllowedPlayer
                && projectile.friendly && !projectile.hostile)
                return true;

            return false;
        }

        public override bool? CanDamage(Projectile p)
        {
            // ★ 冻结期默认禁伤，但放行“飞仙施法者的友方弹幕”
            if (!TimeStopSystem.IsFrozen) return null;
            if (TimeStopSystem.Scope == FreezeScope.Feixian
                && p.owner == TimeStopSystem.AllowedPlayer
                && p.friendly && !p.hostile)
                return true;
            return false;
        }

    }
}
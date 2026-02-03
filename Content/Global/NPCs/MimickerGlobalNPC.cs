using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Global.Projectiles;
using WuDao.Content.Players;
using Microsoft.Xna.Framework;
using WuDao.Content.Systems;

namespace WuDao.Content.Global.NPCs
{
    // 模仿者：击杀敌怪获取它的射弹
    // 记录“最后一次受到来自模仿者弹体的伤害”的NPC
    public class MimickerGlobalNPC : GlobalNPC
    {
        public bool lastHitByMimicker;
        public int lastHitterPlayer = -1;
        public override bool InstancePerEntity => true;
        public int mimickerHitTimer; // 以tick计时，>0 表示最近被模仿者击中过

        // public override void ResetEffects(NPC npc)
        // {
        //     lastHitByMimicker = false; // 每tick重置，由 OnHitByProjectile 再标记
        //     lastHitterPlayer = -1;
        // }
        public override void AI(NPC npc)
        {
            if (mimickerHitTimer > 0) mimickerHitTimer--;
        }

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.GetGlobalProjectile<MimickerGlobalProjectile>().fromMimicker)
            {
                mimickerHitTimer = 60; // 最近1秒内算模仿者命中过
                lastHitterPlayer = projectile.owner; // 由子弹的 owner 获取玩家 whoAmI
            }
        }

        public override void OnKill(NPC npc)
        {
            if (mimickerHitTimer <= 0 || lastHitterPlayer < 0)
                return;
            // 查是否在解锁表里
            if (!MimickerSystem.UnlockByNPC.TryGetValue(npc.type, out var def))
                return;

            Player p = Main.player[lastHitterPlayer];
            if (p == null || !p.active) return;

            var mp = p.GetModPlayer<MimickerPlayer>();
            mp.killProgress.TryGetValue(def.NpcType, out int cur);
            cur++;
            mp.killProgress[def.NpcType] = cur;

            if (cur >= def.Required && !mp.unlockedProjectiles.Contains(def.ProjectileType))
            {
                mp.unlockedProjectiles.Add(def.ProjectileType);

                if (p.whoAmI == Main.myPlayer)
                {
                    CombatText.NewText(p.getRect(), new Color(255, 220, 100), $"解锁：{def.DisplayName} 射弹！");
                    Main.NewText($"[模仿者] 你已解锁 {def.DisplayName} 射弹。", 255, 240, 150);
                }
            }
            else if (p.whoAmI == Main.myPlayer)
            {
                int remain = System.Math.Max(0, def.Required - cur);
                CombatText.NewText(p.getRect(), new Color(180, 220, 255), $"{def.DisplayName} 解锁还需 {remain}");
            }
        }
    }
}
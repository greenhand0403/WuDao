using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using WuDao.Content.Buffs;
using WuDao.Content.Items;

namespace WuDao.Content.Players
{
    // 永生之酒与药水之灵
    public class PotionPlayer : ModPlayer
    {
        /// <summary>饮用永生之酒造成的总生命上限惩罚（每次 -20）。</summary>
        public int maxLifePenalty;

        /// <summary>是否装备了药水之灵。</summary>
        public bool hasPotionSpirit;

        public override void ResetEffects()
        {
            hasPotionSpirit = false;
        }

        public override void UpdateEquips()
        {
            // 这里不需要处理酒的冷却，冷却用 Buff 表示
        }

        public override void PostUpdateMiscEffects()
        {
            // 将上限惩罚应用到 statLifeMax2（不低于 20，避免 0/负数）
            // if (maxLifePenalty > 0)
            // {
            //     Player.statLifeMax2 -= maxLifePenalty;
            //     if (Player.statLifeMax2 < 20)
            //         Player.statLifeMax2 = 20;

            //     // 若当前生命超过新的上限，收敛到上限（避免显示溢出）
            //     if (Player.statLife > Player.statLifeMax2)
            //         Player.statLife = Player.statLifeMax2;
            // }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["maxLifePenalty"] = maxLifePenalty;
        }

        public override void LoadData(TagCompound tag)
        {
            maxLifePenalty = tag.GetInt("maxLifePenalty");
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            ((PotionPlayer)targetCopy).maxLifePenalty = maxLifePenalty;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            // 简单起见，这里不做细粒度同步，依赖 Save/Load + CopyClientState
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket p = Mod.GetPacket();
            p.Write((byte)MessageType.SyncLifePenalty);
            p.Write((byte)Player.whoAmI);
            p.Write(maxLifePenalty);
            p.Send(toWho, fromWho);
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            // 1) 如果装备了“药水之灵”，并且没有药水病，则尝试按背包顺序自动喝一个能恢复生命的药水/蘑菇
            if (hasPotionSpirit && !Player.HasBuff(Terraria.ID.BuffID.PotionSickness))
            {
                TryAutoHealByInventoryOrder();

                // 如果成功把生命抬到 > 0（不再是死亡态），直接取消死亡
                if (!Player.dead && Player.statLife > 0)
                    return false;
            }

            // 2) 若仍将死亡，检查永生之酒冷却是否就绪；就绪则自动饮用 → 增加上限惩罚/赋予无敌/上冷却，并取消这次死亡
            if (!Player.HasBuff(ModContent.BuffType<WineCooldownBuff>()) && ConsumeEverlastingWine(applyEffects: true))
            {
                // 成功饮用永生之酒后，确保角色不是死亡状态
                Player.dead = false;

                // （可选）给点提示
                Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item3, Player.Center);

                return false; // 阻止死亡
            }

            // 3) 以上都没救回来，就按原流程死亡
            return true;
        }

        private void TryAutoHealByInventoryOrder()
        {
            // 需要治疗冷却就绪（没有药水病）
            if (Player.HasBuff(BuffID.PotionSickness))
                return;

            // 从 0 到 57（主背包 + 热键栏），按照实际排序扫描
            for (int i = 0; i < 58; i++)
            {
                ref Item item = ref Player.inventory[i];
                if (item == null || item.IsAir)
                    continue;

                // 认定“能治疗”的道具：healLife > 0（兼容蘑菇等）且可用
                if (item.healLife > 0 && ItemLoader.CanUseItem(item, Player))
                {
                    int heal = item.healLife;

                    // 施加治疗效果
                    int before = Player.statLife;
                    Player.statLife += heal;
                    if (Player.statLife > Player.statLifeMax2)
                        Player.statLife = Player.statLifeMax2;

                    // 药水病（仅对 potion 标记的物品或 Vanilla 习惯做法；大多数治疗药水 item.potion == true）
                    if (item.potion)
                    {
                        // Vanilla 默认 60 秒，你也可以改
                        Player.AddBuff(BuffID.PotionSickness, 60 * 60);
                    }

                    // 展示治疗飘字
                    int actual = Player.statLife - before;
                    if (actual > 0)
                        Player.HealEffect(actual, true);

                    // 消耗物品
                    item.stack--;
                    if (item.stack <= 0)
                        item.TurnToAir();

                    // 只喝一个
                    break;
                }
            }
        }

        // private bool CanAutoDrinkEverlastingWine()
        // {
        //     // 需要背包中存在酒，且没有酒的冷却 Buff
        //     if (Player.HasBuff(ModContent.BuffType<WineCooldownBuff>()))
        //         return false;

        //     // 查找是否有酒
        //     int wineType = ModContent.ItemType<EverlastingWine>();
        //     for (int i = 0; i < 58; i++)
        //     {
        //         if (Player.inventory[i].type == wineType && Player.inventory[i].stack > 0)
        //             return true;
        //     }
        //     return false;
        // }

        /// <summary>
        /// 消耗一瓶永生之酒，并根据需要应用效果。
        /// </summary>
        private bool ConsumeEverlastingWine(bool applyEffects)
        {
            int wineType = ModContent.ItemType<EverlastingWine>();
            for (int i = 0; i < 58; i++)
            {
                ref Item item = ref Player.inventory[i];
                if (item.type == wineType && item.stack > 0)
                {
                    item.stack--;
                    if (item.stack <= 0)
                        item.TurnToAir();

                    if (applyEffects)
                    {
                        // 生命上限惩罚 +20
                        // maxLifePenalty += 20;
                        // 永久减少 20 点生命上限：通过减少“已吃生命水晶数量”实现
                        // 这样生命水晶的可用性判定也会跟着变，后续可以再吃水晶恢复
                        // 如果生命果食用大于4个，则扣除生命果可食用次数，否则扣除生命水晶食用次数
                        if (Player.ConsumedLifeFruit >= 4)
                        {
                            Player.ConsumedLifeFruit = Utils.Clamp(Player.ConsumedLifeFruit - 4, 0, Player.LifeFruitMax);
                        }
                        else
                        {
                            Player.ConsumedLifeCrystals = Utils.Clamp(Player.ConsumedLifeCrystals - 1, 0, Player.LifeCrystalMax);
                        }

                        // 保险：如果当前血量超过新上限，收敛一下
                        if (Player.statLife > Player.statLifeMax2)
                            Player.statLife = Player.statLifeMax2;

                        // 冷却 Buff：5 分钟
                        Player.AddBuff(ModContent.BuffType<WineCooldownBuff>(), 60 * 300);

                        // 50% 概率给予 2 秒无敌
                        if (Main.rand.NextBool(2))
                        {
                            Player.SetImmuneTimeForAllTypes(120); // 120 tick = 2s
                            Player.immune = true;
                            Player.immuneTime = 120;
                        }

                        // 提示效果（可选）
                        CombatText.NewText(Player.Hitbox, Microsoft.Xna.Framework.Color.MediumPurple, "永生之酒！");
                    }

                    return true;
                }
            }
            return false;
        }
    }
}
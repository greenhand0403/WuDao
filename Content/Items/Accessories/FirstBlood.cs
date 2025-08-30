using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
// TODO: 加载外部资源用 load 其他地方要改
namespace WuDao.Content.Items.Accessories
{
    /*
        装备后，对未击败的原版 Boss增加 10% 伤害。
        一旦击败 Boss，就移除对应加成。
        如果所有 Boss 都被击败 → 饰品名称变为【勇者之证】，效果改为 对生命值 >90% 的 NPC 首次攻击 +300% 伤害。
    */
    public class FirstBlood : ModItem
    {
        // 备用贴图（勇者之证）
        private static Asset<Texture2D> _heroTex;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                // _heroTex = ModContent.Request<Texture2D>($"Terraria/Images/Item_{ItemID.HolyWater}");
                // _heroTex = TextureAssets.Item[ItemID.HolyWater]; // 现成的 Vanilla 贴图
                _heroTex = ModContent.Request<Texture2D>($"{nameof(WuDao)}/Content/Items/Accessories/FirstBlood_Hero");
            }
        }

        public override void Unload()
        {
            _heroTex = null;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(gold: 3);
        }

        // 名称动态切换（放在物品在背包就会调用的逻辑里）
        public override void UpdateInventory(Player player)
        {
            if (AllVanillaBossesDowned())
                Item.SetNameOverride("勇者之证");     // 切换名称
            else
                Item.SetNameOverride(null);            // 恢复原名（使用 DisplayName）
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FirstBloodPlayer>().hasFirstBlood = true;
        }

        // 提示文本动态切换
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (AllVanillaBossesDowned())
            {
                // 已完成 → 展示勇者之证说明
                tooltips.Add(new TooltipLine(Mod, "HeroNameInfo", "（已觉醒为：勇者之证）"));
                tooltips.Add(new TooltipLine(Mod, "HeroEffect", "对生命值 >90% 的敌怪的首次攻击 +300% 伤害"));
                return;
            }

            // 尚未全部击败 → 展示“未击败 BOSS 列表”
            var remaining = GetRemainingVanillaBossNames();
            if (remaining.Count > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "FB_Header", "尚未击败的原版BOSS："));

                // 每行放 4 个名字，防止一行太长
                const int perLine = 4;
                for (int i = 0; i < remaining.Count; i += perLine)
                {
                    int take = Math.Min(perLine, remaining.Count - i);
                    string line = string.Join("、", remaining.GetRange(i, take));
                    tooltips.Add(new TooltipLine(Mod, $"FB_List_{i}", line));
                }

                tooltips.Add(new TooltipLine(Mod, "FB_Effect", "装备时：对以上BOSS伤害 +10%"));
            }
        }

        // 背包图标动态绘制
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame,
            Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Main.dedServ) return true;

            if (AllVanillaBossesDowned() && _heroTex?.IsLoaded == true)
            {
                // 用勇者之证贴图替代默认绘制
                spriteBatch.Draw(_heroTex.Value, position, _heroTex.Value.Bounds, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true; // 正常绘制原贴图
        }

        // 地面图标动态绘制
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            if (Main.dedServ) return true; // 服务器不绘制

            if (AllVanillaBossesDowned() && _heroTex?.IsLoaded == true)
            {
                Vector2 pos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height - _heroTex.Height() * 0.5f);
                spriteBatch.Draw(_heroTex.Value, pos, null, alphaColor, rotation, _heroTex.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }

        // 判断是否已击败所有原版 Boss（可按你需求增减）
        public static bool AllVanillaBossesDowned()
        {
            return NPC.downedSlimeKing
                && NPC.downedBoss1      // 眼
                && NPC.downedBoss2      // 世/脑
                && NPC.downedBoss3      // 地牢骷髅
                && NPC.downedQueenBee
                && NPC.downedDeerclops
                && Main.hardMode        // 肉山
                && NPC.downedMechBossAny  // 机械三王至少一个，若想要求三王全清可改成：downedMechBoss1 && downedMechBoss2 && downedMechBoss3
                && NPC.downedPlantBoss
                && NPC.downedGolemBoss
                && NPC.downedFishron
                && NPC.downedQueenSlime
                && NPC.downedAncientCultist
                && NPC.downedMoonlord;
        }
        private static List<string> GetRemainingVanillaBossNames()
        {
            var list = new List<string>();

            // 这里与你效果判定同一套“原版BOSS集合”，并用 downed 标志判断
            // 你可以与 IsBossDefeated/AllVanillaBossesDowned 的逻辑保持一致
            void AddIfNotDowned(int npcType, bool defeatedFlag)
            {
                if (!defeatedFlag)
                    list.Add(Lang.GetNPCNameValue(npcType));
            }

            AddIfNotDowned(NPCID.KingSlime, NPC.downedSlimeKing);
            AddIfNotDowned(NPCID.EyeofCthulhu, NPC.downedBoss1);
            // 世/脑两选一算通过，这里仅在都没打时显示两者
            if (!NPC.downedBoss2)
            {
                list.Add(Lang.GetNPCNameValue(NPCID.EaterofWorldsHead));
                list.Add(Lang.GetNPCNameValue(NPCID.BrainofCthulhu));
            }
            AddIfNotDowned(NPCID.SkeletronHead, NPC.downedBoss3);
            AddIfNotDowned(NPCID.QueenBee, NPC.downedQueenBee);
            AddIfNotDowned(NPCID.Deerclops, NPC.downedDeerclops);
            AddIfNotDowned(NPCID.WallofFlesh, Main.hardMode);

            // 机械三王：如果 downedMechBossAny 但不是“三王全清”，显示具体未击败者
            bool mechAny = NPC.downedMechBossAny;
            bool mech1 = NPC.downedMechBoss1; // 双子魔眼
            bool mech2 = NPC.downedMechBoss2; // 骷髅王Prime
            bool mech3 = NPC.downedMechBoss3; // 毁灭者
            if (!(mech1 && mech2 && mech3))
            {
                if (!mech1)
                {
                    list.Add(Lang.GetNPCNameValue(NPCID.Retinazer));
                    list.Add(Lang.GetNPCNameValue(NPCID.Spazmatism));
                }
                if (!mech2) list.Add(Lang.GetNPCNameValue(NPCID.SkeletronPrime));
                if (!mech3) list.Add(Lang.GetNPCNameValue(NPCID.TheDestroyer));
            }

            AddIfNotDowned(NPCID.Plantera, NPC.downedPlantBoss);
            AddIfNotDowned(NPCID.Golem, NPC.downedGolemBoss);
            AddIfNotDowned(NPCID.DukeFishron, NPC.downedFishron);
            AddIfNotDowned(NPCID.QueenSlimeBoss, NPC.downedQueenSlime);
            AddIfNotDowned(NPCID.CultistBoss, NPC.downedAncientCultist);
            AddIfNotDowned(NPCID.MoonLordCore, NPC.downedMoonlord);

            return list;
        }
    }

    public class FirstBloodPlayer : ModPlayer
    {
        public bool hasFirstBlood;

        public override void ResetEffects()
        {
            hasFirstBlood = false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            ApplyBonus(target, ref modifiers);
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            ApplyBonus(target, ref modifiers);
        }

        private void ApplyBonus(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!hasFirstBlood) return;

            if (FirstBlood.AllVanillaBossesDowned())
            {
                // 勇者之证：对 >90% 生命值的敌怪“首次攻击”+300%
                // “首次攻击”可用本地标记实现，这里示例为：目标满血时触发
                if (target.life >= (int)(target.lifeMax * 0.9f) && target.life == target.lifeMax)
                    modifiers.FinalDamage *= 4f; // +300%
            }
            else
            {
                // 第一滴血：对尚未击败的原版 Boss +10%
                if (IsTrackedVanillaBoss(target.type) && !IsBossDefeated(target.type))
                    modifiers.FinalDamage *= 1.1f;
            }
        }

        private static bool IsTrackedVanillaBoss(int type)
        {
            return type == NPCID.KingSlime
                || type == NPCID.EyeofCthulhu
                || type == NPCID.EaterofWorldsHead || type == NPCID.BrainofCthulhu
                || type == NPCID.QueenBee
                || type == NPCID.SkeletronHead
                || type == NPCID.Deerclops
                || type == NPCID.WallofFlesh
                || type == NPCID.TheDestroyer || type == NPCID.SkeletronPrime || type == NPCID.Retinazer || type == NPCID.Spazmatism
                || type == NPCID.Plantera
                || type == NPCID.Golem
                || type == NPCID.DukeFishron
                || type == NPCID.QueenSlimeBoss
                || type == NPCID.CultistBoss
                || type == NPCID.MoonLordCore;
        }

        private static bool IsBossDefeated(int type)
        {
            return type switch
            {
                NPCID.KingSlime => NPC.downedSlimeKing,
                NPCID.EyeofCthulhu => NPC.downedBoss1,
                NPCID.EaterofWorldsHead => NPC.downedBoss2,
                NPCID.BrainofCthulhu => NPC.downedBoss2,
                NPCID.QueenBee => NPC.downedQueenBee,
                NPCID.SkeletronHead => NPC.downedBoss3,
                NPCID.Deerclops => NPC.downedDeerclops,
                NPCID.WallofFlesh => Main.hardMode,
                NPCID.TheDestroyer => NPC.downedMechBoss3 || NPC.downedMechBossAny,
                NPCID.SkeletronPrime => NPC.downedMechBoss2 || NPC.downedMechBossAny,
                NPCID.Retinazer => NPC.downedMechBoss1 || NPC.downedMechBossAny,
                NPCID.Spazmatism => NPC.downedMechBoss1 || NPC.downedMechBossAny,
                NPCID.Plantera => NPC.downedPlantBoss,
                NPCID.Golem => NPC.downedGolemBoss,
                NPCID.DukeFishron => NPC.downedFishron,
                NPCID.QueenSlimeBoss => NPC.downedQueenSlime,
                NPCID.CultistBoss => NPC.downedAncientCultist,
                NPCID.MoonLordCore => NPC.downedMoonlord,
                _ => false
            };
        }
    }
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Accessories
{
    /*
        装备后，对未击败的原版 Boss增加 10% 伤害。
        一旦击败 Boss，就移除对应加成。
        如果所有 Boss 都被击败 → 饰品名称变为【勇者之证】，效果改为 对生命值 >90% 的 NPC 首次攻击 +300% 伤害。
    */
    public class FirstBlood : ModItem
    {
        private static Asset<Texture2D> TexAsset;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                TexAsset = ModContent.Request<Texture2D>($"{nameof(WuDao)}/Content/Items/Accessories/FirstBlood_Hero", AssetRequestMode.AsyncLoad);
            }
        }
        public override void Unload()
        {
            TexAsset = null;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 3);
        }

        // 名称动态切换（放在物品在背包就会调用的逻辑里）
        public override void UpdateInventory(Player player)
        {
            if (Helpers.AllVanillaBossesDowned())
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
            if (Helpers.AllVanillaBossesDowned())
            {
                // 已完成 → 展示勇者之证说明
                tooltips.Add(new TooltipLine(Mod, "HeroNameInfo", "（已觉醒为：勇者之证）"));
                tooltips.Add(new TooltipLine(Mod, "HeroEffect", "对生命值 >90% 的敌怪的首次攻击 +300% 伤害"));
                return;
            }

            // 尚未全部击败 → 展示“未击败 BOSS 列表”
            var remaining = Helpers.GetRemainingVanillaBossNames();
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

            if (Helpers.AllVanillaBossesDowned() && TexAsset?.IsLoaded == true)
            {
                // 用勇者之证贴图替代默认绘制
                spriteBatch.Draw(TexAsset.Value, position, TexAsset.Value.Bounds, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true; // 正常绘制原贴图
        }

        // 地面图标动态绘制
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor,
            ref float rotation, ref float scale, int whoAmI)
        {
            if (Main.dedServ) return true; // 服务器不绘制

            if (Helpers.AllVanillaBossesDowned() && TexAsset?.IsLoaded == true)
            {
                Vector2 pos = Item.position - Main.screenPosition + new Vector2(Item.width / 2f, Item.height - TexAsset.Height() * 0.5f);
                spriteBatch.Draw(TexAsset.Value, pos, null, alphaColor, rotation, TexAsset.Size() * 0.5f, scale, SpriteEffects.None, 0f);
                return false;
            }
            return true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.LifeCrystal, 5)
                .AddIngredient(ItemID.BloodMoonStarter, 1)
                .AddCondition(Condition.BloodMoon)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
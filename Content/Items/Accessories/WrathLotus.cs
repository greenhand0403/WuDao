using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using WuDao.Common;
using WuDao.Content.Projectiles.Magic;
using System.Collections.Generic;

namespace WuDao.Content.Items.Accessories
{
    /// <summary>
    /// 佛怒火莲，受伤时放出火莲烙印造成伤害
    /// </summary>
    public class WrathLotus : BuffItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(gold: 5);
            Item.defense = 2;
        }
        protected override void BuildStatRules(Player player, Item item, IList<StatRule> rules)
        {
            // 免疫着火了、燃烧、岩浆
            rules.Add(new StatRule(BuffConditions.Always,
                StatEffect.ImmuneTo(BuffID.OnFire, BuffID.Burning),
                StatEffect.LavaImmune()
            ));
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.ObsidianRose)
                .AddIngredient(ItemID.StarVeil)
                .AddIngredient(ItemID.SoulofLight, 5)
                .AddIngredient(ItemID.SoulofNight, 5)
                .AddIngredient(ItemID.HellstoneBar, 5)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace WuDao.Content.Systems
{
    public class ButterflyCaneRecipes : ModSystem
    {
        // tr内置蝴蝶配方组，不需要再手动写任意蝴蝶 参考 Terraria\Recipe.cs
        /*
        public static RecipeGroup ButterflyCaneRecipeGroup;

        public override void Unload()
        {
            ButterflyCaneRecipeGroup = null;
        }

        public override void AddRecipeGroups()
        {
            ButterflyCaneRecipeGroup = new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.MonarchButterfly)}",
            ItemID.ZebraSwallowtailButterfly, 
            ItemID.PurpleEmperorButterfly, // 紫帝蝶
            ItemID.RedAdmiralButterfly,    // 红纹蝶
            ItemID.UlyssesButterfly,       // 尤利西斯蝶
            ItemID.SulphurButterfly,       // 硫蝶
            ItemID.TreeNymphButterfly,     // 树精蝶
            ItemID.JuliaButterfly,         // 朱莉娅蝶
            ItemID.MonarchButterfly );

            RecipeGroup.RegisterGroup("WuDao:Butterfly", ButterflyCaneRecipeGroup);
        }
        */
    }
}
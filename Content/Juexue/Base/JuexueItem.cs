using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Config;
using WuDao.Content.Players;

namespace WuDao.Content.Juexue.Base
{
    public abstract class JuexueItem : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Book}";
        public virtual bool IsActive => true;         // 主动 or 被动
        public virtual int QiCost => 0;               // 主动技能消耗
        public virtual int SpecialCooldownTicks => 0; // 各自绝学的释放冷却时间（单位tick）
        public override bool CanRightClick() => true;

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 5);
            Item.maxStack = 1;
        }

        public virtual bool TryActivate(Player player, QiPlayer qi)
        {
            if (!JuexueRuntime.Enabled)
                return false;

            if (!IsActive)
                return false;

            if (qi.QiMax <= 0)
                return false;

            if (!qi.CanUseActiveNow(Item.type, SpecialCooldownTicks))
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText(Language.GetTextValue("Mods.WuDao.Messages.JueXue.Cooldown"), Color.OrangeRed);
                return false;
            }

            if (!qi.TrySpendQi(QiCost))
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText(Language.GetTextValue("Mods.WuDao.Messages.JueXue.NotEnoughQi"), Color.OrangeRed);
                return false;
            }

            bool ok = OnActivate(player, qi);
            if (ok)
                qi.StampActiveUse(Item.type, SpecialCooldownTicks);

            return ok;
        }

        protected virtual bool OnActivate(Player player, QiPlayer qi) => false;

        public virtual bool IsPassive => !IsActive;
    }
}
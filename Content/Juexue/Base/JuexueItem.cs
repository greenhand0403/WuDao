using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Players;

namespace WuDao.Content.Juexue.Base
{
    public abstract class JuexueItem : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Book}";
        public virtual bool IsActive => true;         // 主动 or 被动
        public virtual int QiCost => 0;               // 主动技能消耗
        public virtual int SpecialCooldownTicks => 0; // 各自较长冷却（单位tick）
        public override bool CanRightClick() => true;
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.buyPrice(0, 5);
            Item.maxStack = 1;
        }

        // 主动释放：默认检查气力、公冷却、专属冷却
        public virtual bool TryActivate(Player player, QiPlayer qi)
        {
            if (!IsActive) return false;
            if (qi.QiMax <= 0) return false;

            if (!qi.CanUseActiveNow(Item.type, SpecialCooldownTicks))
            {
                Main.NewText("绝学尚未冷却。", Microsoft.Xna.Framework.Color.OrangeRed);
                return false;
            }
            if (!qi.TrySpendQi(QiCost))
            {
                Main.NewText("气力不足！", Microsoft.Xna.Framework.Color.OrangeRed);
                return false;
            }

            bool ok = OnActivate(player, qi);
            if (ok) qi.StampActiveUse(Item.type, SpecialCooldownTicks);
            return ok;
        }

        // 由具体主动类实现
        protected virtual bool OnActivate(Player player, QiPlayer qi) => false;

        // 被动类在 GlobalItem.Shoot 中调用各自的 TryPassive...
        public virtual bool IsPassive => !IsActive;
    }
}

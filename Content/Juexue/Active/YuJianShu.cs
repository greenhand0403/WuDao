using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Juexue.Base;
using WuDao.Content.Players;
using WuDao.Content.Systems;
using WuDao.Content.Mounts;

namespace WuDao.Content.Juexue.Active
{
    public class YuJianShu : JuexueItem
    {
        public override bool IsActive => true;

        // 启动消耗（可为 0）；持续消耗交给 QiPlayer 每秒扣
        public override int QiCost => 1;

        // 开关技：一般不需要专属冷却
        public override int SpecialCooldownTicks => 30;

        public const int QiCostPerSecond = 5; // 御剑期间每秒扣气

        public override bool TryActivate(Player player, QiPlayer qi)
        {
            if (player.whoAmI != Main.myPlayer)
                return false;

            // 御剑中：再次按键 -> 关闭（不进CD，不扣气）
            if (qi.YuJianActive)
            {
                qi.EndYuJian();
                // Main.NewText(DisplayName, Color.LightGray);
                return true;
            }

            // 先找“非快捷栏”的第一把剑
            if (!qi.TryFindSwordFromBackpack(out var sword))
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText(Language.GetTextValue("Mods.WuDao.Messages.JueXue.NoSword"), Color.OrangeRed);
                return false;
            }

            // 若在骑乘，先下坐骑
            if (player.mount != null && player.mount.Active)
                player.mount.Dismount(player);

            // 玩家处于正常重力时才能使用御剑飞行
            if (player.gravDir != 1f)
            {
                return false;
            }

            // 启动消耗检查
            if (!qi.TrySpendQi(QiCost))
            {
                if (player.whoAmI == Main.myPlayer)
                    Main.NewText(Language.GetTextValue("Mods.WuDao.Messages.JueXue.NotEnoughQi"), Color.OrangeRed);
                return false;
            }

            // 进入御剑
            qi.BeginYuJian(sword.type, sword.damage, sword.knockBack, QiCostPerSecond);
            player.mount.SetMount(ModContent.MountType<YuJianMount>(), player);

            Main.NewText(DisplayName, Color.SkyBlue);
            return true;
        }
    }
}
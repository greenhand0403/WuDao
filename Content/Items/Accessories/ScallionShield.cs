using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Global;            // CuisineGlobalItem / CuisineCollections
using WuDao.Content.Players;           // CuisinePlayer（提供 Deliciousness）
using static WuDao.Content.Global.CuisineGlobalItem;

namespace WuDao.Content.Items.Accessories
{
    /// <summary>
    /// 葱盾
    /// </summary>
    // [AutoloadEquip(EquipType.Shield)]
    public class ScallionShield : ModItem
    {
        // —— 可调参数 —— //
        public const int BaseDefense = 2;            // 基础防御（额外防御由 CuisineGlobalItem 负责）
        public const int BaseDashCooldown = 50;      // 冲刺冷却（帧）
        public const int BaseDashDuration = 26;      // 基础冲刺时长（帧）
        public const float BaseDashVelocity = 9.5f;  // 基础冲刺速度

        // 把“美味值”折算成冲刺时长倍率： duration *= (1 + Clamp(Delicious * k, 0..MaxExtraMultiplier))
        // 这里直接复用 CuisineGlobalItem 的 PerDeliciousPointToBonus 与 MaxExtraMultiplier
        public static int GetDurationWithDelicious(Player player)
        {
            var cp = player.GetModPlayer<CuisinePlayer>();
            float extra = MathHelper.Clamp(cp.Deliciousness * PerDeliciousPointToBonus, 0f, MaxExtraMultiplier);
            return (int)(BaseDashDuration * (1f + extra));
        }

        public override void SetStaticDefaults()
        {
            // 注册为“美食”，这样能自动享受你在 GlobalItem 里写的“美味值→饰品防御”加成
            CuisineCollections.AddGourmet(ModContent.ItemType<ScallionShield>());
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<LightBlueRarity>();
            Item.value = Item.sellPrice(gold: 5);

            Item.defense = BaseDefense; // 额外防御由 CuisineGlobalItem.UpdateAccessory
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 标记“装备了葱盾”，供 ModPlayer 使用
            player.GetModPlayer<ScallionDashPlayer>().ScallionShieldEquipped = true;

            // 也可以给一点点通用减伤或风味特效（可选）
            // player.endurance = 1f - (0.05f * (1f - player.endurance));

            // 显示冲刺影像（处于冲刺时由 ModPlayer 控制），此处不强制
        }
    }
}

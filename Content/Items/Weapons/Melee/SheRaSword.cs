using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using WuDao.Content.Buffs;
using WuDao.Content.Players;

namespace WuDao.Content.Items.Weapons.Melee
{
    public class SheRaSword : ModItem
    {
        public const int MinTransformTime = 60 * 5;
        public const int MaxTransformTime = 60 * 15;
        public const int CooldownTime = 60 * 60;

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"WuDao/Content/Items/Armor/SheRaSword_{EquipType.Head}", EquipType.Head, this);
            EquipLoader.AddEquipTexture(Mod, $"WuDao/Content/Items/Armor/SheRaSword_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"WuDao/Content/Items/Armor/SheRaSword_{EquipType.Legs}", EquipType.Legs, this);
        }

        public override void SetStaticDefaults()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            int head = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int body = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int legs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

            ArmorIDs.Head.Sets.DrawHead[head] = true;
            ArmorIDs.Body.Sets.HidesTopSkin[body] = true;
            ArmorIDs.Body.Sets.HidesArms[body] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[legs] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 52;

            Item.damage = 50;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6f;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.Pink;
            Item.value = Item.buyPrice(gold: 10);

            Item.noMelee = false;
            Item.noUseGraphic = false;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            // 每次先恢复成默认左键状态，避免状态残留
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = false;

            if (player.altFunctionUse == 2)
            {
                var modPlayer = player.GetModPlayer<SheRaSwordPlayer>();
                if (!modPlayer.CanTransform)
                {
                    if (player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server)
                    {
                        int seconds = modPlayer.TransformCooldown / 60;
                        Main.NewText(
                            Language.GetTextValue("Mods.WuDao.Items.SheRaSword.Cooldown", seconds),
                            255, 180, 60
                        );
                    }

                    return false;
                }

                Item.useStyle = ItemUseStyleID.HoldUp;
                Item.noMelee = true;
            }

            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse != 2)
                return base.UseItem(player);

            var modPlayer = player.GetModPlayer<SheRaSwordPlayer>();

            if (!modPlayer.CanTransform)
                return false;

            // 多人时只允许“本地拥有者”发起这次变身，避免客户端/服务器各自随机一遍
            if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI != Main.myPlayer)
                return true;

            int duration = Main.rand.Next(MinTransformTime, MaxTransformTime + 1);

            modPlayer.StartTransformation(duration, Name, CooldownTime);
            player.AddBuff(ModContent.BuffType<SheRaTransformBuff>(), 2);

            // 纯表现只放客户端
            if (player.whoAmI == Main.myPlayer && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.1f }, player.Center);

                for (int i = 0; i < 30; i++)
                {
                    Dust dust = Dust.NewDustDirect(
                        player.position,
                        player.width,
                        player.height,
                        DustID.GoldFlame
                    );
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1f, 2f);
                    dust.velocity *= Main.rand.NextFloat(24f, 30f);
                }
            }

            return true;
        }
    }
}
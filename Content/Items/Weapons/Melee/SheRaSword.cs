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
        // 你可以自己改成固定 10 秒，或者保留 5~15 秒随机
        public const int MinTransformTime = 60 * 5;   // 5秒
        public const int MaxTransformTime = 60 * 15;  // 15秒
        public const int CooldownTime = 60 * 60; // 1分钟 = 3600 tick
        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            // 手动注册“变身时绘制”的三件套贴图
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

            // 这些设置和 ExampleCostume 的思路一致：
            ArmorIDs.Head.Sets.DrawHead[head] = true;

            // 身体贴图完全覆盖上身皮肤/手臂
            ArmorIDs.Body.Sets.HidesTopSkin[body] = true;
            ArmorIDs.Body.Sets.HidesArms[body] = true;

            // 腿部贴图完全覆盖下身皮肤
            ArmorIDs.Legs.Sets.HidesBottomSkin[legs] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 52;

            // 左键：普通近战剑
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
            if (player.altFunctionUse == 2)
            {
                var modPlayer = player.GetModPlayer<SheRaSwordPlayer>();
                if (!modPlayer.CanTransform)
                {
                    int seconds = modPlayer.TransformCooldown / 60;
                    Main.NewText(Language.GetTextValue("Mods.WuDao.Items.SheRaSword.Cooldown", seconds), 255, 180, 60);
                    return false;
                }
                else
                {
                    Item.useStyle = ItemUseStyleID.HoldUp;
                    // Item.damage = 0;
                    Item.noMelee = true;
                }
            }
            else
            {
                // Item.damage = 50;
                Item.useStyle = ItemUseStyleID.Swing;
                Item.noMelee = false;
            }

            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                var modPlayer = player.GetModPlayer<SheRaSwordPlayer>();

                if (!modPlayer.CanTransform)
                {
                    return false;
                }

                int duration = Main.rand.Next(MinTransformTime, MaxTransformTime + 1);

                modPlayer.StartTransformation(duration, Name, CooldownTime);

                player.AddBuff(ModContent.BuffType<SheRaTransformBuff>(), 2);

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

                return true;
            }

            return base.UseItem(player);
        }
    }
}
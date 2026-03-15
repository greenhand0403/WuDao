using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using WuDao.Content.Projectiles.Summon;

namespace WuDao.Content.Items.Weapons.Summon
{
    public class DandelionStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
            ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
            // 占 1 个哨兵栏
            // ItemID.Sets.StaffMinionSlotsRequired[Type] = 1f;
        }
        public override void SetDefaults()
        {
            Item.damage = 16;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 10;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;

            Item.knockBack = 1f;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item44;

            Item.shoot = ModContent.ProjectileType<DandelionSentry>();
            Item.shootSpeed = 0f;

            Item.sentry = true;
            Item.staff[Item.type] = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity,
            int type, int damage, float knockback)
        {
            if (!TryFindDandelionSpawn(Main.MouseWorld, out Vector2 spawnPos))
                return false;

            Projectile.NewProjectile(
                source,
                spawnPos + new Vector2(0, 5),// 对齐地板
                Vector2.Zero,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            player.UpdateMaxTurrets();
            return false;
        }

        private bool TryFindDandelionSpawn(Vector2 mouseWorld, out Vector2 spawnPos)
        {
            spawnPos = Vector2.Zero;

            int centerX = (int)(mouseWorld.X / 16f);
            int startY = (int)(mouseWorld.Y / 16f);

            int halfScreenTiles = Main.screenWidth / 32; // 半屏横向范围
            int maxDownTiles = 30; // 从鼠标往下搜更稳一些

            for (int offset = 0; offset <= halfScreenTiles; offset++)
            {
                if (TryScanColumn(centerX + offset, startY, maxDownTiles, out spawnPos))
                    return true;

                if (offset > 0 && TryScanColumn(centerX - offset, startY, maxDownTiles, out spawnPos))
                    return true;
            }

            return false;
        }

        private bool TryScanColumn(int leftTileX, int startY, int maxDownTiles, out Vector2 spawnPos)
        {
            spawnPos = Vector2.Zero;

            // leftTileX 作为 2 格宽区域的左边界
            for (int y = startY; y <= startY + maxDownTiles; y++)
            {
                if (!WorldGen.InWorld(leftTileX, y, 10) || !WorldGen.InWorld(leftTileX + 1, y, 10))
                    continue;

                // 需要一个 2x3 的可站立空间：
                // [x,x+1] * [y-2,y] 为空
                // [x,x+1] * [y+1] 为地面支撑
                if (!HasEnoughEmptySpace(leftTileX, y))
                    continue;

                if (!HasSolidGround(leftTileX, y + 1))
                    continue;

                spawnPos = new Vector2(leftTileX * 16 + 16f, y * 16 - 8f);
                return true;
            }

            return false;
        }

        private bool HasEnoughEmptySpace(int leftX, int floorY)
        {
            for (int x = leftX; x <= leftX + 1; x++)
            {
                for (int y = floorY - 2; y <= floorY; y++)
                {
                    if (!WorldGen.InWorld(x, y, 10))
                        return false;

                    Tile tile = Main.tile[x, y];
                    if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                        return false;
                }
            }

            return true;
        }

        private bool HasSolidGround(int leftX, int groundY)
        {
            for (int x = leftX; x <= leftX + 1; x++)
            {
                if (!WorldGen.InWorld(x, groundY, 10))
                    return false;

                Tile tile = Main.tile[x, groundY];
                if (tile == null || !tile.HasTile)
                    return false;

                if (!Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                    return false;
            }

            return true;
        }
    }
}
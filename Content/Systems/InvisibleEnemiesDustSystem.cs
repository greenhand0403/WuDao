using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WuDao.Common;
using WuDao.Content.Config;
using WuDao.Content.Global.Projectiles;

namespace WuDao.Content.Systems
{
    public struct HiddenDustZone
    {
        public Rectangle Area;

        public HiddenDustZone(Rectangle area)
        {
            Area = area;
        }

        public bool Contains(Vector2 position)
        {
            return Area.Contains(position.ToPoint());
        }
    }
    public class InvisibleEnemiesDustSystem : ModSystem
    {
        internal static HiddenDustZone[] HiddenNPCDustZones;
        internal static int HiddenNPCDustZoneCount;

        internal static HiddenDustZone[] HiddenProjectileImpactZones;
        internal static int HiddenProjectileImpactZoneCount;
        internal static HiddenDustZone[] HiddenProjectileDustZones;
        internal static int HiddenProjectileDustZoneCount;

        public override void Load()
        {
            HiddenNPCDustZones = new HiddenDustZone[Main.maxNPCs * 16];
            HiddenProjectileDustZones = new HiddenDustZone[Main.maxProjectiles];
            HiddenProjectileImpactZones = new HiddenDustZone[Main.maxProjectiles * 2];
        }

        public override void Unload()
        {
            HiddenNPCDustZones = null;
            HiddenProjectileImpactZones = null;
            HiddenNPCDustZoneCount = 0;
            HiddenProjectileImpactZoneCount = 0;
            HiddenProjectileDustZones = null;
            HiddenProjectileDustZoneCount = 0;
        }
        private static bool NeedHideDustForLocalPlayer()
        {
            if (Main.gameMenu || Main.dedServ)
                return false;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
                return false;

            // 玩家可见隐藏单位时，不处理
            return !InvisibleEnemies.CanSeeEcho(player);
        }
        public override void PreUpdateNPCs()
        {
            HiddenNPCDustZoneCount = 0;
        }
        public override void PreUpdateProjectiles()
        {
            var flags = InvisibleEnemiesGlobalProjectile.HiddenHostileProjectilesThisFrame;
            if (flags != null)
            {
                for (int i = 0; i < InvisibleEnemiesGlobalProjectile.HiddenHostileProjectileCount; i++)
                {
                    int projIndex = InvisibleEnemiesGlobalProjectile.HiddenHostileProjectileIndices[i];
                    flags[projIndex] = false;
                }

                InvisibleEnemiesGlobalProjectile.HiddenHostileProjectileCount = 0;
            }

            // 这里只清和 projectile 相关的数据
            HiddenProjectileImpactZoneCount = 0;
            HiddenProjectileDustZoneCount = 0;
        }
        internal static void AddHiddenProjectileZone(Rectangle area)
        {
            if (HiddenProjectileDustZones == null || HiddenProjectileDustZoneCount >= HiddenProjectileDustZones.Length)
                return;

            HiddenProjectileDustZones[HiddenProjectileDustZoneCount++] = new HiddenDustZone(area);
        }
        internal static void AddHiddenNPCZone(Rectangle area)
        {
            if (HiddenNPCDustZones == null || HiddenNPCDustZoneCount >= HiddenNPCDustZones.Length)
                return;

            HiddenNPCDustZones[HiddenNPCDustZoneCount++] = new HiddenDustZone(area);
        }

        internal static void AddHiddenProjectileImpactZone(Rectangle area)
        {
            if (HiddenProjectileImpactZones == null || HiddenProjectileImpactZoneCount >= HiddenProjectileImpactZones.Length)
                return;

            HiddenProjectileImpactZones[HiddenProjectileImpactZoneCount++] = new HiddenDustZone(area);
        }

        public override void PostUpdateDusts()
        {
            if (!NeedHideDustForLocalPlayer())
                return;

            int hiddenProjCount = InvisibleEnemiesGlobalProjectile.HiddenHostileProjectileCount;

            if (hiddenProjCount <= 0 &&
                HiddenProjectileDustZoneCount <= 0 &&
                HiddenNPCDustZoneCount <= 0 &&
                HiddenProjectileImpactZoneCount <= 0)
                return;

            for (int i = 0; i < Main.maxDust; i++)
            {
                Dust dust = Main.dust[i];
                if (!dust.active)
                    continue;

                Vector2 dustPos = dust.position;

                if (dust.customData is Projectile ownerProj)
                {
                    int ownerIndex = ownerProj.whoAmI;
                    if (ownerProj.active &&
                        ownerIndex >= 0 &&
                        ownerIndex < Main.maxProjectiles &&
                        InvisibleEnemiesGlobalProjectile.HiddenHostileProjectilesThisFrame[ownerIndex])
                    {
                        dust.active = false;
                        continue;
                    }
                }

                bool hidden = false;

                for (int j = 0; j < HiddenProjectileDustZoneCount; j++)
                {
                    if (HiddenProjectileDustZones[j].Contains(dustPos))
                    {
                        hidden = true;
                        break;
                    }
                }

                if (!hidden)
                {
                    for (int j = 0; j < HiddenNPCDustZoneCount; j++)
                    {
                        if (HiddenNPCDustZones[j].Contains(dustPos))
                        {
                            hidden = true;
                            break;
                        }
                    }
                }

                if (!hidden)
                {
                    for (int j = 0; j < HiddenProjectileImpactZoneCount; j++)
                    {
                        if (HiddenProjectileImpactZones[j].Contains(dustPos))
                        {
                            hidden = true;
                            break;
                        }
                    }
                }

                if (hidden)
                    dust.active = false;
            }
        }
    }
}
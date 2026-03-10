using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Global.NPCs;

namespace WuDao.Content.Systems
{
    public class InvisibleEnemiesDrawHookSystem : ModSystem
    {
        private delegate void Orig_DrawNPCDirect(
            Main self,
            SpriteBatch mySpriteBatch,
            NPC rCurrentNPC,
            bool behindTiles,
            Vector2 screenPos
        );

        private static Hook drawNpcDirectHook;

        public override void Load()
        {
            MethodInfo drawNpcDirect = typeof(Main).GetMethod(
                "DrawNPCDirect",
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                types: new[]
                {
                    typeof(SpriteBatch),
                    typeof(NPC),
                    typeof(bool),
                    typeof(Vector2)
                },
                modifiers: null
            );

            if (drawNpcDirect == null)
            {
                Mod.Logger.Warn("Failed to find Terraria.Main.DrawNPCDirect");
                return;
            }

            drawNpcDirectHook = new Hook(drawNpcDirect, DrawNPCDirect_Detour);
        }

        public override void Unload()
        {
            drawNpcDirectHook?.Dispose();
            drawNpcDirectHook = null;
        }

        private static void DrawNPCDirect_Detour(
            Orig_DrawNPCDirect orig,
            Main self,
            SpriteBatch mySpriteBatch,
            NPC rCurrentNPC,
            bool behindTiles,
            Vector2 screenPos
        )
        {
            if (rCurrentNPC != null &&
                rCurrentNPC.active &&
                (rCurrentNPC.type == NPCID.SkeletronHand ||
                rCurrentNPC.type == NPCID.PrimeCannon ||
                rCurrentNPC.type == NPCID.PrimeSaw ||
                rCurrentNPC.type == NPCID.PrimeLaser ||
                rCurrentNPC.type == NPCID.PrimeVice ||
                rCurrentNPC.type == NPCID.SkeletronPrime ||
                rCurrentNPC.type == NPCID.SkeletronHead ||
                rCurrentNPC.type == NPCID.TheHungry ||//fail
                rCurrentNPC.type == NPCID.TheHungryII ||
                rCurrentNPC.type == NPCID.PlanterasTentacle ||// fail
                rCurrentNPC.type == NPCID.PlanterasHook ||
                rCurrentNPC.type == NPCID.GolemFistLeft ||
                rCurrentNPC.type == NPCID.GolemFistRight ||
                // rCurrentNPC.type == NPCID.GolemHead ||
                rCurrentNPC.type == NPCID.GolemHeadFree ||
                // rCurrentNPC.type == NPCID.CultistBoss ||
                rCurrentNPC.type == NPCID.CultistBossClone
                ) &&
                InvisibleEnemiesGlobalNPC.ShouldHideForDraw(rCurrentNPC))
            {
                return;
            }

            orig(self, mySpriteBatch, rCurrentNPC, behindTiles, screenPos);
        }
    }
}
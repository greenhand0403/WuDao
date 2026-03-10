using System.Reflection;
using MonoMod.RuntimeDetour;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WuDao.Content.Global.NPCs;

namespace WuDao.Content.Systems
{
    public class InvisibleEnemiesWoFDrawHookSystem : ModSystem
    {
        private delegate void Orig_DrawWoF(Main self);

        private static Hook drawWoFHook;

        public override void Load()
        {
            MethodInfo drawWoFMethod = typeof(Main).GetMethod(
                "DrawWoF",
                BindingFlags.Instance | BindingFlags.NonPublic
            );

            if (drawWoFMethod == null)
            {
                Mod.Logger.Warn("Failed to find Terraria.Main.DrawWoF");
                return;
            }

            drawWoFHook = new Hook(drawWoFMethod, DrawWoF_Detour);
        }

        public override void Unload()
        {
            drawWoFHook?.Dispose();
            drawWoFHook = null;
        }

        private static void DrawWoF_Detour(Orig_DrawWoF orig, Main self)
        {
            int wofIndex = Main.wofNPCIndex;

            if (wofIndex >= 0 && wofIndex < Main.maxNPCs)
            {
                NPC wof = Main.npc[wofIndex];

                if (wof.active &&
                    wof.type == NPCID.WallofFlesh &&
                    InvisibleEnemiesGlobalNPC.ShouldHideForDraw(wof))
                {
                    return;
                }
            }

            orig(self);
        }
    }
}
using HarmonyLib;
using RimWorld;
using Verse;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo))]
    public static class PawnIdeoTrackerPatches_SetIdeo
    {
        public static void Prefix(Pawn_IdeoTracker __instance, Ideo ideo, Pawn ___pawn)
        {
            if (___pawn == null)
                return;

            if(__instance.Ideo != ideo)
            {
                var favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
                if (favorTracker != null)
                {
                    favorTracker.ResetFavor(___pawn);
                }
            }
        }
    }
}
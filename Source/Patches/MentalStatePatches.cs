using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using System.Linq;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    public static class MentalStateHandler_TryStartMentalState
    {
        public static void Postfix(bool __result, MentalStateDef stateDef, bool causedByMood, Pawn ___pawn)
        {
            if (!__result || !causedByMood || ___pawn == null || !___pawn.RaceProps.Humanlike)
                return;

            if (!___pawn.IsColonist && !___pawn.IsSlave)
                return;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;

            MentalBreakDef breakDef = DefDatabase<MentalBreakDef>.AllDefsListForReading
                .FirstOrDefault(x => x.mentalState == stateDef);

            if (breakDef == null)
                return;

            // Extreme Breaks (e.g., Murderous Rage, Wild Regression)
            if (breakDef.intensity == MentalBreakIntensity.Extreme)
            {
                tracker.AddFavor(___pawn, -20f);

                if (Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal] {___pawn.LabelShort} suffered an Extreme break ({stateDef.defName}). Gained -20 Favor.");
                }
            }
            // Major Breaks (e.g., Hide In Room, Tantrum)
            else if (breakDef.intensity == MentalBreakIntensity.Major)
            {
                tracker.AddFavor(___pawn, -10f);

                if (Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal] {___pawn.LabelShort} suffered a Major break ({stateDef.defName}). Gained -10 Favor.");
                }
            }
        }
    }
}
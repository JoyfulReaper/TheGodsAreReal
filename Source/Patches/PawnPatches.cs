using HarmonyLib;
using Verse;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetInspectString))]
    public static class Patch_Pawn_GetInspectString
    {
        public static void Postfix(Pawn __instance, ref string __result)
        {
            if (!Prefs.DevMode)
                return;

            if (__instance.Ideo?.KeyDeityName == null)
                return;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;

            float individualFavor = tracker.GetFavor(__instance);
            float ideoFavor = tracker.GetIdeoFavor(__instance.Ideo);

            // Append the hidden stats cleanly to the bottom of the inspect string text block
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // If there's already text in the inspect string, add a newline first
            if (!string.IsNullOrEmpty(__result))
            {
                sb.AppendLine();
            }

            sb.AppendLine("--- DEV: THE GODS ARE REAL ---");
            sb.AppendLine($"Individual Favor: {individualFavor:F1} / 100.0");
            sb.AppendLine($"Ideo Avg Favor:   {ideoFavor:F1}");

            // Stick our debug readout onto the existing result string
            __result += sb.ToString().TrimEnd();
        }
    }
}

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(LordJob_Ritual), "ApplyOutcome")]
    public static class Patch_LordJob_Ritual_ApplyOutcome
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var targetMethod = typeof(RitualOutcomeEffectWorker).GetMethod("Apply");

            for (int i = 0; i < codes.Count; i++)
            {
                yield return codes[i];

                // Check if this instruction is the call to Apply
                if (codes[i].Calls(targetMethod))
                {
                    // Inject our logic right after Apply returns
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // Load 'this'
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch_LordJob_Ritual_ApplyOutcome), nameof(InjectFavorLogic)));
                }
            }
        }

        public static void InjectFavorLogic(LordJob_Ritual instance)
        {
            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal: Patch_LordJob_Ritual_ApplyOutcome]: InjectFavorLogic Fired.");
            }
            var tracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            float favorChange = 5f;

            // Access totalPresenceTmp safely
            var participants = Traverse.Create(instance).Field("totalPresenceTmp").GetValue<Dictionary<Pawn, int>>();

            if (participants != null)
            {
                foreach (var kvp in participants)
                {
                    if (kvp.Key != null)
                    {
                        tracker.AddFavor(kvp.Key, favorChange);
                        if (Prefs.DevMode)
                        {
                            Log.Message($"[TheGodsAreReal]: Added {favorChange} favor to {kvp.Key.Name}");
                        }
                    }
                }
            }
        }
    }
}



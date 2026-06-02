/**
BSD 2-Clause License

Copyright (c) 2026, Kyle Givler

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 **/

using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_FromQuality), nameof(RitualOutcomeEffectWorker_FromQuality.Apply))]
    public static class Patch_Apply_FavorLogic
    {
        public static void Postfix(RitualOutcomeEffectWorker_FromQuality __instance, float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal: RitualOutcomeEffectWorker_FromQuality]: Postfix Fired.");
            }

            // 1. Calculate quality again
            float quality = Traverse.Create(__instance).Method("GetQuality", jobRitual, progress).GetValue<float>();

            // 2. Execute your custom favor logic based on 'outcome' and 'quality'
            Log.Message($"[TheGodsAreReal] Ritual ended with quality: {quality}");


            var tracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            float favorChange = 5f;

            if (totalPresence != null)
            {
                foreach (var kvp in totalPresence)
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
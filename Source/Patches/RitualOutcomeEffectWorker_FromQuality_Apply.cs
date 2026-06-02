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

// TODO: Balance favor

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_FromQuality), nameof(RitualOutcomeEffectWorker_FromQuality.Apply))]
    public static class RitualOutcomeEffectWorker_FromQuality_Apply
    {
        public static void Postfix(RitualOutcomeEffectWorker_FromQuality __instance, float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            if (jobRitual == null)
                return;

            //Calculate quality again
            float quality = Traverse.Create(__instance).Method("GetQuality", jobRitual, progress).GetValue<float>();
            var ritualIdeo = jobRitual.Ritual.ideo;

            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal] Ritual ended with quality: {quality:F2}");
            }

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;

            // Step-logic to calculate favor based on standard RimWorld outcome brackets
            float baseFavorChange = 0f;

            if (quality < 0.25f) // Awful / Boring
            {
                baseFavorChange = -5f; // Displeased gods
            }
            else if (quality < 0.60f) // Flawed / Boring / Ordinary
            {
                baseFavorChange = 2f; // Minimal acknowledgment
            }
            else if (quality < 0.90f) // Satisfying / Fun
            {
                baseFavorChange = 8f; // Decent blessing
            }
            else // Spectacular / Unforgettable (0.90 to 1.0+)
            {
                baseFavorChange = 20f; // Divine favor rain
            }

            // Grab the leader/organizer of the ritual if available to give them a bonus/penalty
            Pawn organizer = jobRitual?.Organizer;

            if (totalPresence != null)
            {
                foreach (var kvp in totalPresence)
                {
                    Pawn participant = kvp.Key;
                    if (participant == null)
                        continue;

                    float individualFavorChange = baseFavorChange;

                    // Give the organizer 25% extra impact for the outcome
                    if (participant == organizer)
                    {
                        individualFavorChange *= 1.25f;
                    }

                    if (participant.Ideo == ritualIdeo)
                    {
                        var currentFavor = tracker.GetFavor(participant);
                        if (currentFavor > 60f)
                        {
                            individualFavorChange *= 0.5f;
                        }

                        tracker.AddFavor(participant, individualFavorChange);
                    }
                    else
                    {
                        individualFavorChange = -3f;
                        tracker.AddFavor(participant, individualFavorChange);
                    }

                    if (Prefs.DevMode)
                    {
                        Log.Message($"[TheGodsAreReal]: Processed ritual favor change of {individualFavorChange} for {participant.LabelShort}");
                    }
                }
            }
        }
    }
}
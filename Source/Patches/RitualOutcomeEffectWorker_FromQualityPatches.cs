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
        // Perfomance affecting options
        private const int DisableMotesThreshold = 10;

        // Ritual Quality
        private const float AwfulQualityThreshold = 0.25f;
        private const float FlawedQualityThreshold = 0.60f;
        private const float SatisfyingQualityThreshold = 0.90f;
        private const float SpectacularQualityThreshold = 1.0f;

        // Favor Rewards/Penalties
        private const float BaseFavorChange = 0f;
        private const float AwfulFavorChange = -5f;
        private const float FlawedFavorChange = 2f;
        private const float SatisfyingFavorChange = 8f;
        private const float SpectacularFavorChange = 20f;
        private const float DisbelieverPenalty = -3f;

        // Modifies and Threshholds
        private const float OrganizerBonusMultiplier = 1.25f;
        private const float HighFavorPenaltyThreshold = 60f;
        private const float HighFavorPenaltyMultiplier = 0.5f;


        public static void Postfix(RitualOutcomeEffectWorker_FromQuality __instance, float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            if (jobRitual == null || totalPresence == null)
                return;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;

            //Calculate quality again
            float quality = Traverse.Create(__instance).Method("GetQuality", jobRitual, progress).GetValue<float>();
            var ritualIdeo = jobRitual.Ritual.ideo;

            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal] Ritual ended with quality: {quality:F2}");
            }

            
            float favorChange = BaseFavorChange;
            if (quality < AwfulQualityThreshold) // Awful / Boring
            {
                favorChange = AwfulFavorChange; // Displeased gods
            }
            else if (quality < FlawedQualityThreshold) // Flawed / Boring / Ordinary
            {
                favorChange = FlawedFavorChange; // Minimal acknowledgment
            }
            else if (quality < SatisfyingQualityThreshold) // Satisfying / Fun
            {
                favorChange = SatisfyingFavorChange; // Decent blessing
            }
            else // Spectacular / Unforgettable (0.90 to 1.0+)
            {
                favorChange = SpectacularFavorChange; // Divine favor rain
            }

            // Grab the leader/organizer of the ritual if available to give them a bonus/penalty
            Pawn organizer = jobRitual?.Organizer;

            bool showMotes = totalPresence.Count < DisableMotesThreshold;

            foreach (var kvp in totalPresence)
            {
                Pawn participant = kvp.Key;
                if (participant == null || !participant.RaceProps.Humanlike)
                    continue;

                float individualFavorChange = favorChange;

                // Give the organizer 25% extra impact for the outcome
                if (participant == organizer)
                {
                    individualFavorChange *= OrganizerBonusMultiplier;
                }

                if (participant.Ideo == ritualIdeo)
                {
                    // Same ideo
                    var currentFavor = tracker.GetFavor(participant);
                    if (currentFavor > HighFavorPenaltyThreshold)
                    {
                        individualFavorChange *= HighFavorPenaltyMultiplier;
                    }

                    tracker.AddFavor(participant, individualFavorChange, showMotes);
                }
                else
                {
                    // different ideo
                    individualFavorChange = DisbelieverPenalty;
                    tracker.AddFavor(participant, individualFavorChange, showMotes);
                }


                if (Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal]: Processed ritual favor change of {individualFavorChange} for {participant.LabelShort}");
                }
            }

            if (!showMotes)
            {
                Messages.Message(
                    "Ritual complete: The gods have observed the ceremony. Favor has shifted among the participants.",
                    organizer ?? new LookTargets(jobRitual.Map.Center, jobRitual.Map),
                    MessageTypeDefOf.NeutralEvent
                );
            }
            

            if(Prefs.DevMode)
                Log.Message($"[TheGodsAreReal] Participant count: {totalPresence.Count}, showMotes: {showMotes}");

        }
    }
}
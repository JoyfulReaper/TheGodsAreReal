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
using System.Linq;
using UnityEngine;
using Verse;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(Quest), nameof(Quest.End))]
    public static class Quest_End
    {
        private const float BaseFavorReward = 5f;
        private const float QuestRatingMultiplier = 5f;
        private const float AcceptorFavorMultiplier = 2f;

        private const float NonBelieverFavorGainChance = 0.3f;
        private const float NonBelieverFavorLossChance = 0.6f;
        private const float NonBelieverFavorLossMultiplier = 0.5f;

        // Perfomance affecting options
        private const int DisableMotesThreshold = 10;

        public static void Postfix(Quest __instance, QuestEndOutcome outcome)
        {
            if (outcome != QuestEndOutcome.Success)
                return;

            var favorTracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (favorTracker == null) 
                return;

            int rating = Mathf.Max(0, __instance.challengeRating);
            float questReward = BaseFavorReward + (rating * QuestRatingMultiplier);

            // Use the Colony's primary Ideo as the baseline for divine rewards
            Ideo questIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
            Pawn accepter = __instance.AccepterPawn;

            var colonyPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction;
            bool showMotes = colonyPawns.Count <= DisableMotesThreshold;

            foreach (Pawn p in colonyPawns)
            {
                if (!p.RaceProps.Humanlike)
                    continue;

                if (p == accepter)
                {
                    // Acceptor can get double favor
                    float accepterReward = questReward * AcceptorFavorMultiplier;
                    favorTracker.AddFavor(p, accepterReward, showMotes);
                    if (Prefs.DevMode)
                    {
                        Log.Message($"[TheGodsAreReal]: QUEST LEADER {p.LabelShort} gained {accepterReward} favor from quest: {__instance.name}");
                    }
                }
                else if (p.Ideo == questIdeo)
                {
                    favorTracker.AddFavor(p, questReward, showMotes);
                    if (Prefs.DevMode)
                    {
                        Log.Message($"[TheGodsAreReal]: {p.LabelShort} gained {questReward} favor from quest: {__instance.name}");
                    }
                }
                else
                {
                    // Pawn is not main ideo
                    float roll = Rand.Value;
                    if (roll < NonBelieverFavorGainChance)
                    {
                        favorTracker.AddFavor(p, questReward, showMotes);
                        if (Prefs.DevMode)
                        {
                            Log.Message($"[TheGodsAreReal]: {p.LabelShort} (Non-believer) gained {questReward} favor from quest: {__instance.name}");
                        }
                    }
                    else if (roll < NonBelieverFavorGainChance + NonBelieverFavorLossChance)
                    {
                        favorTracker.AddFavor(p, (-questReward) * NonBelieverFavorLossMultiplier, showMotes);
                        if (Prefs.DevMode)
                        {
                            Log.Message($"[TheGodsAreReal]: {p.LabelShort} (Non-believer) lost {questReward} favor from quest: {__instance.name}");
                        }
                    }
                    else
                    {
                        // No change for this pawn
                    }
                }
            }

            if (!showMotes)
            {
                var targets = __instance.QuestLookTargets;

                LookTargets finalTarget = targets.Any()
                                            ? new LookTargets(targets)
                                            : new LookTargets();

                Messages.Message(
                    "Quest complete: The gods have observed your quest. Favor has shifted among the participants.",
                    finalTarget,
                    MessageTypeDefOf.NeutralEvent
                );
            }
        }
    }
}
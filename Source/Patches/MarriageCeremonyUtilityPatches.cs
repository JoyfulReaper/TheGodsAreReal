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
using Verse;
using Verse.AI.Group;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(MarriageCeremonyUtility), nameof(MarriageCeremonyUtility.Married))]
    public static class MarriageCeremonyUtility_Married
    {
        private const float BaseWeddingReward = 25f;
        private const int DisableMotesThreshold = 10;
        private const float InterFaithPenalityMultiplier = 0.5f;
        private const float GuestRewardMultiplier = 0.5f;
        private const float NonBeliverRewardChance = 0.5f;
        private const float NonBeliverPosRewardMultipler = 0.5f;
        private const float NonBeliverFavorLoss = 3f;

        static void Postfix(Pawn firstPawn, Pawn secondPawn)
        {
            if(firstPawn == null || secondPawn == null)
                return;

            var favorTracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (favorTracker == null) 
                return;

            float weddingReward = BaseWeddingReward;
            if (firstPawn.Ideo != null && secondPawn.Ideo != null && firstPawn.Ideo != secondPawn.Ideo)
            {
                weddingReward *= InterFaithPenalityMultiplier; // Reduce reward if spouses have different ideologies
            }

            favorTracker.AddFavor(firstPawn, weddingReward);
            favorTracker.AddFavor(secondPawn, weddingReward);

            Log.Message($"[TheGodsAreReal] Wedding detected! {firstPawn.LabelShort} and {secondPawn.LabelShort} gained {weddingReward} favor.");

            bool showMotes = true;
            Lord lord = firstPawn.GetLord();
            if(lord != null)
            {
                if(lord.ownedPawns.Count > DisableMotesThreshold)
                {
                    showMotes = false;
                }

                foreach (var pawn in lord.ownedPawns)
                {
                    if(pawn == firstPawn || pawn == secondPawn)
                        continue;

                    if (pawn.IsColonist || pawn.IsSlaveOfColony)
                    {
                        float guestReward = weddingReward * GuestRewardMultiplier; // Guests gain half the favor of the married couple

                        // Pawn doesnt follow ideo of either spouse
                        if (pawn.Ideo != null && (pawn.Ideo != firstPawn.Ideo && pawn.Ideo != secondPawn.Ideo))
                        {
                            if(Rand.Value < NonBeliverRewardChance)
                            {
                                guestReward *= NonBeliverPosRewardMultipler;
                            }
                            else
                            {
                                guestReward -= NonBeliverFavorLoss;
                            }
                        }
                        favorTracker.AddFavor(pawn, guestReward, showMotes);

                        if(Prefs.DevMode)
                        {
                            Log.Message($"[TheGodsAreReal] Wedding: {pawn.LabelShort} gained {guestReward} favor.");
                        }
                    }
                }
                if (!showMotes)
                {
                    Messages.Message(
                        "Marriage complete: The gods have observed the ceremony. Favor has shifted among the participants.",
                        firstPawn ?? secondPawn,
                        MessageTypeDefOf.NeutralEvent
                    );
                }
            }
        }
    }
}
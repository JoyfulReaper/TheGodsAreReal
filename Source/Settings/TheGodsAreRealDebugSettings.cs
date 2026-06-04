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

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace TheGodsAreReal.Settings
{
    internal static class TheGodsAreRealDebugSettings
    {
        internal const int DebugButtonCount = 6;

        internal static void DoDebugSettingsWindowContents(Listing_Standard listing, List<Pawn> pawns)
        {
            WorldComponent_FavorTracker favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            listing.Label("Dev Mode Settings", 24f);

            // Add 10 Favor
            if (listing.ButtonText("Add 10 Favor to Selected Pawns"))
            {
                var selectedPawns = Find.Selector.SelectedPawns;
                if (selectedPawns.NullOrEmpty())
                {
                    Messages.Message("Select a pawn first!", MessageTypeDefOf.RejectInput);
                }
                else
                {
                    for (int i = 0; i < selectedPawns.Count; i++)
                    {
                        Pawn selectedPawn = selectedPawns[i];
                        if (selectedPawn != null)
                        {
                            favorTracker.AddFavor(selectedPawn, 10f);
                            Messages.Message($"Added 10 favor to {selectedPawn.LabelShort}. New Favor level: {favorTracker.GetFavor(selectedPawn)}", MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }

            // Remove 10 Favor
            if (listing.ButtonText("Remove 10 Favor from Selected Pawns"))
            {
                var selectedPawns = Find.Selector.SelectedPawns;
                if (selectedPawns.NullOrEmpty())
                {
                    Messages.Message("Select a pawn first!", MessageTypeDefOf.RejectInput);
                }
                else
                {
                    for (int i = 0; i < selectedPawns.Count; i++)
                    {
                        Pawn selectedPawn = selectedPawns[i];
                        if (selectedPawn != null)
                        {
                            favorTracker.AddFavor(selectedPawn, -10f);
                            Messages.Message($"Removed 10 favor from {selectedPawn.LabelShort}. New Favor level: {favorTracker.GetFavor(selectedPawn)}", MessageTypeDefOf.PositiveEvent);
                        }
                    }
                }
            }

            // Pawn Favor Level
            if (listing.ButtonText("Selected Pawn's Favor Level"))
            {
                var selectedPawns = Find.Selector.SelectedPawns;
                foreach (var selectedPawn in selectedPawns)
                {
                    if (selectedPawn != null)
                    {
                        Messages.Message($"{selectedPawn.Name}: favor level: {favorTracker.GetFavor(selectedPawn)}", MessageTypeDefOf.PositiveEvent);
                    }
                    else
                    {
                        Messages.Message("Select a pawn first!", MessageTypeDefOf.RejectInput);
                    }
                }
            }

            // Force Prayer
            if (listing.ButtonText("Force Selected Pawn to pray"))
            {
                var pawn = Find.Selector.SingleSelectedThing as Pawn;

                if (pawn != null)
                {
                    var target = pawn?.Map?.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.SteleLarge).FirstOrDefault();

                    if (target != null)
                    {
                        Job job = JobMaker.MakeJob(JobDefOf.MeditatePray, target);
                        pawn.jobs.StopAll();
                        pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                        Messages.Message($"Forced {pawn.Name} to pray", MessageTypeDefOf.PositiveEvent);
                    }
                    else
                    {
                        Messages.Message("Build a Large Stele first!", MessageTypeDefOf.RejectInput);
                    }
                }
                else
                {
                    Messages.Message("Select a pawn first!", MessageTypeDefOf.RejectInput);
                }
            }

            // Ideo Favor
            if (listing.ButtonText("Ideology Favor Level for selected pawns Ideo"))
            {
                var pawn = Find.Selector.SingleSelectedThing as Pawn;
                if (pawn != null)
                {
                    Messages.Message($"Ideology {pawn.Ideo.name} favor level: {favorTracker.GetIdeoFavor(pawn.Ideo)}", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("Select a pawn first!", MessageTypeDefOf.RejectInput);
                }
            }

            DoDebugDataOverview(listing);
            DoPawnFavorList(listing, pawns);
        }

        private static void DoDebugDataOverview(Listing_Standard listing)
        {
            WorldComponent_FavorTracker favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            if (favorTracker == null) return;


            listing.Label("--- Mod Data Overview ---", 24f);
            listing.Label($"Tracked Pawns: {favorTracker.PawnFavor.Count}");

            float totalGlobalFavor = 0f;
            foreach (var val in favorTracker.PawnFavor.Values)
            {
                totalGlobalFavor += val;
            }

            listing.Label($"Total Global Favor: {totalGlobalFavor:F1}");
            listing.Label($"Average Favor/Pawn: {(favorTracker.PawnFavor.Count > 0 ? (totalGlobalFavor / favorTracker.PawnFavor.Count).ToString("F1") : "N/A")}");

            if (listing.ButtonText("Clear All Data (DANGER)"))
            {
                favorTracker.ClearAllPawnFavor();
                Messages.Message("All favor data wiped!", MessageTypeDefOf.CautionInput);
            }
        }

        private static void DoPawnFavorList(Listing_Standard listing, List<Pawn> pawns)
        {
            WorldComponent_FavorTracker favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            if (favorTracker == null) return;

            listing.Label("--- Colony Favor Overview ---", 24f);

            foreach (Pawn p in pawns)
            {
                float favor = favorTracker.GetFavor(p);
                listing.Label($"{p.LabelShort}: {favor:F1}");
            }
        }
    }
}
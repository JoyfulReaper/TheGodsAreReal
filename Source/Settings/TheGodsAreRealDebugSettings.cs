using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheGodsAreReal.Settings
{
    internal static class TheGodsAreRealDebugSettings
    {
        private static Vector2 _scrollPos = Vector2.zero;

        internal static void DoDebugSettingsWindowContents(Rect inRect)
        {
            WorldComponent_FavorTracker favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();

            int buttonCount = 6;
            int pawnCount = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction.Count();
            float calculatedHeight = (buttonCount * 35f) + (pawnCount * 30f) + 200f; // 200f for padding/headers

            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, calculatedHeight);

            Widgets.BeginScrollView(inRect, ref _scrollPos, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);
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
            DoPawnFavorList(listing);

            listing.End();
            Widgets.EndScrollView();
        }

        internal static void DoDebugDataOverview(Listing_Standard listing)
        {
            WorldComponent_FavorTracker favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            if (favorTracker == null) return;


            listing.Label("--- Mod Data Overview ---", 24f);
            listing.Label($"Tracked Pawns: {favorTracker.pawnFavor.Count}");

            float totalGlobalFavor = 0f;
            foreach (var val in favorTracker.pawnFavor.Values)
            {
                totalGlobalFavor += val;
            }

            listing.Label($"Total Global Favor: {totalGlobalFavor:F1}");
            listing.Label($"Average Favor/Pawn: {(favorTracker.pawnFavor.Count > 0 ? (totalGlobalFavor / favorTracker.pawnFavor.Count).ToString("F1") : "N/A")}");

            if (listing.ButtonText("Clear All Data (DANGER)"))
            {
                favorTracker.pawnFavor.Clear();
                Messages.Message("All favor data wiped!", MessageTypeDefOf.CautionInput);
            }
        }

        internal static void DoPawnFavorList(Listing_Standard listing)
        {
            WorldComponent_FavorTracker favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            if (favorTracker == null) return;

            listing.Label("--- Colony Favor Overview ---", 24f);

            var pawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction
                           .Where(p => p.RaceProps.Humanlike);

            foreach (Pawn p in pawns)
            {
                float favor = favorTracker.GetFavor(p);
                listing.Label($"{p.LabelShort}: {favor:F1}");
            }
        }
    }
}
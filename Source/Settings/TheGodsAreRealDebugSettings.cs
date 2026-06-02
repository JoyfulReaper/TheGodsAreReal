using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheGodsAreReal.Settings
{
    internal static class TheGodsAreRealDebugSettings
    {
        internal static void DoDebugSettingsWindowContents(Rect inRect)
        {
            WorldComponent_FavorTracker favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
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

            listing.End();
        }
    }
}
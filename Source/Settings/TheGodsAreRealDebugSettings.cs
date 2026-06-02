using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace TheGodsAreReal.Settings
{
    internal class TheGodsAreRealDebugSettings
    {
        public void DoDebugSettingsWindowContents(Rect inRect)
        {
            WorldComponent_FavorTracker favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.Label("Dev Mode Settings", 24f);
            if (listing.ButtonText("Add 10 Favor to Selected Pawn"))
            {
                var pawn = Find.Selector.SingleSelectedThing as Pawn;
                if (pawn != null)
                {
                    favorTracker.AddFavor(pawn, 10f);
                    Messages.Message($"Added 10 favor to {pawn.Name}. New Favor level: {favorTracker.GetFavor(pawn)}", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("Select a pawn first!", MessageTypeDefOf.RejectInput);
                }
            }

            if (listing.ButtonText("Remove 10 Favor from Selected Pawn"))
            {
                var pawn = Find.Selector.SingleSelectedThing as Pawn;
                if (pawn != null)
                {
                    favorTracker.AddFavor(pawn, -10f);
                    Messages.Message($"Removed 10 favor from {pawn.Name}. New Favor level: {favorTracker.GetFavor(pawn)}", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("Select a pawn first!", MessageTypeDefOf.RejectInput);
                }
            }

            if (listing.ButtonText("Selected Pawn's Favor Level"))
            {
                var pawn = Find.Selector.SingleSelectedThing as Pawn;
                if (pawn != null)
                {
                    Messages.Message($"{pawn.Name}: favor level: {favorTracker.GetFavor(pawn)}", MessageTypeDefOf.PositiveEvent);
                }
                else
                {
                    Messages.Message("Select a pawn first!", MessageTypeDefOf.RejectInput);
                }
            }

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
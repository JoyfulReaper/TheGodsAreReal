using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGodsAreReal.Utilities;
using UnityEngine;
using Verse;

namespace TheGodsAreReal.Handlers
{
    public static class HediffHandler
    {
        // Hediff just used for testing right now
        private static readonly HediffDef _divineTouchDef = HediffDef.Named("TheGodsAreReal_DivineTouch");

        internal static void ApplyHediffs()
        {
            foreach (Pawn pawn in GodsAreRealPawnUtility.GetAllColonyPawns())
            {
                if (pawn == null || pawn.Dead || !pawn.RaceProps.Humanlike || pawn.Ideo?.KeyDeityName == null)
                    continue;

                UpdatePawnDivineHediff(pawn);
            }
        }

        private static void UpdatePawnDivineHediff(Pawn pawn)
        {
            var tracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;

            float favor = tracker.GetFavor(pawn);
            float normalizedSeverity = Mathf.Clamp01((favor + 100f) / 200f);
            // Map the favor score (-100.0 to 100.0) to a C# normalized float scale (0.0 to 1.0)
            // -100 favor becomes 0.0 severity (Pure Wrath)
            //    0 favor becomes 0.5 severity (Neutral / Hidden)
            // +100 favor becomes 1.0 severity (Pure Grace)

            // Look for an existing instance of our Hediff on the pawn
            Hediff existingHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(_divineTouchDef);

            if (existingHediff != null)
            {
                // Update the severity level dynamically as their behavior changes
                existingHediff.Severity = normalizedSeverity;
            }
            else
            {
                // If they don't have the hediff yet, create it and inject it into their health tracker
                Hediff newHediff = HediffMaker.MakeHediff(_divineTouchDef, pawn);
                newHediff.Severity = normalizedSeverity;
                pawn.health?.AddHediff(newHediff);
            }
        }
    }
}

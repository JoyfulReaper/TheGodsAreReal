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
using UnityEngine;
using Verse;

namespace TheGodsAreReal
{
    public class TheGodsAreRealSettings : ModSettings
    {
        public string Version
        {
            get
            {
                return "0.0.1";
            }
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            if (Prefs.DevMode)
                DoDebugSettingsWindowContents(inRect);
        }

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

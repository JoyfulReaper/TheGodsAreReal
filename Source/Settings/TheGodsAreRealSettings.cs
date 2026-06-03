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
using Verse.AI;
using System.Linq;
using TheGodsAreReal.Settings;

namespace TheGodsAreReal
{
    public class TheGodsAreRealSettings : ModSettings
    {
        private string _version = "0.0.4";
        private static Vector2 _scrollPos = Vector2.zero;
        private static int buttonCount = 0;

        public string Version
        {
            get
            {
                return _version;
            }
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            int totalButtonCount = Prefs.DevMode ? TheGodsAreRealDebugSettings.debugButtonCount : buttonCount;
            var trackedPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction
                           .Where(p => p.RaceProps.Humanlike).ToList();

            int pawnCount = Prefs.DevMode ? trackedPawns.Count : 0;

            float calculatedHeight = (totalButtonCount * 35f) + (pawnCount * 30f) + 100f; // 100f for padding/headers
            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, calculatedHeight);

            Widgets.BeginScrollView(inRect, ref _scrollPos, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);
            listing.Label($"The Gods Are Real {Version}", 24f);


            if (Prefs.DevMode)
                TheGodsAreRealDebugSettings.DoDebugSettingsWindowContents(listing, trackedPawns);

            listing.End();
            Widgets.EndScrollView();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _version, "Version", _version);
        }
    }
}
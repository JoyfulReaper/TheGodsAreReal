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
using System.Linq;
using TheGodsAreReal.Settings;
using System.Collections.Generic;

namespace TheGodsAreReal
{
    public class TheGodsAreRealSettings : ModSettings
    {
        private string _version = "0.0.17";
        private bool _alwaysShowMotes = false;
        private static Vector2 _scrollPos = Vector2.zero;
        private const int _buttonCount = 0;

        public bool AlwaysShowMotes
        {
            get => _alwaysShowMotes;
            private set => _alwaysShowMotes = value;
        }

        public string Version => _version;

        public void DoSettingsWindowContents(Rect inRect)
        {
            List<Pawn> trackedPawns = null;
            int totalButtonCount = Prefs.DevMode ? TheGodsAreRealDebugSettings.DebugButtonCount + _buttonCount : _buttonCount;

            if (Prefs.DevMode)
            {
                trackedPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction
                               .Where(p => p.RaceProps.Humanlike).ToList();
            }

            int pawnCount = (Prefs.DevMode && trackedPawns != null) ? trackedPawns.Count : 0;

            float calculatedHeight = (totalButtonCount * 35f) + (pawnCount * 30f) + 50f;
            Rect viewRect = new Rect(0f, 0f, inRect.width - 20f, calculatedHeight);

            Widgets.BeginScrollView(inRect, ref _scrollPos, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);
            listing.Label($"Version {Version}", 24f);
            listing.Gap(12f);

            listing.Label("General Settings", 24f);
            listing.Gap(12f);
            listing.CheckboxLabeled("Always show motes (May decrease performance)", ref _alwaysShowMotes);
            listing.Gap(12f);

            if (Prefs.DevMode)
                TheGodsAreRealDebugSettings.DoDebugSettingsWindowContents(listing, trackedPawns);

            listing.End();
            Widgets.EndScrollView();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _version, "Version", Version);
            Scribe_Values.Look(ref _alwaysShowMotes, "AlwaysShowMotes", false);
        }
    }
}
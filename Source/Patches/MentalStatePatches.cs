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
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(MentalStateHandler), nameof(MentalStateHandler.TryStartMentalState))]
    public static class MentalStateHandler_TryStartMentalState
    {
        private const float ExtremeBreakFavorLoss = -20f;
        private const float MajorBreakFavorLoss = -10f;

        private static readonly Dictionary<MentalStateDef, MentalBreakDef> _breakDefCache = new Dictionary<MentalStateDef, MentalBreakDef>();

        public static void Postfix(bool __result, MentalStateDef stateDef, bool causedByMood, Pawn ___pawn)
        {
            if (!__result || !causedByMood || ___pawn == null || !___pawn.RaceProps.Humanlike)
                return;

            if (!___pawn.IsColonist && !___pawn.IsSlaveOfColony)
                return;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;

            if (!_breakDefCache.TryGetValue(stateDef, out MentalBreakDef breakDef))
            {
                breakDef = DefDatabase<MentalBreakDef>.AllDefsListForReading
                    .FirstOrDefault(x => x.mentalState == stateDef);

                // Cache the result (even if null, preventing repeated failed scans for custom states)
                _breakDefCache[stateDef] = breakDef;
            }

            if (breakDef == null)
                return;

            // Extreme Breaks (e.g., Murderous Rage, Wild Regression)
            if (breakDef.intensity == MentalBreakIntensity.Extreme)
            {
                tracker.AddFavor(___pawn, ExtremeBreakFavorLoss);

                if (Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal] {___pawn.LabelShort} suffered an Extreme break ({stateDef.defName}). Gained -20 Favor.");
                }
            }
            // Major Breaks (e.g., Hide In Room, Tantrum)
            else if (breakDef.intensity == MentalBreakIntensity.Major)
            {
                tracker.AddFavor(___pawn, MajorBreakFavorLoss);

                if (Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal] {___pawn.LabelShort} suffered a Major break ({stateDef.defName}). Gained -10 Favor.");
                }
            }
        }
    }
}
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

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(MemoryThoughtHandler), nameof(MemoryThoughtHandler.TryGainMemory), new System.Type[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class MemoryThoughtHandler_TryGainMemory
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought)
        {
            if (newThought == null || __instance.pawn == null || newThought.pawn == null)
                return;

            Pawn pawn = __instance.pawn;

            if (!pawn.IsColonist && !pawn.IsSlaveOfColony)
                return;

            if (pawn.Dead)
                return;

            if (pawn.Ideo?.KeyDeityName == null)
                return;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;
            
            if (newThought.sourcePrecept != null)
            {
                int tick = Find.TickManager.TicksGame;
                if (tracker.GetLastFavorTick(pawn) == tick)
                    return;

                bool shouldShowMotes = !ShouldSuppressThoughtMote(newThought); // These motes are ALWAYS supppressed even if show all motes are turned on

                float moodOffset = newThought.MoodOffset();
                if (moodOffset < 0f)
                {
                    float negativeMultiplier = 0.5f;
                    tracker.AddFavor(pawn, moodOffset * negativeMultiplier, shouldShowMotes);
                    if (Prefs.DevMode)
                    {
                        Log.Message($"[TheGodsAreReal]: Thought '{newThought.def.defName}' caused {pawn.LabelShort}'s favor to change by: {moodOffset * negativeMultiplier}");
                    }
                }
                else if (moodOffset > 0f)
                {
                    float positiveMultiplier = 0.75f;
                    tracker.AddFavor(pawn, moodOffset * positiveMultiplier, shouldShowMotes);
                    if (Prefs.DevMode)
                    {
                        Log.Message($"[TheGodsAreReal]: Thought '{newThought.def.defName}' caused {pawn.LabelShort}'s favor to change by: {moodOffset * positiveMultiplier}");
                    }
                }
            }
        }

        /// <summary>
        /// Determine if we should ignore a though when showing the motes to avoid huge mote clouds during mass events
        /// </summary>
        /// <param name="thought"></param>
        /// <returns>true if the mote should be supressed, false otherwise</returns>
        internal static bool ShouldSuppressThoughtMote(Thought_Memory thought)
        {
            string defName = thought.def?.defName;
            if (string.IsNullOrEmpty(defName))
                return false;

            // Blacklist filters for collective crowd events where multiple pawns react simultaneously
            string[] massEventKeywords = { "Party", "Ritual", "Speech", "Funeral", "Execution", "Sacrifice", "Festival" };

            foreach (string keyword in massEventKeywords)
            {
                if (defName.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
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
    public static class MemoryThoughHandler_TryGainMemory
    {
        public static void Postfix(MemoryThoughtHandler __instance, Thought_Memory newThought)
        {
            int tick = Find.TickManager.TicksGame;

            if (newThought == null || __instance?.pawn == null)
                return;

            if (__instance.pawn.Dead || !__instance.pawn.Spawned)
                return;

            if (__instance.pawn.Ideo?.KeyDeityName == null)
                return;

            if (newThought.sourcePrecept != null)
            {
                Pawn pawn = __instance.pawn;

                var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
                if (tracker != null && tracker.GetLastFavorTick(pawn) == tick)
                    return;


                float moodOffset = newThought.MoodOffset();
                if (moodOffset < 0f)
                {
                    float negativeMultiplier = 0.5f;
                    tracker.AddFavor(pawn, moodOffset * negativeMultiplier);
                    if (Prefs.DevMode)
                    {
                        Log.Message($"[TheGodsAreReal]: Thought '{newThought.def.defName}' caused {pawn.LabelShort}'s favor to change by: {moodOffset * negativeMultiplier}");
                    }
                }
                else if (moodOffset > 0f)
                {
                    float positiveMultiplier = 0.75f;
                    tracker.AddFavor(pawn, moodOffset * positiveMultiplier);
                    if (Prefs.DevMode)
                    {
                        Log.Message($"[TheGodsAreReal]: Thought '{newThought.def.defName}' caused {pawn.LabelShort}'s favor to change by: {moodOffset * positiveMultiplier}");
                    }
                }
            }
        }
    }
}
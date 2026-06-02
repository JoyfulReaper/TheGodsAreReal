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
using TheGodsAreReal;
using Verse;

// TODO: Balance favor

[HarmonyPatch(typeof(RimWorld.JobDriver_Meditate), "MeditationTick")]
public static class JobDriver_Meditate_MeditationTick
{
    static void Postfix(RimWorld.JobDriver_Meditate __instance)
    {
        // Check if the current job is indeed the prayer one
        if (__instance.job?.def == JobDefOf.MeditatePray)
        {
            Pawn p = __instance.pawn;

            if(Find.TickManager.TicksGame % 250 == 0)
            {
                var favorTracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
                if (favorTracker != null)
                {
                    float favorGained = 0.5f;
                    favorTracker.AddFavor(p, favorGained);

                    if (Prefs.DevMode)
                    {
                        Log.Message($"[TheGodsAreReal] {p.LabelShort} is actively praying. Gained {favorGained} favor.");
                    }
                }
            }
        }
    }
}
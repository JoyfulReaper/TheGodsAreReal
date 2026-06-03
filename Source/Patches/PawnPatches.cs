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
using System.Collections.Generic;
using Verse;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.GetGizmos))]
    public static class Pawn_GetGizmos
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn __instance)
        {
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }

            if (!Prefs.DevMode)
                yield break;

            if (__instance == null || __instance.Ideo?.KeyDeityName == null)
                yield break;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                yield break;

            float individualFavor = tracker.GetFavor(__instance);
            float ideoFavor = tracker.GetIdeoFavor(__instance.Ideo);

            yield return new Command_Action
            {
                defaultLabel = "DEV: Check Favor",
                defaultDesc = $"Click to dump divine standing for {__instance.LabelShort} to the Message Area.",
                icon = TexCommand.DesirePower,
                action = delegate
                {
                    Messages.Message($"[TheGodsAreReal] {__instance.LabelShort.ToUpper()}: -> Individual Favor: {individualFavor:F1} / 100.0 -> Ideo Avg Favor:   {ideoFavor:F1}", MessageTypeDefOf.NeutralEvent);
                    Log.Message($"========================================");
                    Log.Message($"[TheGodsAreReal] DEBUG FOR {__instance.LabelShort.ToUpper()}:");
                    Log.Message($"-> Individual Favor: {individualFavor:F1} / 100.0");
                    Log.Message($"-> Ideo Avg Favor:   {ideoFavor:F1}");
                    Log.Message($"========================================");
                }
            };
        }
    }
}
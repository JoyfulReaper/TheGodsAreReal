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
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheGodsAreReal.Utilities
{
    internal class GodsAreRealEventUtility
    {
        private const float favorForWrath = -70f;
        private const float favorForBlessing = 80f;

        public static void TriggerDivineEvent(Ideo ideo)
        {
            var tracker = Find.World.GetComponent<WorldComponent_FavorTracker>();
            float avgFavor = tracker.GetIdeoFavor(ideo);

            var members = GodsAreRealPawnUtility.GetAllIdeoMembers(ideo);

            if (avgFavor < favorForWrath)
            {
                // Example Wrath Event
                Pawn target = members.RandomElementWithFallback();
                if (target != null && target.Spawned)
                {
                    GenExplosion.DoExplosion(target.Position, target.Map, 1.5f, DamageDefOf.Flame, null);
                    Messages.Message("The Gods are angry at your lack of faith!", target, MessageTypeDefOf.NegativeEvent);
                }
            }
            else if (avgFavor > favorForBlessing)
            {
                // Example Blessing Event
                Pawn target = members.RandomElementWithFallback();
                target?.mindState?.inspirationHandler?.TryStartInspiration(InspirationDefOf.Inspired_Creativity);
                Messages.Message($"The Gods have blessed your colony! {target.LabelShort} has become inspired!", target, MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}
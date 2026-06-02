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

using Verse;
using RimWorld;

namespace TheGodsAreReal
{
    public class ThoughtWorker_GodFavor : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.Ideo == null || p.Ideo.KeyDeityName == null)
                return ThoughtState.Inactive;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return ThoughtState.Inactive;

            float individualFavor = tracker.GetFavor(p);
            float ideoFavor = tracker.GetIdeoFavor(p.Ideo);

            if (individualFavor < -75f)
            {
                return ThoughtState.ActiveAtStage(2); // "Divine Wrath" (Severe -Mood)
            }

            if (individualFavor > 80f)
            {
                // The colony is also highly aligned!
                if (ideoFavor > 70f)
                {
                    return ThoughtState.ActiveAtStage(0); // "Divine Grace" (+Mood)
                }

                // The colony is dragging the god's name down
                if (ideoFavor < -25f)
                {
                    return ThoughtState.ActiveAtStage(1); // "Sorrow of the Faithful" (Minor -Mood)
                }

                // Fallback: The colony is just average, but the pawn is still personally blessed
                return ThoughtState.ActiveAtStage(0);
            }

            return ThoughtState.Inactive;
        }
    }
}
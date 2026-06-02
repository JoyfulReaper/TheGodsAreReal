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
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace TheGodsAreReal
{
    public class WorldComponent_FavorTracker : WorldComponent
    {
        // We use the Pawn's unique ID as the key for performance
        public Dictionary<int, float> pawnFavor = new Dictionary<int, float>();

        public WorldComponent_FavorTracker(World world) : base(world) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref pawnFavor, "pawnFavor", LookMode.Value, LookMode.Value);
        }

        public void AddFavor(Pawn pawn, float amount)
        {
            int id = pawn.thingIDNumber;
            if (!pawnFavor.ContainsKey(id))
            {
                pawnFavor[id] = 0f;
            }
            pawnFavor[id] += amount;
        }

        public float GetFavor(Pawn pawn)
        {
            if (pawnFavor.TryGetValue(pawn.thingIDNumber, out float favor))
            {
                return favor;
            }
            return 0f;
        }

        public bool PawnWorships(Pawn pawn, PreceptDef godPrecept)
        {
            return pawn.Ideo != null && pawn.Ideo.HasPrecept(godPrecept);
        }

        public float GetIdeoFavor(Ideo ideo)
        {
            float totalFavor = 0f;
            int pawnCount = 0;
            var pawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction;

            foreach (var kvp in pawnFavor)
            {
                Pawn p = pawns.FirstOrDefault(x => x.thingIDNumber == kvp.Key);
                if (p != null && p.Faction == Faction.OfPlayer && p.Ideo == ideo)
                {
                    totalFavor += kvp.Value;
                    pawnCount++;
                }
            }
            return pawnCount > 0 ? totalFavor / pawnCount : 0f;
        }



        // // // DEBUG // // //

        // DEBUG METHOD
        public void DebugAddFavor(Pawn pawn, float amount)
        {
            AddFavor(pawn, amount);
            float newFavor = GetFavor(pawn);
            Log.Message($"[TheGodsAreReal] Debug: Added {amount} favor to {pawn.Name}. New total: {newFavor}");
        }

        // DEBUG METHOD
        public void DebugRemoveFavor(Pawn pawn, float amount)
        {
            AddFavor(pawn, -amount);
            float newFavor = GetFavor(pawn);
            Log.Message($"[TheGodsAreReal] Debug: Added {amount} favor to {pawn.Name}. New total: {newFavor}");
        }
    }
}
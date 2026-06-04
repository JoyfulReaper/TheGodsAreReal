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
    public static class GodsAreRealPawnUtility
    {
        public static List<Pawn> GetAllColonyPawns()
        {
            List<Pawn> allLivingPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive;
            List<Pawn> result = new List<Pawn>(allLivingPawns.Count);

            for (int i = 0; i < allLivingPawns.Count; i++)
            {
                Pawn pawn = allLivingPawns[i];

                if ((pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony) && pawn.RaceProps.Humanlike)
                {
                    result.Add(pawn);
                }
            }

            return result;
        }

        public static List<Pawn> GetAllColonyPawnsExceptPrisoners()
        {
            List<Pawn> allLivingPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive;
            List<Pawn> result = new List<Pawn>(allLivingPawns.Count);

            for (int i = 0; i < allLivingPawns.Count; i++)
            {
                Pawn pawn = allLivingPawns[i];

                if ((pawn.IsColonist || pawn.IsSlaveOfColony) && pawn.RaceProps.Humanlike)
                {
                    result.Add(pawn);
                }
            }

            return result;
        }

        public static List<Pawn> GetAllFreeColonyPawns()
        {
            return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;
        }

        public static List<Pawn> GetAllColonyPrisoners()
        {
            return PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony;
        }

        public static List<Pawn> GetAllColonyPrisonersOnMap(Map map)
        {
            return map.mapPawns.PrisonersOfColonySpawned;
        }

        public static List<Pawn> GetAllColonyPawnsOnMap(Map map)
        {
            var allSpawned = map.mapPawns.AllPawnsSpawned;
            List<Pawn> result = new List<Pawn>();

            for (int i = 0; i < allSpawned.Count; i++)
            {
                Pawn p = allSpawned[i];
                if ((p.IsColonist || p.IsSlaveOfColony) && p.RaceProps.Humanlike)
                {
                    result.Add(p);
                }
            }
            return result;
        }

        public static List<Pawn> GetAllFreePawnsOnMap(Map map)
        {
            return map.mapPawns.FreeColonists;
        }

        public static List<Pawn> GetAllSlavePawnsOnMap(Map map)
        {
            return map.mapPawns.SlavesOfColonySpawned;
        }

        public static List<Pawn> GetDownedColonyPawns(Map map)
        {
            var pawns = map.mapPawns.AllPawnsSpawned;
            List<Pawn> result = new List<Pawn>();
            for (int i = 0; i < pawns.Count; i++)
            {
                if ((pawns[i].IsColonist || pawns[i].IsSlaveOfColony) && pawns[i].Downed && pawns[i].RaceProps.Humanlike)
                    result.Add(pawns[i]);
            }
            return result;
        }

        public static List<Pawn> GetCapableColonyPawns(Map map)
        {
            var freeColonists = map.mapPawns.AllPawnsSpawned;
            List<Pawn> result = new List<Pawn>(freeColonists.Count);

            for (int i = 0; i < freeColonists.Count; i++)
            {
                Pawn p = freeColonists[i];
                if (p.Awake() && !p.Downed && !p.InMentalState && (p.IsColonist || p.IsSlaveOfColony) && p.RaceProps.Humanlike)
                {
                    result.Add(p);
                }
            }
            return result;
        }

        public static bool IsPawnConsciousAndActive(Pawn p)
        {
            return p.Spawned && !p.Dead && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && !p.Downed;
        }

        public static List<Pawn> GetPawnsInMentalState(Map map, MentalStateDef stateDef)
        {
            var allPawns = map.mapPawns.AllPawnsSpawned;
            List<Pawn> result = new List<Pawn>();
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn p = allPawns[i];
                if (p.InMentalState && p.MentalStateDef == stateDef)
                    result.Add(p);
            }
            return result;
        }

        public static List<Pawn> GetPawnsCurrentlyPraying(Map map)
        {
            return map.mapPawns.FreeColonists.Where(p => p.CurJobDef == JobDefOf.MeditatePray).ToList();
        }

        public static List<Pawn> GetColonyPawnsByFaction(Map map, Faction faction)
        {
            var pawns = map.mapPawns.AllPawnsSpawned;
            List<Pawn> result = new List<Pawn>();
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].Faction == faction)
                    result.Add(pawns[i]);
            }
            return result;
        }
    }
}
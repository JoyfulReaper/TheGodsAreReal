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

// TODO: We might want to consider changing these from returning lists to using IEnumerable or Yeild Return
// This will avoid allocating new Lists on the heap. Trade off is if you need a list you do a .ToList() and
// just lost the benefits. Ok I did this for all except the methods that just wrap rimworlds methods.

namespace TheGodsAreReal.Utilities
{
    /// <summary>
    /// Utility to make it easier to work with human pawns
    /// </summary>
    public static class GodsAreRealPawnUtility
    {
        /// <summary>
        /// All of the colony pawns including slaves and prisoners
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetAllColonyPawns()
        {
            var allLivingPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive;

            for (int i = 0; i < allLivingPawns.Count; i++)
            {
                Pawn pawn = allLivingPawns[i];

                if ((pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony) && pawn.RaceProps.Humanlike)
                {
                    yield return pawn;
                }
            }
        }

        /// <summary>
        /// All of the colony pawns including slaves, but not prisoners
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetAllColonyPawnsExceptPrisoners()
        {
            var allLivingPawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive;

            for (int i = 0; i < allLivingPawns.Count; i++)
            {
                Pawn pawn = allLivingPawns[i];

                if ((pawn.IsColonist || pawn.IsSlaveOfColony) && pawn.RaceProps.Humanlike)
                {
                    yield return pawn;
                }
            }
        }

        /// <summary>
        /// All of the colony pawns that are "free" (not slave/prisoner)
        /// </summary>
        /// <returns></returns>
        public static List<Pawn> GetAllFreeColonyPawns() => 
            PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_FreeColonists;

        /// <summary>
        /// All of the colony's priosners
        /// </summary>
        /// <returns></returns>
        public static List<Pawn> GetAllColonyPrisoners() => 
            PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony;

        /// <summary>
        /// All of the colony's priosners
        /// </summary>
        /// <param name="map">Map to check</param>
        /// <returns>Priosners on map</returns>
        public static List<Pawn> GetAllColonyPrisonersOnMap(Map map) => 
            map?.mapPawns.PrisonersOfColonySpawned ?? new List<Pawn>();

        /// <summary>
        /// All of the colony's pawns on map
        /// </summary>
        /// <param name="map">Maps to check</param>
        /// <returns>All colony's pawns excluding prisoners</returns>
        public static IEnumerable<Pawn> GetAllColonyPawnsOnMap(Map map)
        {
            if (map == null)
            {
                yield break;
            }

            var allSpawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allSpawned.Count; i++)
            {
                Pawn p = allSpawned[i];
                if ((p.IsColonist || p.IsSlaveOfColony) && p.RaceProps.Humanlike)
                {
                    yield return p;
                }
            }
        }

        /// <summary>
        /// All free colonists on map
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static List<Pawn> GetAllFreePawnsOnMap(Map map) => 
            map?.mapPawns.FreeColonists ?? new List<Pawn>();

        /// <summary>
        /// All Slaves on map
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static List<Pawn> GetAllSlavePawnsOnMap(Map map) => 
            map?.mapPawns.SlavesOfColonySpawned ?? new List<Pawn>();

        /// <summary>
        /// All Downed Colonists on map
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetDownedColonyPawns(Map map)
        {
            if (map == null)
            {
                yield break;
            }

            var pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                if ((pawns[i].IsColonist || pawns[i].IsSlaveOfColony) && pawns[i].Downed && pawns[i].RaceProps.Humanlike)
                    yield return (pawns[i]);
            }
        }

        /// <summary>
        /// All Colonists and slaves that aren't busy downed or mental break
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetCapableColonyPawns(Map map)
        {
            if (map == null)
            {
                yield break;
            }

            var freeColonists = map.mapPawns.AllPawnsSpawned;

            for (int i = 0; i < freeColonists.Count; i++)
            {
                Pawn p = freeColonists[i];
                if (p.Awake() && !p.Downed && !p.InMentalState && (p.IsColonist || p.IsSlaveOfColony) && p.RaceProps.Humanlike)
                {
                    yield return p;
                }
            }
        }

        /// <summary>
        /// Pawn is capable of minipulation
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool IsPawnConsciousAndActive(Pawn p) => 
            p.Spawned && !p.Dead && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && !p.Downed;

        /// <summary>
        /// Get all pawns having a given mental state
        /// </summary>
        /// <param name="map"></param>
        /// <param name="stateDef"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetPawnsInMentalState(Map map, MentalStateDef stateDef)
        {
            if (map == null)
            {
                yield break;
            }

            var allPawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allPawns.Count; i++)
            {
                Pawn p = allPawns[i];
                if (p.InMentalState && p.MentalStateDef == stateDef)
                    yield return p;
            }
        }

        /// <summary>
        /// Get all pawns that are currently praying (Free colonists only)
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetPawnsCurrentlyPraying(Map map)
        {
            if (map == null)
            {
                yield break;
            }

            var freeColonists = map.mapPawns.FreeColonists;

            for (int i = 0; i < freeColonists.Count; i++)
            {
                Pawn p = freeColonists[i];
                if (p.CurJobDef == JobDefOf.MeditatePray)
                {
                    yield return p;
                }
            }
        }

        /// <summary>
        /// Get all pawns by faction
        /// </summary>
        /// <param name="map"></param>
        /// <param name="faction"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetColonyPawnsByFaction(Map map, Faction faction)
        {
            if (map == null)
            {
                yield break;
            }

            var pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].Faction == faction)
                    yield return pawns[i];
            }
        }

        /// <summary>
        /// ALL member of an Ideology WARNING PERFOMANCE HEAVY
        /// </summary>
        /// <param name="ideo"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetAllIdeoMembers(Ideo ideo)
        {
            var allLivingPawns = PawnsFinder.AllMapsAndWorld_Alive;

            for (int i = 0; i < allLivingPawns.Count; i++)
            {
                Pawn pawn = allLivingPawns[i];
                if (pawn.Ideo == ideo && pawn.RaceProps.Humanlike)
                {
                    yield return pawn;
                }
            }
        }

        /// <summary>
        /// Get all memember of an ideo on the given map
        /// </summary>
        /// <param name="ideo"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetAllIdeoMembers(Ideo ideo, Map map)
        {
            if (map == null)
            {
                yield break;
            }

            var allSpawned = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < allSpawned.Count; i++)
            {
                Pawn p = allSpawned[i];
                if (p.Ideo == ideo && p.RaceProps.Humanlike)
                    yield return p;
            }
        }

        /// <summary>
        /// Get all colony members for the given ideology
        /// </summary>
        /// <param name="ideo"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetColonyIdeoMembers(Ideo ideo)
        {
            foreach (Pawn pawn in GetAllColonyPawns())
            {
                if (pawn.Ideo == ideo && pawn.RaceProps.Humanlike)
                {
                    yield return pawn;
                }
            }
        }

        /// <summary>
        /// Get all colony member of the given ideology on the given map
        /// </summary>
        /// <param name="ideo"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        public static IEnumerable<Pawn> GetColonyIdeoMembers(Ideo ideo, Map map)
        {
            if (map == null)
            {
                yield break;
            }

            foreach (Pawn pawn in GetAllColonyPawnsOnMap(map))
            {
                if (pawn.Ideo == ideo && pawn.RaceProps.Humanlike)
                {
                    yield return pawn;
                }
            }
        }


        // This checks against an indvidual diety the ideo could have 0 or more dieties
        // Currently not used
        public static bool PawnWorships(Pawn pawn, PreceptDef godPrecept)
        {
            return pawn.Ideo != null && pawn.Ideo.HasPrecept(godPrecept);
        }
    }
}
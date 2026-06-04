using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

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

                if (pawn.IsColonist || pawn.IsPrisonerOfColony || pawn.IsSlaveOfColony)
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
                if (p.IsColonist || p.IsPrisonerOfColony)
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
                if (pawns[i].IsColonist && pawns[i].Downed)
                    result.Add(pawns[i]);
            }
            return result;
        }

        public static List<Pawn> GetCapableColonyPawns(Map map)
        {
            var freeColonists = map.mapPawns.FreeColonists;
            List<Pawn> result = new List<Pawn>(freeColonists.Count);

            for (int i = 0; i < freeColonists.Count; i++)
            {
                Pawn p = freeColonists[i];
                if (p.Awake() && !p.Downed && !p.InMentalState)
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
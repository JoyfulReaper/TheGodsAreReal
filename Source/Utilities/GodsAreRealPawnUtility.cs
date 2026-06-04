using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TheGodsAreReal.Utilities
{
    public static class GodsAreRealPawnUtility
    {
        public static List<Pawn> GetAllColonyPawns()
        {
            List<Pawn> colonistsAndSlaves = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists;
            List<Pawn> prisoners = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_PrisonersOfColony;

            List<Pawn> result = new List<Pawn>(colonistsAndSlaves.Count + prisoners.Count);

            result.AddRange(colonistsAndSlaves);
            result.AddRange(prisoners);

            return result;
        }
    }
}
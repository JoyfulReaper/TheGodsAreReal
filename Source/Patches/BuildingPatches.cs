
using HarmonyLib;
using RimWorld;
using System.Diagnostics;
using System.Linq;
using TheGodsAreReal.Utilities;
using Verse;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(Building), nameof(Building.Destroy))]
    public static class Building_Destroy
    {
        private const int DisableMotesThreshold = 10;

        private const float BaseFavorReward = 20f;
        private const float HighFavorPenaltyThreshold = 70f;
        private const float HighFavorPenaltyMultiplier = 0.6f;

        private const float NonBeliverFavorChangeChance = 0.5f;
        private const float NonBeliverPosFavorChangeAmount = 2f;
        private const float NonBeliverNegFavorChangeAmount = -2f;

        static void Postfix(Building __instance, DestroyMode mode)
        {
            if (mode != DestroyMode.KillFinalize && mode != DestroyMode.Deconstruct) 
                return;

            var map = __instance.Map;
            if (map == null || !IsDivineThreat(__instance.def))
                return;

            var favorTracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (favorTracker == null)
                return;

            if(Faction.OfPlayer.ideos == null || Faction.OfPlayer.ideos.PrimaryIdeo == null)
            {
                Log.Warning("[TheGodsAreReal: Building_Destroy] Player faction has no ideology or primary ideology. No favor will be granted.");
                return;
            }

            Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
            var validPawns = GodsAreRealPawnUtility.GetAllColonyPawnsOnMap(map);
            bool showMotes = validPawns.Take(DisableMotesThreshold + 1).Count() <= DisableMotesThreshold;

            foreach (var pawn in validPawns)
            {
                float indvidualReward = BaseFavorReward;
                string pawnDesc = pawn.LabelShort;

                if (pawn.Ideo != null && pawn.Ideo == primaryIdeo)
                {
                    var currentFavor = favorTracker.GetFavor(pawn);
                    if (currentFavor > HighFavorPenaltyThreshold)
                    {
                        indvidualReward *= HighFavorPenaltyMultiplier;
                    }

                    favorTracker.AddFavor(pawn, indvidualReward, showMotes);
                }
                else
                {
                    pawnDesc += " (non-beleiver)";
                    if (Rand.Value < NonBeliverFavorChangeChance)
                    {
                        indvidualReward = NonBeliverPosFavorChangeAmount;
                    }
                    else
                    {
                        indvidualReward = NonBeliverNegFavorChangeAmount;
                    }

                    favorTracker.AddFavor(pawn, indvidualReward, showMotes);
                }

                if (Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal] {pawnDesc} gained {indvidualReward} favor from destroying a Divine Threat: {__instance.LabelShort}");
                }
            }

            Messages.Message(
                "Devine Destruction Complete: The gods have observed the destruction. Favor has shifted among the participants.",
                MessageTypeDefOf.NeutralEvent
            );
        }

        private static bool IsDivineThreat(ThingDef def)
        {
            if (def.isUnfinishedThing || def.IsWall || def.IsDoor || def.BuildableByPlayer)
                return false;

            if (def.isMechClusterThreat)
                return true;

            // TODO Find a bettter way to do this
            if (def.defName.Contains("ShipPart") || def.defName.Contains("MechCluster"))
            {
                Log.Warning($"[TheGodsAreReal]: Identified threat ({def.defName}) by defName.Contains(), please verify.");
                return true;
            }

            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal: IsDivineThreat]: {def.defName} is not a threat.");
            }

            return false;
        }
    }
}
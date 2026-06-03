
using HarmonyLib;
using RimWorld;
using System.Diagnostics;
using System.Linq;
using Verse;

namespace TheGodsAreReal.Patches
{
    [HarmonyPatch(typeof(Building), nameof(Building.Destroy))]
    public static class Building_Destroy
    {
        static void Postfix(Building __instance)
        {
            if (__instance.Map == null || !IsDivineThreat(__instance.def))
                return;

            var favorTracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (favorTracker == null)
                return;

            float baseReward = 20.0f;
            if(Faction.OfPlayer.ideos == null || Faction.OfPlayer.ideos.PrimaryIdeo == null)
            {
                Log.Warning("[TheGodsAreReal: Building_Destroy] Player faction has no ideology or primary ideology. No favor will be granted.");
                return;
            }

            Ideo primaryIdeo = Faction.OfPlayer.ideos.PrimaryIdeo;
            var validPawns = __instance.Map.mapPawns.AllPawnsSpawned.Where(p => (p.IsColonist || p.IsSlave) && p.RaceProps.Humanlike);

            var showMotes = true;
            if (validPawns.Count() > 10)
            {
                showMotes = false;
            }

            foreach (var pawn in validPawns)
            {
                float indvidualReward = baseReward;
                string pawnDesc = pawn.LabelShort;

                if (pawn.Ideo == primaryIdeo)
                {
                    var currentFavor = favorTracker.GetFavor(pawn);
                    if (currentFavor > 70f)
                    {
                        indvidualReward *= 0.6f;
                    }

                    favorTracker.AddFavor(pawn, indvidualReward, showMotes);
                }
                else
                {
                    pawnDesc += " (non-beleiver)";
                    if (Rand.Value < 0.5f)
                    {
                        indvidualReward = -2f;
                    }
                    else
                    {
                        indvidualReward = 2f;
                    }

                    favorTracker.AddFavor(pawn, indvidualReward, showMotes);
                }

                if (Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal] {pawnDesc} gained {indvidualReward} favor from destroying a Divine Threat: {__instance.LabelShort}");
                }
            }

            if (!showMotes)
            {
                Messages.Message(
                    "Devine Destruction Complete: The gods have observed the destruction. Favor has shifted among the participants.",
                    MessageTypeDefOf.NeutralEvent
                );
            }
        }

        private static bool IsDivineThreat(ThingDef def)
        {
            if (def.isUnfinishedThing || def.IsWall || def.IsDoor || def.BuildableByPlayer)
                return false;

            if (def.isMechClusterThreat)
                return true;

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
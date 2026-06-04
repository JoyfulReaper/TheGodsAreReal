using RimWorld;
using Verse;
using System.Collections.Generic;

namespace TheGodsAreReal.Handlers
{
    [StaticConstructorOnStartup]
    public static class PawnDeathHandler
    {
        public static void HandlePawnDeath(Pawn pawn, float favor)
        {
            if (pawn == null || !pawn.RaceProps.Humanlike)
                return;

            if (pawn.Map == null || pawn.Ideo == null)
                return;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;

            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal] Processing death of {pawn.LabelShort}. Favor was {favor}.");
            }

            if (favor <= -75f)
            {
                // WRATH: The gods smite the corpse of the forsaken.
                GenExplosion.DoExplosion(pawn.Position, pawn.Map, 2.9f, DamageDefOf.EMP, null);
                FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, 2.0f);

                Messages.Message($"The gods have violently rejected {pawn.LabelShort}'s soul!", new TargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.NegativeEvent);
            }
            else if (favor >= 75f)
            {
                // GRACE: The gods bless the site of their favored martyr.
                FleckMaker.ThrowDustPuff(pawn.Position, pawn.Map, 1.5f);

                ThingDef ambrosiaDef = DefDatabase<ThingDef>.GetNamed("Ambrosia", errorOnFail: false);
                if (ambrosiaDef != null)
                {
                    Thing gift = ThingMaker.MakeThing(ambrosiaDef);
                    gift.stackCount = 5;
                    GenPlace.TryPlaceThing(gift, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }

                Messages.Message($"The gods weep for {pawn.LabelShort}, leaving a divine gift behind.", new TargetInfo(pawn.Position, pawn.Map), MessageTypeDefOf.PositiveEvent);
            }

            List<Pawn> allColonists = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists;

            foreach (Pawn colonist in allColonists)
            {
                if (colonist == pawn || colonist.Ideo == null || !colonist.RaceProps.Humanlike)
                    continue;

                bool sameIdeo = colonist.Ideo == pawn.Ideo;

                if (favor >= 50f)
                {
                    if (sameIdeo)
                    {
                        tracker.AddFavor(colonist, 12.5f, false);
                    }
                    else
                    {
                        tracker.AddFavor(colonist, -7.5f, false);
                    }
                }
                else if (favor <= -50f)
                {
                    if (sameIdeo)
                    {
                        tracker.AddFavor(colonist, -15.0f, false);
                    }
                    else
                    {
                        tracker.AddFavor(colonist, 10.0f, false);
                    }
                }
            }
        }
    }
}
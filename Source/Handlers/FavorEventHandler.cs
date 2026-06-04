using Verse;

namespace TheGodsAreReal.Handlers
{
    [StaticConstructorOnStartup]
    public static class FavorEventHandler
    {
        public static void HandlePawnDeath(Pawn pawn, float favor)
        {
            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal] Processing death of {pawn.LabelShort}. Favor was {favor}.");
            }

            // TODO: WE NEED SOME FUCKING LOGIC HERE, DON'T WE SON?
        }
    }
}
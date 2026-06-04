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

namespace TheGodsAreReal.Handlers
{
    public static class ConversionHandler
    {
        private const float BaseConverterReward = 2f;
        private const float BaseConverteePenalty = -1f;
        private const float SuccessfullConversionReward = 10f;

        public static void HandleConversionFavor(Pawn initiator, Pawn recipient, bool wasSuccessful, string logContext)
        {
            if (initiator == null || recipient == null)
                return;

            if (!initiator.RaceProps.Humanlike || !recipient.RaceProps.Humanlike)
                return;

            var tracker = Find.World?.GetComponent<WorldComponent_FavorTracker>();
            if (tracker == null)
                return;

            var converterReward = BaseConverterReward;
            if (wasSuccessful)
            {
                converterReward = SuccessfullConversionReward;
            }

            // Reward the converter
            if (initiator.IsColonist || initiator.IsSlaveOfColony)
            {
                tracker.AddFavor(initiator, converterReward, showMote: true);
            }

            // Punish the convertee
            if (recipient.IsColonist || recipient.IsSlaveOfColony)
            {
                tracker.AddFavor(recipient, BaseConverteePenalty, showMote: true);
            }

            if (Prefs.DevMode)
            {
                string status = wasSuccessful ? "SUCCESS" : "FAILED";
                Log.Message($"[TheGodsAreReal] {logContext} [{status}]: {initiator.LabelShort} (+{converterReward}) preached to {recipient.LabelShort}.");
            }
        }
    }
}
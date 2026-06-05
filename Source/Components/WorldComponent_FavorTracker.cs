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
using TheGodsAreReal.Handlers;
using TheGodsAreReal.Utilities;
using UnityEngine;
using Verse;

namespace TheGodsAreReal
{
    public class WorldComponent_FavorTracker : WorldComponent
    {
        // Core game dictionaries
        private Dictionary<Pawn, float> _pawnFavor = new Dictionary<Pawn, float>(); // Key: Pawn, Value: Favor score (-100.0 to 100.0) for key
        private Dictionary<Pawn, int> _lastFavorTick = new Dictionary<Pawn, int>(); // Key: Pawn, Value: Last game tick when favor was updated for key

        // Used by Scribe for saving/loading the dictionaries
        private List<Pawn> _pawnFavorKeys = new List<Pawn>();
        private List<float> _pawnFavorValues = new List<float>();

        private List<Pawn> _lastFavorTickKeys = new List<Pawn>();
        private List<int> _lastFavorTickValues = new List<int>();

        private List<Ideo> _ideoFavorKeys = new List<Ideo>();
        private List<float> _ideoFavorValues = new List<float>();

        private List<Ideo> _ideoCountKeys = new List<Ideo>();
        private List<int> _ideoCountValues = new List<int>();


        private bool _suppressAllMotes = false; // Not currently used

        private const float MinimumMoteThreshold = 0.5f;
        private const float MaxFavor = 100;
        private const float MinFavor = -100;

        // Favor decay variables
        private const int _decayIntervalTicks = 2500; // Run once every 2,500 ticks (approx. 1 game hour)
        private const float _passiveDecayAmount = 0.05f; // How much favor slips away per interval

        // Dedicated zero-allocation operational buffers
        private readonly List<Pawn> _tempFavorKeys = new List<Pawn>();
        private readonly List<Pawn> _tempPurgeList = new List<Pawn>();
        private Dictionary<Ideo, float> _ideoFavorCache = new Dictionary<Ideo, float>();
        private Dictionary<Ideo, int> _ideoPawnCountCache = new Dictionary<Ideo, int>();

        // Hediff just used for testing right now
        private static readonly HediffDef _divineTouchDef = HediffDef.Named("TheGodsAreReal_DivineTouch");

        private const int RareTickValue = 250;

        private TheGodsAreRealSettings Settings => TheGodsAreRealMod.Settings;

        public IReadOnlyDictionary<Pawn, float> PawnFavor =>
            _pawnFavor;


        public WorldComponent_FavorTracker(World world) : base(world) { }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            // Decay Favor
            if (Find.TickManager.TicksGame % _decayIntervalTicks == 0)
            {
                DecayPassiveFavor();
            }

            // Rare Tick //
            if (Find.TickManager.TicksGame % RareTickValue != 0)
                return;

            RareTick();
        }

        private void RareTick()
        {
            ApplyHediffs();
        }

        // TODO: This will need to be moved into a different file
        private void ApplyHediffs()
        {
            foreach (Pawn pawn in GodsAreRealPawnUtility.GetAllColonyPawns())
            {
                if (pawn == null || pawn.Dead || !pawn.RaceProps.Humanlike || pawn.Ideo?.KeyDeityName == null)
                    continue;

                UpdatePawnDivineHediff(pawn);
            }
        }

        // TODO: This will need to be moved into a different file
        private void UpdatePawnDivineHediff(Pawn pawn)
        {
            float favor = this.GetFavor(pawn);
            float normalizedSeverity = Mathf.Clamp01((favor + 100f) / 200f);
            // Map the favor score (-100.0 to 100.0) to a C# normalized float scale (0.0 to 1.0)
            // -100 favor becomes 0.0 severity (Pure Wrath)
            //    0 favor becomes 0.5 severity (Neutral / Hidden)
            // +100 favor becomes 1.0 severity (Pure Grace)

            // Look for an existing instance of our Hediff on the pawn
            Hediff existingHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(_divineTouchDef);

            if (existingHediff != null)
            {
                // Update the severity level dynamically as their behavior changes
                existingHediff.Severity = normalizedSeverity;
            }
            else
            {
                // If they don't have the hediff yet, create it and inject it into their health tracker
                Hediff newHediff = HediffMaker.MakeHediff(_divineTouchDef, pawn);
                newHediff.Severity = normalizedSeverity;
                pawn.health?.AddHediff(newHediff);
            }
        }


        private void DecayPassiveFavor()
        {
            if (_pawnFavor.Count == 0)
                return;

            _tempFavorKeys.Clear();
            _tempPurgeList.Clear();

            foreach (KeyValuePair<Pawn, float> kvp in _pawnFavor)
            {
                _tempFavorKeys.Add(kvp.Key);
            }

            for (int i = 0; i < _tempFavorKeys.Count; i++)
            {
                Pawn pawn = _tempFavorKeys[i];

                if (pawn == null || pawn.Destroyed || pawn.Dead || !pawn.RaceProps.Humanlike)
                {
                    _tempPurgeList.Add(pawn);
                    continue;
                }

                float favor = _pawnFavor[pawn];

                if (favor > 0f)
                    _pawnFavor[pawn] = Mathf.Max(0f, favor - _passiveDecayAmount);
                else if (favor < 0f)
                    _pawnFavor[pawn] = Mathf.Min(0f, favor + _passiveDecayAmount);
            }

            // Execute purge
            for (int i = 0; i < _tempPurgeList.Count; i++)
            {
                _pawnFavor.Remove(_tempPurgeList[i]);
                _lastFavorTick.Remove(_tempPurgeList[i]);
            }

            _tempFavorKeys.Clear();
            _tempPurgeList.Clear();

            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal] Background divine decay processed for {_pawnFavor.Count} tracked pawns.");
            }
        }


        /// <summary>
        /// Adds favor to a pawm
        /// </summary>
        /// <param name="pawn">The pawn to add favor to</param>
        /// <param name="amount">The amount of favor to add, can be negative</param>
        public void AddFavor(Pawn pawn, float amount, bool showMote = true)
        {
            if (pawn == null || !pawn.RaceProps.Humanlike)
                return;

            if (!pawn.IsColonist && !pawn.IsSlaveOfColony)
                return;

            if (pawn.Ideo == null)
                return;

            // If the Ideo doesn't have a Deity then don't track favor
            if (pawn.Ideo.KeyDeityName == null)
                return;

            int tick = Find.TickManager.TicksGame;
            // If the same pawn gained favor in the same tick, ignore the duplicate
            if (_lastFavorTick.TryGetValue(pawn, out int lastTick) && tick == lastTick)
                return;

            float oldFavor = GetFavor(pawn);
            _pawnFavor[pawn] = Mathf.Clamp(oldFavor + amount, MinFavor, MaxFavor);

            // Update Cache
            if (pawn.Ideo != null)
            {
                float delta = _pawnFavor[pawn] - oldFavor;

                if (!_ideoFavorCache.ContainsKey(pawn.Ideo))
                {
                    _ideoFavorCache[pawn.Ideo] = 0f;
                    _ideoPawnCountCache[pawn.Ideo] = 0;
                }

                _ideoFavorCache[pawn.Ideo] += delta;
                // Increment count only if this is a new pawn tracking entry
                if (!_lastFavorTick.ContainsKey(pawn)) _ideoPawnCountCache[pawn.Ideo]++;
            }

            if (pawn.Spawned && ((Mathf.Abs(amount) >= MinimumMoteThreshold && showMote && !_suppressAllMotes) || Settings.AlwaysShowMotes))
            {
                Color favorColor = (amount >= 0) ? Color.cyan : Color.red;
                string text = (amount > 0) ? $"+{amount} Favor" : $"{amount} Favor";
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, text, favorColor, 3.6f);
            }

            _lastFavorTick[pawn] = tick;
        }

        /// <summary>
        /// Get a pawn's favor level
        /// </summary>
        /// <param name="pawn">The target pawn</param>
        /// <returns>The favor level of the targeted pawn</returns>
        public float GetFavor(Pawn pawn)
        {
            if (_pawnFavor.TryGetValue(pawn, out float favor))
            {
                return favor;
            }
            return 0f;
        }

        /// <summary>
        /// Reset a pawn's favor
        /// </summary>
        /// <param name="pawn">The pawn to reset</param>
        /// <param name="newValue">The amount of favor to assign if not zero</param>
        public void ResetFavor(Pawn pawn, float newValue = 0)
        {
            if (pawn == null)
                return;

            if (_pawnFavor.ContainsKey(pawn))
                _pawnFavor[pawn] = newValue;

            if (Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal] Reset favor for {pawn.LabelShort} due to ideology change.");
            }
        }

        /// <summary>
        /// Get the tick of the last favor change for a pawn
        /// </summary>
        /// <param name="pawn">Target Pawn</param>
        /// <returns>Last tick favor changed</returns>
        public int GetLastFavorTick(Pawn pawn)
        {
            if (pawn == null)
                return -1;

            return _lastFavorTick.TryGetValue(pawn, out int tick) ? tick : -1;
        }

        /// <summary>
        /// Get the favor score of an Ideology
        /// </summary>
        /// <param name="ideo">The ideo to score</param>
        /// <returns>The favor score of the Ideo (avg of pawn favor for ideo)</returns>
        public float GetIdeoFavor(Ideo ideo)
        {
            if (ideo == null || !_ideoFavorCache.ContainsKey(ideo))
                return 0f;

            int count = _ideoPawnCountCache[ideo];
            return count > 0 ? _ideoFavorCache[ideo] / count : 0f;
        }

        /// <summary>
        /// Clears the favor dictionay
        /// </summary>
        public void ClearAllPawnFavor()
        {
            _pawnFavor.Clear();
            Log.Warning("!!![TheGodsAreReal] All favor data DELETED!!!");
        }

        /// <summary>
        /// Run the pawn death handler for the given pawn
        /// </summary>
        /// <param name="pawn">The pawn that died</param>
        public void Notify_PawnDied(Pawn pawn)
        {
            if (pawn.Ideo != null && _pawnFavor.TryGetValue(pawn, out float favor))
            {
                if (_ideoFavorCache.ContainsKey(pawn.Ideo))
                {
                    _ideoFavorCache[pawn.Ideo] -= favor;
                    _ideoPawnCountCache[pawn.Ideo]--;
                }

                PawnDeathHandler.HandlePawnDeath(pawn, favor);
            }

            // clean up the tracker data
            _pawnFavor.Remove(pawn);
            _lastFavorTick.Remove(pawn);
        }

        public void Notify_PawnIdeoChanged(Pawn pawn, Ideo oldIdeo, Ideo newIdeo)
        {
            if (pawn == null)
                return;

            if (oldIdeo != null && _ideoFavorCache.ContainsKey(oldIdeo))
            {
                float favor = GetFavor(pawn);
                _ideoFavorCache[oldIdeo] -= favor;
                _ideoPawnCountCache[oldIdeo]--;
            }

            if (newIdeo != null)
            {
                if (!_ideoFavorCache.ContainsKey(newIdeo))
                {
                    _ideoFavorCache[newIdeo] = 0f;
                    _ideoPawnCountCache[newIdeo] = 0;
                }

                float favor = GetFavor(pawn);
                _ideoFavorCache[newIdeo] += favor;
                _ideoPawnCountCache[newIdeo]++;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            // Scribe needs the ref lists to hold data temporarily during the loading phase
            Scribe_Collections.Look(ref _pawnFavor, "pawnFavor", LookMode.Reference, LookMode.Value, ref _pawnFavorKeys, ref _pawnFavorValues);
            Scribe_Collections.Look(ref _lastFavorTick, "lastFavorTick", LookMode.Reference, LookMode.Value, ref _lastFavorTickKeys, ref _lastFavorTickValues);
            Scribe_Collections.Look(ref _ideoFavorCache, "ideoFavorCache", LookMode.Reference, LookMode.Value, ref _ideoFavorKeys, ref _ideoFavorValues);
            Scribe_Collections.Look(ref _ideoPawnCountCache, "ideoCountCache", LookMode.Reference, LookMode.Value, ref _ideoCountKeys, ref _ideoCountValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_pawnFavor == null) _pawnFavor = new Dictionary<Pawn, float>();
                if (_lastFavorTick == null) _lastFavorTick = new Dictionary<Pawn, int>();
                if (_ideoFavorCache == null) _ideoFavorCache = new Dictionary<Ideo, float>();
                if (_ideoPawnCountCache == null) _ideoPawnCountCache = new Dictionary<Ideo, int>();

                // Drop any entries that failed to cross-reference on load
                _pawnFavor.RemoveAll(kvp => kvp.Key == null);
                _lastFavorTick.RemoveAll(kvp => kvp.Key == null);

            }
        }
    }
}
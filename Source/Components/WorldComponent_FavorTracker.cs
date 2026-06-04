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
using System.Linq;
using TheGodsAreReal.Handlers;
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
        private List<Pawn> _pawnFavorKeys;
        private List<float> _pawnFavorValues;
        private List<Pawn> _lastFavorTickKeys;
        private List<int> _lastFavorTickValues;

        private bool _suppressAllMotes = false; // Not currently used

        // Favor decay variables
        private const int _decayIntervalTicks = 2500; // Run once every 2,500 ticks (approx. 1 game hour)
        private const float _passiveDecayAmount = 0.05f; // How much favor slips away per interval

        // Hediff just used for testing right now
        private static readonly HediffDef _divineTouchDef = HediffDef.Named("TheGodsAreReal_DivineTouch");

        private TheGodsAreRealSettings Settings => LoadedModManager.GetMod<TheGodsAreRealMod>().GetSettings<TheGodsAreRealSettings>();

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
            if (Find.TickManager.TicksGame % 250 != 0)
                return;

            RareTick();
        }

        private void RareTick()
        {
            ApplyHediffs();
        }

        // TODO Make a handler for hediffs
        private void ApplyHediffs()
        {
            // Grab all maps currently loaded in the game
            List<Map> maps = Find.Maps;
            for (int m = 0; m < maps.Count; m++)
            {
                // Grab all colonists on the current map
                List<Pawn> freeColonists = maps[m].mapPawns.FreeColonists;
                for (int p = 0; p < freeColonists.Count; p++)
                {
                    Pawn pawn = freeColonists[p];

                    if (pawn == null || !pawn.Spawned || pawn.Dead)
                        continue;

                    if (pawn.Ideo?.KeyDeityName == null || !pawn.RaceProps.Humanlike)
                        continue;

                    UpdatePawnDivineHediff(pawn);
                }

                List<Pawn> slaves = maps[m].mapPawns.SlavesAndPrisonersOfColonySpawned;
                for (int s = 0; s < slaves.Count; s++)
                {
                    Pawn pawn = slaves[s];
                    if (pawn != null && pawn.Spawned && !pawn.Dead && pawn.Ideo?.KeyDeityName != null)
                    {
                        UpdatePawnDivineHediff(pawn);
                    }
                }
            }
        }

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

            List<Pawn> keys = _pawnFavor.Keys.ToList();
            List<Pawn> toPurge = new List<Pawn>();

            for (int i = 0; i < keys.Count; i++)
            {
                Pawn pawn = keys[i];


                if (pawn == null || pawn.Destroyed || pawn.Dead)
                {
                    toPurge.Add(pawn);
                    continue;
                }

                // Apply decay
                float favor = _pawnFavor[pawn];
                if (favor > 0f)
                    _pawnFavor[pawn] = Mathf.Max(0f, favor - _passiveDecayAmount);
                else if (favor < 0f)
                    _pawnFavor[pawn] = Mathf.Min(0f, favor + _passiveDecayAmount);
            }

            // Cleanup
            for (int i = 0; i < toPurge.Count; i++)
            {
                _pawnFavor.Remove(toPurge[i]);
                _lastFavorTick.Remove(toPurge[i]);
            }

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

            if (!pawn.IsColonist && !pawn.IsSlave)
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

            if (!_pawnFavor.ContainsKey(pawn))
            {
                _pawnFavor[pawn] = 0f;
            }
            _pawnFavor[pawn] = Mathf.Clamp(_pawnFavor[pawn] + amount, -100f, 100f);

            if ( (Mathf.Abs(amount) >= 0.5f && showMote && !_suppressAllMotes) || Settings.AlwaysShowMotes)
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

            if(_pawnFavor.ContainsKey(pawn))
            {
                _pawnFavor[pawn] = newValue;
            }

            if(Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal] Reset favor for {pawn.LabelShort} due to ideology change.");
            }
        }

        public int GetLastFavorTick(Pawn pawn)
        {
            if (_lastFavorTick == null)
            {
                _lastFavorTick = new Dictionary<Pawn, int>();
                Log.Warning("[TheGodsAreReal]: Dude, the _lastFavorTick dict was missing, WTF?");

                return -1;
            }

            if (pawn == null) 
                return -1;

            return _lastFavorTick.TryGetValue(pawn, out int tick) ? tick : -1;
        }

        // This checks against an indvidual diety the ideo could have 0 or more dieties
        // Currently not used
        public bool PawnWorships(Pawn pawn, PreceptDef godPrecept)
        {
            return pawn.Ideo != null && pawn.Ideo.HasPrecept(godPrecept);
        }

        /// <summary>
        /// Get the favor score of an Ideology
        /// </summary>
        /// <param name="ideo">The ideo to score</param>
        /// <returns>The favor score of the Ideo (avg of pawn favor for ideo)</returns>
        public float GetIdeoFavor(Ideo ideo)
        {
            if (ideo == null)
                return 0f;

            float totalFavor = 0f;
            int pawnCount = 0;
            var pawns = PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_OfPlayerFaction;

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p.Ideo == ideo && p.RaceProps.Humanlike)
                {
                    pawnCount++;
                    if (_pawnFavor.TryGetValue(p, out float favor))
                    {
                        totalFavor += favor;
                    }
                }
            }

            return pawnCount > 0 ? totalFavor / pawnCount : 0f;
        }

        public bool ShouldSuppressThoughtMote(Thought_Memory thought)
        {
            // TODO: I don't think we should show motes here ever, so ignore any settings to turn them on

            // Define what constitutes "Mass Event" thoughts that should silence motes
            if (thought.def.defName.Contains("Party") || thought.def.defName.Contains("Ritual"))
            {
                return true;
            }

            return false;
        }

        public void ClearAllPawnFavor()
        {
            _pawnFavor.Clear();
            Log.Warning("!!![TheGodsAreReal] All favor data DELETED!!!");
        }

        public void Notify_PawnDied(Pawn pawn)
        {
            float favor = GetFavor(pawn);
            PawnDeathHandler.HandlePawnDeath(pawn, favor);

            // clean up the tracker data
            _pawnFavor.Remove(pawn);
            _lastFavorTick.Remove(pawn);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            // Scribe needs the ref lists to hold data temporarily during the loading phase
            Scribe_Collections.Look(ref _pawnFavor, "pawnFavor", LookMode.Reference, LookMode.Value, ref _pawnFavorKeys, ref _pawnFavorValues);
            Scribe_Collections.Look(ref _lastFavorTick, "lastFavorTick", LookMode.Reference, LookMode.Value, ref _lastFavorTickKeys, ref _lastFavorTickValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_pawnFavor == null) _pawnFavor = new Dictionary<Pawn, float>();
                if (_lastFavorTick == null) _lastFavorTick = new Dictionary<Pawn, int>();

                // Drop any entries that failed to cross-reference on load
                _pawnFavor.RemoveAll(kvp => kvp.Key == null);
                _lastFavorTick.RemoveAll(kvp => kvp.Key == null);
            }
        }
    }
}
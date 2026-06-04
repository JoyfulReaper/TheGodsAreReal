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
using UnityEngine;
using Verse;

namespace TheGodsAreReal
{
    public class WorldComponent_FavorTracker : WorldComponent
    {
        // We use the Pawn's unique ID as the key for performance
        private Dictionary<int, float> _pawnFavor = new Dictionary<int, float>();
        private bool _suppressAllMotes = false; // Not currently used

        // Favor decay
        private const int _decayIntervalTicks = 2500; // Run once every 2,500 ticks (approx. 1 game hour)
        private const float _passiveDecayAmount = 0.05f; // How much favor slips away per interval
        private static readonly HediffDef _divineTouchDef = HediffDef.Named("TheGodsAreReal_DivineTouch");
        private Dictionary<int, int> _lastFavorTick = new Dictionary<int, int>();
        private TheGodsAreRealSettings Settings => LoadedModManager.GetMod<TheGodsAreRealMod>().GetSettings<TheGodsAreRealSettings>();

        public IReadOnlyDictionary<int, float> PawnFavor => _pawnFavor;

        public WorldComponent_FavorTracker(World world) : base(world) { }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            // Decay Favor
            if (Find.TickManager.TicksGame % _decayIntervalTicks == 0)
            {
                DecayPassiveFavor();
            }


            // Rare Tick
            if (Find.TickManager.TicksGame % 250 != 0)
                return;

            // Grab all maps currently loaded in the game
            List<Map> maps = Find.Maps;
            for (int m = 0; m < maps.Count; m++)
            {
                // Grab all colonists on the current map
                List<Pawn> playerPawns = maps[m].mapPawns.PawnsInFaction(Faction.OfPlayer);
                for (int p = 0; p < playerPawns.Count; p++)
                {
                    Pawn pawn = playerPawns[p];

                    if (pawn == null || !pawn.Spawned || pawn.Dead)
                        continue;

                    if (pawn.Ideo?.KeyDeityName == null || !pawn.RaceProps.Humanlike)
                        continue;

                    UpdatePawnDivineHediff(pawn);
                }
            }
        }

        private void UpdatePawnDivineHediff(Pawn pawn)
        {
            float favor = this.GetFavor(pawn);

            // Map the favor score (-100.0 to 100.0) to a C# normalized float scale (0.0 to 1.0)
            // -100 favor becomes 0.0 severity (Pure Wrath)
            //    0 favor becomes 0.5 severity (Neutral / Hidden)
            // +100 favor becomes 1.0 severity (Pure Grace)
            float normalizedSeverity = (favor + 100f) / 200f;
            normalizedSeverity = UnityEngine.Mathf.Clamp01(normalizedSeverity);

            // 3. Look for an existing instance of our tracking Hediff on the pawn
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

            Dictionary<int, Pawn> pawnCache = new Dictionary<int, Pawn>();

            // Add all map pawns
            var maps = Find.Maps;
            for (int m = 0; m < maps.Count; m++)
            {
                var mapPawns = maps[m].mapPawns.AllPawns;
                for (int p = 0; p < mapPawns.Count; p++)
                {
                    pawnCache[mapPawns[p].thingIDNumber] = mapPawns[p];
                }
            }

            // Add all world pawns
            var worldPawns = Find.WorldPawns.AllPawnsAliveOrDead;
            for (int i = 0; i < worldPawns.Count; i++)
            {
                pawnCache[worldPawns[i].thingIDNumber] = worldPawns[i];
            }

            List<int> currentIds = _pawnFavor.Keys.ToList();
            List<int> idsToPurge = new List<int>();

            for (int i = 0; i < currentIds.Count; i++)
            {
                int id = currentIds[i];

                pawnCache.TryGetValue(id, out Pawn pawn);

                if (pawn == null || pawn.Destroyed || pawn.Dead)
                {
                    idsToPurge.Add(id);
                    continue;
                }

                // Apply decay
                float favor = _pawnFavor[id]; // Use the ID to access
                if (favor > 0f)
                    _pawnFavor[id] = Mathf.Max(0f, favor - _passiveDecayAmount);
                else if (favor < 0f)
                    _pawnFavor[id] = Mathf.Min(0f, favor + _passiveDecayAmount);
            }

            // Cleanup
            for (int i = 0; i < idsToPurge.Count; i++)
            {
                _pawnFavor.Remove(idsToPurge[i]);
                _lastFavorTick.Remove(idsToPurge[i]);
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
            if (_lastFavorTick.ContainsKey(pawn.thingIDNumber) && tick == _lastFavorTick[pawn.thingIDNumber])
                return;

            int id = pawn.thingIDNumber;
            if (!_pawnFavor.ContainsKey(id))
            {
                _pawnFavor[id] = 0f;
            }
            _pawnFavor[id] = Mathf.Clamp(_pawnFavor[id] + amount, -100f, 100f);

            if ( (Mathf.Abs(amount) >= 0.5f && showMote && !_suppressAllMotes) || Settings.AlwaysShowMotes)
            {
                Color favorColor = (amount >= 0) ? Color.cyan : Color.red;
                string text = (amount > 0) ? $"+{amount} Favor" : $"{amount} Favor";
                MoteMaker.ThrowText(pawn.DrawPos, pawn.Map, text, favorColor, 3.6f);
            }

            _lastFavorTick[pawn.thingIDNumber] = tick;
        }

        /// <summary>
        /// Get a pawn's favor level
        /// </summary>
        /// <param name="pawn">The target pawn</param>
        /// <returns>The favor level of the targeted pawn</returns>
        public float GetFavor(Pawn pawn)
        {
            if (_pawnFavor.TryGetValue(pawn.thingIDNumber, out float favor))
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

            if(_pawnFavor.ContainsKey(pawn.thingIDNumber))
            {
                _pawnFavor[pawn.thingIDNumber] = newValue;
            }

            if(Prefs.DevMode)
            {
                Log.Message($"[TheGodsAreReal] Reset favor for {pawn.LabelShort} due to ideology change.");
            }
        }

        public int GetLastFavorTick(Pawn pawn)
        {
            return _lastFavorTick.TryGetValue(pawn.thingIDNumber, out int tick) ? tick : -1;
        }

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
                    if (_pawnFavor.TryGetValue(p.thingIDNumber, out float favor))
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

        public override void ExposeData()
        {
            base.ExposeData();

            // Loading
            if (Scribe.mode == LoadSaveMode.LoadingVars && _pawnFavor == null)
            {
                _pawnFavor = new Dictionary<int, float>();
            }

            // Saving
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                HashSet<int> activePawnIds = new HashSet<int>();
                List<Map> maps = Find.Maps;

                for (int m = 0; m < maps.Count; m++)
                {
                    var mapPawns = maps[m].mapPawns.AllPawns;
                    for (int p = 0; p < mapPawns.Count; p++)
                    {
                        Pawn pawn = mapPawns[p];
                        if (pawn != null && pawn.RaceProps.Humanlike)
                            activePawnIds.Add(pawn.thingIDNumber);
                    }
                }

                var allWorldPawns = Find.WorldPawns.AllPawnsAliveOrDead;
                for (int i = 0; i < allWorldPawns.Count; i++)
                {
                    if (allWorldPawns[i] != null)
                    {
                        activePawnIds.Add(allWorldPawns[i].thingIDNumber);
                    }
                }

                // Create a list of keys to remove so we don't mutate the dictionary during iteration
                List<int> keysToRemove = new List<int>();
                foreach (var key in _pawnFavor.Keys)
                {
                    if (!activePawnIds.Contains(key))
                    {
                        keysToRemove.Add(key);
                    }
                }

                // Purge Ghost Entries
                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    _pawnFavor.Remove(keysToRemove[i]);
                }

                if (keysToRemove.Count > 0 && Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal] Purged {keysToRemove.Count} dead/discarded pawn IDs from favor tracking.");
                }
            }

            Scribe_Collections.Look(ref _pawnFavor, "pawnFavor", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref _lastFavorTick, "lastFavorTick", LookMode.Value, LookMode.Value);
        }
    }
}
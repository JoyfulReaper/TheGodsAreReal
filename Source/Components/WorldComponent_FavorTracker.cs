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
using UnityEngine;
using Verse;

namespace TheGodsAreReal
{
    public class WorldComponent_FavorTracker : WorldComponent
    {
        // We use the Pawn's unique ID as the key for performance
        internal Dictionary<int, float> pawnFavor = new Dictionary<int, float>();

        private const int DecayIntervalTicks = 2500; // Run once every 2,500 ticks (approx. 1 game hour)
        private const float PassiveDecayAmount = 0.05f; // How much favor slips away per interval
        private static readonly HediffDef DivineTouchDef = HediffDef.Named("TheGodsAreReal_DivineTouch");
        private Dictionary<int, int> _lastFavorTick = new Dictionary<int, int>();

        public WorldComponent_FavorTracker(World world) : base(world) { }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            // Decay Favor
            if (Find.TickManager.TicksGame % DecayIntervalTicks == 0)
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
                // Grab all free colonists on the current map
                List<Pawn> freeColonists = maps[m].mapPawns.FreeColonists;
                for (int p = 0; p < freeColonists.Count; p++)
                {
                    Pawn pawn = freeColonists[p];

                    if (pawn == null || !pawn.Spawned || pawn.Dead)
                        continue;

                    if (pawn.Ideo?.KeyDeityName == null)
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
            Hediff existingHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(DivineTouchDef);

            if (existingHediff != null)
            {
                // Update the severity level dynamically as their behavior changes
                existingHediff.Severity = normalizedSeverity;
            }
            else
            {
                // If they don't have the hediff yet, create it and inject it into their health tracker
                Hediff newHediff = HediffMaker.MakeHediff(DivineTouchDef, pawn);
                newHediff.Severity = normalizedSeverity;
                pawn.health?.AddHediff(newHediff);
            }
        }

        private void DecayPassiveFavor()
        {
            if (pawnFavor.Count == 0)
                return;

            List<int> pawnIds = new List<int>(pawnFavor.Keys);

            for (int i = 0; i < pawnIds.Count; i++)
            {
                int id = pawnIds[i];
                Pawn pawn = null;

                pawn = Find.CurrentMap?.mapPawns.AllPawns.Find(p => p.thingIDNumber == id);

                if (pawn == null)
                {
                    List<Map> maps = Find.Maps;
                    for (int m = 0; m < maps.Count; m++)
                    {
                        pawn = maps[m].mapPawns.AllPawns.Find(p => p.thingIDNumber == id);
                        if (pawn != null) break;
                    }
                }

                if (pawn == null)
                {
                    pawn = Find.WorldPawns.AllPawnsAliveOrDead.Find(p => p.thingIDNumber == id);
                }

                // If the pawn died, was destroyed, or left the map, we can entirely purge them from tracking
                if (pawn == null || pawn.Destroyed || pawn.Dead)
                {
                    pawnFavor.Remove(id);
                    continue;
                }

                // Apply decay
                if (pawnFavor[id] > 0f)
                {
                    // Subtract decay, but use Mathf.Max to guarantee it never drops below absolute zero
                    pawnFavor[id] = Mathf.Max(0f, pawnFavor[id] - PassiveDecayAmount);
                }
                else if (pawnFavor[id] < 0f)
                {
                    // if negative decay towards zero
                    pawnFavor[id] = Mathf.Min(0f, pawnFavor[id] + PassiveDecayAmount);
                }
            }

            if (Prefs.DevMode && pawnFavor.Count > 0)
            {
                Log.Message($"[TheGodsAreReal] Background divine decay processed for {pawnFavor.Count} tracked pawns.");
            }
        }

        /// <summary>
        /// Adds favor to a pawm
        /// </summary>
        /// <param name="pawn">The pawn to add favor to</param>
        /// <param name="amount">The amount of favor to add, can be negative</param>
        public void AddFavor(Pawn pawn, float amount)
        {
            if (pawn == null || !pawn.RaceProps.Humanlike)
                return;

            if (pawn == null)
                return;

            if (pawn.Ideo == null)
                return;

            int tick = Find.TickManager.TicksGame;
            // If the same pawn gained favor in the same tick, ignore the duplicate
            if (_lastFavorTick.ContainsKey(pawn.thingIDNumber) && tick == _lastFavorTick[pawn.thingIDNumber])
                return;

            _lastFavorTick[pawn.thingIDNumber] = tick;

            // If the Ideo doesn't have a Deity then don't track favor
            if (pawn.Ideo.KeyDeityName == null) 
                return;

            int id = pawn.thingIDNumber;
            if (!pawnFavor.ContainsKey(id))
            {
                pawnFavor[id] = 0f;
            }
            pawnFavor[id] = Mathf.Clamp(pawnFavor[id] + amount, -100f, 100f);
        }

        /// <summary>
        /// Get a pawn's favor level
        /// </summary>
        /// <param name="pawn">The target pawn</param>
        /// <returns>The favor level of the targeted pawn</returns>
        public float GetFavor(Pawn pawn)
        {
            if (pawnFavor.TryGetValue(pawn.thingIDNumber, out float favor))
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

            if(pawnFavor.ContainsKey(pawn.thingIDNumber))
            {
                pawnFavor[pawn.thingIDNumber] = newValue;
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
                    if (pawnFavor.TryGetValue(p.thingIDNumber, out float favor))
                    {
                        totalFavor += favor;
                    }
                }
            }

            return pawnCount > 0 ? totalFavor / pawnCount : 0f;
        }

        public override void ExposeData()
        {
            base.ExposeData();

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
                foreach (var key in pawnFavor.Keys)
                {
                    if (!activePawnIds.Contains(key))
                    {
                        keysToRemove.Add(key);
                    }
                }

                // Purge Ghost Entries
                for (int i = 0; i < keysToRemove.Count; i++)
                {
                    pawnFavor.Remove(keysToRemove[i]);
                }

                if (keysToRemove.Count > 0 && Prefs.DevMode)
                {
                    Log.Message($"[TheGodsAreReal] Purged {keysToRemove.Count} dead/discarded pawn IDs from favor tracking.");
                }
            }

            Scribe_Collections.Look(ref pawnFavor, "pawnFavor", LookMode.Value, LookMode.Value);
        }
    }
}
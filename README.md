# [TGAR-CORE: SYSTEMS ARCHITECTURE MANUAL]

**PROTOCOL ID:** TGAR-FAVOR-01  
**CODENAME:** The Gods Are Real  
**SYSTEM STATUS:** PRE-ALPHA / ACTIVE DEVELOPMENT  
**MAINTAINER:** K. GIVLER (ADMIN)  

---

## 1.0 SYSTEM OVERVIEW

**The Gods Are Real** utility introduces a dynamic, per-pawn tracking architecture that quantifies religious devotion into a measurable, shifting resource known as **Favor**. Under this framework, pawns no longer maintain static ideological alignments; instead, their active devotion directly influences and determines their real-time relationship metrics with their designated deity.

## 2.0 THE FAVOR SUBSYSTEM

The core engine monitors and updates individual pawn dedication metrics using three primary tracking routines:

* **Favor Tracking:** Individual pawn standing is tracked in real-time, operating within a strict numerical range from `-100` (Divine Wrath) to `+100` (Divine Grace).
* **Decay Routine:** Favor is a volatile resource. In the absence of active worship or religious engagement, values will naturally decay toward a baseline of `0` over time.
* **Divine Hediff Implementation:** A pawn's current standing with their deity is physically manifested via the **Divine Touch** health condition (Hediff), which updates its severity and effects dynamically based on real-time favor fluctuations.

## 3.0 INGESTION VECTORS (FAVOR ACQUISITION)

Pawns can modify their individual Favor metrics upward through three primary operational paths:

| Vector | Operational Description | Impact Tier |
| --- | --- | --- |
| **Rituals** | Structured ideological events. "Spectacular" outcomes grant major positive deltas. Failed execution or the presence of heretics (non-believers) incurs severe penalties. | High (Variable) |
| **Prayer** | Steady, reliable accumulation of favor achieved when a pawn actively executes prayer subroutines at an authorized Stele structure. | Continuous (Low) |
| **Daily Devotion** | Minor, passive step-increases triggered by positive thoughts tied directly to the Ideology's core precepts, rewarding compliant lifestyles. | Constant (Minimal) |

## 4.0 SYSTEM OUTPUTS & CONSEQUENCES

Fluctuations in a pawn's favor metric will trigger automated environmental and psychological shifts:

* **Divine Grace:** Maintaining a high favor metric rewards the faithful pawn with positive mood optimization buffs.
* **Divine Wrath:** Low favor metrics—resulting from failed rituals, heretical behavior, or prolonged worship neglect—induce severe mood penalties and force the pawn into the **Divine Wrath** operational state.
* **Conversion Protocol:** If a pawn undergoes an Ideology shift, the system executes an automated favor reset. Legacy data is purged to establish a clean baseline with the newly adopted faith.

## 5.0 DIAGNOSTIC & TELEMETRY TOOLS

For testing and simulation validation, developers can utilize integrated debugging features:

* **Debug Mode Interface:** Users operating with the host application's `Dev Mode` enabled gain access to a dedicated settings console.
* **Available Overrides:** The diagnostic console permits manual favor inflation/deflation, forced pawn prayer initialization, and real-time visualization of ideology-wide favor statistics.

## 6.0 LEGAL & COMPLIANCE

**DISTRIBUTION NOTICE:** This software framework is provided under the terms of the BSD 2-Clause License. Refer to the root `LICENSE` file for full distribution rights and liability limitations.

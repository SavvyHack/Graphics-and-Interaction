# Progression, saving and resets — proposed defaults

## Separate three forms of state
| State | Lifetime | Storage |
|---|---|---|
| Audio preferences | Across launches and New Game | Versioned local preferences |
| Unlocked/completed levels and last selected unfinished level | Across launches | Small versioned local progress record |
| Lives, checkpoint, block positions, latch/portal state | Current level attempt | Runtime room/checkpoint baseline only |

Existing RatLevelSlots is editor authoring, not this save system. No player persistence currently exists. For three stages, local PlayerPrefs or a small local JSON record is sufficient; avoid a backend. Treat WebGL storage as browser/device-local and potentially unavailable/cleared, not cloud sync.

## Campaign rules
Level 1 starts unlocked. Clearing 1 unlocks 2; clearing 2 unlocks 3; clearing 3 records campaign complete. Save the unlock before offering Next. An explicit ordered catalog maps stable level IDs to scene paths; do not trust a stored arbitrary scene string/build index. Clamp invalid IDs and versioned values; missing/corrupt data falls back to a safe new campaign without crashing.
Continue loads the last valid unfinished level from its beginning with three rats. After all stages are complete, offer Replay/Level Select clearly instead of an implied Level 4. Replaying earlier stages cannot relock later stages. New Game confirmation clears progress only; leave settings intact. Restart Level never wipes campaign unlocks.

## Shared profile extension
Alongside the three original state categories above, persist [coins/cosmetics](coins-and-cosmetics.md) and [statistics/attempt journal](statistics.md). New Game clears campaign unlock/completion progress only, preserving lifetime/per-level statistics, collected coin IDs, wallet, purchases and equipped outfit as well as settings. Only [Settings: Erase All Data](settings-page.md) clears the entire local profile/preferences.

Use one versioned profile and one persistence writer. Pickup ID, wallet and earned statistics must commit together; purchases and terminal outcome/unlock/statistics also need internally consistent logical transactions and recovery. Preserve a previous valid snapshot where supported. On corruption show a recovery notice; on unavailable storage retain coherent session state and show that progress will not be saved. Do not silently promise durability. Follow the statistics journal rules for interrupted attempts and time snapshots.

Additional levels receive new stable catalog IDs and collection manifests; never reuse removed IDs. Catalog order determines the final ending; Level 3 is final only in the initial catalog. Migrations preserve known records and ignore unknown scene references safely. Room resets exclude collected coin/profile state.

## Checkpoint reset details
Each checkpoint records a safe spawn and a known solvable puzzle baseline. Decide baseline at authoring time: earlier permanently solved access stays open; the current room's blocks/latches/portals reset together. For the first implementation favour defined authored baselines over arbitrary live physics snapshots.
Death restores the matching room baseline then respawns the next rat, retaining the reduced lives count. Reset Puzzle restores that baseline without life changes. Retry after total loss restarts the whole level with three rats. Reload/Continue does not promise mid-level checkpoint restoration. UI wording must match.

## Checks
Fresh launch; win/save/relaunch; locked level selection; replay earlier stage; New Game cancel/confirm; corrupt/missing/out-of-range data; unavailable storage; quitting while paused; reset after moving each block; death after activating each latch. Menu returns restore normal time scale and clean input/transition state. Never load a disabled/missing scene without visible recovery to Home.

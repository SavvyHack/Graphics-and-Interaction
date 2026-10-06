# Statistics page and attempt accounting

## Implementation update - 6 October 2026
CampaignProfile records lifetime and per-stage attempts, wins, failures, abandoned attempts, deaths, active-play time, best clear time, best survivors and unique tokens. The active journal recovers interrupted sessions once. CampaignUI supplies the Statistics page. Isolated transaction/recovery and runtime accounting checks passed; persistence remains device/browser-local. Runtime paths are under `Assets/Scripts/Gameplay` and `Assets/Scripts/Presentation`; scenes and verification limits are listed in [CURRENT_STATE](../CURRENT_STATE.md).

## Earlier acceptance specification
The requirements below are retained for design context; any old "not implemented" or three-level statements are superseded by the update above.

Status: target specification; no persistent statistics system currently verified. Home has Statistics. Show lifetime totals and per-level rows; optionally expose the current attempt from Pause without advancing its timer. Back returns to caller.

## Definitions
A 'run' in this UI means one level attempt, not an entire campaign. Label it 'Level attempts (runs)' with that explanation. Start exactly once when a playable level becomes ready for input with three rats. Continue, Replay, Retry and confirmed Restart each create a new attempt. Loading, Help, previews and opening menus do not. A campaign clear is recorded only when a previously incomplete campaign first completes its last level; replaying that ending does not add another clear until New Game resets campaign progress.

| Statistic | Counting rule |
|---|---|
| Attempts started | Every accepted attempt start |
| Levels cleared / failed attempts | One terminal win / zero-lives loss per attempt |
| Abandoned attempts | Active attempt ended by confirmed restart/Home/New Game, or recovered after app closure |
| Total deaths | Every accepted life decrement, including the last; hazard callbacks during immunity do not count |
| Active play time | Seconds while Playing with input enabled, excluding pause, menus, loading, focus loss, respawn lock and results |
| Coins collected | Lifetime unique pickups, not wallet balance; spending does not reduce it |
| Coins spent / current wallet | Accepted purchase cost total / earned minus spent |
| Campaign clears | Rule above; no survival score across stages |

Per-level rows show attempts, wins, failures, abandons, deaths, total active time, lifetime coins/available coins, fastest successful attempt and best remaining rats (maximum successful 1–3). Fastest time and best survivors are separate records and may come from different attempts. Unplayed/unwon records display an em dash, not zero seconds or three survivors. Result panels show the immutable completed attempt, not lifetime totals. Format time as mm:ss or h:mm:ss; retain subsecond precision internally. No leaderboard or mandatory score/rank.

## Events, persistence and recovery
Subscribe at established accepted transitions: RatLifeManager's life-loss event, the game-state owner's terminal outcome and the shared coin/purchase transaction. One subscriber path per event; unsubscribe on teardown. Never count raw trigger entries or UI button clicks as deaths/wins. Use stable level IDs and unique attempt IDs in the shared versioned profile, with one active journal record and idempotent finalization. Store start, terminal status, counters and elapsed time together; do not create separate competing PlayerPrefs writers.

Persist start, death, pickup, purchase and terminal events, plus active-time snapshots every 10 active seconds and on pause/focus loss/navigation. On relaunch, a journal still marked active is finalized once as abandoned using the last persisted counters/time. Abrupt termination may lose at most the interval since the last successful snapshot; unavailable storage carries no durability guarantee. Report that limitation without claiming exact crash-proof time. The accounting invariant is started = cleared + failed + abandoned + active (active is zero or one). Failure followed by Retry never also records abandonment. A canceled navigation does not finalize anything. Commit a terminal win before scene navigation; transaction recovery cannot count it again.

New Game preserves lifetime and per-level records, wallet and purchases. Restart/room reset never erase them. Only confirmed Erase All Data resets the profile; do not offer independent stats reset that breaks economy totals. On corrupt/unsupported data, retain a recoverable copy where storage permits, use a coherent safe profile and show a recovery notice. Clamp invalid numbers and reject unknown IDs without loading arbitrary scenes; version migrations preserve known entries.

## Acceptance checklist
- [ ] Start, pause, resume, die, reset puzzle, win, retry, restart and Home yield the defined counts and timer.
- [ ] Three accepted deaths produce one failed attempt, not three; immunity collisions add nothing.
- [ ] Spending affects spent/wallet but not collected; New Game preserves lifetime statistics.
- [ ] Relaunch during an attempt records one abandonment; repeated relaunch does not repeat it.
- [ ] Best-time/survivor records update independently on wins only; new levels appear by catalog ID.
- [ ] Crash/recovery, unavailable storage, duplicate events and rapid navigation preserve accounting consistency.

# Settings page

## Implementation update - 6 October 2026
CampaignUI and CampaignSettings implement the shared Settings page, persisted volume/mute, test cue/defaults, 0/4/8x MSAA, texture quality, VSync, desktop fullscreen and reduced decorative motion. ScreenSpaceOverlay TMP scales with resolution. Browser fullscreen/audio policies and laptop performance remain manual checks. Runtime paths are under `Assets/Scripts/Gameplay` and `Assets/Scripts/Presentation`; scenes and verification limits are listed in [CURRENT_STATE](../CURRENT_STATE.md).

## Earlier acceptance specification
The requirements below are retained for design context; any old "not implemented" or three-level statements are superseded by the update above.

Status: target specification; expands [UI/audio](ui-and-audio.md), not a claim of working settings.

Home and Pause open the same Settings page. Back/Escape returns to its caller; returning from paused settings stays paused. Use UGUI/TMP, laboratory panel styling, high contrast labels, visible keyboard focus and mouse support. Do not use prototype debug OnGUI as the final page.

## Controls and defaults
| Control | Default | Behaviour |
|---|---|---|
| Master volume | 80% | Scales every game and UI audio source |
| Music volume | 50% | Music and ambient loops |
| Sound effects volume | 80% | Player, hazards, portals, coins, outcomes and UI cues |
| Mute all | Off | Silences all buses while retaining slider values |
| Test sound | Button | One short existing checkpoint cue through Master and SFX |
| Restore audio defaults | Button | Restores these audio values only, immediately |
| Back | Button | Saves changes and returns to caller |

Sliders display 0–100%, support keyboard steps of 5%, preview immediately and save on completed adjustment (release/navigation commit), not every frame. Zero is silent. The test cue is intentionally silent while muted or either relevant bus is zero; show a label explaining that state. Do not temporarily bypass mute. Restore Defaults does not erase progression, cosmetics or statistics.

Load versioned, clamped settings before any audible playback. Reuse AudioManager and one shared audio service; route every AudioSource correctly. Mute does not overwrite remembered values. Music continues consistently across menus; no duplicate listeners/loops. Gameplay loops pause with gameplay; UI sounds work while paused. Browser focus loss pauses gameplay and prevents hidden hazard audio. Resume audio following a browser user gesture when required. No dependency or rendering migration is required.

Place a separate Erase All Data control under a clearly labelled Data section. Confirmation lists campaign progress, coins, outfits, stats and audio settings, focuses Cancel, and requires a second explicit Erase action. It clears the whole local profile and preferences and returns to Home with defaults. Hide/disable this control during an active attempt; it is available from Home settings only. New Game is not Erase All Data.

## Acceptance checklist
- [ ] Mouse/keyboard, small window and 16:9 layouts have readable labels and correct Back/Escape focus.
- [ ] Each bus at zero/full, mute/unmute, Test Sound and Restore Defaults obey routing and persistence.
- [ ] Relaunch restores preferences; invalid/missing preferences use safe defaults.
- [ ] Settings from Pause never advances gameplay, time or hazards; UI audio still works.
- [ ] Scene changes do not duplicate audio; real WebGL first-gesture audio and storage failure are checked.
- [ ] Erase cancel changes nothing; confirm clears all listed data; active-attempt access is unavailable.

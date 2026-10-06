# UI and audio

## Implementation update - 6 October 2026
CampaignUI supplies Home, Pause, Failure, map, Help, Shop/Wardrobe, Settings, Statistics and survivor outcomes. AudioManager routes saved volume settings to event cues and a quiet generated ambient loop. Pause/menu flow and displays passed developer checks. Human listening, keyboard-only browsing at all resolutions and browser user-gesture audio remain to check. Runtime paths are under `Assets/Scripts/Gameplay` and `Assets/Scripts/Presentation`; scenes and verification limits are listed in [CURRENT_STATE](../CURRENT_STATE.md).

## Earlier acceptance specification
The requirements below are retained for design context; any old "not implemented" or three-level statements are superseded by the update above.

Status: target specification. Existing PrototypeHUD/AudioManager are starting points, not a complete menu system. Main scene HUD references are currently empty.

## Screens and navigation
| Screen | Required controls/behaviour |
|---|---|
| Home | New Game; Continue only with progress; Level Select; Wardrobe; Statistics; Settings; Help (including Controls); Credits; platform-appropriate Quit |
| Level Select | Three labelled levels, clear locked/unlocked/completed states, Back |
| HUD | Three rat icons, level/checkpoint, attempt coins and wallet with distinct labels, short contextual prompts; no permanent debug overlay |
| Pause | Resume, Settings, Controls, Reset Puzzle if supported, Restart Level, Home |
| Settings | Master/Music/SFX sliders, mute, numeric percentage, Restore Defaults, Back |
| Controls | Existing move/jump/sprint plus portal/interact, pause and restart; Back |
| Failure | Clear loss message; Retry Level and Home; never a blank frozen scene |
| Level Complete | Next Level when available, Replay, Home |
| Final Escape | Brief non-interactive success beat, replay/level select/home, credits access |
| Credits | Team, external asset licences/attribution and AI assistance as appropriate |

Escape pauses/resumes; from a sub-menu it returns one level rather than accidentally resuming gameplay. Settings opened from pause return to pause. Confirm New Game when progress exists, Restart Level and Home when they discard the current attempt. Focus defaults to the safe/cancel action for confirmations. New Game resets campaign progress but preserves audio preferences. No unusable Quit button in WebGL.

## Input and layout

Detailed contracts: [Settings](settings-page.md), [Help](help-and-lore.md), [Wardrobe](coins-and-cosmetics.md), [Statistics](statistics.md) and [victory variants](victory-screen.md). Controls opens Help's Controls tab. Home pages have Back and restored keyboard focus. New Game preserves lifetime economy/statistics as well as audio; only separately confirmed Erase All Data clears them.
Use existing UGUI/TMP packages. One compatible EventSystem and one owner for navigation; prevent simultaneous gameplay input. Visible keyboard focus, mouse support, readable text and 16:9 plus narrower-window checks. Keep important UI away from browser edges and show loading feedback during transitions. UI should not depend on a running simulation clock.

## Audio settings
Extend/reuse AudioManager for event calls. Introduce Master, Music and SFX buses, preferably a Unity AudioMixer asset; ambient loops may initially share Music to avoid another control. Route every source including UI, portals and hazards consistently. Clamp normalized settings to 0..1; use a silence floor when converting to decibels to avoid log(0). Mute preserves slider values; unmute restores them. Persist with versioned settings keys, load before audible playback and save on deliberate settings change/exit, not every frame.
Sliders preview immediately; Restore Defaults affects audio settings only. Music must not restart or duplicate on every pause/settings visit. On pause, freeze/pause gameplay loops as appropriate while UI cues remain available. Browser first interaction must permit audio startup; verify in a real WebGL build.

## Required feedback
Jump/landing, loss, checkpoint, plate engagement/release, switch, portal departure/arrival, denied interaction, menu selection, level clear and final escape. Existing five WAVs cover only jump/checkpoint/loss/complete/failure. Add missing sounds with documented origins; limit repeated overlap-trigger sounds.

## Acceptance
Navigate every screen with mouse and keyboard; return correctly from pause/settings; leave/re-enter levels; hear no duplicate music. Set each slider to zero and full, mute/unmute, change while paused and restart the application. Verify stored values, silent sources, completion panels, losing all lives, browser focus changes and UI scale. Do not report audio implemented from silent null-safe method calls alone.

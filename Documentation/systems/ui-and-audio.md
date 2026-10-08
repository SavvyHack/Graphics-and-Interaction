# UI and audio

## Implementation update - 6 October 2026
CampaignUI supplies Home, Pause, Failure, map, Help, Shop/Wardrobe, Settings, Statistics and survivor outcomes. AudioManager routes saved volume settings to event cues and a quiet generated ambient loop. Pause/menu flow and displays passed developer checks. Human listening, keyboard-only browsing at all resolutions and browser user-gesture audio remain to check. Runtime paths are under `Assets/Scripts/Gameplay` and `Assets/Scripts/Presentation`; scenes and verification limits are listed in [CURRENT_STATE](../CURRENT_STATE.md).

## Earlier acceptance specification
Home has a dedicated laboratory presentation in `Assets/Scripts/Presentation/CampaignUI.cs` (7 October): gold title and New Game emphasis, static decorative specimens, and a framed rat exhibit. The top-right badge and button outlines were removed at user request. Buttons use brighter hover/keyboard-focus fills and distinct pressed colours. The in-game Credits page and its Home/final-result links were removed; repository attributions remain. Background audio is unchanged. Decorations ignore raycasts and have no animation, so reduced-motion users see the same layout. Existing scroll/navigation and saved outfit preview remain. Open `Assets/Scenes/Home.unity` to review; Unity visual and interaction acceptance for this revision is pending.

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
Default menu typography is now Bouncy Bun from `Assets/Resources/Fonts/BouncybunDemo-V4K8y.otf`, loaded centrally by CampaignUI into a shared dynamic 2048px TextMesh Pro SDF atlas. Serialized scene fonts remain fallback fonts for missing glyphs, not the primary menu font. No per-scene setup is needed; new menus should use CampaignUI's shared text helpers. The original supplied font remains in Downloads; the project's source note records its demo designation. Bundled TextMesh Pro resources are untouched.

In-level signs (world-space TextMesh Pro) match the menu font. The tutor reported them confusing and too hard to see. `Assets/Editor/LevelLabelStyle.cs` (menu **Project RAT > Restyle Level Labels**, also run at the end of `EnclosureLevelBuilder.BuildAll`) restyles every sign in the seven level scenes:
- **Font:** a saved, static `Assets/Resources/Fonts/Level Sign SDF.asset` built from Bouncy Bun. The demo font has no punctuation, so `' + , - . / : < > ^` fall back to Liberation Sans.
- **Material:** `Assets/Materials/Campaign/Level sign.mat`, with a thick dark-navy outline and a soft drop shadow.
- **Colour by meaning:** yellow for instructions and arrows; white for power-up names, titles, RELEASE and EXIT; mint for checkpoint numbers.
- **Size:** power-up names 3.6, checkpoints 3.4, instructions at least 4.2.
- **Placement:** z -2.45, in front of the observation glass, so its glare no longer washes text out.
- **Collisions:** colliding signs are lifted apart, and a decorative arrow lying on text is hidden.

The tool is safe to re-run. Rendered gameplay-zoom views of levels 1, 3, 5, 6 and 7 were inspected before and after.

Controls use vertically centred text with padding and bounded autosizing; map labels cap at 18, ordinary buttons at 22 and Home title at 48. Paragraphs retain readable fixed text size and grow with TMP preferred height. Unity 6000.3.18f1 import/compilation passed on 8 October. MenuFontValidation.Run passed primary-font checks on ten menu pages at 1600x1000 and 900x1000 with no text-overflow warnings; Home, settings, map and story captures were visually inspected. Remaining manual checks: gameplay Pause/results, keyboard/text input interaction and browser rendering.


Detailed contracts: [Settings](settings-page.md), [Help](help-and-lore.md), [Wardrobe](coins-and-cosmetics.md), [Statistics](statistics.md) and [victory variants](victory-screen.md). Controls opens Help's Controls tab. Home pages have Back and restored keyboard focus. New Game preserves lifetime economy/statistics as well as audio; only separately confirmed Erase All Data clears them.
Use existing UGUI/TMP packages. One compatible EventSystem and one owner for navigation; prevent simultaneous gameplay input. Visible keyboard focus, mouse support, readable text and 16:9 plus narrower-window checks. Keep important UI away from browser edges and show loading feedback during transitions. UI should not depend on a running simulation clock.

## Audio settings
Settings uses the normal button fill for persistent selection, so Test sound and other clicked actions do not remain aqua. Mute All uses aqua for both normal and selected states only when mute is ON. Hover and pressed feedback remain; this is presentation only and does not change saved audio values.

Runtime button creation configures the image, target graphic and colour states before activation. UGUI then applies the initial tint instantly instead of fading from white whenever Settings is rebuilt (for example, toggling mute). Hover/focus transitions retain their short fade. Visual regression check: repeatedly toggle mute in Home and Pause settings, then check hover, keyboard focus and other settings toggles for flashes.

Extend/reuse AudioManager for event calls. Introduce Master, Music and SFX buses, preferably a Unity AudioMixer asset; ambient loops may initially share Music to avoid another control. Route every source including UI, portals and hazards consistently. Clamp normalized settings to 0..1; use a silence floor when converting to decibels to avoid log(0). Mute preserves slider values; unmute restores them. Persist with versioned settings keys, load before audible playback and save on deliberate settings change/exit, not every frame.
Sliders preview immediately; Restore Defaults affects audio settings only. Music must not restart or duplicate on every pause/settings visit. On pause, freeze/pause gameplay loops as appropriate while UI cues remain available. Browser first interaction must permit audio startup; verify in a real WebGL build.

## Required feedback
Jump/landing, loss, checkpoint, plate engagement/release, switch, portal departure/arrival, denied interaction, menu selection, level clear and final escape. Existing five WAVs cover only jump/checkpoint/loss/complete/failure. Add missing sounds with documented origins; limit repeated overlap-trigger sounds.

## Acceptance
Navigate every screen with mouse and keyboard; return correctly from pause/settings; leave/re-enter levels; hear no duplicate music. Set each slider to zero and full, mute/unmute, change while paused and restart the application. Verify stored values, silent sources, completion panels, losing all lives, browser focus changes and UI scale. Do not report audio implemented from silent null-safe method calls alone.

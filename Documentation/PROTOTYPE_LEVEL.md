# Observation Enclosure — prototype level

Open the project in Unity 6000.3.18f1 and press **Play**. The editor automatically starts `Assets/Scenes/RatEnclosure.unity`, even if `StartScene` or another scene is open. The full level, laboratory wall, recessed observation window and black floor are saved assets and render on the first frame; supervisors do not run a builder or style command. Built players use this same scene as their first enabled build entry. Move with A/D or the arrow keys, jump with Space, sprint with Shift, and restart the complete attempt with R. Sprint is optional for every static jump.

## Design basis

This implementation follows the repository's README game design document and `Images/Level1.png`: one continuous glass enclosure, side-on 2.5D movement, and three rats per attempt. The observation-room backdrop uses a muted grey metal wall with a broad recessed one-way observation screen, dark teal mirror glass, and subtle welded joints. The floor band is pure black. Platforms are charcoal steel with restrained silver-green edge trim; warm warning accents and cyan runoff identify hazards. The bright full-height white grid has been removed.

## Route

| Section | X coordinates | Intended lesson |
|---|---|---|
| Acclimation | -3.5 to 19 | Safe starting deck; three short gaps; small height changes; checkpoint 1 |
| Habitat conduit | 19 to 31 | Gentle ramp, open-front tube, drop to a safe landing; checkpoint 2 |
| Rotation trial | 31 to 37 | Observe the two orange sweep arms and cross during an opening; checkpoint 3 |
| Transfer | 37 to 48 | Board a 2.5-unit shuttle, ride over runoff, step off onto a broad landing; checkpoint 4 |
| Exit ascent | 48 to 63.5 | Recombine three 1.4-unit gaps with 0.9-unit rises; enter the illuminated hatch |

All play takes place at Z = 0. Walkable platforms are 2.6 units deep, with a lit front edge and darker underside. The rat is approximately 0.9 units tall. The existing controller walks at 6 units/second and jumps 2 units high under gravity of -25 units/second squared. The final ascent deliberately uses less than half the available jump height. Checkpoint spawn positions are on broad static decks, away from the hazards.

The enclosure lines, tube ribs, doors and wheel rim are scenery without collision. The wheel's two orange arms are hazards. Cyan runoff below gaps costs a rat; muted platform edging is safe. The tube's front is open to keep the player visible. No powers, ladders, crushers or flame effects are required by this first prototype.

## Editing and integration

`Assets/Editor/PrototypePlayMode.cs` sets Unity's Play entry scene automatically after editor startup and script reload. Stopping Play restores the previously open editing scene through Unity's normal Play-mode handling. Developers who want to play the currently open scene can uncheck `Project R.A.T. > Start Play Mode in Prototype`; this preference is local to that project on their machine and defaults on for new checkouts. The launch hook never generates geometry or rewrites scene files. The builder below is an optional authoring tool, not part of startup or rendering.

The scene contains ordinary saved GameObjects, grouped by numbered section. Move or resize individual platforms in the Scene view. Keep their top edges reachable with the existing controller. The shuttle's `travel` and `period` and the wheel's `degreesPerSecond` are exposed in the Inspector.

`Project R.A.T. > Create Prototype Level` regenerates the dedicated scene and materials from `Assets/Editor/RatLevelBuilder.cs`. Save manual edits as another scene before regenerating, because regeneration replaces `RatEnclosure.unity`. `StartScene.unity` is preserved. The new level is the enabled build entry; the original starter scene remains listed but disabled.

`Project R.A.T. > Apply Observation Style to Both Scenes` applies the visual treatment to the existing scene objects without regenerating platforms, triggers, or gameplay references. Both saved scenes already have the treatment. The builder also applies it when creating a new prototype. The starter scene retains the group's water shader, checkpoint flag, rat shader, and wheel, with thin muted trim on its charcoal platforms.

Only the floor material uses `Unlit/Color` at RGB (0, 0, 0). The wall, charcoal platforms and muted trim have separate materials in `Assets/Materials/Environment/`. `LaboratoryBackdrop` saves non-colliding scenery behind the course: a recessed dark mirror, frame, sill, subdued light fixtures and thin weld seams that stop at the window. The opaque screen represents a one-way observation window with the observer room concealed; no scientists are shown. The front enclosure glass remains transparent, with pale sunlight bars. Runoff retains its separate hazard colour.

The observation glass uses a shared scene capture in the built-in render pipeline, drawn after the water. It transmits the scene with a faint cool tint, minimal refraction at its bevel, polished border highlights, and cartoon-style diagonal reflections: a broad light bar paired with a fine parallel highlight. The bars soften toward the bottom and fade at the pane edges to preserve gameplay readability. They shift with the controlled rat's horizontal position and jump height, including when the camera stops at its bounds. They remain still when the rat stops.

`FixedCameraFollow` sends its current target's world position to the global `_GlassActiveRatPosition` in `LateUpdate` and immediately on `SetTarget`. The prototype's respawns and the team's `TavishPrototypeIntegration.ActivateRat` already use this target, so both scenes work immediately without new component hookups. Missing/inactive targets, disabled camera follow components and play-mode startup clear the global. This is a stylised reflection effect for the single active gameplay camera, not ray tracing or a light cast onto the platforms.

Tune either glass material in `Assets/Materials/Environment/`:

| Inspector control | Shader property | Default |
|---|---|---|
| Light Bar Colour / Strength | `_ShimmerColor` / `_ShimmerStrength` | Pale warm white / 0.32 |
| Light Spacing (World Units) | `_ShimmerScale` | 12 |
| Main Bar Half Width (World Units) | `_BarWidth` | 0.48 |
| Bar Edge Softness (World Units) | `_BarFeather` | 0.06 |
| Bar Diagonal Slant | `_BarSlant` | -0.55 |
| Active Rat Reflection Movement | `_RatParallax` | 0.55 (0 makes the bars stationary) |

`_BaseAlpha` controls the transmission tint and `_DistortionStrength` the edge refraction. The shader source is `Assets/Shaders/GlassEnclosure.shader`; the optional observation-style tool retains these new defaults when reapplied. For automated render and gameplay verification, set `RAT_OUTPUT` and run Unity in batch mode with `-executeMethod ActiveGlassValidation.VerifyAndPlaytest`, without `-quit` or `-nographics`. It captures both scenes, compares stationary/horizontal/jump reflections with the camera held still, checks target switching and cleanup, and runs the existing course smoke tests with live shader-target checks.

`RatTrialSession` is a small replacement-ready playtest harness. It handles checkpoint respawn, three attempts, two visible waiting rats, completion/failure, and keyboard restart. A team's `PrototypeHUD` can optionally be connected to its `teamHUD` field. Replace the harness with the eventual team life system as that system becomes available; the `TrialZone` components hold the checkpoint, hazard and exit hookups in one place.

The only integration changes to `PlayerRatController` are shuttle support, a public respawn method, clearing upward velocity when hitting a ceiling, and rejecting ground detection while ascending. Existing movement inputs and jump tuning are retained. All runtime scripts live under `Assets/Scripts/`, grouped by role; there are no duplicate source copies outside Assets.

The primitive rat is a placeholder assembled from spheres and has no imported animation or external art dependency. The front glass reuses the repository's material and shader. The repository uses the built-in rendering pipeline, with Standard materials for lit actors and warning accents and unlit materials for the laboratory wall, dark floor and subdued metal trim.

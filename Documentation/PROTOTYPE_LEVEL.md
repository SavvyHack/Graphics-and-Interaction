# Observation Enclosure — prototype level

Open `Assets/PrototypeLevel/RatEnclosure.unity` and press Play. Move with A/D or the arrow keys, jump with Space, sprint with Shift, and restart the complete attempt with R. Sprint is optional for every static jump.

## Design basis

This implementation follows the repository's README game design document and `Images/Level1.png`: one continuous glass enclosure, side-on 2.5D movement, blue-grey machinery, cyan route edges, warm warning accents, and three rats per attempt. The supplied ChatGPT share opened without readable conversation content during development; its additional requirements have not been verified. This is a playable interpretation of the repository references, not a literal recreation of that unavailable conversation.

## Route

| Section | X coordinates | Intended lesson |
|---|---|---|
| Acclimation | -3.5 to 19 | Safe starting deck; three short gaps; small height changes; checkpoint 1 |
| Habitat conduit | 19 to 31 | Gentle ramp, open-front tube, drop to a safe landing; checkpoint 2 |
| Rotation trial | 31 to 37 | Observe the two orange sweep arms and cross during an opening; checkpoint 3 |
| Transfer | 37 to 48 | Board a 2.5-unit shuttle, ride over runoff, step off onto a broad landing; checkpoint 4 |
| Exit ascent | 48 to 63.5 | Recombine three 1.4-unit gaps with 0.9-unit rises; enter the illuminated hatch |

All play takes place at Z = 0. Walkable platforms are 2.6 units deep, with a lit front edge and darker underside. The rat is approximately 0.9 units tall. The existing controller walks at 6 units/second and jumps 2 units high under gravity of -25 units/second squared. The final ascent deliberately uses less than half the available jump height. Checkpoint spawn positions are on broad static decks, away from the hazards.

The enclosure wall panels, tube ribs, doors and wheel rim are scenery without collision. The wheel's two orange arms are hazards. Cyan runoff below gaps costs a rat; the cyan platform edging is safe. The tube's front is open to keep the player visible. No powers, ladders, crushers or flame effects are required by this first prototype.

## Editing and integration

The scene contains ordinary saved GameObjects, grouped by numbered section. Move or resize individual platforms in the Scene view. Keep their top edges reachable with the existing controller. The shuttle's `travel` and `period` and the wheel's `degreesPerSecond` are exposed in the Inspector.

`Project R.A.T. > Create Prototype Level` regenerates the dedicated scene and materials from `Assets/Editor/RatLevelBuilder.cs`. Save manual edits as another scene before regenerating, because regeneration replaces `RatEnclosure.unity`. `StartScene.unity` is preserved. The new level is the enabled build entry; the original starter scene remains listed but disabled.

`RatTrialSession` is a small replacement-ready playtest harness. It handles checkpoint respawn, three attempts, two visible waiting rats, completion/failure, and keyboard restart. A team's `PrototypeHUD` can optionally be connected to its `teamHUD` field. Replace the harness with the eventual team life system as that system becomes available; the `TrialZone` components hold the checkpoint, hazard and exit hookups in one place.

The only integration changes to `PlayerRatController` are shuttle support, a public respawn method, clearing upward velocity when hitting a ceiling, and rejecting ground detection while ascending. Existing movement inputs and jump tuning are retained. `Scripts/` outside Assets is an existing reference copy and is not compiled by Unity.

The primitive rat is a placeholder assembled from spheres and has no imported animation or external art dependency. The front glass reuses the repository's material and shader. The repository currently uses the built-in rendering pipeline, so the generated materials use its Standard shader; this change does not migrate the rendering pipeline.

## Validation

`RatLevelBuilder.Validate` checks checkpoint floors, all zone-to-session references and the single playable rat. `RatLevelSmokeTests.Run` enters Play mode and exercises the real input system and CharacterController for all six static jumps, ramp traversal, shuttle carrying, checkpoint activation, hazard respawn and exit completion. This is an automated smoke test; wheel timing, readability and difficulty still benefit from human playtesting.

Created with AI assistance. No third-party models or textures were added.

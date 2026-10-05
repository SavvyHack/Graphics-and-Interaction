# Three-level campaign briefs

## Collectibles and extensions
Each authored level targets 20 unique optional coins (12 normal route, 8 recoverable detours), per [coins/cosmetics](../systems/coins-and-cosmetics.md). Record stable IDs, locations and measured traversal timings in each brief before authoring. Follow the rollout in [extra hazards](../systems/extra-hazards-and-obstacles.md); update affected briefs with an explicit solo solution and checkpoint reset baseline. Extra levels need the same evidence and a unique ordered-catalog ID. Preserve the prototype's side-on laboratory aesthetic. The last catalog entry receives the [final victory](../systems/victory-screen.md).

## Original campaign direction

These are original proposed layouts inspired by the user's desired puzzle-platformer direction. No exact Fireboy and Watergirl levels were supplied or reproduced. The old GDD images remain art/history, not level requirements.

Read [Level 1](level-1.md), [Level 2](level-2.md), [Level 3](level-3.md) and the [portal contract](../systems/portals.md). Each brief states a solo solution before geometry is built. Author blockout with the existing controller first; measure jumps/camera framing in Unity before visual polish. Budget at least one comfortable waiting platform before each timed hazard.

Cross-level invariants: one active rat, three lives per stage, fixed labelled portal pairs, only the rat teleports, crates stay on bounded tracks, clear reset boundaries and no simultaneous distant button requirement. Latched switches are a new feature, not an existing capability. Camera should show the relevant control/output relationship or provide an explicit short reveal.

Do not enable the old backup scenes as Level 2/3. New campaign scenes use `Assets/Scenes/Level1.unity`, `Level2.unity`, `Level3.unity` after deliberately saving authored layouts. Home becomes the entry only when full navigation exists.

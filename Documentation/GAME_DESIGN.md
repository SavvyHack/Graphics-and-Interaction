# Current game design

Updated 6 October 2026 to reflect explicit user direction: seven levels, including two layouts adapted from the supplied Graphics-and-Interaction repository. This supersedes the earlier three-level proposal and conflicting historical README requirements. See [campaign order](levels/README.md), [implementation evidence](CURRENT_STATE.md) and [decisions](DECISIONS.md).

## Identity and loop
Single-player 2.5D laboratory escape. One controllable rat and two visible waiting rats represent three lives. Retain a fixed side-on perspective, blue-grey laboratory panels, bright route lights, observation glass and stylised hazards. Each level has traversal or puzzle challenges, safe checkpoints and an exit. Rats that survive a level carry into the next; retrying a level restores the rats that entered it, and New Game restores three. Checkpoints are local to the attempt.

## Campaign
Augmentation Lab and Reactor Divide use the reference five-tier layouts and six temporary augments required to traverse them. Relay Archive, Coolant Foundry, Scanner Gallery, Containment Core and Escape Spire use the same enclosure structure. Each adds one augment (dash, wall jump, glide, phase, ground pound), first on a safe practice section, then combined with older augments under harder hazards and fewer checkpoints ([levels 3-7](levels/enclosure-levels-3-7.md)). The crate, plate, transfer-pad and latch puzzles are no longer used in the campaign. Final victory occurs after stage seven, with different text and displayed rats for one, two or three survivors.

## Recovery and progression
Death clears temporary powers and releases a waiting rat at the latest checkpoint. Three losses show failure. Retry resets the level and lives. Pause stops movement, timers and hazards. Reset current puzzle returns to the checkpoint, resets that room's crates/latches/portals and clears augments without restoring lives; magnetic gates in the first two stages remain open for the attempt. Continue begins the selected/unlocked stage, not an exact suspended physics snapshot.

## Rewards and menus
Twenty optional tokens per stage; they respawn on each attempt, can pay once per attempt, and add to a saved wallet. Cosmetic outfits never change physics or hazard immunity. Home includes the seven-node map, Shop, Wardrobe, Help, Settings, Statistics and Credits. New Game resets unlocks while retaining tokens, outfits, settings and lifetime records. Erase Profile is a separate confirmation in Home settings.

Help explains movement, augments, both replacement routes, portals, crates, hazards and rewards. Settings include master/music/effects volume, mute, graphics quality, VSync, fullscreen and reduced decorative motion. Keyboard and mouse menu input are supported. Text uses resolution-scaled TextMesh Pro; rendering remains built-in with optional MSAA.

## Scope limits
No multiplayer, companion AI, free-placement portals, crate teleportation, combat, online account or cloud saves. Existing shaders are retained, not reauthored. Human route playtesting and browser performance remain separate acceptance steps; implementation does not establish those results. See CURRENT_STATE for outstanding particle work and verification.

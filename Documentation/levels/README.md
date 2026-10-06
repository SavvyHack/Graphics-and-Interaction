# Seven-level campaign

Confirmed user direction, 6 October 2026: seven levels total. Stages 1–2 adapt the supplied [reference repository](https://github.com/SavvyHack/Graphics-and-Interaction/tree/11bf1fb1e1a05e778ecbed0d6a9e4beb3ec54ba9). Stages 3–7 were rebuilt in the same five-floor enclosure style, each introducing one new augment ([details](enclosure-levels-3-7.md)). Earlier three-level briefs below are historical proposals, not the active scene order.

| Stage | Scene in Assets/Scenes | Main route |
|---|---|---|
| 1 Augmentation Lab | AugmentationLab.unity | Five-tier climb: speed gaps, shield laser, jet shafts, magnetic gate, elevator and wheel |
| 2 Reactor Divide | ReactorDivide.unity | Coolant jumps, return gallery, magnetic gate, two moving shuttles, high exit route |
| 3 Relay Archive | RelayArchive.unity | New: dash. Trench practice, coolant, gaps, wheel, elevator, jetpack |
| 4 Coolant Foundry | CoolantFoundry.unity | New: wall jump. Four chimneys, coolant shuttles, dash pits |
| 5 Scanner Gallery | ScannerGallery.unity | New: glide. Updraft climbs, laser corridors, long glides, magnetic gates |
| 6 Containment Core | ContainmentCore.unity | New: phase. Containment fields, timed hazard gauntlets |
| 7 Escape Spire | EscapeSpire.unity | New: ground pound. Hatches, service channels, every augment, exit vault |

All stages start with three lives, contain 20 permanent gold tokens and expose Retry, Pause and Help. Home is build entry zero. RatEnclosure and TransferWorks remain preserved outside the enabled campaign build.

## Reference stages
Both use nine checkpoints. Blue-grey panels, illuminated walkable edges and the current observation-glass, rat, water and hazard materials retain local rendering. Cyan spherical stations grant temporary augments; gold cylindrical tokens buy cosmetic outfits. Stages 1–2 keep stable save IDs `enclosure` and `transfer`, including token numbers 0–19, despite changed layouts. Previously collected tokens remain collected at their replacement positions.

The optional `ReferenceLevelAdaptation.Adapt` tool adapts source copies once and refuses to run again on a scene with CampaignSession. Do not rerun old CampaignAuthoring or source builders over authored scenes. Use Unity Inspector for further edits.

## Checks still requiring human review
Play every full route without developer repositioning, collect every optional token, assess difficulty and clarity of power durations, test narrow windows, and test the browser build. Developer traversal checks cover selected required jumps and shuttles; they do not establish a balanced full campaign or a measured completion time.

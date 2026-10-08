# Coins and rat cosmetics

## Implementation update - 6 October 2026
CampaignProfile/CoinPickup implement 140 permanent IDs (20 per stage). CampaignUI supplies Shop and Wardrobe, RatCosmetic applies colour/scarf/vest to active and waiting rats. Replacement stages retain enclosure/transfer IDs. Cyan augment stations are separate temporary gameplay pickups.

Coin update (user direction, 6 October 2026): coins respawn on every new attempt and each pickup adds one coin to `CampaignProfile.Data.earned`. `RatAttempt.pickedCoins` stops a coin paying twice within one attempt. `Data.coins` still records unique discoveries for the map's `n/20 unique coins` count. Wallet = earned - spent. Old profiles migrate with `earned = max(earned, coins.Count)`, so existing balances and outfits are preserved. Before this change, every collected ID stayed hidden forever, so replayed levels (including level 1) looked empty. This supersedes the "no respawn / no farming" rule below. Runtime paths are under `Assets/Scripts/Gameplay` and `Assets/Scripts/Presentation`; scenes and verification limits are listed in [CURRENT_STATE](../CURRENT_STATE.md).

## Outfit catalog update - 7 October 2026
8 October layout update: Shop and Wardrobe keep the rat preview and selected name fixed above a separate scrolling outfit list. Clicking the left side of a row changes only the preview. Each Shop row has a right-side Buy action (or disabled Owned/Equipped/insufficient-funds status); each owned-only Wardrobe row has an Equip action. Purchase confirmation is retained, and returning from purchase/cancel/equip preserves list position. Bouncy Bun remains the default font. Runtime changes are in CampaignUI.ShowWardrobe, with no scene regeneration.

Pattern alignment uses the visible Model child's world-to-local matrix, with the cosmetic root as fallback for RatDisplay. Gameplay turns Model independently of the controller, so the root matrix incorrectly produced depth rings/ovals. Verify lengthwise scrolling in both facing directions and previews. The authored Augmentation harness and Harness status light are independent of outfits and remain attached.

Purple-preview regression: the Diamond facet code used the reserved HLSL keyword `triangle` as a variable, causing the shared shader to fail compilation for Gold, Diamond and Rainbow. Compiler logs identified the error; renamed the variable to `facetSide`. Reimport and inspect all three outfits in Unity to confirm recovery.

Gold and Diamond now use opt-in stylised luxury finishes in RatStylizedLighting: Gold has warm metallic shading, a broad reflection highlight and a tight glint; Diamond has triangular icy facets, fine cut lines, bright edges and specular glints. RatCosmetic sets `_LuxuryFinish` (0 ordinary, 1 Gold, 2 Diamond) and updates the body-local frame after movement. Standard-material previews temporarily use the same shader; changing outfits restores their original materials and clears the finish. These are Codex-assisted cosmetic shader extensions, not changes to the original shader authorship. No prices or gameplay properties changed. Unity shader/visual verification is pending.


The current catalog supersedes the old table below: Classic (free), Copper (10), Silver (100), Gold (250), Diamond (500), Rainbow (1,000 tokens). Lab Scarf and Technician are removed. Silver, Gold and Diamond recolour fur; Rainbow now displays seven repeating coloured stripes scrolling across the body every four seconds, including in previews, while paused and with reduced decorative motion enabled. RatStylizedLighting.shader contains a Codex-assisted optional stripe extension (default off); RatCosmetic supplies the shared rat-local frame and unscaled scroll phase through property blocks. This replaces the whole-body hue cycle. Existing shader authorship remains credited; this new extension is separate AI-assisted work. Shader compilation and visual checks in Unity remain pending. Existing rat shader property blocks apply these colours without gameplay changes or new materials.

Shop lists every outfit, showing EQUIPPED, OWNED or the purchase price. Wardrobe lists only owned outfits; a locked shop preview resets to the equipped outfit when entering Wardrobe. Buying still requires confirmation and does not automatically equip. Preview components remain enabled for rainbow animation, and their explicit outfit survives Start. Active rats, waiting rats and outcome previews share RatCosmetic.

Existing profile validation removes retired IDs and falls back to Classic if needed; wallet and spending totals remain unchanged (no automatic refund). Classic and Copper ownership persist. Static source/diff checks passed; Unity rendering, purchase/relaunch, old-save fallback and reduced-motion verification remain pending.

## Earlier acceptance specification (historical)
The requirements below are retained for design context; any old "not implemented" or three-level statements are superseded by the update above.

Status: target specification; not implemented or verified. User requested collectible coins and purchasable rat outfits/designs. The economy below is the chosen implementation default, tunable after playtesting.

## Collection and economy
Coins are laboratory reward tokens: small brass discs with an embossed rat/paw symbol, gentle rotation and a restrained glow. Use the prototype's simple geometry and built-in materials. Tokens never resemble switches or hazards. One pickup grants one coin; no random rewards, real money, gameplay advantages or mandatory purchases.

Author 20 unique coins per main level: 12 on the normal route, 8 on optional, recoverable detours. Never require damage, blind jumps, crate teleportation or inaccessible return routes. Optional additional levels follow the same target and use stable catalog IDs. Every coin has a stable level-local ID; the save key is level ID plus coin ID, never scene index or object instance ID. Validate uniqueness before shipping.

Collection is permanent per local profile. Accepted contact with the active rat atomically adds the ID, increments wallet and lifetime coins, then hides the pickup and gives a short SFX/sparkle. Repeated callbacks do nothing. Death, room reset, retry, replay and New Game do not respawn collected coins. There is deliberately no repeatable farming. Level Select shows lifetime collected/total for each level; HUD shows this attempt's pickups and the wallet with distinct labels. Collected coins remain absent on replay, not collectible ghost duplicates.

## Wardrobe
Home has a Wardrobe button. Show wallet, a rotating or static rat preview, name, price, owned/equipped/locked state and Back. Mouse and keyboard can select, preview and buy. Preview never spends coins. Buy opens confirmation naming item and cost, with Cancel initially focused. Insufficient funds disables Buy and states the shortfall. Commit wallet debit and ownership together, once, before presenting success; double-clicks cannot debit twice. Buying does not automatically equip. Equip is free and immediate; exactly one outfit is equipped.

| Stable ID | Appearance | Price |
|---|---|---|
| classic | Existing grey laboratory rat | Free, always owned |
| copper_patch | Copper-coloured fur patches, original silhouette | 10 |
| lab_scarf | Small teal fabric neck scarf | 20 |
| technician | Short off-white vest with a tiny ID patch | 30 |

The initial campaign contains exactly enough coins to buy all three (60). Coins from future levels remain spendable on this catalog; do not silently reset balances or add mandatory sinks. No outfit changes speed, collision, lives or hazard immunity. Apply the equipped design to the active rat, waiting-rat props, previews and victory survivors. Preserve ears, tail, feet and readability through glass; avoid large hats, dangling cloth simulation and new renderer dependencies. Existing meshes can use material variants and small simple accessories. Missing/removed equipped items fall back to classic without destroying other valid ownership.

## Ownership, storage and wiring
Extend the shared versioned profile in [saving](progression-and-saving.md); use stable cosmetic IDs, owned IDs, equipped ID, non-negative wallet, collected IDs and lifetime earnings/spending. Do not add a second save writer. Invariant: wallet = lifetime earned coins minus lifetime spent coins (no coin bonuses in this version). New Game resets campaign progression only. Only separately confirmed Erase All Data clears coins, purchases and stats. Persist pickups and purchases immediately at logical transaction boundaries; WebGL flush/failure handling must be explicit. If storage is unavailable, retain internally consistent session state and show a persistent 'Progress will not be saved' notice; do not claim durability.

Use active-rat identity and Playing-state checks, not a broad tag that waiting props can trigger. Configure coin prefabs, trigger colliders, audio, UI references, outfit catalog and all three authored levels. Checkpoints and puzzle resets must exclude profile collectible state. No new economy manager should own life loss or game outcome.

## Acceptance checklist
- [ ] Collect once, collide repeatedly, die/reset/retry/relaunch: exactly one coin, one earned count and no respawn.
- [ ] Paused, dead and waiting rats cannot collect; coin and exit in the same frame settle before outcome snapshot.
- [ ] Buy, cancel, insufficient funds, rapid double-click, equip and relaunch preserve wallet/ownership correctly.
- [ ] All rat representations use the selected design with unchanged colliders and movement.
- [ ] New Game preserves collection/ownership; Erase All Data restores classic and zero wallet.
- [ ] Missing/duplicate IDs, invalid catalog items and unavailable storage have visible, safe handling.
- [ ] Walk all routes: 20 reachable coins per level, optional detours cannot trap the rat; UI totals match authored content.

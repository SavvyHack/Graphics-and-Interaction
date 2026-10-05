# Coins and rat cosmetics

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

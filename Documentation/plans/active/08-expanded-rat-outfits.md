# Expanded rat outfits

Status: implemented but unverified in Unity.

## Goal and design sources
Implement requested Silver, Gold, Diamond and 1,000-token Rainbow; remove Lab Scarf/Technician; show ownership in Shop and only owned outfits in Wardrobe. See [cosmetics](../../systems/coins-and-cosmetics.md). Intermediate prices 100/250/500 are chosen defaults.

## Current evidence
CampaignProfile owns the catalog and transactions. RatCosmetic uses serialized fur renderers and material property blocks. CampaignUI builds both menus and rat previews at runtime from existing scene references.

## Branch and delivery approvals
Branch `feature/expanded-rat-outfits`, based on the existing `feature/fanciful-main-menu` checkout. Follow [Git workflow](../../GIT_WORKFLOW.md). Commit, push, PR and merge are not authorized.

## Changes
Follow-up: Gold uses polished warm highlights; Diamond uses triangular icy facets and glints. Both use the shared opt-in shader finish in previews/gameplay. Check switching back to Classic/Copper/Silver/Rainbow clears the luxury finish, and inspect at gameplay scale through glass.

Catalog and menu filtering; fur colours and four-second repeating scroll of seven rainbow stripes; enabled previews retain explicitly chosen outfits through Start. Existing accessory references retained but hidden. Existing save validation drops removed IDs, falls back to Classic and preserves wallet totals.

## Steps
- [x] Inspect runtime, shader tint properties and existing purchase checks.
- [x] Implement catalog, filtering and shared rat appearance through existing scene wiring.
- [x] Update documentation and run static diff checks.
- [ ] Unity import/compilation and visual verification.
- [ ] Buy/cancel/insufficient funds/duplicate purchase, equip/relaunch, old-save fallback.
- [ ] Verify active/waiting/preview rats and continuous rainbow animation with reduced motion both on and off, including while paused.

## Acceptance and validation
Shop lists all six entries with correct prices or OWNED/EQUIPPED. Wardrobe never previews or lists locked items, including after leaving Shop. No gameplay changes. Existing purchase guards cover arbitrary catalog indices. Static checks are not Unity runtime evidence.

## Handoff
Open Home in Unity 6000.3.18f1 and run the scenarios above with an isolated profile. No scene builder required. Human visual acceptance remains outstanding.

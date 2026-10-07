# Bouncy Bun menu typography

Status: implemented; Unity import and targeted menu verification passed; gameplay/browser visual checks remain.

## Goal and design sources
Apply the user-supplied BouncybunDemo-V4K8y.otf to all menus and make it the default for future menu work. See [UI/audio](../../systems/ui-and-audio.md).

## Current evidence
All campaign menus use CampaignUI.Label; their old font is serialized per scene. Installed TMP supports dynamic font creation from an imported Font. The original OTF and source note exist in the user's Downloads.

## Branch and delivery approvals
User explicitly requested main, overriding the feature-branch default. Local main fast-forwarded to already-merged origin/main d8090ab with approval; the pre-existing VS Code settings change matched the remote file exactly. No commit, push or PR authorized.

## Changes
Copied font and supplied note into Resources/Fonts with metadata. Shared runtime TMP font retains original scene fonts for missing-glyph fallback. Centred controls, bounded text sizing, measured paragraph height; runtime-script instructions document the new typography default. No scene regeneration or third-party font edits.

## Steps
- [x] Recheck and synchronize main; inspect TMP APIs and instructions.
- [x] Import source asset, wire shared default, adjust text layout and documentation.
- [x] Static diff and asset metadata checks.
- [x] Unity compilation/import and targeted menu checks at 1600x1000 and 900x1000; RAT_MENU_FONT_OK.
- [ ] Gameplay Pause/results and browser visual checks.

## Acceptance and validation
Home, map, shop, wardrobe, Help, settings, statistics, pause and result screens use Bouncy Bun. Long paragraphs scroll without clipping. Token input stays centred; missing symbols fall back. No change to gameplay or saving.

## Handoff
User closed Unity. Unrestricted batch import succeeded after the sandboxed launch failed licensing/module loading. Optional MenuFontValidation.Run uses an isolated disposable profile and never saves scenes. It renders ten pages at two sizes to Logs/FontReview and verifies primary font/overflow. Desktop interaction and browser checks remain manual.

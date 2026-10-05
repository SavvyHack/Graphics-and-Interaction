# Victory screens and surviving rats

Status: target specification. RatLifeManager exposes LivesRemaining; GameManager has a win entry point, but the existing HUD is not wired. Extend these owners rather than creating another win/life system.

## Outcome rules
On accepted exit during Playing, resolve already accepted same-frame life loss/pickups, then snapshot level ID, 1–3 remaining rats, attempt time, deaths, coins collected this attempt and lifetime level collection. If the rat has lost its last life, failure takes precedence; zero survivors never produces victory. Lock input, stop the attempt timer and commit result/unlock/stats once before presenting navigation. Repeated exit callbacks and reopening UI must not repeat rewards or stats. There are no victory coin bonuses.

| Remaining rats | Heading | Presentation |
|---|---|---|
| 3 | All paws accounted for! | Three equipped rats together; bright but restrained laboratory lights and a short cheerful cue |
| 2 | Two rats, one way out! | Two equipped rats; warm lighting and a softer success cue |
| 1 | A narrow escape! | One equipped rat; calm lighting and a quiet relieved success cue |

Show 'Rats remaining this level: N/3' and the snapshot time, deaths and coins. Every variant is a successful completion with identical unlocks and access to the next level. No shame text or hidden penalty for losing rats. Use visible counts and poses as well as audio/colour. A brief skippable reveal may precede buttons; buttons must become usable without waiting for a long animation.

Non-final levels show Next Level, Replay and Home. The last catalog level shows an outside-the-laboratory escape backdrop, 'You escaped the laboratory', and Replay, Level Select, Credits and Home. Use the ordered level catalog's last entry, not a hard-coded Level 3 check, so additional levels work. Final survivor variant is based on that final attempt only: each level starts with three rats. Prior level losses are represented in Statistics, not subtracted again. Pause/settings must not leave the result screen trapped; Escape returns to Home via an explicit navigation action without starting a new attempt.

Use existing rat geometry, equipped designs, glass/metal vocabulary and modest particles; no separate photorealistic cutscene. Result screen composition must fit narrow windows and preserve keyboard focus. Replay starts a fresh three-rat attempt; Home preserves profile. Failure remains a separate Retry/Home screen and counts as a failed attempt.

## Acceptance checklist
- [ ] Complete with 3, 2 and 1 rats: correct count, art/copy, stats and identical progression access.
- [ ] Exit/hazard overlap, repeated triggers and repeated navigation never produce both loss and win or double counts.
- [ ] Time stops at accepted outcome; pickups settle before display; reopening result does not alter its snapshot.
- [ ] Next/Replay/Home and final Credits/Level Select work with keyboard/mouse and correct audio/input state.
- [ ] Extra catalog level moves the final ending to the new last entry; no phantom Next button.

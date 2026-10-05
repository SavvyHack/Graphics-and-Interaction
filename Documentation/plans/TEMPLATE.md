# Feature name

Status: planned / in progress / implemented but unverified / completed / blocked

## Goal and design sources
Link exact system and level documents. Distinguish confirmed requirements and proposed defaults.

## Current evidence
List actual scripts, scene objects, serialized fields and limitations.

## Branch and delivery approvals
Follow [Git workflow](../GIT_WORKFLOW.md). Record the dedicated feature branch, intended base and any prerequisite PRs. Track each action separately: commit, push, PR creation (including draft), merge. Initially all are pending user permission. Record the exact authorized scope when permission is given; do not infer approval of subsequent actions. Include actual commit hashes/PR URL only after authorized execution. When copying this template into active/ or completed/, change the workflow link to `../../GIT_WORKFLOW.md`.

## Changes
List runtime scripts, editor tools, assets, scene wiring and documentation. Identify anything a tool will overwrite.

## Steps
- [ ] Inspect current setup and applicable AGENTS instructions.
- [ ] Implement smallest complete playable feature.
- [ ] Configure scenes/prefabs/Inspector references.
- [ ] Verify success, failure/reset and relevant menu transitions.
- [ ] Update CURRENT_STATE and affected docs with actual results.

## Acceptance and validation
Observable outcomes; checks run; exact remaining manual steps. No new formal test suite required.

## Handoff
Known limits and next dependency. Move to completed only after criteria are met, or explicitly label accepted validation limitations.

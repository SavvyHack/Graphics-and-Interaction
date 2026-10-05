# Feature branches, pull requests and user approval

Status: mandatory workflow explicitly requested by the user. Applies to Codex work throughout this repository, including gameplay, assets, documentation, maintenance and cleanup.

## One branch per feature
Before starting a new feature, inspect the current branch, working tree and applicable instructions. Create or use a dedicated branch such as `feature/coins-and-cosmetics`, `feature/audio-settings` or `docs/feature-specifications`, from the team's intended integration branch. Do not assume a historical branch name is the current merge target; inspect repository context and ask if it remains ambiguous.

Keep unrelated features on separate branches and separate PRs. A feature's scripts, scene/prefab wiring, feedback, validation and documentation belong together on its branch. Closely coupled foundation work can have its own bounded branch/PR with explicit dependencies. For dependent features, preferably merge the approved prerequisite first; otherwise record the stacked base and retarget deliberately before merging. Do not silently include another unfinished feature in a PR.

Local branch creation, editing and verification are allowed as part of authorized work; they do not authorize commits or publication. Do not implement directly on the integration branch. If existing uncommitted work predates this workflow, preserve it: do not automatically commit, stash, discard or move unrelated edits to manufacture a clean tree. Identify ownership and isolate new work with a branch/worktree where safe. Ask when existing changes cannot be separated safely. This policy does not retroactively authorize committing current documentation changes.

## Explicit approval gates
| Action | Prepare before asking | Required permission |
|---|---|---|
| Each commit, including amend | Reviewable diff, intended files, proposed message and validation results | User explicitly approves this commit and its scope |
| Each push, including subsequent updates | Source branch, destination remote/branch and exact commits to publish | User explicitly approves this push |
| Each PR creation, including draft PRs | Head/base branches, title, complete description, change summary and validation/limitations | User explicitly approves opening this PR |
| Each merge, local or hosted | Existing PR link, current head/base, review/check status, merge method and outstanding risks | User explicitly approves merging this PR at the reviewed revision |

Approval of a commit does not approve its push. Approval of a push does not approve PR creation. Approval of PR creation, a reviewer approval or passing checks does not approve merge. An implementation request such as 'build this feature', 'finish it' or 'fix the tests' authorizes implementation, not these Git actions. Silence, elapsed time, tool availability and approval of a previous feature are not permission. Tool/sandbox execution permission alone is not a substitute for user approval of the Git action and scope.

The user may explicitly approve several named actions together once their concrete scope is reviewable; record exactly which actions that approval covers and do not ask again for the same unchanged authorized action. A generic 'yes' covers only the concrete pending question. Permission is consumed by the specified action, not a standing license for future commits/pushes/PRs/merges. If files, commits, head/base or merge scope materially change, obtain fresh permission for the changed action. A failed action may be retried within the same unchanged approved scope; do not broaden its destination or payload.

Never enable automatic merge or perform an unattended merge. Every feature must have an open PR before it can be merged; do not bypass the PR by locally merging/cherry-picking feature work into the integration branch or pushing directly to it. Do not force-push, rewrite shared history or delete branches merely because an ordinary push/merge was approved; these require explicit authorization for that additional operation.

## Prepare, then pause at the relevant boundary
Complete authorized implementation, wiring, documentation and proportionate verification first. Present a concise reviewable summary and the specific next action needing approval. Explain that this repository's user-requested workflow requires it, linking this file. Keep completed work available for review while awaiting approval; do not leave implementation unfinished merely because a later commit will need permission.

PR descriptions explain the problem and resulting behaviour, relevant design/spec links, validation actually performed and exact remaining Unity/manual checks. Never present planned checks as passed. Before an approved merge, confirm the PR still matches the approved revision and report conflicts/failing checks; resolving conflicts that changes the patch requires reviewing the new result before merging. Approval of merge does not automatically authorize a merge-conflict commit or a push.

## Handoff record
Each feature plan or final handoff records branch, intended base, implementation/validation state and whether commit/push/PR/merge are pending or explicitly authorized. Include commit hashes and PR URL only when those actions actually occurred. Do not claim the feature is merged because it works locally. Keep runtime acceptance distinct from delivery/approval status; no approval implies Unity testing passed.

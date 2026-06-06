End-of-session wrap-up. Three stages with a confirmation pause between each. Do not advance to the next stage without the user confirming.

**Stage 1 — Review**
Run `git fetch --quiet` first to update remote tracking data. Then run `git status --short` and `git diff` to show all uncommitted changes. Also run `gh pr list --repo Smallbone-Studios/Umbraco-Community-PropertyGuard --state open --json number,title` to show any open PRs (skip silently if the remote does not exist yet). Summarise what changed. Ask: "Happy to move to Stage 2 (update current state)?"

**Stage 2 — Update current state**
Read `ROADMAP.md` and `f:\Bifrost-Studios\.bifrost\current-state.md`. Run `gh pr list --repo Smallbone-Studios/Umbraco-Community-PropertyGuard --state all --limit 5 --json number,title,state,mergedAt` to get authoritative PR state (skip if remote not yet set up). Propose updated text for the PropertyGuard section of `f:\Bifrost-Studios\.bifrost\current-state.md` reflecting what was done this session. Show the proposed content and wait for confirmation before saving. Ask: "Happy to move to Stage 3 (commit)?"

**Stage 3 — Commit**
Show the files that will be staged in this repo. Propose a conventional commit message. Wait for confirmation before committing. Then commit and push this repo. Finally run `/sync` to update the Bifrost Studios root.

Produce a status report for the PropertyGuard project. Do the following in order:

1. Read `ROADMAP.md` — find the active phase and any feature spec in `docs/features/` for it.
2. Run `git fetch --quiet` — update remote tracking data first.
3. Run `git log --oneline -5`
4. Run `git status --short`
5. Run `git branch --show-current`
6. Run `gh pr list --repo Smallbone-Studios/Umbraco-Community-PropertyGuard --state all --limit 5 --json number,title,state,mergedAt` (skip silently if remote not yet set up)

Then output a report in this format:

---
## PropertyGuard Status

**Branch:** <current branch>
**Active phase:** <phase name and number from ROADMAP.md>

### Phase status
<copy the status table from ROADMAP.md>

### Recent commits
<last 5 commits, one per line>

### Uncommitted changes
<list of changed/untracked files, or "none">

### Open PRs
<list, or "No remote set up yet">

### Next action
<one clear sentence — the next task to start>
---

Be factual. Read the files and git output; do not guess.

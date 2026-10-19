# Adoption Plan

> **Generated**: 2026-10-19
> **Project phase**: Production (inferred: 52 scripts under `Assets/Project/Scripts/`; no `project.stage` set)
> **Engine**: Unity 6000.5.2f1 (stated in CLAUDE.md, but NOT configured in `project.yaml`)
> **Template version**: v1.0+
> **Workflow tier**: minimal (rigor: minimal). GDDs, ADRs, stories, TR registry, control manifest, sprint-status and stage file are not expected, so their absence is not a gap.

Work through these steps in order. Check off each item as you complete it.
Re-run `/adopt` anytime to check remaining gaps.

---

## Step 1: Fix Blocking Gaps

None. No v1.0 migration is needed (`project.yaml` exists, no legacy config files; converter dry-run: "Nothing to do").

---

## Step 2: Fix High-Priority Gaps

### 2a. Engine not configured in `project.yaml`
`project.yaml` has no `engine:` block, and `.claude/docs/technical-preferences.md` has Engine, Language, Rendering, Physics and Target Platforms all set to `[TO BE CONFIGURED]`. Skills that read engine config will fail or guess.
**Fix**: run `/setup-engine` (Unity 6000.5.2f1, C#, URP 2D, Mobile portrait).
**Time**: 15 min
- [ ] `engine.name` / `version` / `language` / `rendering` / `physics` set in `project.yaml`

### 2b. Engine reference docs missing
`docs/engine-reference/unity/VERSION.md` does not exist. Unity 6000.5.2f1 is newer than the model's training data, so API facts must come from sourced reference docs, not memory.
**Fix**: `/setup-engine` fills these via WebSearch. Anything it cannot source must be recorded as `NOT SOURCEABLE`, not guessed.
**Time**: 15 min (same run as 2a)
- [ ] `docs/engine-reference/unity/VERSION.md` created

### 2c. `design/game-brief.md` missing
At the minimal tier the game brief is the design record. The concept currently lives only in `CLAUDE.md`.
**Fix**: create `design/game-brief.md` from `.claude/docs/templates/game-brief.md` (six required fields, no placeholders). Most content can be lifted from the "What This Project Is" section of CLAUDE.md.
**Time**: 30 min
- [ ] `design/game-brief.md` written, all six required fields filled

---

## Step 3: Bootstrap Infrastructure

Not on the minimal path. The brief's build order is the plan. Next: `/create-stories` (or `/dev-story` once stories exist).

---

## Step 4: Medium-Priority Gaps

### 4a. `naming.*` not configured
**Fix**: set naming conventions in `project.yaml` (match existing C# style in `Assets/Project/Scripts/`).
**Time**: 5 min
- [ ] `naming.*` set

### 4b. `performance.*` not configured
**Fix**: set performance budgets (target FPS, draw-call budget for the biggest level; see the open perf question in CLAUDE.md).
**Time**: 5 min
- [ ] `performance.*` set

---

## Step 5: Optional Improvements

### 5a. Forbidden Patterns / Allowed Libraries empty
Empty by design. Candidates from CLAUDE.md: no UGUI, no TextMeshPro in `Assets/Project`, no DOTween (PrimeTween only).
**Time**: 5 min
- [ ] Reviewed

---

## What to Expect from Existing Stories

No stories exist yet. When created, they will carry TR-IDs and manifest stamps where the tier requires them.

---

## Review Mode

Resolves to `solo` from `modes.rigor: minimal`, unless something pins it. To change depth, change `modes.rigor` or pin it with `/settings --local modes.review_mode=<full|lean|solo>`.

---

## Re-run

Run `/adopt` again after completing Step 2 to verify all high gaps are resolved.

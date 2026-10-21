# Session Subagent Cost

**Session:** `f794299e-c4d8-469a-a89f-cc523b7d3ef5`  
**Updated:** 20261020_162404

Subagent spawns this session: **3**

| Agent | Spawns |
|-------|--------|
| qa-tester | 1 |
| fork | 1 |
| agent-skills:code-reviewer | 1 |

Each spawn is a fresh context window that re-reads its own inputs.
To reduce this: lower `modes.review_mode` (`/settings modes.review_mode=solo`)
or `modes.rigor` (`/settings modes.rigor=minimal`).

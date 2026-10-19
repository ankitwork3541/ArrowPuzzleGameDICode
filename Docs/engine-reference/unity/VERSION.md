# Unity — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | 6000.6.3f1 (Unity 6.6) |
| **Project Pinned** | 2026-10-19 |
| **Installed at pin time** | 6000.6.3f1 (Hub: `C:/Program Files/Unity/Hub/Editor/6000.6.3f1`; matches `ProjectSettings/ProjectVersion.txt`) |
| **Last Docs Verified** | 2026-10-19 |
| **LLM Knowledge Cutoff** | Unity: training data likely covers up to ~2022 LTS (2022.3) per the skill's coverage table |
| **Risk Level** | HIGH — version is well beyond LLM training data |

## Sources (fetched 2026-10-19)

- https://docs.unity3d.com/6000.6/Documentation/Manual/UpgradeGuideUnity66.html
- https://docs.unity3d.com/6000.6/Documentation/Manual/UpgradeGuideUnity65.html
- https://docs.unity3d.com/6000.6/Documentation/Manual/EditorCommandLineArguments.html
- https://docs.unity.com/en-us/engine/6000.6/manual/whats-new/unity66

## Post-cutoff timeline

| Version | Release date | Notes |
|---------|--------------|-------|
| 6.5 (6000.5) | NOT SOURCEABLE — release date not stated at the upgrade guide URL | See breaking-changes.md |
| 6.6 (6000.6) | NOT SOURCEABLE — release date not stated at the upgrade guide URL (page footer "Built on: 2026-10-17" is a docs build date) | See breaking-changes.md |
| 6.0 – 6.4 | NOT SOURCEABLE — upgrade guides for these were not fetched this run | Project began on 6000.5.2f1 per earlier CLAUDE.md |

## Not covered by this reference

- Per-subsystem modules (`modules/`) were not created. NOT SOURCEABLE: UI Toolkit, URP 2D, Input System behaviour beyond the items in breaking-changes.md was not fetched.
- Android build command: no `-build<Platform>Player` flag exists for Android at the command-line page; `-build <pathName>` builds from the active build profile.

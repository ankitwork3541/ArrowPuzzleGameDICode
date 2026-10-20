# Technical Preferences

<!-- project.yaml at the repo root is the machine-readable source of truth for
     engine, specialists, naming, performance, platform, and testing.framework.
     This file is the human-readable LEGACY FALLBACK: agents and skills resolve
     each key from project.yaml first and fall back here only when the
     project.yaml key is absent. /setup-engine dual-writes both.
     Forbidden patterns and allowed libraries are NOT migrated — they live only
     in this file. Populated by /setup-engine; updated as decisions are made. -->

## Engine & Language

- **Engine**: Unity 6000.6.3f1
- **Language**: C#
- **Rendering**: URP (2D)
- **Physics**: PhysX (2D physics unused; input is not collider-dependent)

## Input & Platform

<!-- Written by /setup-engine. Read by /ux-design, /ux-review, /test-setup, /team-ui, and /dev-story -->
<!-- to scope interaction specs, test helpers, and implementation to the correct input methods. -->

- **Target Platforms**: Mobile (Android)
- **Input Methods**: Touch (mouse in Editor)
- **Primary Input**: Touch
- **Gamepad Support**: None
- **Touch Support**: Full
- **Platform Notes**: Portrait-locked. New Input System only. UI Toolkit only (no UGUI).

## Naming Conventions

- **Classes**: PascalCase
- **Variables**: camelCase (no underscore prefix)
- **Signals/Events**: PascalCase On<Event>
- **Files**: PascalCase matching class
- **Scenes/Prefabs**: PascalCase
- **Constants**: PascalCase

## Performance Budgets

- **Target Framerate**: 60 fps
- **Frame Budget**: 16.6 ms
- **Draw Calls**: <= 150 (default; LineRenderer is one draw call per arrow, measure on biggest level)
- **Memory Ceiling**: [TO BE CONFIGURED]

## Testing

- **Framework**: NUnit (Unity Test Framework)
- **Minimum Coverage**: [TO BE CONFIGURED]
- **Required Tests**: Balance formulas, gameplay systems, networking (if applicable)

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- [None configured yet — add as architectural decisions are made]

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- [None configured yet — add as dependencies are approved]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one]

## Engine Specialists

<!-- Written by /setup-engine when engine is configured. -->
<!-- Read by /code-review, /architecture-decision, /architecture-review, and team skills -->
<!-- to know which specialist to spawn for engine-specific validation. -->

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist
- **Shader Specialist**: unity-shader-specialist
- **UI Specialist**: unity-ui-specialist (UI Toolkit UXML/USS)
- **Additional Specialists**: unity-dots-specialist, unity-addressables-specialist
- **Routing Notes**: Primary for architecture and C# review; shader specialist for rendering/VFX; UI specialist for all interface work.

### File Extension Routing

<!-- Skills use this table to select the right specialist per file type. -->
<!-- If a row says [TO BE CONFIGURED], fall back to Primary for that file type. -->

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (.cs) | unity-specialist |
| Shader / material files (.shader, .shadergraph, .mat) | unity-shader-specialist |
| UI / screen files (.uxml, .uss) | unity-ui-specialist |
| Scene / prefab / level files (.unity, .prefab) | unity-specialist |
| Native extension / plugin files | unity-specialist |
| General architecture review | Primary |

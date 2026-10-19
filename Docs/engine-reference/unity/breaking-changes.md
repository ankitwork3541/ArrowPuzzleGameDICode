# Unity — Breaking Changes

Last verified: 2026-10-19. Sources: Unity 6.5 and 6.6 upgrade guides (URLs in VERSION.md). Anything not listed here was NOT fetched; 6.0–6.4 breaking changes are NOT SOURCEABLE from this run.

## 6.5 → 6.6

- **Graphics**: dynamic batching is obsolete (no replacement API). Render Pipeline Core no longer depends on Terrain (add `com.unity.modules.terrain` if needed). Legacy Rendering Debugger Debug UI (`DebugState*`, `DebugStateAttribute`, legacy `DebugUIDrawer`, `LegacyStyles`) is `[Obsolete(..., true)]` — compile error; implement `ISerializedDebugDisplaySettings`.
- **Progressive CPU Lightmapper** deprecated; use Unity Compute Light Baker or Progressive GPU Lightmapper.
- **Cinemachine** is a core package; Cinemachine 2 projects auto-upgrade to Cinemachine 3 (different API and data format).
- **Android**: legacy/round icons removed (Adaptive only); `-androidChainedSignalHandlerBehavior legacy` removed; minimum OpenGL ES is 3.1 (`PlayerSettings.openGLRequireES31` getter always `true`, setter no-op).
- **Scripting symbols**: `UNITY_64` and `DEVELOPMENT_BUILD` deprecated (analyzer warnings UAC0008/UAC0009); page says they are fully removed in 6.8. `BuildOptions.ForceEnableAssertions` and `CoreBuildData.developmentBuild` obsolete.
- **UI Toolkit**: `DefaultEventSystem.LegacyInputProcessor` removed; `UIToolkitInputConfiguration.SetRuntimeInputBackend` is a compile error; UXML Factory/Traits removed in favour of `UxmlElement` / `UxmlAttribute`.
- **Editor**: "Reduce Version Control Noise" removed.

## 6.4 → 6.5

- **2D**: `LowLevelPhysics2D` renamed `PhysicsCore2D`; namespace `UnityEngine.LowLevelPhysics2D` → `Unity.U2D.Physics`.
- **Graphics**: Built-In Render Pipeline deprecated (supported through 6.7 LTS). Render Pipeline Core no longer depends on uGUI. Legacy Render Graph API obsolete (custom SRPs only).
- **Android**: minimum API level 26; x86-64 removed; Gradle 8.13 → 9.1.0, AGP 8.10.0 → 9.0.0; `Screen.fullScreen` has no effect (use `AndroidApplication.currentWindowInsets`); `startInFullscreen` obsolete.
- **Programming**: `InstanceID` replaced by `EntityId` (8 bytes). Removed `GameObject.rigidbody`/`camera`-style shortcuts (use `GetComponent<T>()`) and `AddComponent(string)`. Unity now supplies `System.Text.Json`, `System.Collections.Immutable`, `System.Reflection.Metadata`, `System.Runtime.CompilerServices.Unsafe`.
- **Editor**: project-local Editor.log is default (`-useGlobalLog` restores global).

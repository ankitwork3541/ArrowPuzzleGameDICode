# Unity — Deprecated APIs

Last verified: 2026-10-19. Sources: Unity 6.5 / 6.6 upgrade guides. Only items stated there are listed.

| Don't use | Use instead |
|-----------|-------------|
| `UNITY_64` define | `IntPtr.Size` for bitness |
| `DEVELOPMENT_BUILD` define | `UNITY_ENABLE_CHECKS`, `UNITY_INCLUDE_INSTRUMENTATION`, or runtime `Debug.isDebugBuild` |
| `BuildOptions.ForceEnableAssertions` | `PlayerSettings.SetManagedCodeVariant(namedBuildTarget, ManagedCodeVariant.Checked)` |
| `CoreBuildData.developmentBuild` | `CoreBuildData.useDiagnosticChecks` |
| UXML Factory / Traits | `UxmlElement` / `UxmlAttribute` |
| `UIToolkitInputConfiguration.SetRuntimeInputBackend` | Remove the call |
| `PlayerSettings.openGLRequireES31` | `openGLRequireES31AEP` or `openGLRequireES32` |
| `UnityEngine.LowLevelPhysics2D` | `Unity.U2D.Physics` (`PhysicsCore2D`) |
| `Screen.fullScreen` on Android | `AndroidApplication.currentWindowInsets.Show` / `Hide` |
| `PlayerSettings.Android.startInFullscreen` | `requestedVisibleWindowInsets` |
| `AndroidAssetPacks.RequestToUseMobileDataAsync` | `AndroidAssetPacks.ShowConfirmationDialogAsync` |
| `GameObject.rigidbody`, `GameObject.camera` | `GetComponent<T>()` |
| `GameObject.AddComponent(string)` | `AddComponent<T>()` |
| Built-In Render Pipeline | URP (BiRP deprecated) |
| Dynamic batching | No replacement stated (see "Choose a method for optimizing calls") |

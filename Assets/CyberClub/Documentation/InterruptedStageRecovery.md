# Interrupted Stage Recovery

## Initial snapshot (2026-07-24)

- Branch: `main`.
- Staged files: none.
- Deleted tracked files: none.
- `git diff --check`: passed; only Git line-ending warnings (`LF` will be converted to `CRLF`) were reported.
- Last failed Development WebGL build: 2026-07-24 11:05:41–11:05:49 (the preceding full attempt ran 11:01:19–11:05:06).
- Unity log: `C:/Users/barin/AppData/Local/Unity/Editor/Editor.log`.
- Failed build output folder: `C:/Users/barin/OneDrive/Рабочий стол/Сборка CyberClub`.
- Unity build report: `D:/UnityProjects/CyberClub/Library/LastBuild.buildreport`.
- Running processes at snapshot time: one main `Unity.exe` process and two Unity asset worker processes; no running `clang++`, `emcc`, or `wasm-ld` process.

### Modified tracked files

- `Assets/CyberClub/Scripts/Admin/AdminLogic/VisitorService.cs`
- `Assets/CyberClub/Scripts/DeviceSystem/Spawn/DeviceRegistry.cs`
- `Assets/CyberClub/Scripts/DeviceSystem/Spawn/SpawnPointsHolder.cs`
- `Assets/CyberClub/Scripts/Player/MobileControlsHUD.cs`
- `Assets/CyberClub/Scripts/Player/MobileLookArea.cs`
- `Assets/CyberClub/Scripts/Player/MobileVirtualJoystick.cs`
- `Assets/CyberClub/Scripts/Player/PlayerInputReader.cs`
- `Assets/CyberClub/Scripts/Player/Rotation/PlayerRotation.cs`
- `Assets/CyberClub/Scripts/Purchases/Root/GemsPurchase.cs`
- `Assets/CyberClub/Scripts/Visitors/Visitor.cs`
- `Assets/CyberClub/Scripts/YG2Services/CyberClubYG2PaymentsService.cs`
- `Assets/CyberClub/Scripts/Zones/Barrier/BarrierDissolve.cs`
- `Assets/CyberClub/Scripts/Zones/UI/LocationPurchaseDialog.cs`
- `Assets/Scenes/MainScene.unity`

Initial tracked diff: 14 files, 1,316 insertions, 310 deletions.

### Added/untracked files

- `Assets/CyberClub/Documentation.meta`
- `Assets/CyberClub/Documentation/OptimizationAudit.md`
- `Assets/CyberClub/Documentation/OptimizationAudit.md.meta`
- `Assets/CyberClub/Plugins.meta`
- `Assets/CyberClub/Plugins/WebGL.meta`
- `Assets/CyberClub/Plugins/WebGL/CyberClubPaymentsBridge.jslib`
- `Assets/CyberClub/Plugins/WebGL/CyberClubPaymentsBridge.jslib.meta`
- `Assets/CyberClub/Scripts/Editor/CyberClubMainSceneValidator.cs`
- `Assets/CyberClub/Scripts/Editor/CyberClubMainSceneValidator.cs.meta`
- `Assets/CyberClub/Scripts/Player/MobilePointerUiGuard.cs`
- `Assets/CyberClub/Scripts/Player/MobilePointerUiGuard.cs.meta`
- `Assets/CyberClub/Scripts/Settings/Camera/LookSensitivitySettingsUI.cs`
- `Assets/CyberClub/Scripts/Settings/Camera/LookSensitivitySettingsUI.cs.meta`
- `Assets/Resources/PerformanceTestRunInfo.json`
- `Assets/Resources/PerformanceTestRunInfo.json.meta`
- `Assets/Resources/PerformanceTestRunSettings.json`
- `Assets/Resources/PerformanceTestRunSettings.json.meta`
- `Assets/_Recovery/0 (1).unity`
- `Assets/_Recovery/0 (1).unity.meta`

### Bee/IL2CPP artifacts at snapshot time

- `Library/Bee/artifacts/WebGL` exists and was last updated 2026-07-24 11:03:18.
- `Temp/StagingArea` exists and was last updated 2026-07-24 11:05:44.
- `Library/Il2cppBuildCache` and `Library/BuildCache` do not exist as separate directories in this Unity version.
- `Library/Bee/artifacts/WebGL/il2cppOutput/build/GameAssembly.a` was written 2026-07-24 11:04:59.
- Conflicting object `otfoggrj1dvj.o` was written 2026-07-21 01:40:28.
- Conflicting object `v3r07366ri3p.o` was written 2026-07-24 11:03:26.

The different object timestamps are recorded as evidence, not yet as the final diagnosis. No gameplay file was changed while taking this snapshot.

## Proven linker diagnosis (before remediation)

The `duplicate symbol` failure is caused by a stale WebGL Bee object that no
longer matches its current generated C++ input.

Evidence:

- Current Bee DAG maps `v3r07366ri3p.o` to `Generics__107.cpp`.
- Current Bee DAG maps `otfoggrj1dvj.o` to `Generics__119.cpp`.
- Both object files are explicitly present in the current
  `6070537824006410562.rsp` archive response and therefore both enter
  `GameAssembly.a`.
- `v3r07366ri3p.o` was rebuilt on 2026-07-24 11:03:26 from the current
  generated C++.
- `otfoggrj1dvj.o` remained from 2026-07-21 01:40:28, even though its current
  input `Generics__119.cpp` was regenerated on 2026-07-24 11:02:54.
- The symbol
  `Property_2_Unity_Properties_Internal_IAttributes_get_Attributes_m6E6EE10EB4A6E464E96239D40DFD8DE5333591F4_gshared`
  exists in current `Generics__107.cpp`, but does not exist in current
  `Generics__119.cpp`.
- Nevertheless, `wasm-ld` finds that symbol in both current
  `v3r07366ri3p.o` and stale `otfoggrj1dvj.o`. The stale object therefore
  contains code from an older generic partition while Bee treats it as the
  output of the newly generated `Generics__119.cpp`.
- The archive was rewritten on 2026-07-24 11:04:59 with both objects, which
  explains why repeated incremental link attempts fail immediately.

Excluded alternatives:

- No `Unity.Properties.dll` exists in `Assets`, `Packages`, PackageCache, the
  PlayerScriptAssemblies directory, or the installed Unity data directory.
- `com.unity.properties` is not present as a registry, embedded, or `file:`
  dependency in `manifest.json`, `packages-lock.json`, or PackageCache.
- No duplicate asmdef assembly names were found.
- No copied `Unity.Properties` sources were found in `Assets`.
- None of the newly added runtime or validator files declares or uses
  `Property<TContainer,TValue>`, `PropertyBag`, `GeneratePropertyBag`, or
  `IAttributes`.
- The only `using Unity.Properties` hit is inside a package `Samples~`
  directory and is not part of the player compilation.
- The editor validator is already under `Assets/CyberClub/Scripts/Editor`.
- The official Unity 6000.4.0f1 release notes and Issue Tracker search did not
  identify a matching product-code defect with an official Issue ID. Unity's
  documented `BuildOptions.CleanBuildCache` is the supported way to discard
  cached build results and force complete regeneration.

Minimal remediation:

1. Close the Unity editor for this project.
2. Remove only `Library/Bee/artifacts/WebGL` and `Temp/StagingArea`; preserve
   all source assets, packages, project settings, and the rest of `Library`.
3. Run the next Development WebGL build with
   `BuildOptions.CleanBuildCache` into a new output directory.

Risk: the next build is slower because all WebGL build products are
regenerated. No gameplay data or user source change is required to fix the
linker fault.

## Remediation performed

- Unity Editor and all Unity/IL2CPP/WebGL child processes were confirmed
  stopped.
- Removed `D:/UnityProjects/CyberClub/Library/Bee/artifacts/WebGL`.
- `D:/UnityProjects/CyberClub/Temp/StagingArea` had already been removed when
  Unity closed, so no action was needed for it.
- No other part of `Library`, no source asset, package, project setting, or
  previous build output was deleted.
- The dedicated batch build method now combines `Development` with
  `CleanBuildCache`.

## Unity validation after remediation

- Unity version: `6000.4.0f1`.
- Batchmode script compilation completed without C# compiler errors.
- The real project scene `Assets/Scenes/MainScene.unity` was opened and
  validated.
- Validation result: `valid=True`.
- Scene objects: 833.
- Canvases: 11 (10 world-space).
- Missing scripts: 0.
- Broken serialized references: 0.
- EventSystem: exactly 1.
- InputSystemUIInputModule: exactly 1.
- Payment service, mobile joystick, mobile look area, and PlayerRotation:
  exactly 1 each with required references.
- LocationPurchaseDialog: exactly 1 component after removing the obsolete
  inactive duplicate.
- Validation log:
  `C:/Users/barin/.codex/visualizations/2026/07/18/019f7441-0fae-76e1-a07f-abdb449b9ee3/CyberClub.Validation.log`.

## Recovered previous work

### Premium purchase

- Product ID is exactly `premium_zone_100`.
- Catalog states are implemented: `NotStarted`, `Loading`, `Loaded`,
  `ProductNotFound`, `SdkUnavailable`, `Failed`, and the explicit
  editor-only `EditorFallback`.
- Catalog timeout is 12 seconds (Inspector range 10–15 seconds).
- Failure states expose Retry and cannot leave the dialog in an endless
  loading state.
- Production UI uses the price returned by the Yandex catalog. The
  `100 YAN` value is editor preview only.
- The editor preview cannot make a real purchase.
- The WebGL bridge calls the current Yandex SDK form
  `payments.purchase({ id: productId })`.
- The premium purchase is deliberately not consumed. Existing unprocessed
  `getPurchases()` data restores the entitlement after save data is ready.
- An already unlocked zone is not offered again.
- Premium purchase does not grant the old one-time 100-gem reward.
- The premium zone session configuration remains 100 coins and 1 gem per
  hour in `Assets/CyberClub/Configs/Zones/PremiumZone.asset`.

### Mobile input and camera

- One floating joystick is present in the real MainScene; Floating is the
  default mode.
- Activation area is the left 40% of the screen.
- Pointer ID, dead zone, radius, edge clamp, reset, UI guard, and multitouch
  separation are implemented.
- Mobile look uses its own captured touch and pointer ID, excludes the
  movement joystick pointer, and does not use `primaryTouch`.
- Screen-space and world-space UI filtering goes through
  `MobilePointerUiGuard`.
- Technical joystick/look-area graphics have raycast target disabled.
- Direct Cinemachine input is disabled; filtered look goes through
  `PlayerInputReader` and `PlayerRotation`.
- Mouse/trackpad pointer deltas are not multiplied by `Time.deltaTime`.
  Gamepad angular speed is time-based; mobile input is normalized against
  screen dimensions.
- Desktop mouse, trackpad, and mobile sensitivities are separate and saved
  in PlayerPrefs.
- `LookSensitivitySettingsUI` binds existing Inspector-assigned sliders; it
  does not create UI controls at runtime.

### Safe optimization work

- Empty per-frame methods were removed from `Visitor` and `GemsPurchase`.
- `DeviceRegistry` avoids producing a temporary list in its random-free-device
  hot path.
- The payment dialog reacts to state-change events instead of polling the
  catalog every frame.
- `OptimizationAudit.md` separates static observations from measurements and
  does not invent Profiler values.
- No blind lighting, balance, or quality change was made in this recovery.

## Final Development WebGL build

- Unity command/exit path completed successfully.
- Build result: `Succeeded`.
- Errors: 0.
- Warnings: 35 (existing obsolete API and shader warnings plus one benign
  player-only unused editor-preview-field warning).
- Duration: 00:11:12.6378520.
- Reported size: 197,385,141 bytes.
- `duplicate symbol`: 0.
- `undefined symbol`: 0.
- `emcc: error`: 0.
- `wasm-ld: error`: 0.
- `.jslib` errors: 0.
- C# compiler errors: 0.
- Required files exist:
  - `index.html`
  - `Build/CyberClub_WebGL_Development_20260724_1152.loader.js`
  - `Build/CyberClub_WebGL_Development_20260724_1152.framework.js`
  - `Build/CyberClub_WebGL_Development_20260724_1152.data`
  - `Build/CyberClub_WebGL_Development_20260724_1152.wasm`
- Both `CyberClub_RetryPaymentsCatalog_js` and
  `CyberClub_BuyNonConsumable_js`, including the new non-consumable purchase
  implementation, are present in the generated `framework.js`.
- Build log:
  `C:/Users/barin/.codex/visualizations/2026/07/18/019f7441-0fae-76e1-a07f-abdb449b9ee3/CyberClub.WebGL.Development.log`.
- Build output:
  `C:/Users/barin/.codex/visualizations/2026/07/18/019f7441-0fae-76e1-a07f-abdb449b9ee3/CyberClub_WebGL_Development_20260724_1152`.
- The build was not published.

The regenerated objects `otfoggrj1dvj.o` and `v3r07366ri3p.o` now both have
timestamps from the successful build (12:01:37 and 12:02:50 respectively).
The rebuilt archive links successfully, providing the A/B confirmation that
the original fault was stale incremental state rather than source code or a
duplicate package.

## Yandex Console and Inspector handoff

Create one in-app item in the Yandex Games console:

- ID: `premium_zone_100`
- Suggested configured price: 100 YAN (the runtime still displays the actual
  catalog price)
- Type/handling: permanent/non-consumable entitlement
- Add matching Russian and English title, description, and a 256x256 PNG icon
- Enable purchases for the game as required by the Yandex console, but do not
  upload or publish this local build as part of this task

Inspector assignments already validated in MainScene:

- `CyberClubYG2PaymentsService`: Product ID, 12-second timeout, premium
  unlocker, SaveLoadManager, and feedback presenter.
- Active `LocationPurchaseDialog`: view, regular ZonePurchase, payment
  service, premium unlocker/config/information, feedback presenter, and
  InteractionWithUI.
- `PlayerRotation`: player head, InteractionWithUI, PlayerInputReader, and
  third-person orbit.
- Floating joystick: left 40% activation area, radius 120, dead zone 0.12,
  edge padding 24.

If sensitivity sliders are added to the settings screen, attach
`LookSensitivitySettingsUI` to that screen and assign PlayerRotation, the
desktop mouse slider, trackpad slider, mobile slider, and the optional
trackpad-preference toggle. The component intentionally does not generate
sliders itself.

## Remaining device checks

These require a real phone/tablet and cannot be proven in batchmode:

- simultaneous movement and look with two fingers;
- touches over repair controls, ScrollView, screen-space UI, and world-space
  UI;
- floating joystick placement and edge clamp at different aspect ratios;
- open/close-window pointer reset;
- mobile look speed and vertical/horizontal feel;
- returning from the Yandex payment frame and focus restoration;
- purchase cancellation, offline timeout/Retry, completed purchase, and
  entitlement restoration on another browser/device.

## Final working-tree summary

- No staged files.
- No commit was created.
- `git diff --check`: passed; only line-ending conversion warnings were
  printed.
- Tracked diff: 15 files, 1,317 insertions, 364 deletions.
- The YG2 build process updated
  `Assets/PluginYourGames/Editor/BuildLogYG2.txt` from build number 1 to 2 and
  recorded the successful output path.
- The real `D:/UnityProjects/CyberClub/Assets/Scenes/MainScene.unity` contains
  the payment and mobile-input changes and now has exactly one
  LocationPurchaseDialog component.

### Modified tracked files

- `Assets/CyberClub/Scripts/Admin/AdminLogic/VisitorService.cs`
- `Assets/CyberClub/Scripts/DeviceSystem/Spawn/DeviceRegistry.cs`
- `Assets/CyberClub/Scripts/DeviceSystem/Spawn/SpawnPointsHolder.cs`
- `Assets/CyberClub/Scripts/Player/MobileControlsHUD.cs`
- `Assets/CyberClub/Scripts/Player/MobileLookArea.cs`
- `Assets/CyberClub/Scripts/Player/MobileVirtualJoystick.cs`
- `Assets/CyberClub/Scripts/Player/PlayerInputReader.cs`
- `Assets/CyberClub/Scripts/Player/Rotation/PlayerRotation.cs`
- `Assets/CyberClub/Scripts/Purchases/Root/GemsPurchase.cs`
- `Assets/CyberClub/Scripts/Visitors/Visitor.cs`
- `Assets/CyberClub/Scripts/YG2Services/CyberClubYG2PaymentsService.cs`
- `Assets/CyberClub/Scripts/Zones/Barrier/BarrierDissolve.cs`
- `Assets/CyberClub/Scripts/Zones/UI/LocationPurchaseDialog.cs`
- `Assets/PluginYourGames/Editor/BuildLogYG2.txt`
- `Assets/Scenes/MainScene.unity`

### Untracked files

- `Assets/CyberClub/Documentation.meta`
- `Assets/CyberClub/Documentation/InterruptedStageRecovery.md`
- `Assets/CyberClub/Documentation/InterruptedStageRecovery.md.meta`
- `Assets/CyberClub/Documentation/OptimizationAudit.md`
- `Assets/CyberClub/Documentation/OptimizationAudit.md.meta`
- `Assets/CyberClub/Plugins.meta`
- `Assets/CyberClub/Plugins/WebGL.meta`
- `Assets/CyberClub/Plugins/WebGL/CyberClubPaymentsBridge.jslib`
- `Assets/CyberClub/Plugins/WebGL/CyberClubPaymentsBridge.jslib.meta`
- `Assets/CyberClub/Scripts/Editor/CyberClubMainSceneValidator.cs`
- `Assets/CyberClub/Scripts/Editor/CyberClubMainSceneValidator.cs.meta`
- `Assets/CyberClub/Scripts/Player/MobilePointerUiGuard.cs`
- `Assets/CyberClub/Scripts/Player/MobilePointerUiGuard.cs.meta`
- `Assets/CyberClub/Scripts/Settings/Camera/LookSensitivitySettingsUI.cs`
- `Assets/CyberClub/Scripts/Settings/Camera/LookSensitivitySettingsUI.cs.meta`
- `Assets/_Recovery/0 (1).unity`
- `Assets/_Recovery/0 (1).unity.meta`

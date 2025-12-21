# KettlebellAR Copilot Instructions

**AR educational app** built with **Unity + AR Foundation**, teaching kettlebell/lego assembly through image tracking, object placement, and interactive UI guidance.

## Architecture Overview

### Four-Stage State Flow

1. **Stage 0** (Menu) → `MainMenuManager.OnExplorePressed()` triggers entry
2. **Stage 1** (Detection) → `LegoImageDetector.EnableTracking()` scans "hub_for_detection" marker
3. **Stage 2** (Placement) → `PlaceByTouch` raycasts AR plane, user selects and rotates model
4. **Stage 3** (Instructions) → `InstructionSwipe` shows video/text/images with swipe navigation

### Critical Control Flow

```
MainMenuManager.OnExplorePressed()
  ├─ detector.ResetDetector()          // Clear all previous state
  ├─ detector.EnableTracking()         // Start image tracking
  ├─ instructionOpening0.OpenInstructions()  // Show Stage 0 UI
  └─ BackButton.SetActive(true)        // Enable exit

LegoImageDetector.OnImageChanged()  [After image detected]
  ├─ placeByTouch.enabled = true      // Activate plane detection
  ├─ Play photo_scanned_sound
  └─ Fire OnModelPlaced event → triggers Stage 3

PlaceByTouch.HandleModelPlaced()
  ├─ Model spawns at plane center
  ├─ Play model_spawned_sound
  └─ Open stage-appropriate InstructionSwipe

BackButtonUI.OnBackPressed()
  ├─ detector.ResetDetector()         // Teardown tracking
  ├─ Disable all fullScreenPopups
  ├─ Deactivate scannedModel
  └─ mainMenuPanel.SetActive(true)
```

## Key Files & Responsibilities

| File | Role |
|------|------|
| [LegoImageDetector.cs](Assets/Scripts/AR/LegoImageDetector.cs) | AR image tracking lifecycle, model placement trigger |
| [PlaceByTouch.cs](Assets/Scripts/AR/PlaceByTouch.cs) | Plane raycast, model positioning, rotation via touch |
| [InstructionSwipe.cs](Assets/Scripts/AR/InstructionSwipe.cs) | Instruction UI: video/text/image steps, swipe navigation |
| [MainMenuManager.cs](Assets/Scripts/AR/MainMenuManager.cs) | Entry point, orchestrates stage 0→1 transition |
| [BackButtonUI.cs](Assets/Scripts/AR/BackButtonUI.cs) | Cleanup & reset state on menu return |
| [ResetExploreManager.cs](Assets/Scripts/AR/ResetExploreManager.cs) | Full explore teardown (windows, popups, AR) |

## State Management: The Reset Pattern

⚠️ **Critical**: Explicit state reset prevents ghost interactions after menu return.

### Full Reset Checklist (after any stage change or menu return):
```csharp
// In LegoImageDetector.ResetDetector() or BackButtonUI.OnBackPressed()
imageDetected = false;              // Block re-detection
planePlaced = false;                // Allow placement restart
stage0Completed = false;            // Allow instruction restart
placeByTouch.ResetPlacement();      // Deactivate model/planes
placeByTouch.enabled = false;       // Disable raycast update
manager.enabled = false;            // Stop tracking
manager.enabled = true;             // Restart (NO ARSession.Reset()!)
```

**Why**: `ARSession.Reset()` destroys all planes. Toggling `.enabled` re-initializes without destroying session.

## Component Communication

### Events (Preferred Over Direct Calls)
- `ARTrackedImageManager.trackedImagesChanged` → `LegoImageDetector.OnImageChanged()`
- `PlaceByTouch.OnModelPlaced` (System.Action) → triggers `LegoImageDetector.HandleModelPlaced()`
- `InstructionSwipe.OnInstructionsClosed` (System.Action) → fires on swipe completion

### Null-Safe Component Lookup
```csharp
// Fallback when Inspector reference missing (Awake phase)
if (manager == null)
    manager = FindFirstObjectByType<ARTrackedImageManager>();
if (placeByTouch == null)
    placeByTouch = FindFirstObjectByType<PlaceBytouch>();
```
Prefer **explicit Inspector assignment** for clarity, but fallback ensures robustness.

## AR Foundation Specifics

### Image Tracking
- Marker image **must** be named exactly `"hub_for_detection"` in AR database
- Detection is **idempotent**: set `imageDetected = true` immediately to prevent re-triggers
- Check `TrackingState.Tracking` in `OnImageChanged()` before spawning model

### Plane Placement
```csharp
// From screen touch, raycast to detect plane
raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon);
selectedPlane = planeManager.GetPlane(hits[0].trackableId);

// Position model at plane center (NOT plane-rotated)
Lego3D.transform.position = selectedPlane.center;
Lego3D.transform.rotation = Quaternion.identity;  // Always upright
```
Model rotation handled separately via `HandleRotationDrag()`, **not** plane alignment.

## UI & Localization

### InstructionSwipe Step Structure
```csharp
[System.Serializable]
public class Step
{
    public VideoClip video;        // Primary content
    public Sprite image;           // Fallback if no video
    [TextArea(5, 10)]
    public string text;            // Localized (RTL-safe)
}
```

### Mutual-Exclusion Popup Pattern
```csharp
// When opening any InstructionSwipe, close siblings
foreach (var swipe in FindObjectsOfType<InstructionSwipe>())
{
    if (swipe != this)
    {
        swipe.fullScreenPopup.SetActive(false);
        swipe.isActive = false;
    }
}
```
Prevents overlapping UI popups across stages.

### RTL Text Support
Project uses **RTLTextMeshPro** for Hebrew/English bidirectional text:
```csharp
using RTLTMPro;
public RTLTextMeshPro stepText;  // Reference RTLTextMeshPro directly, not TextMeshProUGUI
```

## Audio Polish

Sound effects tied to state transitions:
- `photo_scanned_sound` → plays in `LegoImageDetector` on image detect
- `model_spawned_sound` → plays on model placement
- `swipeSound` → plays on instruction swipe
- `buttonClickSound` → plays on all UI clicks

Use `audioSource.PlayOneShot()` to queue multiple sounds without stopping current playback.

## Coding Conventions

- **Hebrew comments preserved** — Codebase has Hebrew comments for context; add English equivalents for clarity
- **Debug prefix pattern** → `Debug.Log("[ComponentName] MESSAGE")` for easy log filtering
- **Guard clauses** → early `return` statements prevent nested pyramids
- **Guard flags** → e.g., `stage0Completed` block stage 1 until stage 0 instruction closes

## Common Pitfalls

❌ **Never call `ARSession.Reset()`** — destroys all planes, use `.enabled` toggle instead  
❌ **Multiple `OpenInstructions()` calls** — causes popup overlap, use mutual-exclusion pattern  
❌ **Skip state flag reset** → ghost interactions after menu return (see Reset Pattern)  
❌ **Assume model is instantiated** → it's repositioned from prefab, always check `SetActive()`  
❌ **Forget `placeByTouch.enabled = true` after detection** → raycast won't work  

## Debugging Workflow

**Log the happy path**: Watch for this sequence in Console:
```
[LegoImageDetector] EnableTracking
[LegoImageDetector] OnImageChanged → IMAGE DETECTED
[PlaceByTouch] Plane selected
[PlaceByTouch] MODEL PLACED → Stage 3 opens
```

**Verify AR setup**:
- Marker image in AR database: Project Settings → XR Plug-in Management → Reference Images
- Device camera permissions enabled (iOS/Android)
- Test on real device (simulator plane detection unreliable)

**UI state inspection**:
- MainMenuManager has `exploreWindows[]` debug array — check `Debug.Log` output in `DebugExploreWindows()`

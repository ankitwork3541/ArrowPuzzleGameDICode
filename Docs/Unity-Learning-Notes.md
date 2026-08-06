# Unity Beginner Learning Notes — ArrowPuzzleGame

These are personal reference notes covering everything learned step-by-step before building the real game. Written in simple language on purpose — read this any time something is forgotten.

---

## Table of Contents

1. [Step 1-2: C# Scripting Basics + Movement](#step-1-2-c-scripting-basics--movement)
2. [Step 3: Input Handling (Click/Touch Detection)](#step-3-input-handling-clicktouch-detection)
3. [Step 4: Prefabs + Instantiate/Destroy](#step-4-prefabs--instantiatedestroy)
4. [Step 5: Lists, Arrays, and Loops](#step-5-lists-arrays-and-loops)
5. [Step 6: UI Basics (Canvas, Button, Text)](#step-6-ui-basics-canvas-button-text)
6. [Step 7: ScriptableObjects](#step-7-scriptableobjects)
7. [Step 8: Grid Math](#step-8-grid-math)
8. [Step 9: DOTween Animations](#step-9-dotween-animations)
9. [Step 10: Object Pooling](#step-10-object-pooling)
10. [Old Input System vs New Input System](#old-input-system-vs-new-input-system)
11. [Notes on the MultiTech Studio Template](#notes-on-the-multitech-studio-template)
12. [Quick Cheat Sheet](#quick-cheat-sheet)

---

## Step 1-2: C# Scripting Basics + Movement

**Why this matters:** Everything in Unity is controlled by attaching a script (a C# file) to a GameObject. This is the very first building block — without this, nothing else works.

### How to create and attach a script

1. In the Project window, go to `Assets/Project/Scripts/Core`
2. Right-click → `Create > C# Script`, give it a name (e.g. `TestMove`)
3. Drag the script onto a GameObject in the Hierarchy (or use `Add Component` in the Inspector)
4. Double-click the script to open and edit it in the code editor

### Basic movement code (New Input System version)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class TestMove : MonoBehaviour
{
    public float speed = 3f;

    void Update()
    {
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
        if (Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;
        if (Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;

        transform.position += (Vector3)moveInput.normalized * speed * Time.deltaTime;
    }
}
```

### Line-by-line meaning

| Line | Meaning |
|------|---------|
| `using UnityEngine;` | Needed to use Unity's built-in things (Transform, Vector3, etc.) |
| `public class TestMove : MonoBehaviour` | This script is a "component" that can be attached to a GameObject |
| `public float speed = 3f;` | A variable — also shows up in the Inspector, can be changed without touching code |
| `void Update()` | A special function that Unity calls **automatically, every frame** (60+ times per second) |
| `Keyboard.current.leftArrowKey.isPressed` | Checks if a specific key is currently held down |
| `transform.position += ...` | Changes the object's position — this **is** the movement |
| `Time.deltaTime` | The time between frames. Without this, movement speed would depend on how fast the computer is (bad) |

### Key concepts to remember

- A **variable** marked `public` shows up in the Inspector and can be tweaked without editing code.
- `Update()` runs constantly, every single frame, automatically — no need to call it manually.
- `transform.position` is how you read or change where an object is in the world.

---

## Step 3: Input Handling (Click/Touch Detection)

**Why this matters:** In the real game, tapping an arrow is what makes it move — not continuous keyboard movement. This step is about detecting "was this specific object clicked?"

### Click detection code (New Input System version)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class TestClickMove : MonoBehaviour
{
    public float moveDistance = 1f;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (col.OverlapPoint(worldPos))
                transform.position += Vector3.up * moveDistance;
        }
    }
}
```

### Setup requirement

The GameObject **must have a Collider2D** (e.g. `Box Collider 2D`) attached — otherwise there's nothing to check the click against.

### Line-by-line meaning

| Line | Meaning |
|------|---------|
| `Collider2D col;` + `GetComponent<Collider2D>()` | Grabs a reference to this object's own collider, once, at startup |
| `Mouse.current.leftButton.wasPressedThisFrame` | True only on the exact frame the mouse button was clicked (not held) |
| `Mouse.current.position.ReadValue()` | Gets the mouse's screen position (in pixels) |
| `Camera.main.ScreenToWorldPoint(...)` | Converts a screen pixel position into a world position (needed because clicks are in pixels, but objects live in world coordinates) |
| `col.OverlapPoint(worldPos)` | Checks: "does this collider actually contain this world point?" — this is how we know *this specific object* was clicked |

### Key concept

Each object with this script has its **own independent** `Collider2D` reference — clicking on Box A never affects Box B, because each box runs its own separate copy of the script.

The old way (`OnMouseDown()`) does something similar automatically, but the manual `OverlapPoint` approach (shown above) is closer to what the real game code (`ArrowLine.cs`) actually does, and works better across mouse + touch.

---

## Step 4: Prefabs + Instantiate/Destroy

**Why this matters:** In the real game, arrows are *spawned* when a level starts, and *destroyed* when they exit the screen. This requires Prefabs + `Instantiate`/`Destroy`.

### What is a Prefab

A Prefab is a **reusable blueprint** for a GameObject, saved as an asset file (not living in any particular scene). Think of it like a cookie-cutter mould — one design, many independent copies stamped out from it.

### How to make a Prefab

1. Build the GameObject in the Hierarchy exactly how it should look (sprite, collider, scripts, etc.)
2. Drag it from the **Hierarchy** into the **Project window** (into a folder like `Assets/Project/Prefabs`)
3. This creates a `.prefab` file — that's the Prefab
4. Delete the leftover instance from the Hierarchy (the asset file is what matters now)

### Destroy-on-click script (goes on the Prefab itself)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class TestClickDestroy : MonoBehaviour
{
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (col.OverlapPoint(worldPos))
            {
                Destroy(gameObject);
            }
        }
    }
}
```

### Spawner script (goes on a SEPARATE, empty GameObject — never on the thing being spawned!)

```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class TestSpawner : MonoBehaviour
{
    public GameObject boxPrefab;
    private int spawnCount = 0;

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Vector3 spawnPosition = new Vector3(spawnCount * 1.5f, 0f, 0f);
            Instantiate(boxPrefab, spawnPosition, Quaternion.identity);
            spawnCount++;
        }
    }
}
```

### Line-by-line meaning

| Line | Meaning |
|------|---------|
| `public GameObject boxPrefab;` | A slot in the Inspector — drag a Prefab asset into it |
| `Instantiate(boxPrefab, position, rotation)` | Creates a **new copy** of the Prefab at runtime, at the given position |
| `Quaternion.identity` | Means "no rotation" — the default/neutral rotation |
| `Destroy(gameObject)` | Removes **this specific object** from the game completely |

### ⚠️ Important bug to avoid (self-reference infinite loop)

**Never assign a Prefab that has the Spawner script on it, into its own Spawner's prefab field.** If the GameObject that spawns things *is itself* the thing being spawned, then every spawned copy will *also* try to spawn more copies (because it has the same script) — this creates exponential, infinite object creation and crashes/freezes Unity.

**Rule to remember:** the object doing the spawning and the object being spawned must always be **two separate things**.

---

## Step 5: Lists, Arrays, and Loops

**Why this matters:** The real game stores each arrow's path as a `List<Vector2Int>` — a growable list of grid coordinates. This step builds the exact same pattern in miniature.

### List vs Array

| | Array | List |
|---|---|---|
| Size | Fixed — decided when created | Can grow — use `.Add()` anytime |
| Used for | Fixed known quantity | Unknown/variable quantity (like arrow paths — different lengths per arrow) |

### For loop — repeating an action

```csharp
for (int i = 0; i < 5; i++)
{
    Debug.Log("Hello " + i);
}
```
This runs 5 times automatically, printing "Hello 0" through "Hello 4" — instead of writing the same line 5 times by hand.

### Mini path system — combining List + loop + Instantiate

```csharp
using System.Collections.Generic;
using UnityEngine;

public class TestPathSpawner : MonoBehaviour
{
    public GameObject boxPrefab;
    public List<Vector2Int> path = new List<Vector2Int>();

    void Start()
    {
        path.Add(new Vector2Int(0, 0));
        path.Add(new Vector2Int(1, 0));
        path.Add(new Vector2Int(2, 0));
        path.Add(new Vector2Int(2, 1));
        path.Add(new Vector2Int(2, 2));

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int gridPos = path[i];
            Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0f);
            Instantiate(boxPrefab, worldPos, Quaternion.identity);
        }
    }
}
```

### Line-by-line meaning

| Line | Meaning |
|------|---------|
| `using System.Collections.Generic;` | Required at the top of the file to use `List<T>` |
| `List<Vector2Int> path = new List<Vector2Int>();` | Creates an empty list that will hold `Vector2Int` (grid coordinates, x and y) |
| `path.Add(new Vector2Int(0, 0));` | Adds one item to the list |
| `path.Count` | How many items are currently in the list |
| `for (int i = 0; i < path.Count; i++)` | Loops once for every item in the list |
| `path[i]` | Grabs the item at index `i` from the list |

### Real game connection

`LevelAsset.cs` (in the MultiTech Studio template) has this exact same field:
```csharp
public List<Vector2Int> path = new();
```
This is the field that stores one arrow's full path for a level.

---

## Step 6: UI Basics (Canvas, Button, Text)

**Why this matters:** Hearts, level numbers, hint buttons — all of this is UI. This step covers how to build UI and connect it to code.

### What is a Canvas

A **Canvas** is the required container for all UI elements (buttons, text, images). Nothing UI-related shows up without one.

- Hierarchy → right-click → `UI > Canvas` (this also auto-creates an `EventSystem`, required for UI clicks to register — never delete it)

### Adding Text and a Button

- Under the Canvas: right-click → `UI > Text - TextMeshPro` (first time ever, Unity will prompt "Import TMP Essentials" — click **Import**)
- Under the Canvas: right-click → `UI > Button - TextMeshPro`

### Script to update text and respond to button clicks

```csharp
using UnityEngine;
using TMPro;

public class TestUIManager : MonoBehaviour
{
    public TMP_Text scoreText;
    private int score = 0;

    public void IncreaseScore()
    {
        score++;
        scoreText.text = "Score: " + score;
    }
}
```

### Line-by-line meaning

| Line | Meaning |
|------|---------|
| `using TMPro;` | Required to use `TMP_Text` (TextMeshPro's text type) |
| `public TMP_Text scoreText;` | A slot in the Inspector — drag the Text object into it |
| `public void IncreaseScore()` | Must be `public` — only public functions can be hooked up to a Button's click event |
| `scoreText.text = "Score: " + score;` | Updates what the text actually displays on screen |

### Wiring a Button to call the function

1. Put `TestUIManager` on some GameObject (e.g. an empty `UIManager` object)
2. Drag the `ScoreText` object into the script's `Score Text` field in the Inspector
3. Select the Button → in the Inspector, find the **Button** component → **OnClick()** section
4. Click `+` to add a new entry
5. Drag the `UIManager` object into the object slot
6. From the dropdown, choose `TestUIManager > IncreaseScore()`

Now clicking the button in Play mode increases the score and updates the text.

---

## Step 7: ScriptableObjects

**Why this matters:** This is the single most important concept for handling 50,000 levels. Every level in the real game (`Level1.asset` through `Level100.asset`) is a ScriptableObject.

### MonoBehaviour vs ScriptableObject

| | MonoBehaviour | ScriptableObject |
|---|---|---|
| Where it lives | Attached to a GameObject, inside a Scene | Lives as a standalone file (asset) in the Project window |
| What happens when scene changes | Gets destroyed/reset | Stays exactly as it is — has no connection to any scene |
| What it's for | Behavior/logic (movement, input, etc.) | Pure **data** (level info, settings, configuration) |

### Creating a ScriptableObject class

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "TestLevelData", menuName = "Tools/Test Level Data")]
public class TestLevelData : ScriptableObject
{
    public string levelName = "Level 1";
    public int difficulty = 1;
    public int coinsReward = 10;
}
```

### Line-by-line meaning

| Line | Meaning |
|------|---------|
| `[CreateAssetMenu(fileName = ..., menuName = ...)]` | Adds a new option to the Project window's right-click `Create` menu |
| `: ScriptableObject` | Instead of `: MonoBehaviour` — this is a data file, not something that goes on a GameObject |
| The fields below | Plain data — editable directly in the Inspector once the asset is created |

### Creating and using the actual data file

1. Right-click in the Project window → `Create > Tools > Test Level Data`
2. Name it something like `Level1Data`
3. Click on it — the Inspector shows editable fields (`levelName`, `difficulty`, `coinsReward`) — no code needed to change values

### Reading the data from a normal script

```csharp
using UnityEngine;
using TMPro;

public class TestLevelDataReader : MonoBehaviour
{
    public TestLevelData levelData;
    public TMP_Text displayText;

    void Start()
    {
        displayText.text = levelData.levelName + " (Difficulty: " + levelData.difficulty + ")";
    }
}
```

Drag the `Level1Data` asset into the `Level Data` field, and a Text object into `Display Text`. On Play, it reads the values straight from the asset file.

### Real game connection

`LevelAsset.cs` in the template is exactly this pattern — a ScriptableObject holding a level's grid size and all its arrow paths. There are 100 separate `.asset` files, each with different data, but the game logic code (`LevelManager`) that reads them is the same for every single one. For 50,000 levels, the same idea applies: 50,000 data files, one shared piece of code.

---

## Step 8: Grid Math

**Why this matters:** Code thinks in simple grid coordinates like `(2, 3)`. Unity needs an actual world position like `(3.5, -2.1, 0)` to place things. This is the formula that converts between the two.

### The core idea (in plain words)

Think of graph paper made of small boxes, each exactly 1 cm wide.

- "Box number 0" is the very first box.
- "Box number 3" is, by ruler, `3 × 1 cm = 3 cm` away from the start.

**The formula is just multiplication:**
```
World Position = Grid Number × Box Size
```

- **Grid Number** = the simple counting coordinate (e.g. row 2, column 3) — this is the `grid coordinate`.
- **Box Size** = how big each cell is in Unity's world units — this is called `spacing`.
- **World Position** = the real `transform.position` Unity actually uses.

### Example table (spacing = 1)

| Grid Number | Calculation | World Position |
|---|---|---|
| 0 | 0 × 1 | 0 |
| 1 | 1 × 1 | 1 |
| 2 | 2 × 1 | 2 |
| 5 | 5 × 1 | 5 |

### Example table (spacing = 2 — bigger cells)

| Grid Number | Calculation | World Position |
|---|---|---|
| 0 | 0 × 2 | 0 |
| 1 | 1 × 2 | 2 |
| 2 | 2 × 2 | 4 |
| 3 | 3 × 2 | 6 |

Same grid numbers, but bigger spacing pushes the actual world position further apart.

### Grid → World code

```csharp
float worldX = col * spacing;
float worldY = row * spacing;
```

### World → Grid code (the reverse — used to figure out which cell was clicked)

```csharp
int gridX = Mathf.RoundToInt((worldPos.x - origin.x) / spacing);
int gridY = Mathf.RoundToInt((worldPos.y - origin.y) / spacing);
```
`Mathf.RoundToInt` is needed because a click's exact position is rarely perfectly on a grid line — this rounds to the *nearest* cell.

### Full example — spawning an entire grid (nested loop)

```csharp
using UnityEngine;

public class TestGridMath : MonoBehaviour
{
    public GameObject boxPrefab;
    public int width = 5;
    public int height = 5;
    public float spacing = 1f;

    void Start()
    {
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                float worldX = col * spacing;
                float worldY = row * spacing;

                Vector3 worldPos = new Vector3(worldX, worldY, 0f);
                Instantiate(boxPrefab, worldPos, Quaternion.identity);
            }
        }
    }
}
```

A **nested loop** (a loop inside another loop) is required because a grid has two dimensions: the outer loop walks through each **row**, and for every row, the inner loop walks through every **column**.

### Real game connection

`GridManager.cs`'s `GridToWorld()` and `WorldToGrid()` functions use this exact same math, just written with slightly different variable names (`origin.x + g.x * spacing`).

---

## Step 9: DOTween Animations

**Why this matters:** Without DOTween, moving something to a new position happens **instantly** (a jump/teleport). The real game slides arrows smoothly. DOTween is the animation library (already installed in this project) that makes smooth movement easy.

### The core idea (in plain words)

- **Without DOTween** (`transform.position += ...`): the object **teleports** — one frame it's here, the next frame it's there, no in-between.
- **With DOTween** (`transform.DOMove(...)`): you say "walk from here to there, taking this many seconds" — DOTween calculates every position in between automatically and moves smoothly through all of them.

### The one-line magic

```csharp
transform.DOMove(destinationPosition, 2f);
```
Read it like English: *"Transform, do a Move to destinationPosition, taking 2 seconds."*

### Example with numbers

Object starts at `(0, 0)`. This code:
```csharp
transform.DOMove(new Vector3(5, 0, 0), 2f);
```
means: over the next 2 seconds, the object smoothly slides from `(0,0)` to `(5,0)`, passing through every point in between — visibly moving, not jumping.

### Practical example — smooth click-to-move

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class TestDOTweenMove : MonoBehaviour
{
    public float moveDistance = 2f;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (col.OverlapPoint(worldPos))
            {
                Vector3 targetPos = transform.position + Vector3.up * moveDistance;
                transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutBack);
            }
        }
    }
}
```

### Line-by-line meaning

| Line | Meaning |
|------|---------|
| `using DG.Tweening;` | Required at the top to use DOTween functions |
| `transform.DOMove(targetPos, 0.5f)` | Smoothly moves to `targetPos` over `0.5` seconds instead of jumping instantly |
| `.SetEase(Ease.OutBack)` | Controls the *feel* of the animation — `OutBack` overshoots slightly then settles (like a soft bounce). Other options: `Linear` (constant speed), `OutBounce` (bouncy ball feel), `InOutQuad` (smooth start/stop) |

### Other useful tweens (bonus, same pattern)

```csharp
transform.DOScale(1.5f, 0.3f);              // Smoothly scales to 1.5x size over 0.3s
spriteRenderer.DOColor(Color.red, 0.3f);    // Smoothly fades color to red over 0.3s
```

### Running code after an animation finishes

```csharp
transform.DOMove(targetPos, 0.5f).OnComplete(() => {
    Debug.Log("Animation finished!");
});
```

### Real game connection

`ArrowLine.cs` uses `DOTween.To(...)` to smoothly slide the arrow's line points. `GridManager.cs` uses `DOScale`/`DOColor` to animate the grid dots when a level starts or completes.

---

## Step 10: Object Pooling

**Why this matters:** Repeatedly `Instantiate`-ing and `Destroy`-ing objects thousands of times (across 50,000 levels) is slow and can lag the game, especially on phones. Object Pooling solves this by reusing objects instead of constantly creating/destroying them.

### The core idea (in plain words)

A restaurant doesn't buy a brand-new plate for every customer and throw it away after one use — that would be wasteful and slow. Instead, the plate is **washed and reused**.

Object Pooling does the same thing for GameObjects:
- Create a fixed batch of objects **once**, right at the start
- When one is "needed," don't create a new one — just turn a hidden one **ON** and reposition it
- When done with it, don't `Destroy()` it — just turn it **OFF** (hide it) so it's ready to be reused later

### Pool manager script

```csharp
using System.Collections.Generic;
using UnityEngine;

public class TestObjectPool : MonoBehaviour
{
    public GameObject boxPrefab;
    public int poolSize = 5;

    private List<GameObject> pool = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(boxPrefab);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    public GameObject GetFromPool(Vector3 position)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                obj.transform.position = position;
                obj.SetActive(true);
                return obj;
            }
        }

        Debug.Log("Pool is empty!");
        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }
}
```

### Line-by-line meaning

| Line | Meaning |
|------|---------|
| `Start()` loop | Creates `poolSize` objects **one time only**, immediately hides them all (`SetActive(false)`) |
| `GetFromPool(position)` | Searches the pool for any currently-OFF (inactive) object, turns it ON at the requested position, and returns it — **never creates anything new** |
| `if (!obj.activeInHierarchy)` | Checks "is this object currently hidden/free?" |
| `ReturnToPool(obj)` | Instead of `Destroy`, just turns the object OFF — it becomes available for reuse again |

### Proof that reuse is actually happening

- Press Space 5 times → 5 boxes appear
- Press Space a 6th time → console logs "Pool is empty!" because all 5 are currently active — nothing new gets created
- Check the Hierarchy window: the total object count stays fixed at 5 no matter how many times Space is pressed — proof that `Instantiate` only ran once, at `Start()`

### Real game connection

With thousands of levels, arrows constantly being created and destroyed would eventually slow the game down. Pooling keeps a fixed set of reusable objects instead, keeping performance consistent no matter how many levels are played — directly supporting the "premium performance" goal for this game.

---

## Old Input System vs New Input System

The MultiTech Studio template's own scripts (`ArrowLine.cs`, `ArrowLineRuntimePainterL.cs`) use Unity's **old/legacy Input system** (`Input.GetMouseButtonDown`, `Input.touchCount`, etc.) — that's fine, it's proven and working, and it does not need to be changed.

Going forward, all **custom scripts** in `Assets/Project` use the **New Input System** package instead, since it's the modern, actively-developed approach with better multi-platform/touch support.

### Setup (one-time)

1. `Window > Package Manager` → Unity Registry → search "Input System" → Install
2. `Edit > Project Settings > Player > Active Input Handling` → set to **"Both"** (keeps old and new systems working side-by-side, safest option)

### Quick comparison table

| | Old Input | New Input System |
|---|---|---|
| Keyboard check | `Input.GetAxis("Horizontal")` | `Keyboard.current.leftArrowKey.isPressed` |
| Mouse click (this frame only) | `Input.GetMouseButtonDown(0)` | `Mouse.current.leftButton.wasPressedThisFrame` |
| Mouse position | `Input.mousePosition` | `Mouse.current.position.ReadValue()` |
| Detect click on a specific object | `OnMouseDown()` (automatic Unity message) | Manually check with `Collider2D.OverlapPoint()` |

The manual `OverlapPoint` approach (not `OnMouseDown`) is the one that matches the real game's actual code pattern in `ArrowLine.cs`.

---

## Notes on the MultiTech Studio Template

This is a **purchased Unity Asset Store template** (`Assets/MultiTech Studio/Escape Game/`) that already implements a full working version of this exact type of arrow-path puzzle game.

### License

- Commercial use: **Allowed**
- Modification: **Allowed**
- Redistribution/Sublicensing: **Not allowed**

### Key scripts and what they do

| Script | Purpose |
|--------|---------|
| `GridManager.cs` | Builds and manages the dot grid, converts grid coordinates ↔ world positions |
| `Dot.cs` | Represents one grid cell; tracks which arrow (if any) currently occupies it |
| `ArrowLine.cs` | Controls one arrow's movement, tap-to-advance, blocking/collision, and exit-when-clear behavior. Uses `LineRenderer` + `EdgeCollider2D` to draw the path — **not** individually stitched sprite pieces |
| `LevelAsset.cs` | ScriptableObject holding one level's grid size and all arrow paths (`List<Vector2Int>` per arrow) |
| `GameMode.cs` | Toggles between Draw mode (level editing) and Play mode (testing) — press `H` to switch |
| `ArrowLineRuntimePainterL.cs` | The actual **level design tool** — works in Play Mode: click-drag across dots to draw an arrow path, release to commit it, then use the on-screen "Save Arrows as Level Asset" button to save the level. Also has an experimental (off by default) feature to auto-detect arrow paths from a screenshot image |
| `LevelManager.cs`, `ThemeManager.cs`, `SoundManager.cs`, `AdsManager.cs`, `IapManager.cs`, `FirebaseManager.cs`, `TutorialManager.cs`, `UiManager.cs` | Supporting systems: level progression, visual themes/skins, sound, ads, in-app purchases, analytics, tutorial, and general UI |

### Important gotchas found while exploring

1. **The bundled `Setup Guide.pdf` does NOT match the actual code.** It mentions scripts like `GridBoard`, `GameManager`, `InputHandler`, `ArrowPiece`, `ArrowVisuals` — none of these exist in this project. Ignore that PDF's setup instructions; use the real scene files and scripts instead (`Scene/Level Screen.unity` for playing, `Scene/Level Editor.unity` for designing levels).
2. **DOTween animations need a manual toggle.** Even though DOTween is already installed in the project (`Assets/Plugins/Demigiant/DOTween`), the `MT_DOTWEEN` scripting define is not enabled automatically. To turn on smooth animations in the template: `Tools > MultiTech Studio > Escape Game Setup > Settings tab > check "Enable DOTween (MT_DOTWEEN)"`.
3. **Rendering approach:** the template draws arrow paths using Unity's built-in `LineRenderer` component (one continuous line) plus a separate "head" sprite riding the tip — not individual straight/corner/cap sprite pieces stitched together.

### Level generation plan for 50,000 levels

Manually drawing 50,000 levels by hand is not realistic. The plan:

1. Write a **C# procedural generator** (an Editor script) that builds levels automatically, directly as `LevelAsset` ScriptableObject files — no JSON or AI-per-level generation needed.
2. Use the **"reverse construction"** technique to guarantee every generated level is solvable:
   - Decide the removal order arrows would be solved in: Arrow-1 first, Arrow-2 second, ... Arrow-N last.
   - Build the level in the **opposite** order: place Arrow-N first (its path can be anything, nothing else exists yet), then Arrow-(N-1) — making sure *its* exit path is clear of Arrow-N's body — and so on, down to Arrow-1.
   - Because construction enforces exactly the condition needed for forward solving, the level is automatically solvable — no separate solver/checker needed.
3. Control **difficulty** using a "recipe" of parameters per difficulty tier (grid size range, arrow count range, path length range) — quantity (50 levels vs 1,500 levels) is just a loop counter using the same recipe, completely decoupled from level *quality*.
4. Optional **hybrid** approach: mostly procedural levels, with a small number of hand-crafted "milestone" levels sprinkled in using a formula-based position (e.g. "roughly every 10th level," not a fixed absolute number) so it scales cleanly if the total count later increases. Hand-crafted levels still need to be checked for solvability manually, since the drawing tool doesn't validate that automatically.

---

## Quick Cheat Sheet

| I want to... | Use this |
|---|---|
| Move something instantly | `transform.position += direction;` |
| Move something smoothly | `transform.DOMove(target, duration);` |
| Detect a key held down | `Keyboard.current.someKey.isPressed` |
| Detect a mouse click (once) | `Mouse.current.leftButton.wasPressedThisFrame` |
| Convert screen click → world position | `Camera.main.ScreenToWorldPoint(screenPos)` |
| Check if a click hit this object | `GetComponent<Collider2D>().OverlapPoint(worldPos)` |
| Create a copy of a Prefab | `Instantiate(prefab, position, rotation)` |
| Remove an object | `Destroy(gameObject)` |
| Store a growable list of items | `List<T> myList = new List<T>();` |
| Repeat something N times | `for (int i = 0; i < N; i++) { ... }` |
| Store reusable data as a file (not in a scene) | `ScriptableObject` + `[CreateAssetMenu]` |
| Convert grid coordinate → world position | `worldPos = gridCoord * spacing` |
| Convert world position → grid coordinate | `gridCoord = Mathf.RoundToInt(worldPos / spacing)` |
| Reuse objects instead of constantly creating/destroying | Object Pooling (`SetActive(true/false)` instead of `Instantiate`/`Destroy`) |

---

*Written while learning Unity from scratch, before rebuilding this game's systems in `Assets/Project`, using the MultiTech Studio template as a reference.*

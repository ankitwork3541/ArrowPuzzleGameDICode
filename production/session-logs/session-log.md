## Session End: 20261019_104312
### Uncommitted Changes
.vscode/settings.json
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/ProjectVersion.txt
---

## Session End: 20261019_105257
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
---

## Session End: 20261019_170627
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
---

## Session End: 20261019_174018
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/LevelManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
---

## Archived Session State: 20261019_180829
<!-- CHECKPOINT -->
**Updated:** 2026-10-19
**Branch:** `main`
**Current task:** /story-done — production/epics/lantern-dragons/story-001-locked-arrows.md closed: COMPLETE WITH NOTES
**Next step:** /dev-story production/epics/lantern-dragons/story-002-new-level-pipeline.md
**Blocked on:** nothing
**Files in progress:** none
**Open questions:** AC3 shake visual deferred; remove LevelLoader debug lock hook in Story 002; stray Assets/production folder to delete by user; Editor save is at level 2 from a test Win
<!-- /CHECKPOINT -->
---

## Archived Session State: 20261019_181926
<!-- CHECKPOINT -->
**Updated:** 2026-10-19
**Branch:** `main`
**Current task:** /dev-story — production/epics/lantern-dragons/story-002-new-level-pipeline.md implemented; awaiting /story-done
**Next step:** /story-done production/epics/lantern-dragons/story-002-new-level-pipeline.md
**Blocked on:** nothing
**Files in progress:** none
**Open questions:** user to delete stray Assets/production/ and Assets/story-002-level6.png(+.meta); LevelShapeSources still holds 132 OLD-game silhouettes (Story 003); lock label is small on dense boards (Story 004)
<!-- /CHECKPOINT -->
---

## Session End: 20261019_181926
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Data/ArrowPathData.cs
Assets/Project/Scripts/Data/LevelBinary.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs
Assets/Project/Scripts/Editor/LevelDifficulty.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/LevelManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
---

## Archived Session State: 20261019_182045
<!-- CHECKPOINT -->
**Updated:** 2026-10-19
**Branch:** `main`
**Current task:** /story-done — production/epics/lantern-dragons/story-002-new-level-pipeline.md closed: COMPLETE WITH NOTES
**Next step:** /dev-story production/epics/lantern-dragons/story-003-festival-board-shapes.md (needs ~20 AI silhouette PNGs in LevelShapeSources/) or /dev-story story-004 meanwhile
**Blocked on:** Story 003 needs the user's silhouette PNGs
**Files in progress:** none
**Open questions:** delete stray Assets/production/ and Assets/story-002-level6.png(+.meta); old 132 silhouettes still in LevelShapeSources/
<!-- /CHECKPOINT -->
---

## Archived Session State: 20261019_183227
<!-- CHECKPOINT -->
**Updated:** 2026-10-19
**Branch:** `main`
**Current task:** /dev-story — production/epics/lantern-dragons/story-004-lantern-theme-art.md implemented; awaiting developer sign-off in production/qa/evidence/story-004-evidence.md, then /story-done
**Next step:** developer reviews screenshots and ticks [x] Approved, then /story-done production/epics/lantern-dragons/story-004-lantern-theme-art.md
**Blocked on:** lead sign-off (Visual/Feel gate)
**Files in progress:** Assets/Project/Scripts/Core/ArrowController.cs, Data/ArrowPalette.cs, Prefabs/ArrowPrefab.prefab, Scenes/Game.unity (backdrop sprites), Sprites/{DragonHead,DragonPearl,Padlock,BackdropLantern,BackdropCloud,BackdropBlossom}.png — all uncommitted
**Open questions:** LevelShapeSources still old shapes (Story 003); `git status` also holds user's earlier ads/save edits
<!-- /CHECKPOINT -->
---

## Session End: 20261019_183227
### Commits
ad4a449 docs: lantern-dragons brief, epic and stories, Unity 6.6 engine reference, adoption plan
8e53916 feat: new level pipeline with lock thresholds, new seeds and ramp (brief MVP feature 3)
3555717 feat: locked arrows (brief MVP feature 2)
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Fonts/Oxanium-Bold SDF.asset
Assets/Project/Fonts/Oxanium-ExtraBold SDF.asset
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
production/epics/lantern-dragons/story-004-lantern-theme-art.md
---

## Session End: 20261019_185843
### Commits
ad4a449 docs: lantern-dragons brief, epic and stories, Unity 6.6 engine reference, adoption plan
8e53916 feat: new level pipeline with lock thresholds, new seeds and ramp (brief MVP feature 3)
3555717 feat: locked arrows (brief MVP feature 2)
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Fonts/Oxanium-Bold SDF.asset
Assets/Project/Fonts/Oxanium-ExtraBold SDF.asset
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
---

## Session End: 20261020_101656
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
---

## Session End: 20261020_103238
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
---

## Session End: 20261020_104300
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_104448
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Themes/GameplayScreen.uss
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_104849
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_104858
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_104937
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_105441
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_105521
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_105718
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_110248
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_110439
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Sprites/AppIcon/AppIconBackground.png
Assets/Project/Sprites/AppIcon/AppIconForeground.png
Assets/Project/Sprites/AppIcon/AppIconRound.png
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LogoRound.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_110457
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Sprites/AppIcon/AppIconBackground.png
Assets/Project/Sprites/AppIcon/AppIconForeground.png
Assets/Project/Sprites/AppIcon/AppIconRound.png
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LogoRound.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_111026
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Sprites/AppIcon/AppIconBackground.png
Assets/Project/Sprites/AppIcon/AppIconForeground.png
Assets/Project/Sprites/AppIcon/AppIconRound.png
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LogoRound.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_111949
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Sprites/AppIcon/AppIconBackground.png
Assets/Project/Sprites/AppIcon/AppIconForeground.png
Assets/Project/Sprites/AppIcon/AppIconRound.png
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LogoRound.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Settings/UniversalRP.asset
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/GraphicsSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_113513
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Scripts/UI/ResultScreenController.cs
Assets/Project/Sprites/AppIcon/AppIconBackground.png
Assets/Project/Sprites/AppIcon/AppIconForeground.png
Assets/Project/Sprites/AppIcon/AppIconRound.png
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LogoRound.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/LoseScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Project/UXML/WinScreen.uxml
Assets/Settings/UniversalRP.asset
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/GraphicsSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_113621
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Scripts/UI/ResultScreenController.cs
Assets/Project/Scripts/UI/SettingsScreenController.cs
Assets/Project/Sprites/AppIcon/AppIconBackground.png
Assets/Project/Sprites/AppIcon/AppIconForeground.png
Assets/Project/Sprites/AppIcon/AppIconRound.png
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LogoRound.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/LoseScreen.uxml
Assets/Project/UXML/SettingsScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Project/UXML/WinScreen.uxml
Assets/Settings/UniversalRP.asset
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/GraphicsSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Session End: 20261020_115023
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/LevelTracker.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/BannerTestScreenController.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Scripts/UI/ResultScreenController.cs
Assets/Project/Scripts/UI/SettingsScreenController.cs
Assets/Project/Scripts/UI/SplashScreenController.cs
Assets/Project/Sprites/AppIcon/AppIconBackground.png
Assets/Project/Sprites/AppIcon/AppIconForeground.png
Assets/Project/Sprites/AppIcon/AppIconRound.png
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LogoRound.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/LoseScreen.uxml
Assets/Project/UXML/SettingsScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Project/UXML/WinScreen.uxml
Assets/Settings/UniversalRP.asset
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/GraphicsSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
---

## Archived Session State: 20261020_115149
<!-- CHECKPOINT -->
**Updated:** 2026-10-20
**Branch:** `main` (nothing committed since `ad4a449`; everything below is uncommitted)
**Current task:** Lantern Dragons finish line, sprint `production/sprints/sprint-2026-10-20.md`. Stories 003 to 008 are implemented. 003 complete except git commit; 004 superseded by 008; 005 and 008 In Review (screenshots in `production/qa/evidence/story-008/`, sign-off boxes unticked); 006 and 007 In Progress. A polish round is running: gear icon redraw, pearl tail marker, splash logo scaling, `VersionedUIReloadCallback` migration, then a rebuild plus tests (map node tap, restart persistence, "LEVEL 1500", 360x640, ad modal).
**Next step:** when the polish fork finishes, rebuild the Windows dev player (`EvidenceCapture.cs`, detached build job), recapture, update the evidence docs. Then wait for the developer: sign-offs for Stories 008 and 005, then `/story-done`.
**Blocked on (developer):** (1) sign-offs; (2) new package id, AdMob units and host-app / backend contract for Story 007; (3) listening to the 7 `LD_*` audio clips; (4) a git commit decision (plan in the sprint file); (5) an Android device for the ad and full-loop test; (6) deleting the stray `Assets/production/` folder.
**Files in progress:** Assets/Project/Sprites/SettingsIcon.png, Assets/Project/Scripts/Core/ArrowController.cs, Assets/Project/Themes/SplashScreen.uss and GameplayScreen.uss, Assets/Project/Scripts/UI/*Controller.cs (callback migration), Assets/Project/Scripts/Managers/EvidenceCapture.cs. Docs written this round: design/accessibility-requirements.md (Standard tier, ASSUMED), design/ux/interaction-patterns.md, design/ux/ui-redesign.md (Layout v2), production/sprints/sprint-2026-10-20.md, production/qa/smoke-2026-10-20.md.
**Open questions:** Moon silhouette is missing from the 19 shapes (brief listed it); is the gear icon right; is Layout v2 (Splash name and logo on top, HUD dock, Home level card, bottom-sheet Win/Lose) accepted.
**Working rules in force:** Claude does all the work (CLAUDE.md, 2026-10-20); replies in romanized Gujarati; the Editor never paints UI Toolkit unless it is the active app, so UI evidence comes from a Windows dev player; scripts recompile after Play mode ends (`ScriptCompilationDuringPlay=1`); `rm -rf` is denied to Claude; only one agent may use the Editor at a time.
<!-- /CHECKPOINT -->
---

## Session End: 20261020_115149
### Uncommitted Changes
.vscode/settings.json
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/LevelTracker.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/BannerTestScreenController.cs
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Scripts/UI/ResultScreenController.cs
Assets/Project/Scripts/UI/SettingsScreenController.cs
Assets/Project/Scripts/UI/SplashScreenController.cs
Assets/Project/Sprites/AppIcon/AppIconBackground.png
Assets/Project/Sprites/AppIcon/AppIconForeground.png
Assets/Project/Sprites/AppIcon/AppIconRound.png
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LogoRound.png
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SplashScreen.uss
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/LoseScreen.uxml
Assets/Project/UXML/SettingsScreen.uxml
Assets/Project/UXML/SplashScreen.uxml
Assets/Project/UXML/WinScreen.uxml
Assets/Settings/UniversalRP.asset
Assets/Settings/UniversalRenderPipelineGlobalSettings.asset
Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset
CLAUDE.md
LevelShapeSources/README.md
LevelShapeSources/anchor (1).png
LevelShapeSources/anchor.png
LevelShapeSources/animal (1).png
LevelShapeSources/animal (2).png
LevelShapeSources/animal-cartoon-fauna-5-svgrepo-com.png
LevelShapeSources/animal-domestic-farm-svgrepo-com.png
LevelShapeSources/animal.png
LevelShapeSources/animals-christmas-deer-svgrepo-com.png
LevelShapeSources/apple.png
LevelShapeSources/armor.png
LevelShapeSources/avatar.png
LevelShapeSources/bat.png
LevelShapeSources/bat_10097105.png
LevelShapeSources/black.png
LevelShapeSources/bulb.png
LevelShapeSources/butterfly-silhouette-side-view-facing-left.png
LevelShapeSources/butterfly.png
LevelShapeSources/cactus (1).png
LevelShapeSources/cactus (2).png
LevelShapeSources/cactus (3).png
LevelShapeSources/cactus.png
LevelShapeSources/camel-1-svgrepo-com.png
LevelShapeSources/car.png
LevelShapeSources/chameleon.png
LevelShapeSources/charging-station.png
LevelShapeSources/chess.jpeg
LevelShapeSources/chick.png
LevelShapeSources/chipmunk-svgrepo-com.png
LevelShapeSources/coffee-bag.png
LevelShapeSources/crab (1).png
LevelShapeSources/crab.png
LevelShapeSources/crocodile.png
LevelShapeSources/crown (1).png
LevelShapeSources/crown.png
LevelShapeSources/cupcake.png
LevelShapeSources/delivery-truck.png
LevelShapeSources/diamond (1).png
LevelShapeSources/diamond-symbol.png
LevelShapeSources/diamond.png
LevelShapeSources/dinosaur-hand-drawn-toy-animal-svgrepo-com.png
LevelShapeSources/dinosaur.png
LevelShapeSources/dog-food.png
LevelShapeSources/dog.png
LevelShapeSources/dolphin.png
LevelShapeSources/dove.png
LevelShapeSources/duck (1).png
LevelShapeSources/duck (2).png
LevelShapeSources/duck.png
LevelShapeSources/eagle.png
LevelShapeSources/elephant.png
LevelShapeSources/excavator.png
LevelShapeSources/excavator_9696844.png
LevelShapeSources/fish.png
LevelShapeSources/flash.png
LevelShapeSources/fork.png
LevelShapeSources/fox.png
LevelShapeSources/frontal-truck.png
LevelShapeSources/giraffe.png
LevelShapeSources/glasses.png
LevelShapeSources/goose.png
LevelShapeSources/gorilla-facing-right-svgrepo-com.png
LevelShapeSources/hamburger.png
LevelShapeSources/heart (1).png
LevelShapeSources/heart.png
LevelShapeSources/helicopter.png
LevelShapeSources/hen.png
LevelShapeSources/home.png
LevelShapeSources/horse.png
LevelShapeSources/ice-cream-cone.png
LevelShapeSources/ice-cream.png
LevelShapeSources/key (1).png
LevelShapeSources/key-hole.png
LevelShapeSources/key.png
LevelShapeSources/koala.png
LevelShapeSources/moon.png
LevelShapeSources/mouse.png
LevelShapeSources/muffin.png
LevelShapeSources/mug (1).png
LevelShapeSources/mug (2).png
LevelShapeSources/mug.png
LevelShapeSources/music-player.png
LevelShapeSources/music.png
LevelShapeSources/noodles.png
LevelShapeSources/notification.png
LevelShapeSources/nutrition.png
LevelShapeSources/octopus (1).png
LevelShapeSources/octopus.png
LevelShapeSources/origami.png
LevelShapeSources/parrot.png
LevelShapeSources/paw.png
LevelShapeSources/pea_6798892.png
LevelShapeSources/penguin (1).png
LevelShapeSources/penguin (2).png
LevelShapeSources/penguin.png
LevelShapeSources/pets.png
LevelShapeSources/pine-tree.png
LevelShapeSources/pizza.png
LevelShapeSources/popsicle.png
LevelShapeSources/puzzle-piece.png
LevelShapeSources/puzzle.png
LevelShapeSources/rabbit.png
LevelShapeSources/rhino.png
LevelShapeSources/rocket.png
LevelShapeSources/sail-boat.png
LevelShapeSources/sailing-boat.png
LevelShapeSources/sea (1).png
LevelShapeSources/sea (2).png
LevelShapeSources/sea.png
LevelShapeSources/seashell.png
LevelShapeSources/sheep.png
LevelShapeSources/shield (1).png
LevelShapeSources/shield.png
LevelShapeSources/skull.png
LevelShapeSources/sloth.png
LevelShapeSources/snake.png
LevelShapeSources/spoon.png
LevelShapeSources/squirrel (1).png
LevelShapeSources/squirrel.png
LevelShapeSources/star.png
LevelShapeSources/toad (1).png
LevelShapeSources/toad.png
LevelShapeSources/tractor.png
LevelShapeSources/tree.png
LevelShapeSources/turtle (1).png
LevelShapeSources/turtle.png
LevelShapeSources/twitter-black-shape.png
LevelShapeSources/umbrella (1).png
LevelShapeSources/umbrella.png
LevelShapeSources/whale (1).png
LevelShapeSources/whale.png
LevelShapeSources/wolf.png
LevelShapeSources/zoo-svgrepo-com.png
Packages/manifest.json
Packages/packages-lock.json
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/GraphicsSettings.asset
ProjectSettings/ProjectSettings.asset
ProjectSettings/ProjectVersion.txt
ProjectSettings/QualitySettings.asset
design/game-brief.md
production/epics/lantern-dragons/EPIC.md
production/epics/lantern-dragons/story-003-festival-board-shapes.md
production/epics/lantern-dragons/story-004-lantern-theme-art.md
production/epics/lantern-dragons/story-005-level-display-and-map.md
production/epics/lantern-dragons/story-006-ui-retheme-names-audio.md
production/epics/lantern-dragons/story-007-new-identity-and-kept-systems.md
---

## Session End: 20261020_120455
### Commits
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
---

## Session End: 20261020_121021
### Commits
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_121843
### Commits
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_122059
### Commits
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Audio/ArrowClick_Blocked.ogg
Assets/Project/Audio/ArrowClick_Blocked.ogg.meta
Assets/Project/Audio/ArrowClick_Success.mp3
Assets/Project/Audio/ArrowClick_Success.mp3.meta
Assets/Project/Audio/ArrowTap.wav
Assets/Project/Audio/ArrowTap.wav.meta
Assets/Project/Audio/LevelComplete.wav
Assets/Project/Audio/LevelComplete.wav.meta
Assets/Project/Audio/LevelFailed.wav
Assets/Project/Audio/LevelFailed.wav.meta
Assets/Project/Audio/MainMixer.mixer
Assets/Project/Audio/MainMixer.mixer.meta
Assets/Project/Audio/UIClick.wav
Assets/Project/Audio/UIClick.wav.meta
Assets/Project/Materials/Mat_NeonDot.mat
Assets/Project/Materials/Mat_NeonDot.mat.meta
Assets/Project/Materials/Mat_NeonFlare.mat
Assets/Project/Materials/Mat_NeonFlare.mat.meta
Assets/Project/Materials/Mat_NeonTrail.mat
Assets/Project/Materials/Mat_NeonTrail.mat.meta
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Prefabs/ArrowPrefab.prefab.meta
Assets/Project/Prefabs/CommittedLineTemplate.prefab
Assets/Project/Prefabs/CommittedLineTemplate.prefab.meta
Assets/Project/Prefabs/Dot.prefab
Assets/Project/Prefabs/Dot.prefab.meta
Assets/Project/Resources/AdConfig.asset
Assets/Project/Resources/AdConfig.asset.meta
Assets/Project/Resources/Levels.meta
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_00.bytes.meta
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes.meta
Assets/Project/Scenes/Game.meta
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game.unity.meta
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scenes/Game/Global Volume Profile.asset.meta
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/ArrowController.cs.meta
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/Dot.cs.meta
Assets/Project/Scripts/Core/GridBuilder.cs
Assets/Project/Scripts/Core/GridBuilder.cs.meta
Assets/Project/Scripts/Core/GridCell.cs
Assets/Project/Scripts/Core/GridCell.cs.meta
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Core/GuideLines.cs.meta
Assets/Project/Scripts/Data/AdConfig.cs
Assets/Project/Scripts/Data/AdConfig.cs.meta
Assets/Project/Scripts/Data/AdRemoteConfig.cs
Assets/Project/Scripts/Data/AdRemoteConfig.cs.meta
Assets/Project/Scripts/Data/AdTestIds.cs
Assets/Project/Scripts/Data/AdTestIds.cs.meta
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/ArrowPalette.cs.meta
Assets/Project/Scripts/Data/ArrowPathData.cs
Assets/Project/Scripts/Data/ArrowPathData.cs.meta
Assets/Project/Scripts/Data/LevelBinary.cs
Assets/Project/Scripts/Data/LevelBinary.cs.meta
Assets/Project/Scripts/Data/LevelData.cs
Assets/Project/Scripts/Data/LevelData.cs.meta
Assets/Project/Scripts/Data/SaveData.cs
Assets/Project/Scripts/Data/SaveData.cs.meta
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Data/SaveSystem.cs.meta
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs.meta
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs.meta
Assets/Project/Scripts/Editor/LevelCarver.cs
Assets/Project/Scripts/Editor/LevelCarver.cs.meta
Assets/Project/Scripts/Editor/LevelDifficulty.cs
Assets/Project/Scripts/Editor/LevelDifficulty.cs.meta
Assets/Project/Scripts/Editor/LevelLocks.cs
Assets/Project/Scripts/Editor/LevelLocks.cs.meta
Assets/Project/Scripts/Editor/LevelShapeMask.cs
Assets/Project/Scripts/Editor/LevelShapeMask.cs.meta
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs.meta
Assets/Project/Scripts/Managers/AdSettings.cs
Assets/Project/Scripts/Managers/AdSettings.cs.meta
Assets/Project/Scripts/Managers/AdsManager.cs
Assets/Project/Scripts/Managers/AdsManager.cs.meta
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs.meta
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/AudioManager.cs.meta
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs.meta
Assets/Project/Scripts/Managers/BannerAdUnit.cs
Assets/Project/Scripts/Managers/BannerAdUnit.cs.meta
Assets/Project/Scripts/Managers/CameraController.cs
Assets/Project/Scripts/Managers/CameraController.cs.meta
Assets/Project/Scripts/Managers/EvidenceCapture.cs
Assets/Project/Scripts/Managers/EvidenceCapture.cs.meta
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs.meta
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/HintManager.cs.meta
Assets/Project/Scripts/Managers/InterstitialAdUnit.cs
Assets/Project/Scripts/Managers/InterstitialAdUnit.cs.meta
Assets/Project/Scripts/Managers/InterstitialManager.cs
Assets/Project/Scripts/Managers/InterstitialManager.cs.meta
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/LevelLoader.cs.meta
Assets/Project/Scripts/Managers/LevelManager.cs
Assets/Project/Scripts/Managers/LevelManager.cs.meta
Assets/Project/Scripts/Managers/LevelTracker.cs
Assets/Project/Scripts/Managers/LevelTracker.cs.meta
Assets/Project/Scripts/Managers/LivesManager.cs
Assets/Project/Scripts/Managers/LivesManager.cs.meta
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs.meta
Assets/Project/Scripts/Managers/RewardedAdUnit.cs
Assets/Project/Scripts/Managers/RewardedAdUnit.cs.meta
Assets/Project/Scripts/Managers/ScreenBackdrop.cs
Assets/Project/Scripts/Managers/ScreenBackdrop.cs.meta
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs.meta
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs.meta
Assets/Project/Scripts/UI/AdPlaceholder.cs
Assets/Project/Scripts/UI/AdPlaceholder.cs.meta
Assets/Project/Scripts/UI/BannerTestScreenController.cs
Assets/Project/Scripts/UI/BannerTestScreenController.cs.meta
Assets/Project/Scripts/UI/BottomNavBinder.cs
Assets/Project/Scripts/UI/BottomNavBinder.cs.meta
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/ConfettiView.cs.meta
Assets/Project/Scripts/UI/DeviceScaleCategory.cs
Assets/Project/Scripts/UI/DeviceScaleCategory.cs.meta
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs.meta
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs.meta
Assets/Project/Scripts/UI/LevelMapController.cs
Assets/Project/Scripts/UI/LevelMapController.cs.meta
Assets/Project/Scripts/UI/NativeAdSpace.cs
Assets/Project/Scripts/UI/NativeAdSpace.cs.meta
Assets/Project/Scripts/UI/ResultScreenController.cs
Assets/Project/Scripts/UI/ResultScreenController.cs.meta
Assets/Project/Scripts/UI/SettingsScreenController.cs
Assets/Project/Scripts/UI/SettingsScreenController.cs.meta
Assets/Project/Scripts/UI/SplashScreenController.cs
Assets/Project/Scripts/UI/SplashScreenController.cs.meta
Assets/Project/Scripts/UI/UIClickSound.cs
Assets/Project/Scripts/UI/UIClickSound.cs.meta
Assets/Project/Sprites/BackdropArrowBend.png
Assets/Project/Sprites/BackdropArrowBend.png.meta
Assets/Project/Sprites/BackdropArrowStraight.png
Assets/Project/Sprites/BackdropArrowStraight.png.meta
Assets/Project/Sprites/BackdropArrowZigzag.png
Assets/Project/Sprites/BackdropArrowZigzag.png.meta
Assets/Project/Sprites/Circle.png
Assets/Project/Sprites/Circle.png.meta
Assets/Project/Sprites/Circle01.png
Assets/Project/Sprites/Circle01.png.meta
Assets/Project/Sprites/Flare01.png
Assets/Project/Sprites/Flare01.png.meta
Assets/Project/Sprites/Gradient01.png
Assets/Project/Sprites/Gradient01.png.meta
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/HomeIcon.png.meta
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconAd.png.meta
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBack.png.meta
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconBulb.png.meta
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconGrid.png.meta
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconHeart.png.meta
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconRestart.png.meta
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconStar.png.meta
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LockIcon.png.meta
Assets/Project/Sprites/LogoSquare.png
Assets/Project/Sprites/LogoSquare.png.meta
Assets/Project/Sprites/NeonGradient.png
Assets/Project/Sprites/NeonGradient.png.meta
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/SettingsIcon.png.meta
Assets/Project/Sprites/Triangle.png
Assets/Project/Sprites/Triangle.png.meta
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarFill.png.meta
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Sprites/UIBarTrack.png.meta
Assets/Project/Sprites/UIHexPillFill.png
Assets/Project/Sprites/UIHexPillFill.png.meta
Assets/Project/Sprites/UIHexPillFrame.png
Assets/Project/Sprites/UIHexPillFrame.png.meta
Assets/Project/Sprites/UIOctagonFill.png
Assets/Project/Sprites/UIOctagonFill.png.meta
Assets/Project/Sprites/UIOctagonFrame.png
Assets/Project/Sprites/UIOctagonFrame.png.meta
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdLoadingOverlay.uss.meta
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/AdPlaceholder.uss.meta
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BannerTestScreen.uss.meta
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/BottomNav.uss.meta
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/GameplayScreen.uss.meta
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/HomeScreen.uss.meta
Assets/Project/Themes/LevelMapScreen.uss
Assets/Project/Themes/LevelMapScreen.uss.meta
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/ResultScreen.uss.meta
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SettingsScreen.uss.meta
Assets/Project/Themes/SplashScreen.uss
Assets/Project/Themes/SplashScreen.uss.meta
Assets/Project/Themes/Typography.uss
Assets/Project/Themes/Typography.uss.meta
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/AdLoadingOverlay.uxml.meta
Assets/Project/UXML/AdPlaceholder.uxml
Assets/Project/UXML/AdPlaceholder.uxml.meta
Assets/Project/UXML/BannerTestScreen.uxml
Assets/Project/UXML/BannerTestScreen.uxml.meta
Assets/Project/UXML/BottomNav.uxml
Assets/Project/UXML/BottomNav.uxml.meta
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/GameplayScreen.uxml.meta
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/HomeScreen.uxml.meta
Assets/Project/UXML/LevelMapScreen.uxml
Assets/Project/UXML/LevelMapScreen.uxml.meta
Assets/Project/UXML/LoseScreen.uxml
Assets/Project/UXML/LoseScreen.uxml.meta
Assets/Project/UXML/MainPanelSettings.asset
Assets/Project/UXML/MainPanelSettings.asset.meta
Assets/Project/UXML/OverlayPanelSettings.asset
Assets/Project/UXML/OverlayPanelSettings.asset.meta
Assets/Project/UXML/SettingsScreen.uxml
Assets/Project/UXML/SettingsScreen.uxml.meta
Assets/Project/UXML/SplashScreen.uxml
Assets/Project/UXML/SplashScreen.uxml.meta
Assets/Project/UXML/WinScreen.uxml
Assets/Project/UXML/WinScreen.uxml.meta
Assets/Tests/EditMode/Editor/ArrowPaletteTests.cs
Assets/Tests/EditMode/Editor/ArrowPaletteTests.cs.meta
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs.meta
Assets/Tests/EditMode/Editor/LevelBinaryTests.cs
Assets/Tests/EditMode/Editor/LevelBinaryTests.cs.meta
Assets/Tests/EditMode/Editor/LevelCarverTests.cs
Assets/Tests/EditMode/Editor/LevelCarverTests.cs.meta
Assets/Tests/EditMode/Editor/LevelLocksTests.cs
Assets/Tests/EditMode/Editor/LevelLocksTests.cs.meta
Assets/Tests/EditMode/Editor/LevelSolverTests.cs
Assets/Tests/EditMode/Editor/LevelSolverTests.cs.meta
Assets/Tests/EditMode/Editor/SaveMigrationTests.cs
Assets/Tests/EditMode/Editor/SaveMigrationTests.cs.meta
Assets/Tests/EditMode/Editor/TestLevels.cs
Assets/Tests/EditMode/Editor/TestLevels.cs.meta
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_122942
### Commits
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Audio/ArrowClick_Blocked.ogg
Assets/Project/Audio/ArrowClick_Blocked.ogg.meta
Assets/Project/Audio/ArrowClick_Success.mp3
Assets/Project/Audio/ArrowClick_Success.mp3.meta
Assets/Project/Audio/ArrowTap.wav
Assets/Project/Audio/ArrowTap.wav.meta
Assets/Project/Audio/LD_ArrowTap.wav
Assets/Project/Audio/LD_ArrowTap.wav.meta
Assets/Project/Audio/LevelComplete.wav
Assets/Project/Audio/LevelComplete.wav.meta
Assets/Project/Audio/LevelFailed.wav
Assets/Project/Audio/LevelFailed.wav.meta
Assets/Project/Audio/MainMixer.mixer
Assets/Project/Audio/MainMixer.mixer.meta
Assets/Project/Audio/UIClick.wav
Assets/Project/Audio/UIClick.wav.meta
Assets/Project/Fonts/Oxanium-Bold SDF.asset
Assets/Project/Materials/Mat_NeonDot.mat
Assets/Project/Materials/Mat_NeonDot.mat.meta
Assets/Project/Materials/Mat_NeonFlare.mat
Assets/Project/Materials/Mat_NeonFlare.mat.meta
Assets/Project/Materials/Mat_NeonTrail.mat
Assets/Project/Materials/Mat_NeonTrail.mat.meta
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Prefabs/ArrowPrefab.prefab.meta
Assets/Project/Prefabs/CommittedLineTemplate.prefab
Assets/Project/Prefabs/CommittedLineTemplate.prefab.meta
Assets/Project/Prefabs/Dot.prefab
Assets/Project/Prefabs/Dot.prefab.meta
Assets/Project/Resources/AdConfig.asset
Assets/Project/Resources/AdConfig.asset.meta
Assets/Project/Resources/Levels.meta
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_00.bytes.meta
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes.meta
Assets/Project/Scenes/Game.meta
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game.unity.meta
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scenes/Game/Global Volume Profile.asset.meta
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/ArrowController.cs.meta
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/Dot.cs.meta
Assets/Project/Scripts/Core/GridBuilder.cs
Assets/Project/Scripts/Core/GridBuilder.cs.meta
Assets/Project/Scripts/Core/GridCell.cs
Assets/Project/Scripts/Core/GridCell.cs.meta
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Core/GuideLines.cs.meta
Assets/Project/Scripts/Data/AdConfig.cs
Assets/Project/Scripts/Data/AdConfig.cs.meta
Assets/Project/Scripts/Data/AdRemoteConfig.cs
Assets/Project/Scripts/Data/AdRemoteConfig.cs.meta
Assets/Project/Scripts/Data/AdTestIds.cs
Assets/Project/Scripts/Data/AdTestIds.cs.meta
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/ArrowPalette.cs.meta
Assets/Project/Scripts/Data/ArrowPathData.cs
Assets/Project/Scripts/Data/ArrowPathData.cs.meta
Assets/Project/Scripts/Data/LevelBinary.cs
Assets/Project/Scripts/Data/LevelBinary.cs.meta
Assets/Project/Scripts/Data/LevelData.cs
Assets/Project/Scripts/Data/LevelData.cs.meta
Assets/Project/Scripts/Data/SaveData.cs
Assets/Project/Scripts/Data/SaveData.cs.meta
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Data/SaveSystem.cs.meta
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs.meta
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs.meta
Assets/Project/Scripts/Editor/LevelCarver.cs
Assets/Project/Scripts/Editor/LevelCarver.cs.meta
Assets/Project/Scripts/Editor/LevelDifficulty.cs
Assets/Project/Scripts/Editor/LevelDifficulty.cs.meta
Assets/Project/Scripts/Editor/LevelLocks.cs
Assets/Project/Scripts/Editor/LevelLocks.cs.meta
Assets/Project/Scripts/Editor/LevelShapeMask.cs
Assets/Project/Scripts/Editor/LevelShapeMask.cs.meta
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs.meta
Assets/Project/Scripts/Managers/AdSettings.cs
Assets/Project/Scripts/Managers/AdSettings.cs.meta
Assets/Project/Scripts/Managers/AdsManager.cs
Assets/Project/Scripts/Managers/AdsManager.cs.meta
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs.meta
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/AudioManager.cs.meta
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs.meta
Assets/Project/Scripts/Managers/BannerAdUnit.cs
Assets/Project/Scripts/Managers/BannerAdUnit.cs.meta
Assets/Project/Scripts/Managers/CameraController.cs
Assets/Project/Scripts/Managers/CameraController.cs.meta
Assets/Project/Scripts/Managers/EvidenceCapture.cs
Assets/Project/Scripts/Managers/EvidenceCapture.cs.meta
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs.meta
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/HintManager.cs.meta
Assets/Project/Scripts/Managers/InterstitialAdUnit.cs
Assets/Project/Scripts/Managers/InterstitialAdUnit.cs.meta
Assets/Project/Scripts/Managers/InterstitialManager.cs
Assets/Project/Scripts/Managers/InterstitialManager.cs.meta
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/LevelLoader.cs.meta
Assets/Project/Scripts/Managers/LevelManager.cs
Assets/Project/Scripts/Managers/LevelManager.cs.meta
Assets/Project/Scripts/Managers/LevelTracker.cs
Assets/Project/Scripts/Managers/LevelTracker.cs.meta
Assets/Project/Scripts/Managers/LivesManager.cs
Assets/Project/Scripts/Managers/LivesManager.cs.meta
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs.meta
Assets/Project/Scripts/Managers/RewardedAdUnit.cs
Assets/Project/Scripts/Managers/RewardedAdUnit.cs.meta
Assets/Project/Scripts/Managers/ScreenBackdrop.cs
Assets/Project/Scripts/Managers/ScreenBackdrop.cs.meta
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs.meta
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs.meta
Assets/Project/Scripts/UI/AdPlaceholder.cs
Assets/Project/Scripts/UI/AdPlaceholder.cs.meta
Assets/Project/Scripts/UI/BannerTestScreenController.cs
Assets/Project/Scripts/UI/BannerTestScreenController.cs.meta
Assets/Project/Scripts/UI/BottomNavBinder.cs
Assets/Project/Scripts/UI/BottomNavBinder.cs.meta
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/ConfettiView.cs.meta
Assets/Project/Scripts/UI/DeviceScaleCategory.cs
Assets/Project/Scripts/UI/DeviceScaleCategory.cs.meta
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs.meta
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs.meta
Assets/Project/Scripts/UI/LevelMapController.cs
Assets/Project/Scripts/UI/LevelMapController.cs.meta
Assets/Project/Scripts/UI/NativeAdSpace.cs
Assets/Project/Scripts/UI/NativeAdSpace.cs.meta
Assets/Project/Scripts/UI/ResultScreenController.cs
Assets/Project/Scripts/UI/ResultScreenController.cs.meta
Assets/Project/Scripts/UI/SettingsScreenController.cs
Assets/Project/Scripts/UI/SettingsScreenController.cs.meta
Assets/Project/Scripts/UI/SplashScreenController.cs
Assets/Project/Scripts/UI/SplashScreenController.cs.meta
Assets/Project/Scripts/UI/UIClickSound.cs
Assets/Project/Scripts/UI/UIClickSound.cs.meta
Assets/Project/Sprites/BackdropArrowBend.png
Assets/Project/Sprites/BackdropArrowBend.png.meta
Assets/Project/Sprites/BackdropArrowStraight.png
Assets/Project/Sprites/BackdropArrowStraight.png.meta
Assets/Project/Sprites/BackdropArrowZigzag.png
Assets/Project/Sprites/BackdropArrowZigzag.png.meta
Assets/Project/Sprites/Circle.png
Assets/Project/Sprites/Circle.png.meta
Assets/Project/Sprites/Circle01.png
Assets/Project/Sprites/Circle01.png.meta
Assets/Project/Sprites/Flare01.png
Assets/Project/Sprites/Flare01.png.meta
Assets/Project/Sprites/Gradient01.png
Assets/Project/Sprites/Gradient01.png.meta
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/HomeIcon.png.meta
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconAd.png.meta
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBack.png.meta
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconBulb.png.meta
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconGrid.png.meta
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconHeart.png.meta
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconRestart.png.meta
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconStar.png.meta
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LockIcon.png.meta
Assets/Project/Sprites/LogoSquare.png
Assets/Project/Sprites/LogoSquare.png.meta
Assets/Project/Sprites/NeonGradient.png
Assets/Project/Sprites/NeonGradient.png.meta
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/SettingsIcon.png.meta
Assets/Project/Sprites/Triangle.png
Assets/Project/Sprites/Triangle.png.meta
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarFill.png.meta
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Sprites/UIBarTrack.png.meta
Assets/Project/Sprites/UIHexPillFill.png
Assets/Project/Sprites/UIHexPillFill.png.meta
Assets/Project/Sprites/UIHexPillFrame.png
Assets/Project/Sprites/UIHexPillFrame.png.meta
Assets/Project/Sprites/UIOctagonFill.png
Assets/Project/Sprites/UIOctagonFill.png.meta
Assets/Project/Sprites/UIOctagonFrame.png
Assets/Project/Sprites/UIOctagonFrame.png.meta
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdLoadingOverlay.uss.meta
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/AdPlaceholder.uss.meta
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BannerTestScreen.uss.meta
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/BottomNav.uss.meta
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/GameplayScreen.uss.meta
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/HomeScreen.uss.meta
Assets/Project/Themes/LevelMapScreen.uss
Assets/Project/Themes/LevelMapScreen.uss.meta
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/ResultScreen.uss.meta
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SettingsScreen.uss.meta
Assets/Project/Themes/SplashScreen.uss
Assets/Project/Themes/SplashScreen.uss.meta
Assets/Project/Themes/Typography.uss
Assets/Project/Themes/Typography.uss.meta
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/AdLoadingOverlay.uxml.meta
Assets/Project/UXML/AdPlaceholder.uxml
Assets/Project/UXML/AdPlaceholder.uxml.meta
Assets/Project/UXML/BannerTestScreen.uxml
Assets/Project/UXML/BannerTestScreen.uxml.meta
Assets/Project/UXML/BottomNav.uxml
Assets/Project/UXML/BottomNav.uxml.meta
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/GameplayScreen.uxml.meta
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/HomeScreen.uxml.meta
Assets/Project/UXML/LevelMapScreen.uxml
Assets/Project/UXML/LevelMapScreen.uxml.meta
Assets/Project/UXML/LoseScreen.uxml
Assets/Project/UXML/LoseScreen.uxml.meta
Assets/Project/UXML/MainPanelSettings.asset
Assets/Project/UXML/MainPanelSettings.asset.meta
Assets/Project/UXML/OverlayPanelSettings.asset
Assets/Project/UXML/OverlayPanelSettings.asset.meta
Assets/Project/UXML/SettingsScreen.uxml
Assets/Project/UXML/SettingsScreen.uxml.meta
Assets/Project/UXML/SplashScreen.uxml
Assets/Project/UXML/SplashScreen.uxml.meta
Assets/Project/UXML/WinScreen.uxml
Assets/Project/UXML/WinScreen.uxml.meta
Assets/Tests/EditMode/Editor/ArrowPaletteTests.cs
Assets/Tests/EditMode/Editor/ArrowPaletteTests.cs.meta
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs.meta
Assets/Tests/EditMode/Editor/LevelBinaryTests.cs
Assets/Tests/EditMode/Editor/LevelBinaryTests.cs.meta
Assets/Tests/EditMode/Editor/LevelCarverTests.cs
Assets/Tests/EditMode/Editor/LevelCarverTests.cs.meta
Assets/Tests/EditMode/Editor/LevelLocksTests.cs
Assets/Tests/EditMode/Editor/LevelLocksTests.cs.meta
Assets/Tests/EditMode/Editor/LevelSolverTests.cs
Assets/Tests/EditMode/Editor/LevelSolverTests.cs.meta
Assets/Tests/EditMode/Editor/SaveMigrationTests.cs
Assets/Tests/EditMode/Editor/SaveMigrationTests.cs.meta
Assets/Tests/EditMode/Editor/TestLevels.cs
Assets/Tests/EditMode/Editor/TestLevels.cs.meta
ProjectSettings/EditorBuildSettings.asset
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_123734
### Commits
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Audio/ArrowClick_Blocked.ogg
Assets/Project/Audio/ArrowClick_Blocked.ogg.meta
Assets/Project/Audio/ArrowClick_Success.mp3
Assets/Project/Audio/ArrowClick_Success.mp3.meta
Assets/Project/Audio/ArrowTap.wav
Assets/Project/Audio/ArrowTap.wav.meta
Assets/Project/Audio/LD_ArrowTap.wav
Assets/Project/Audio/LD_ArrowTap.wav.meta
Assets/Project/Audio/LevelComplete.wav
Assets/Project/Audio/LevelComplete.wav.meta
Assets/Project/Audio/LevelFailed.wav
Assets/Project/Audio/LevelFailed.wav.meta
Assets/Project/Audio/MainMixer.mixer
Assets/Project/Audio/MainMixer.mixer.meta
Assets/Project/Audio/UIClick.wav
Assets/Project/Audio/UIClick.wav.meta
Assets/Project/Materials/Mat_NeonDot.mat
Assets/Project/Materials/Mat_NeonDot.mat.meta
Assets/Project/Materials/Mat_NeonFlare.mat
Assets/Project/Materials/Mat_NeonFlare.mat.meta
Assets/Project/Materials/Mat_NeonTrail.mat
Assets/Project/Materials/Mat_NeonTrail.mat.meta
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Prefabs/ArrowPrefab.prefab.meta
Assets/Project/Prefabs/CommittedLineTemplate.prefab
Assets/Project/Prefabs/CommittedLineTemplate.prefab.meta
Assets/Project/Prefabs/Dot.prefab
Assets/Project/Prefabs/Dot.prefab.meta
Assets/Project/Resources/AdConfig.asset
Assets/Project/Resources/AdConfig.asset.meta
Assets/Project/Resources/Levels.meta
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_00.bytes.meta
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes.meta
Assets/Project/Scenes/Game.meta
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game.unity.meta
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scenes/Game/Global Volume Profile.asset.meta
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/ArrowController.cs.meta
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/Dot.cs.meta
Assets/Project/Scripts/Core/GridBuilder.cs
Assets/Project/Scripts/Core/GridBuilder.cs.meta
Assets/Project/Scripts/Core/GridCell.cs
Assets/Project/Scripts/Core/GridCell.cs.meta
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Core/GuideLines.cs.meta
Assets/Project/Scripts/Data/AdConfig.cs
Assets/Project/Scripts/Data/AdConfig.cs.meta
Assets/Project/Scripts/Data/AdRemoteConfig.cs
Assets/Project/Scripts/Data/AdRemoteConfig.cs.meta
Assets/Project/Scripts/Data/AdTestIds.cs
Assets/Project/Scripts/Data/AdTestIds.cs.meta
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/ArrowPalette.cs.meta
Assets/Project/Scripts/Data/ArrowPathData.cs
Assets/Project/Scripts/Data/ArrowPathData.cs.meta
Assets/Project/Scripts/Data/LevelBinary.cs
Assets/Project/Scripts/Data/LevelBinary.cs.meta
Assets/Project/Scripts/Data/LevelData.cs
Assets/Project/Scripts/Data/LevelData.cs.meta
Assets/Project/Scripts/Data/SaveData.cs
Assets/Project/Scripts/Data/SaveData.cs.meta
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Data/SaveSystem.cs.meta
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs.meta
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs.meta
Assets/Project/Scripts/Editor/LevelCarver.cs
Assets/Project/Scripts/Editor/LevelCarver.cs.meta
Assets/Project/Scripts/Editor/LevelDifficulty.cs
Assets/Project/Scripts/Editor/LevelDifficulty.cs.meta
Assets/Project/Scripts/Editor/LevelLocks.cs
Assets/Project/Scripts/Editor/LevelLocks.cs.meta
Assets/Project/Scripts/Editor/LevelShapeMask.cs
Assets/Project/Scripts/Editor/LevelShapeMask.cs.meta
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs.meta
Assets/Project/Scripts/Managers/AdSettings.cs
Assets/Project/Scripts/Managers/AdSettings.cs.meta
Assets/Project/Scripts/Managers/AdsManager.cs
Assets/Project/Scripts/Managers/AdsManager.cs.meta
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs.meta
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/AudioManager.cs.meta
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs.meta
Assets/Project/Scripts/Managers/BannerAdUnit.cs
Assets/Project/Scripts/Managers/BannerAdUnit.cs.meta
Assets/Project/Scripts/Managers/CameraController.cs
Assets/Project/Scripts/Managers/CameraController.cs.meta
Assets/Project/Scripts/Managers/EvidenceCapture.cs
Assets/Project/Scripts/Managers/EvidenceCapture.cs.meta
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs.meta
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/HintManager.cs.meta
Assets/Project/Scripts/Managers/InterstitialAdUnit.cs
Assets/Project/Scripts/Managers/InterstitialAdUnit.cs.meta
Assets/Project/Scripts/Managers/InterstitialManager.cs
Assets/Project/Scripts/Managers/InterstitialManager.cs.meta
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/LevelLoader.cs.meta
Assets/Project/Scripts/Managers/LevelManager.cs
Assets/Project/Scripts/Managers/LevelManager.cs.meta
Assets/Project/Scripts/Managers/LevelTracker.cs
Assets/Project/Scripts/Managers/LevelTracker.cs.meta
Assets/Project/Scripts/Managers/LivesManager.cs
Assets/Project/Scripts/Managers/LivesManager.cs.meta
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs.meta
Assets/Project/Scripts/Managers/RewardedAdUnit.cs
Assets/Project/Scripts/Managers/RewardedAdUnit.cs.meta
Assets/Project/Scripts/Managers/ScreenBackdrop.cs
Assets/Project/Scripts/Managers/ScreenBackdrop.cs.meta
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs.meta
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs.meta
Assets/Project/Scripts/UI/AdPlaceholder.cs
Assets/Project/Scripts/UI/AdPlaceholder.cs.meta
Assets/Project/Scripts/UI/BannerTestScreenController.cs
Assets/Project/Scripts/UI/BannerTestScreenController.cs.meta
Assets/Project/Scripts/UI/BottomNavBinder.cs
Assets/Project/Scripts/UI/BottomNavBinder.cs.meta
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/ConfettiView.cs.meta
Assets/Project/Scripts/UI/DeviceScaleCategory.cs
Assets/Project/Scripts/UI/DeviceScaleCategory.cs.meta
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs.meta
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs.meta
Assets/Project/Scripts/UI/LevelMapController.cs
Assets/Project/Scripts/UI/LevelMapController.cs.meta
Assets/Project/Scripts/UI/NativeAdSpace.cs
Assets/Project/Scripts/UI/NativeAdSpace.cs.meta
Assets/Project/Scripts/UI/ResultScreenController.cs
Assets/Project/Scripts/UI/ResultScreenController.cs.meta
Assets/Project/Scripts/UI/SettingsScreenController.cs
Assets/Project/Scripts/UI/SettingsScreenController.cs.meta
Assets/Project/Scripts/UI/SplashScreenController.cs
Assets/Project/Scripts/UI/SplashScreenController.cs.meta
Assets/Project/Scripts/UI/UIClickSound.cs
Assets/Project/Scripts/UI/UIClickSound.cs.meta
Assets/Project/Sprites/BackdropArrowBend.png
Assets/Project/Sprites/BackdropArrowBend.png.meta
Assets/Project/Sprites/BackdropArrowStraight.png
Assets/Project/Sprites/BackdropArrowStraight.png.meta
Assets/Project/Sprites/BackdropArrowZigzag.png
Assets/Project/Sprites/BackdropArrowZigzag.png.meta
Assets/Project/Sprites/Circle.png
Assets/Project/Sprites/Circle.png.meta
Assets/Project/Sprites/Circle01.png
Assets/Project/Sprites/Circle01.png.meta
Assets/Project/Sprites/Flare01.png
Assets/Project/Sprites/Flare01.png.meta
Assets/Project/Sprites/Gradient01.png
Assets/Project/Sprites/Gradient01.png.meta
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/HomeIcon.png.meta
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconAd.png.meta
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBack.png.meta
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconBulb.png.meta
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconGrid.png.meta
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconHeart.png.meta
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconRestart.png.meta
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconStar.png.meta
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LockIcon.png.meta
Assets/Project/Sprites/LogoSquare.png
Assets/Project/Sprites/LogoSquare.png.meta
Assets/Project/Sprites/NeonGradient.png
Assets/Project/Sprites/NeonGradient.png.meta
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/SettingsIcon.png.meta
Assets/Project/Sprites/Triangle.png
Assets/Project/Sprites/Triangle.png.meta
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarFill.png.meta
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Sprites/UIBarTrack.png.meta
Assets/Project/Sprites/UIHexPillFill.png
Assets/Project/Sprites/UIHexPillFill.png.meta
Assets/Project/Sprites/UIHexPillFrame.png
Assets/Project/Sprites/UIHexPillFrame.png.meta
Assets/Project/Sprites/UIOctagonFill.png
Assets/Project/Sprites/UIOctagonFill.png.meta
Assets/Project/Sprites/UIOctagonFrame.png
Assets/Project/Sprites/UIOctagonFrame.png.meta
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdLoadingOverlay.uss.meta
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/AdPlaceholder.uss.meta
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BannerTestScreen.uss.meta
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/BottomNav.uss.meta
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/GameplayScreen.uss.meta
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/HomeScreen.uss.meta
Assets/Project/Themes/LevelMapScreen.uss
Assets/Project/Themes/LevelMapScreen.uss.meta
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/ResultScreen.uss.meta
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SettingsScreen.uss.meta
Assets/Project/Themes/SplashScreen.uss
Assets/Project/Themes/SplashScreen.uss.meta
Assets/Project/Themes/Typography.uss
Assets/Project/Themes/Typography.uss.meta
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/AdLoadingOverlay.uxml.meta
Assets/Project/UXML/AdPlaceholder.uxml
Assets/Project/UXML/AdPlaceholder.uxml.meta
Assets/Project/UXML/BannerTestScreen.uxml
Assets/Project/UXML/BannerTestScreen.uxml.meta
Assets/Project/UXML/BottomNav.uxml
Assets/Project/UXML/BottomNav.uxml.meta
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/GameplayScreen.uxml.meta
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/HomeScreen.uxml.meta
Assets/Project/UXML/LevelMapScreen.uxml
Assets/Project/UXML/LevelMapScreen.uxml.meta
Assets/Project/UXML/LoseScreen.uxml
Assets/Project/UXML/LoseScreen.uxml.meta
Assets/Project/UXML/MainPanelSettings.asset
Assets/Project/UXML/MainPanelSettings.asset.meta
Assets/Project/UXML/OverlayPanelSettings.asset
Assets/Project/UXML/OverlayPanelSettings.asset.meta
Assets/Project/UXML/SettingsScreen.uxml
Assets/Project/UXML/SettingsScreen.uxml.meta
Assets/Project/UXML/SplashScreen.uxml
Assets/Project/UXML/SplashScreen.uxml.meta
Assets/Project/UXML/WinScreen.uxml
Assets/Project/UXML/WinScreen.uxml.meta
Assets/Tests/EditMode/Editor/ArrowPaletteTests.cs
Assets/Tests/EditMode/Editor/ArrowPaletteTests.cs.meta
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs.meta
Assets/Tests/EditMode/Editor/LevelBinaryTests.cs
Assets/Tests/EditMode/Editor/LevelBinaryTests.cs.meta
Assets/Tests/EditMode/Editor/LevelCarverTests.cs
Assets/Tests/EditMode/Editor/LevelCarverTests.cs.meta
Assets/Tests/EditMode/Editor/LevelLocksTests.cs
Assets/Tests/EditMode/Editor/LevelLocksTests.cs.meta
Assets/Tests/EditMode/Editor/LevelSolverTests.cs
Assets/Tests/EditMode/Editor/LevelSolverTests.cs.meta
Assets/Tests/EditMode/Editor/SaveMigrationTests.cs
Assets/Tests/EditMode/Editor/SaveMigrationTests.cs.meta
Assets/Tests/EditMode/Editor/TestLevels.cs
Assets/Tests/EditMode/Editor/TestLevels.cs.meta
CLAUDE.md
ProjectSettings/EditorBuildSettings.asset
production/qa/evidence/story-005-evidence.md
production/qa/evidence/story-008-evidence.md
production/qa/evidence/story-008/01-splash.png
production/qa/evidence/story-008/02-home.png
production/qa/evidence/story-008/03-level-map.png
production/qa/evidence/story-008/04-settings.png
production/qa/evidence/story-008/05-hud-board.png
production/qa/evidence/story-008/06-bump.png
production/qa/evidence/story-008/07-hint.png
production/qa/evidence/story-008/08-unlock.png
production/qa/evidence/story-008/09-win.png
production/qa/evidence/story-008/10-lose.png
production/qa/test-plan-2026-10-20.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_124226
### Commits
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Audio/ArrowClick_Blocked.ogg
Assets/Project/Audio/ArrowClick_Blocked.ogg.meta
Assets/Project/Audio/ArrowClick_Success.mp3
Assets/Project/Audio/ArrowClick_Success.mp3.meta
Assets/Project/Audio/ArrowTap.wav
Assets/Project/Audio/ArrowTap.wav.meta
Assets/Project/Audio/LD_ArrowTap.wav
Assets/Project/Audio/LD_ArrowTap.wav.meta
Assets/Project/Audio/LevelComplete.wav
Assets/Project/Audio/LevelComplete.wav.meta
Assets/Project/Audio/LevelFailed.wav
Assets/Project/Audio/LevelFailed.wav.meta
Assets/Project/Audio/MainMixer.mixer
Assets/Project/Audio/MainMixer.mixer.meta
Assets/Project/Audio/UIClick.wav
Assets/Project/Audio/UIClick.wav.meta
Assets/Project/Materials/Mat_NeonDot.mat
Assets/Project/Materials/Mat_NeonDot.mat.meta
Assets/Project/Materials/Mat_NeonFlare.mat
Assets/Project/Materials/Mat_NeonFlare.mat.meta
Assets/Project/Materials/Mat_NeonTrail.mat
Assets/Project/Materials/Mat_NeonTrail.mat.meta
Assets/Project/Prefabs/ArrowPrefab.prefab
Assets/Project/Prefabs/ArrowPrefab.prefab.meta
Assets/Project/Prefabs/CommittedLineTemplate.prefab
Assets/Project/Prefabs/CommittedLineTemplate.prefab.meta
Assets/Project/Prefabs/Dot.prefab
Assets/Project/Prefabs/Dot.prefab.meta
Assets/Project/Resources/AdConfig.asset
Assets/Project/Resources/AdConfig.asset.meta
Assets/Project/Resources/Levels.meta
Assets/Project/Resources/Levels/Chunk_00.bytes
Assets/Project/Resources/Levels/Chunk_00.bytes.meta
Assets/Project/Resources/Levels/Chunk_01.bytes
Assets/Project/Resources/Levels/Chunk_01.bytes.meta
Assets/Project/Scenes/Game.meta
Assets/Project/Scenes/Game.unity
Assets/Project/Scenes/Game.unity.meta
Assets/Project/Scenes/Game/Global Volume Profile.asset
Assets/Project/Scenes/Game/Global Volume Profile.asset.meta
Assets/Project/Scripts/Core/ArrowController.cs
Assets/Project/Scripts/Core/ArrowController.cs.meta
Assets/Project/Scripts/Core/Dot.cs
Assets/Project/Scripts/Core/Dot.cs.meta
Assets/Project/Scripts/Core/GridBuilder.cs
Assets/Project/Scripts/Core/GridBuilder.cs.meta
Assets/Project/Scripts/Core/GridCell.cs
Assets/Project/Scripts/Core/GridCell.cs.meta
Assets/Project/Scripts/Core/GuideLines.cs
Assets/Project/Scripts/Core/GuideLines.cs.meta
Assets/Project/Scripts/Data/AdConfig.cs
Assets/Project/Scripts/Data/AdConfig.cs.meta
Assets/Project/Scripts/Data/AdRemoteConfig.cs
Assets/Project/Scripts/Data/AdRemoteConfig.cs.meta
Assets/Project/Scripts/Data/AdTestIds.cs
Assets/Project/Scripts/Data/AdTestIds.cs.meta
Assets/Project/Scripts/Data/ArrowPalette.cs
Assets/Project/Scripts/Data/ArrowPalette.cs.meta
Assets/Project/Scripts/Data/ArrowPathData.cs
Assets/Project/Scripts/Data/ArrowPathData.cs.meta
Assets/Project/Scripts/Data/LevelBinary.cs
Assets/Project/Scripts/Data/LevelBinary.cs.meta
Assets/Project/Scripts/Data/LevelData.cs
Assets/Project/Scripts/Data/LevelData.cs.meta
Assets/Project/Scripts/Data/SaveData.cs
Assets/Project/Scripts/Data/SaveData.cs.meta
Assets/Project/Scripts/Data/SaveSystem.cs
Assets/Project/Scripts/Data/SaveSystem.cs.meta
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs
Assets/Project/Scripts/Editor/LevelBinaryWriter.cs.meta
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs
Assets/Project/Scripts/Editor/LevelCampaignGenerator.cs.meta
Assets/Project/Scripts/Editor/LevelCarver.cs
Assets/Project/Scripts/Editor/LevelCarver.cs.meta
Assets/Project/Scripts/Editor/LevelDifficulty.cs
Assets/Project/Scripts/Editor/LevelDifficulty.cs.meta
Assets/Project/Scripts/Editor/LevelLocks.cs
Assets/Project/Scripts/Editor/LevelLocks.cs.meta
Assets/Project/Scripts/Editor/LevelShapeMask.cs
Assets/Project/Scripts/Editor/LevelShapeMask.cs.meta
Assets/Project/Scripts/Managers/AdConfigFetcher.cs
Assets/Project/Scripts/Managers/AdConfigFetcher.cs.meta
Assets/Project/Scripts/Managers/AdSettings.cs
Assets/Project/Scripts/Managers/AdSettings.cs.meta
Assets/Project/Scripts/Managers/AdsManager.cs
Assets/Project/Scripts/Managers/AdsManager.cs.meta
Assets/Project/Scripts/Managers/AndroidDataBridge.cs
Assets/Project/Scripts/Managers/AndroidDataBridge.cs.meta
Assets/Project/Scripts/Managers/AudioManager.cs
Assets/Project/Scripts/Managers/AudioManager.cs.meta
Assets/Project/Scripts/Managers/BackdropArrows.cs
Assets/Project/Scripts/Managers/BackdropArrows.cs.meta
Assets/Project/Scripts/Managers/BannerAdUnit.cs
Assets/Project/Scripts/Managers/BannerAdUnit.cs.meta
Assets/Project/Scripts/Managers/CameraController.cs
Assets/Project/Scripts/Managers/CameraController.cs.meta
Assets/Project/Scripts/Managers/EvidenceCapture.cs
Assets/Project/Scripts/Managers/EvidenceCapture.cs.meta
Assets/Project/Scripts/Managers/GameFlowManager.cs
Assets/Project/Scripts/Managers/GameFlowManager.cs.meta
Assets/Project/Scripts/Managers/HintManager.cs
Assets/Project/Scripts/Managers/HintManager.cs.meta
Assets/Project/Scripts/Managers/InterstitialAdUnit.cs
Assets/Project/Scripts/Managers/InterstitialAdUnit.cs.meta
Assets/Project/Scripts/Managers/InterstitialManager.cs
Assets/Project/Scripts/Managers/InterstitialManager.cs.meta
Assets/Project/Scripts/Managers/LevelLoader.cs
Assets/Project/Scripts/Managers/LevelLoader.cs.meta
Assets/Project/Scripts/Managers/LevelManager.cs
Assets/Project/Scripts/Managers/LevelManager.cs.meta
Assets/Project/Scripts/Managers/LevelTracker.cs
Assets/Project/Scripts/Managers/LevelTracker.cs.meta
Assets/Project/Scripts/Managers/LivesManager.cs
Assets/Project/Scripts/Managers/LivesManager.cs.meta
Assets/Project/Scripts/Managers/NativeAdUnit.cs
Assets/Project/Scripts/Managers/NativeAdUnit.cs.meta
Assets/Project/Scripts/Managers/RewardedAdUnit.cs
Assets/Project/Scripts/Managers/RewardedAdUnit.cs.meta
Assets/Project/Scripts/Managers/ScreenBackdrop.cs
Assets/Project/Scripts/Managers/ScreenBackdrop.cs.meta
Assets/Project/Scripts/Managers/SettingsManager.cs
Assets/Project/Scripts/Managers/SettingsManager.cs.meta
Assets/Project/Scripts/UI/AdLoadingOverlay.cs
Assets/Project/Scripts/UI/AdLoadingOverlay.cs.meta
Assets/Project/Scripts/UI/AdPlaceholder.cs
Assets/Project/Scripts/UI/AdPlaceholder.cs.meta
Assets/Project/Scripts/UI/BannerTestScreenController.cs
Assets/Project/Scripts/UI/BannerTestScreenController.cs.meta
Assets/Project/Scripts/UI/BottomNavBinder.cs
Assets/Project/Scripts/UI/BottomNavBinder.cs.meta
Assets/Project/Scripts/UI/ConfettiView.cs
Assets/Project/Scripts/UI/ConfettiView.cs.meta
Assets/Project/Scripts/UI/DeviceScaleCategory.cs
Assets/Project/Scripts/UI/DeviceScaleCategory.cs.meta
Assets/Project/Scripts/UI/GameplayHUDController.cs
Assets/Project/Scripts/UI/GameplayHUDController.cs.meta
Assets/Project/Scripts/UI/HomeScreenController.cs
Assets/Project/Scripts/UI/HomeScreenController.cs.meta
Assets/Project/Scripts/UI/LevelMapController.cs
Assets/Project/Scripts/UI/LevelMapController.cs.meta
Assets/Project/Scripts/UI/NativeAdSpace.cs
Assets/Project/Scripts/UI/NativeAdSpace.cs.meta
Assets/Project/Scripts/UI/ResultScreenController.cs
Assets/Project/Scripts/UI/ResultScreenController.cs.meta
Assets/Project/Scripts/UI/SettingsScreenController.cs
Assets/Project/Scripts/UI/SettingsScreenController.cs.meta
Assets/Project/Scripts/UI/SplashScreenController.cs
Assets/Project/Scripts/UI/SplashScreenController.cs.meta
Assets/Project/Scripts/UI/UIClickSound.cs
Assets/Project/Scripts/UI/UIClickSound.cs.meta
Assets/Project/Sprites/BackdropArrowBend.png
Assets/Project/Sprites/BackdropArrowBend.png.meta
Assets/Project/Sprites/BackdropArrowStraight.png
Assets/Project/Sprites/BackdropArrowStraight.png.meta
Assets/Project/Sprites/BackdropArrowZigzag.png
Assets/Project/Sprites/BackdropArrowZigzag.png.meta
Assets/Project/Sprites/Circle.png
Assets/Project/Sprites/Circle.png.meta
Assets/Project/Sprites/Circle01.png
Assets/Project/Sprites/Circle01.png.meta
Assets/Project/Sprites/Flare01.png
Assets/Project/Sprites/Flare01.png.meta
Assets/Project/Sprites/Gradient01.png
Assets/Project/Sprites/Gradient01.png.meta
Assets/Project/Sprites/HomeIcon.png
Assets/Project/Sprites/HomeIcon.png.meta
Assets/Project/Sprites/IconAd.png
Assets/Project/Sprites/IconAd.png.meta
Assets/Project/Sprites/IconBack.png
Assets/Project/Sprites/IconBack.png.meta
Assets/Project/Sprites/IconBulb.png
Assets/Project/Sprites/IconBulb.png.meta
Assets/Project/Sprites/IconDocument.png
Assets/Project/Sprites/IconDocument.png.meta
Assets/Project/Sprites/IconGrid.png
Assets/Project/Sprites/IconGrid.png.meta
Assets/Project/Sprites/IconHeart.png
Assets/Project/Sprites/IconHeart.png.meta
Assets/Project/Sprites/IconInfo.png
Assets/Project/Sprites/IconInfo.png.meta
Assets/Project/Sprites/IconPencil.png
Assets/Project/Sprites/IconPencil.png.meta
Assets/Project/Sprites/IconRefreshRate.png
Assets/Project/Sprites/IconRefreshRate.png.meta
Assets/Project/Sprites/IconRestart.png
Assets/Project/Sprites/IconRestart.png.meta
Assets/Project/Sprites/IconSound.png
Assets/Project/Sprites/IconSound.png.meta
Assets/Project/Sprites/IconStar.png
Assets/Project/Sprites/IconStar.png.meta
Assets/Project/Sprites/IconVibration.png
Assets/Project/Sprites/IconVibration.png.meta
Assets/Project/Sprites/LockIcon.png
Assets/Project/Sprites/LockIcon.png.meta
Assets/Project/Sprites/LogoSquare.png
Assets/Project/Sprites/LogoSquare.png.meta
Assets/Project/Sprites/NeonGradient.png
Assets/Project/Sprites/NeonGradient.png.meta
Assets/Project/Sprites/SettingsIcon.png
Assets/Project/Sprites/SettingsIcon.png.meta
Assets/Project/Sprites/Triangle.png
Assets/Project/Sprites/Triangle.png.meta
Assets/Project/Sprites/UIBarFill.png
Assets/Project/Sprites/UIBarFill.png.meta
Assets/Project/Sprites/UIBarTrack.png
Assets/Project/Sprites/UIBarTrack.png.meta
Assets/Project/Sprites/UIHexPillFill.png
Assets/Project/Sprites/UIHexPillFill.png.meta
Assets/Project/Sprites/UIHexPillFrame.png
Assets/Project/Sprites/UIHexPillFrame.png.meta
Assets/Project/Sprites/UIOctagonFill.png
Assets/Project/Sprites/UIOctagonFill.png.meta
Assets/Project/Sprites/UIOctagonFrame.png
Assets/Project/Sprites/UIOctagonFrame.png.meta
Assets/Project/Themes/AdLoadingOverlay.uss
Assets/Project/Themes/AdLoadingOverlay.uss.meta
Assets/Project/Themes/AdPlaceholder.uss
Assets/Project/Themes/AdPlaceholder.uss.meta
Assets/Project/Themes/BannerTestScreen.uss
Assets/Project/Themes/BannerTestScreen.uss.meta
Assets/Project/Themes/BottomNav.uss
Assets/Project/Themes/BottomNav.uss.meta
Assets/Project/Themes/GameplayScreen.uss
Assets/Project/Themes/GameplayScreen.uss.meta
Assets/Project/Themes/HomeScreen.uss
Assets/Project/Themes/HomeScreen.uss.meta
Assets/Project/Themes/LevelMapScreen.uss
Assets/Project/Themes/LevelMapScreen.uss.meta
Assets/Project/Themes/ResultScreen.uss
Assets/Project/Themes/ResultScreen.uss.meta
Assets/Project/Themes/SettingsScreen.uss
Assets/Project/Themes/SettingsScreen.uss.meta
Assets/Project/Themes/SplashScreen.uss
Assets/Project/Themes/SplashScreen.uss.meta
Assets/Project/Themes/Typography.uss
Assets/Project/Themes/Typography.uss.meta
Assets/Project/UXML/AdLoadingOverlay.uxml
Assets/Project/UXML/AdLoadingOverlay.uxml.meta
Assets/Project/UXML/AdPlaceholder.uxml
Assets/Project/UXML/AdPlaceholder.uxml.meta
Assets/Project/UXML/BannerTestScreen.uxml
Assets/Project/UXML/BannerTestScreen.uxml.meta
Assets/Project/UXML/BottomNav.uxml
Assets/Project/UXML/BottomNav.uxml.meta
Assets/Project/UXML/GameplayScreen.uxml
Assets/Project/UXML/GameplayScreen.uxml.meta
Assets/Project/UXML/HomeScreen.uxml
Assets/Project/UXML/HomeScreen.uxml.meta
Assets/Project/UXML/LevelMapScreen.uxml
Assets/Project/UXML/LevelMapScreen.uxml.meta
Assets/Project/UXML/LoseScreen.uxml
Assets/Project/UXML/LoseScreen.uxml.meta
Assets/Project/UXML/MainPanelSettings.asset
Assets/Project/UXML/MainPanelSettings.asset.meta
Assets/Project/UXML/OverlayPanelSettings.asset
Assets/Project/UXML/OverlayPanelSettings.asset.meta
Assets/Project/UXML/SettingsScreen.uxml
Assets/Project/UXML/SettingsScreen.uxml.meta
Assets/Project/UXML/SplashScreen.uxml
Assets/Project/UXML/SplashScreen.uxml.meta
Assets/Project/UXML/WinScreen.uxml
Assets/Project/UXML/WinScreen.uxml.meta
Assets/Tests/EditMode/Editor/ArrowPaletteTests.cs
Assets/Tests/EditMode/Editor/ArrowPaletteTests.cs.meta
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs
Assets/Tests/EditMode/Editor/BakedCampaignTests.cs.meta
Assets/Tests/EditMode/Editor/LevelBinaryTests.cs
Assets/Tests/EditMode/Editor/LevelBinaryTests.cs.meta
Assets/Tests/EditMode/Editor/LevelCarverTests.cs
Assets/Tests/EditMode/Editor/LevelCarverTests.cs.meta
Assets/Tests/EditMode/Editor/LevelLocksTests.cs
Assets/Tests/EditMode/Editor/LevelLocksTests.cs.meta
Assets/Tests/EditMode/Editor/LevelSolverTests.cs
Assets/Tests/EditMode/Editor/LevelSolverTests.cs.meta
Assets/Tests/EditMode/Editor/SaveMigrationTests.cs
Assets/Tests/EditMode/Editor/SaveMigrationTests.cs.meta
Assets/Tests/EditMode/Editor/TestLevels.cs
Assets/Tests/EditMode/Editor/TestLevels.cs.meta
CLAUDE.md
ProjectSettings/EditorBuildSettings.asset
design/accessibility-requirements.md
production/qa/evidence/story-005-evidence.md
production/qa/evidence/story-008-evidence.md
production/qa/evidence/story-008/01-splash.png
production/qa/evidence/story-008/02-home.png
production/qa/evidence/story-008/03-level-map.png
production/qa/evidence/story-008/04-settings.png
production/qa/evidence/story-008/05-hud-board.png
production/qa/evidence/story-008/06-bump.png
production/qa/evidence/story-008/07-hint.png
production/qa/evidence/story-008/08-unlock.png
production/qa/evidence/story-008/09-win.png
production/qa/evidence/story-008/10-lose.png
production/qa/test-plan-2026-10-20.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_130008
### Commits
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
---

## Session End: 20261020_130451
### Commits
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_131016
### Commits
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scenes/LanternDragons.unity
Assets/Project/Scripts/Managers/StageTracker.cs
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_131216
### Commits
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scenes/LanternDragons.unity
Assets/Project/Scripts/Managers/FlowDirector.cs
Assets/Project/Scripts/Managers/HostBridge.cs
Assets/Project/Scripts/Managers/HostBridge.cs.meta
Assets/Project/Scripts/Managers/PromoBannerUnit.cs
Assets/Project/Scripts/Managers/PromoConfigFetcher.cs
Assets/Project/Scripts/Managers/PromoInterstitialUnit.cs
Assets/Project/Scripts/Managers/PromoManager.cs
Assets/Project/Scripts/Managers/PromoNativeUnit.cs
Assets/Project/Scripts/Managers/PromoRewardedUnit.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageTracker.cs
Assets/Project/Scripts/Managers/StageTracker.cs.meta
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_131531
### Commits
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scenes/LanternDragons.unity
Assets/Project/Scripts/Managers/FlowDirector.cs
Assets/Project/Scripts/Managers/HostBridge.cs
Assets/Project/Scripts/Managers/HostBridge.cs.meta
Assets/Project/Scripts/Managers/PromoBannerUnit.cs
Assets/Project/Scripts/Managers/PromoConfigFetcher.cs
Assets/Project/Scripts/Managers/PromoInterstitialUnit.cs
Assets/Project/Scripts/Managers/PromoManager.cs
Assets/Project/Scripts/Managers/PromoNativeUnit.cs
Assets/Project/Scripts/Managers/PromoRewardedUnit.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageTracker.cs
Assets/Project/Scripts/Managers/StageTracker.cs.meta
CLAUDE.md
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
production/epics/lantern-dragons/story-007-new-identity-and-kept-systems.md
production/qa/story-007-identity-switch-plan.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_131941
### Commits
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scenes/LanternDragons.unity
Assets/Project/Scripts/Data/PromoConfig.cs
Assets/Project/Scripts/Managers/ClueManager.cs
Assets/Project/Scripts/Managers/FlowDirector.cs
Assets/Project/Scripts/Managers/HostBridge.cs
Assets/Project/Scripts/Managers/HostBridge.cs.meta
Assets/Project/Scripts/Managers/PromoBannerUnit.cs
Assets/Project/Scripts/Managers/PromoConfigFetcher.cs
Assets/Project/Scripts/Managers/PromoInterstitialManager.cs
Assets/Project/Scripts/Managers/PromoInterstitialUnit.cs
Assets/Project/Scripts/Managers/PromoManager.cs
Assets/Project/Scripts/Managers/PromoNativeUnit.cs
Assets/Project/Scripts/Managers/PromoRewardedUnit.cs
Assets/Project/Scripts/Managers/PromoSettings.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageTracker.cs
Assets/Project/Scripts/Managers/StageTracker.cs.meta
CLAUDE.md
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
production/epics/lantern-dragons/story-007-new-identity-and-kept-systems.md
production/qa/story-007-identity-switch-plan.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_132021
### Commits
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scenes/LanternDragons.unity
Assets/Project/Scripts/Data/PromoConfig.cs
Assets/Project/Scripts/Managers/ClueManager.cs
Assets/Project/Scripts/Managers/EmberManager.cs
Assets/Project/Scripts/Managers/FlowDirector.cs
Assets/Project/Scripts/Managers/HostBridge.cs
Assets/Project/Scripts/Managers/HostBridge.cs.meta
Assets/Project/Scripts/Managers/OptionsManager.cs
Assets/Project/Scripts/Managers/PromoBannerUnit.cs
Assets/Project/Scripts/Managers/PromoConfigFetcher.cs
Assets/Project/Scripts/Managers/PromoInterstitialManager.cs
Assets/Project/Scripts/Managers/PromoInterstitialUnit.cs
Assets/Project/Scripts/Managers/PromoManager.cs
Assets/Project/Scripts/Managers/PromoNativeUnit.cs
Assets/Project/Scripts/Managers/PromoRewardedUnit.cs
Assets/Project/Scripts/Managers/PromoSettings.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageTracker.cs
Assets/Project/Scripts/Managers/StageTracker.cs.meta
CLAUDE.md
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
production/epics/lantern-dragons/story-007-new-identity-and-kept-systems.md
production/qa/story-007-identity-switch-plan.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_132636
### Commits
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scenes/LanternDragons.unity
Assets/Project/Scripts/Data/ProgressData.cs
Assets/Project/Scripts/Data/ProgressStore.cs
Assets/Project/Scripts/Data/PromoConfig.cs
Assets/Project/Scripts/Managers/ClueManager.cs
Assets/Project/Scripts/Managers/EmberManager.cs
Assets/Project/Scripts/Managers/FlowDirector.cs
Assets/Project/Scripts/Managers/HostBridge.cs
Assets/Project/Scripts/Managers/HostBridge.cs.meta
Assets/Project/Scripts/Managers/OptionsManager.cs
Assets/Project/Scripts/Managers/PromoBannerUnit.cs
Assets/Project/Scripts/Managers/PromoConfigFetcher.cs
Assets/Project/Scripts/Managers/PromoInterstitialManager.cs
Assets/Project/Scripts/Managers/PromoInterstitialUnit.cs
Assets/Project/Scripts/Managers/PromoManager.cs
Assets/Project/Scripts/Managers/PromoNativeUnit.cs
Assets/Project/Scripts/Managers/PromoRewardedUnit.cs
Assets/Project/Scripts/Managers/PromoSettings.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageTracker.cs
Assets/Project/Scripts/Managers/StageTracker.cs.meta
Assets/Project/Scripts/UI/HallScreenController.cs
Assets/Project/Scripts/UI/OptionsScreenController.cs
Assets/Project/Scripts/UI/PlayScreenController.cs
Assets/Project/Scripts/UI/PromoLabScreenController.cs
Assets/Project/Scripts/UI/SheetScreenController.cs
Assets/Project/Scripts/UI/TabDockBinder.cs
Assets/Project/Themes/HallScreen.uss
Assets/Project/Themes/OptionsScreen.uss
Assets/Project/Themes/TabDock.uss
Assets/Project/UXML/HallScreen.uxml
Assets/Project/UXML/TabDock.uxml
Assets/Tests/EditMode/Editor/ProgressMigrationTests.cs
CLAUDE.md
ProjectSettings/AndroidResolverDependencies.xml
ProjectSettings/ProjectSettings.asset
design/accessibility-requirements.md
design/ux/interaction-patterns.md
design/ux/ui-redesign.md
production/epics/lantern-dragons/story-007-new-identity-and-kept-systems.md
production/qa/story-007-identity-switch-plan.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_133525
### Commits
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
---

## Session End: 20261020_133710
### Commits
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_135551
### Commits
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Audio/LD_Bump.wav
Assets/Project/Audio/LD_Bump.wav.meta
Assets/Project/Audio/LD_DragonTap.wav
Assets/Project/Audio/LD_DragonTap.wav.meta
Assets/Project/Audio/LD_LockShake.wav
Assets/Project/Audio/LD_LockShake.wav.meta
Assets/Project/Audio/LD_Lose.wav
Assets/Project/Audio/LD_Lose.wav.meta
Assets/Project/Audio/LD_UIClick.wav
Assets/Project/Audio/LD_UIClick.wav.meta
Assets/Project/Audio/LD_Unlock.wav
Assets/Project/Audio/LD_Unlock.wav.meta
Assets/Project/Audio/LD_Win.wav
Assets/Project/Audio/LD_Win.wav.meta
Assets/Project/Audio/LanternMixer.mixer
Assets/Project/Scenes/LanternDragons.unity
Assets/Project/Scripts/Core/DragonController.cs
Assets/Project/Scripts/Managers/EmberManager.cs
Assets/Project/Scripts/Managers/FlowDirector.cs
Assets/Project/Scripts/Managers/OptionsManager.cs
Assets/Project/Scripts/Managers/SoundDirector.cs
Assets/Project/Scripts/Managers/StageLoader.cs
Assets/Project/Scripts/UI/HallScreenController.cs
Assets/Project/Scripts/UI/OptionsScreenController.cs
Assets/Project/Scripts/UI/SheetScreenController.cs
Assets/Project/Scripts/UI/TrailMapController.cs
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_135914
### Commits
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
---

## Session End: 20261020_140243
### Commits
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scripts/Core/DragonController.cs
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_140533
### Commits
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scripts/Core/DragonController.cs
Assets/Project/Scripts/Data/GameConfig.cs
Assets/Project/Themes/IntroScreen.uss
Assets/Project/UXML/HallScreen.uxml
Assets/Project/UXML/IntroScreen.uxml
design/config/game-config.sample.json
design/game-brief.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_141946
### Commits
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scripts/Core/DragonController.cs
Assets/Project/Scripts/Data/GameConfig.cs
Assets/Project/Scripts/Data/ProgressData.cs
Assets/Project/Scripts/Managers/StageLoader.cs
Assets/Project/Sprites/Padlock.png
Assets/Project/Themes/IntroScreen.uss
Assets/Project/UXML/HallScreen.uxml
Assets/Project/UXML/IntroScreen.uxml
Assets/Project/UXML/PlayScreen.uxml
Assets/Tests/EditMode/Editor/GameConfigTests.cs
CLAUDE.md
ProjectSettings/ProjectSettings.asset
design/accessibility-requirements.md
design/audio/audio-lantern-dragons-sfx.md
design/config/README.md
design/config/game-config.sample.json
design/game-brief.md
design/ux/ui-redesign.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_142946
### Commits
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scripts/Core/DragonController.cs
Assets/Project/Scripts/Data/GameConfig.cs
Assets/Project/Scripts/Data/ProgressData.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageLoader.cs
Assets/Project/Scripts/UI/PlayScreenController.cs
Assets/Project/Sprites/Padlock.png
Assets/Project/Themes/IntroScreen.uss
Assets/Project/UXML/HallScreen.uxml
Assets/Project/UXML/IntroScreen.uxml
Assets/Project/UXML/PlayScreen.uxml
Assets/Tests/EditMode/Editor/GameConfigTests.cs
CLAUDE.md
ProjectSettings/ProjectSettings.asset
design/accessibility-requirements.md
design/audio/audio-lantern-dragons-sfx.md
design/config/README.md
design/config/game-config.sample.json
design/game-brief.md
design/ux/interaction-patterns.md
design/ux/ui-redesign.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_145525
### Commits
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
---

## Session End: 20261020_150315
### Commits
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scripts/Core/BoardBuilder.cs
Assets/Project/Scripts/Core/DragonController.cs
Assets/Project/Scripts/Core/Spark.cs
Assets/Project/Scripts/Data/GameConfig.cs
Assets/Project/Scripts/Managers/GameConfigService.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageLoader.cs
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_150634
### Commits
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scripts/Core/BoardBuilder.cs
Assets/Project/Scripts/Core/DragonController.cs
Assets/Project/Scripts/Core/Spark.cs
Assets/Project/Scripts/Data/GameConfig.cs
Assets/Project/Scripts/Managers/GameConfigService.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageLoader.cs
design/config/README.md
design/config/game-config.sample.json
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_151906
### Commits
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
---

## Session End: 20261020_152711
### Commits
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_154926
### Commits
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scripts/Core/BoardBuilder.cs
Assets/Project/Scripts/Core/DragonController.cs
Assets/Project/Scripts/Data/DragonPathData.cs
Assets/Project/Scripts/Data/StageData.cs
Assets/Project/Scripts/Data/StagePack.cs
Assets/Project/Scripts/Editor/StageCarver.cs
Assets/Project/Scripts/Editor/StageRating.cs
Assets/Project/Scripts/Managers/StageLoader.cs
Assets/Project/Scripts/Managers/StageManager.cs
design/game-brief.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_155033
### Commits
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Project/Scripts/Core/BoardBuilder.cs
Assets/Project/Scripts/Core/DragonController.cs
Assets/Project/Scripts/Data/DragonPathData.cs
Assets/Project/Scripts/Data/StageData.cs
Assets/Project/Scripts/Data/StagePack.cs
Assets/Project/Scripts/Editor/StageCarver.cs
Assets/Project/Scripts/Editor/StageRating.cs
Assets/Project/Scripts/Managers/ShotCapture.cs
Assets/Project/Scripts/Managers/StageLoader.cs
Assets/Project/Scripts/Managers/StageManager.cs
design/game-brief.md
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_160243
### Commits
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_160836
### Commits
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
ProjectSettings/ProjectSettings.asset
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_161134
### Commits
a964b2a chore: lock Android orientation to portrait only
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_161723
### Commits
a964b2a chore: lock Android orientation to portrait only
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib/AndroidManifest.xml
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261020_162306
### Commits
a964b2a chore: lock Android orientation to portrait only
726c40c feat: level entrance animation styles (ink default, fly, classic), tap to skip
8821ee5 docs: lock UX evidence and frames, audio design doc touch-ups, product name in specs
3186950 feat: first-lock tutorial, legible padlock number, Arrow Dragon Puzzle name, silent dragon exit
0a79927 docs: audio design document, SFX spec and audio research report
bd97844 feat: real CC0 audio, eight new sound events, SoundDirector rewrite and light haptics
d6f2842 docs: game config schema and README, Story 007 evidence and frames, updated specs
4f7e684 feat: new package id, standalone ad/config stack, fully dynamic game config, two-item dock
8369eba docs: QA evidence, rename report, accessibility tier, sprint plan and Layout v2 spec
f341ca0 refactor: rename the whole project to Lantern Dragons names, plus review fixes and accessibility
0063ab5 chore: safety snapshot before the file rename (light paper theme, new UI, shapes, level map)
### Uncommitted Changes
Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib/AndroidManifest.xml
Assets/Plugins/Android/proguard-user.txt
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261021_101754
### Uncommitted Changes
Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib/AndroidManifest.xml
Assets/Plugins/Android/proguard-user.txt
Assets/Project/Scripts/UI/TrailMapController.cs
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---

## Session End: 20261021_105014
### Uncommitted Changes
Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib/AndroidManifest.xml
Assets/Plugins/Android/proguard-user.txt
Assets/Project/Scripts/Managers/BoardCamera.cs
Assets/Project/Scripts/UI/TrailMapController.cs
production/session-logs/.session-end.hash
production/session-logs/session-cost.md
production/session-logs/session-log.md
---


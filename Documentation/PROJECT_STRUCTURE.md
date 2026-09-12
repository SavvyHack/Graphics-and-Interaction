# Project layout

Open this repository in Unity 6000.3.18f1 and press **Play**. The saved prototype opens automatically. No builder or setup command is required.

```text
Assets/
  Scenes/                 RatEnclosure (playable prototype), StartScene (original scene)
  Scripts/
    Gameplay/             Player movement, lives, game state, triggers, spinning wheel
    Presentation/         Camera, HUD, audio, team integration
    Prototype/            Trial session, zones, moving platform, trial wheel
    Testing/              Runtime driver used by the editor's gameplay smoke tests
  Shaders/                The four project shader sources; each exists only once
  Materials/
    Environment/          Laboratory wall, black floor, metal trim, observation glass, warning accents
    Characters/           Rat body, ears, and stylised lighting
    Water/                Water body and surface
    Checkpoints/          Checkpoint flag and metal
    Physics/              Frictionless obstacle physics material
  Editor/                 Optional authoring tools, automatic Play entry, validation
  TextMesh Pro/           Bundled third-party fonts, resources and shaders
Documentation/
  Images/                 Design sketches and reference images used by the README
  PROTOTYPE_LEVEL.md       Controls, route, art direction, integration and test notes
  PROJECT_STRUCTURE.md     This directory guide
Packages/                 Unity package manifest and lockfile
ProjectSettings/           Unity project and build settings
README.md                  Game design document and quick start
REPORT.md                  Coursework shader report
metadata.json              Coursework team metadata
.github/                   Coursework submission workflow
```

## Where to make changes

- Edit game behaviour in `Assets/Scripts/` and project shaders in `Assets/Shaders/`. There are no reference copies of these source files at the repository root.
- Edit the playable course in `Assets/Scenes/RatEnclosure.unity`. `StartScene.unity` remains available for the team's original scene and is disabled in build settings.
- Keep all materials under `Assets/Materials/`, in the relevant category. The base glass material and observation preset remain separate assets for the starter scene and prototype.
- Keep design documents and their images together under `Documentation/`. The README, shader report, team metadata and submission workflow remain at their expected coursework locations.
- Keep bundled TextMesh Pro resources together; its shaders and sample assets belong to that dependency, rather than the project's custom shader collection.

Unity references assets using the GUID in each `.meta` file. Move an asset together with its `.meta`, preferably using Unity's Project panel. The reorganisation preserved every existing asset GUID. `Assets/Editor/PrototypeAssetPaths.cs` centralises the paths used by the builder, styling tools, startup and tests.

## Generated local files

Unity maintains `Library/`, `Logs/`, `Temp/`, and `UserSettings/` locally. These are ignored by Git and are not source folders. Visual Studio project/solution files (`.csproj`, `.sln`, `.slnx`) and `.vsconfig` are generated as needed and ignored too. They can reappear when Unity opens an IDE; do not move them into `Assets/`.

## Verification

`Project R.A.T. > Validate Project References` checks the prototype build entry and both scenes for missing scripts, materials and shaders.

For automated verification, set `RAT_OUTPUT` to an output directory and run Unity in batch mode with `-executeMethod ObservationStyle.VerifyAndPlaytest`, without `-quit` or `-nographics`. This validates references, renders both scenes, verifies automatic Play startup from the starter scene, and runs all 14 startup and gameplay checks. The test suite exits Unity when complete. It does not regenerate the level.

Verified after reorganisation on 12 September 2026 with Unity 6000.3.18f1: all 96 pre-existing asset GUIDs were retained; 78 non-editor asset files (including both scenes, runtime scripts, materials and shaders) were byte-identical to their pre-move versions; reference validation, rendering and all 14 startup/gameplay checks passed.

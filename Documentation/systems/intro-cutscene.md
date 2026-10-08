# Intro cutscene

Start scene (`Assets/Scenes/Start.unity`, build index 0) plays a 7-panel narrated sequence, then loads `Home`. Skip (top-right) loads Home immediately. The old `Assets/StartScene.unity` template is unrelated and stays disabled.

## Files
- `Assets/Scripts/Presentation/CutsceneData.cs`: ScriptableObject; list of panels (Sprite, caption, HardCut/SlideIn).
- `Assets/Scripts/Presentation/CutsceneController.cs`: drives the UI. Serialized `data`, `nextSceneName` (default `Home`), UI refs and timing. Uses the Bouncy Bun menu font with the scene font as glyph fallback.
- `Assets/Editor/CutsceneAuthoring.cs`: menu `Project R.A.T./Cutscene/Build Intro Cutscene`. Imports sprites, creates the data asset and scene, sets Build Settings. Refuses to overwrite an existing Start scene; keeps an existing data asset.
- `Assets/Art/Cutscene/Panel_01.png` to `Panel_07.png` (1920x810 placeholders), `UI/NextArrow.png`, `IntroCutsceneData.asset`.

## Behaviour
Right Arrow (Input System keyboard) or a click/tap on the bobbing arrow advances; nothing else does. Text appears instantly. SlideIn panels slide from the right over 0.35s (ease-out cubic) and input is blocked meanwhile. The last advance loads Home. No audio. Canvas scales from 1920x1080; picture area is the top 75%, text box the bottom 25%.

## Editing
- Swap art: overwrite `Panel_NN.png` keeping the name and 1920x810 size (references survive), or drag a new sprite into the asset's panel.
- Captions/transitions: select `IntroCutsceneData` and edit in the Inspector; add or reorder panels freely.
- Next scene: `nextSceneName` on the controller in Start.

## Limits
- Placeholder art only; the font lacks some punctuation, so captions rely on the fallback font for those glyphs.
- `PrototypePlayMode` now also exempts Start so Play runs the cutscene.
- Right Arrow itself was not pressed in a test; advance logic was called directly.

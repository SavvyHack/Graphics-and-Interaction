> **Historical prototype notes:** See [CURRENT_STATE.md](CURRENT_STATE.md) for the inspected uploaded snapshot and [GAME_DESIGN.md](GAME_DESIGN.md) for the new target. Setup instructions and past verification claims below are not proof of current scene wiring or successful validation.

# Game Systems setup

Open your current scene and use Project R.A.T. > Game Systems > Install or Refresh Game Systems to refresh gameplay/audio wiring. This does not rebuild platforms, relocate hazards, or regenerate prefabs. It saves the current layout.

Project R.A.T. > Game Systems > Add Pressure Plate Laser Puzzle upgrades a scene once, after creating a backup. If the puzzle is already installed, its layout is preserved.

Rebuild Default Hazard Course (Resets Layout) is a separate command intended only to restore the original generated course.

Scripts are under Assets/Editor and Assets/Scripts/Gameplay. Shaders are under Assets/Shaders, hazard materials under Assets/Materials/Hazards, and sounds under Assets/Audio. Their class names and references match the filenames.

Use the Level 1, Level 2 and Level 3 Save Scene / Load Scene menus to manage separate scenes. See OPEN-FIRST.md at the project root for details and validation status.

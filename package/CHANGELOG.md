# 0.3.1

- Added standalone player and enemy map arrows.
- Added configurable player and enemy arrow colors.
- Added custom hex color support.
- Added player head color mode.
- Added configurable arrow scale and outline.
- Added configurable map zoom multiplier.
- Added mouse wheel and keyboard zoom controls.
- Added cleanup to reset map zoom when the plugin is unloaded.
- Cleaned tracker code and package documentation.
- Fixed admin-spawned enemies not appearing on the map by tracking enemy spawn-point registration events.
- Fixed arrow outline scaling so outlines are visible at small arrow sizes.
- Fixed local player arrows being removed if they slip through registration.
- Tuned light color presets for better map readability.
- Removed constant enemy/player safety scans and switched admin-spawned enemies to event-driven enemy hooks.

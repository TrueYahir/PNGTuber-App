# Changelog

## [Unreleased]

### Added
- Hierarchical layer system with drag-and-drop nesting capabilities.
- Layer management tools including visibility and lock toggles.
- Transformation controls for individual layers (Position X/Y, Scale X/Y, Rotation).
- Avatar state management supporting 8 distinct states (Idle, Talking, Sad, Crying, Mad, Angry, Laughing, Thinking).
- Import and export functionality for custom `.pngvt` avatar project files.
- Live preview rendering of the avatar based on selected states and layers.
- Avatar launch functionality via a dedicated viewer window.
- Basic application settings menu.

### Changed
- Merged the Transformation panel directly into the Layers tab to improve user experience and reduce navigation overhead.
- Configured the Transformation panel to render only when an active layer is selected.
- Translated all user interface elements, tooltips, and file dialogs to English.
- Optimized drag-and-drop logic with a descendant check to prevent recursive infinite loops when moving layers.


## 19-09-2026
### Added
- Added a file browser button to the Image Override Path in the Animations tab to natively select system images.

### Changed
- Converted the Target Layer Name field from a manual text input into a dropdown menu that automatically populates with active layers.

### Fixed
- Resolved an invalid cast exception in the Playback Mode dropdown to ensure animation loop states apply correctly.
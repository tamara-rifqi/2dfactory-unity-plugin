# Changelog

All notable changes to 2DFactory are documented in this file.

## [1.0.0] - 2026-09-20

### Additions
- Blender-to-Unity sprite workflow using 2DFactory JSON metadata.
- Single Sheet and Separate Sheet workflows.
- Sprite creation from JSON metadata.
- Animation Clip generation.
- Animator Controller generation.
- Prefab generation and synchronization.
- Socket data import and synchronization.
- Socket position updates based on the currently displayed sprite.
- Secondary texture support for:
  - Normal maps (`_N`)
  - Roughness maps (`_R`)
  - Ambient occlusion maps (`_AO`)
  - Emission maps (`_E`)
- Project Settings for configuring:
  - Default SpriteRenderer material.
  - Separate Sheet output naming.
  - Recursive JSON scan depth.
  - Main texture filter mode.
  - Main texture compression.
  - Applying texture settings to secondary textures.
  - Socket reticle creation.
- 2DFactory PBR sample material and Shader Graph.
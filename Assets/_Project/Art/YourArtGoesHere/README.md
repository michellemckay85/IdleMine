# Art goes here

Drop your imported art assets (models, textures, sprites, animations) under
`Assets/_Project/Art/`. Suggested subfolders:

```
Art/
  Blocks/       # mine block meshes (dirt, stone, ore, key, chest)
  Goblins/      # goblin character models + animations
  Characters/   # miner/player character if visible
  Environment/  # mine shaft, background, props
  UI/           # icons, buttons, banners for the shop/events
  VFX/          # break particles, gold sparkle, gem sparkle
```

Once a block mesh is imported, build a prefab out of it (mesh + Collider for tap
input + the GritLine Toon Shader material applied to its Renderer) and assign that
prefab to the matching `BlockDataSO.visualPrefab` field under
`Assets/_Project/ScriptableObjects/Blocks/`. Same pattern for goblins via
`GoblinDataSO.visualPrefab`.

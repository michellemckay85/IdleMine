# GritLine Toon Shader goes here

Import the GritLine Toon Shader package into this folder (or wherever its own
package structure expects — most Asset Store shader packages come with their own
folder layout; you can leave it there and just reference the shader/material from
here).

To use it on mine blocks and goblins:

1. Create a Material per block/goblin type using the GritLine shader.
2. Assign textures/colors per your art direction.
3. Put that Material on the Renderer of each art prefab referenced by
   `BlockDataSO.visualPrefab` / `GoblinDataSO.visualPrefab`.

This project defaults to **URP** (Universal Render Pipeline). If your GritLine
package targets the Built-in Render Pipeline instead, either:
- get the URP-compatible version of the shader if the asset offers one, or
- tell me and I'll switch the project's render pipeline package/settings to
  Built-in RP to match.

Nothing in the gameplay code depends on a specific shader — `Block.cs` and the
goblin visuals just instantiate whatever prefab is assigned, so swapping shaders
or materials later doesn't require code changes.

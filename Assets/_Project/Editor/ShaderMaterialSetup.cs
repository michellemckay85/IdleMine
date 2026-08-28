using System.IO;
using UnityEditor;
using UnityEngine;

namespace GoldAndGoblins.EditorTools
{
    // Run after "Wire Up Imported Art": that tool builds visual prefabs from the raw
    // meshes but leaves them on whatever default material the FBX import produced --
    // nothing was actually using the GritLine Toon Shader yet. This creates real
    // materials on it (property names read off the shader's own demo material,
    // Gritline_LitToon.mat -- _Texture, _Color, _Shades, _Min, _Max, _PosterizeSteps,
    // _Metallic, _Smoothness) and assigns them to the block/environment/character
    // prefabs' renderers.
    //
    // character_skeleton_warrior.fbx (the goblin placeholder) has no texture in the
    // repo -- the KayKit dungeon pack ships a separate skeleton texture atlas that
    // wasn't part of this import. Its material is created with no texture (flat tint)
    // rather than guessing the wrong one; drop the real texture into Art/Goblins and
    // re-run this tool once you have it.
    public static class ShaderMaterialSetup
    {
        private const string ShaderPath = "Assets/_Project/Shaders/GritlineToonShader/Shaders/LitToon.shadergraph";
        private const string MaterialOutputPath = "Assets/_Project/Art/Materials";
        private const string VisualPrefabPath = "Assets/_Project/Art/Prefabs";

        private struct MaterialSpec
        {
            public string materialName;
            public string texturePath; // null/empty = no texture found for this mesh yet
            public string[] prefabNames; // prefab asset names under Art/Prefabs to apply this material to
        }

        [MenuItem("Gold And Goblins/Wire Up GritLine Materials")]
        public static void WireUpMaterials()
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            if (shader == null)
            {
                Debug.LogError($"[ShaderMaterialSetup] Couldn't find the GritLine shader at {ShaderPath}. " +
                                "If the package re-imported into a different folder, update ShaderPath.");
                return;
            }

            Directory.CreateDirectory(MaterialOutputPath);

            // Delete the incorrectly-textured material from a previous run of this tool,
            // if present, rather than leaving it as orphaned clutter under Art/Materials.
            const string obsoleteMaterialPath = MaterialOutputPath + "/Mat_Dungeon.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(obsoleteMaterialPath) != null)
            {
                AssetDatabase.DeleteAsset(obsoleteMaterialPath);
            }

            var specs = new[]
            {
                // block_bits_texture.png and dungeon_texture.png are two different KayKit
                // palette sheets that happen to share most swatch colors -- but not all.
                // dirt.fbx's UVs land on a swatch that's hot magenta in dungeon_texture.png
                // and a normal orange/brown in block_bits_texture.png, which is the one
                // actually meant for these meshes (the Environment pieces use the other).
                new MaterialSpec
                {
                    materialName = "Mat_Blocks",
                    texturePath = "Assets/_Project/Art/Blocks/block_bits_texture.png",
                    prefabNames = new[] { "dirt", "stone", "ore_common", "ore_rare", "key", "chest" }
                },
                new MaterialSpec
                {
                    materialName = "Mat_Environment",
                    texturePath = "Assets/_Project/Art/Environment/dungeon_texture.png",
                    prefabNames = new[] { "env_wall_doorway", "env_floor_dirt", "env_torch_lit" }
                },
                new MaterialSpec
                {
                    materialName = "Mat_Miner",
                    texturePath = "Assets/_Project/Art/Characters/barbarian_texture.png",
                    prefabNames = new[] { "character_miner" }
                },
                new MaterialSpec
                {
                    materialName = "Mat_Goblin",
                    texturePath = null, // no skeleton texture in the repo yet -- see class comment
                    prefabNames = new[] { "goblin_skeleton" }
                },
            };

            var missingTextureWarnings = 0;

            foreach (var spec in specs)
            {
                var material = BuildOrUpdateMaterial(shader, spec.materialName, spec.texturePath, ref missingTextureWarnings);

                foreach (var prefabName in spec.prefabNames)
                {
                    ApplyMaterialToPrefab(prefabName, material);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ShaderMaterialSetup] Created/updated {specs.Length} GritLine materials under {MaterialOutputPath} " +
                      "and assigned them to the block/environment/character/goblin visual prefabs." +
                      (missingTextureWarnings > 0
                          ? " One or more materials have no texture yet -- see preceding warnings."
                          : ""));
        }

        private static Material BuildOrUpdateMaterial(Shader shader, string materialName, string texturePath, ref int missingTextureWarnings)
        {
            var path = $"{MaterialOutputPath}/{materialName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_Color", Color.white);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.1f);
            material.SetFloat("_Shades", 0.28f);
            material.SetFloat("_Min", 0.1f);
            material.SetFloat("_Max", 1.2f);
            material.SetFloat("_PosterizeSteps", 10f);

            if (!string.IsNullOrEmpty(texturePath))
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture != null)
                {
                    material.SetTexture("_Texture", texture);
                }
                else
                {
                    Debug.LogWarning($"[ShaderMaterialSetup] Texture not found at {texturePath} for material {materialName}.");
                    missingTextureWarnings++;
                }
            }
            else
            {
                missingTextureWarnings++;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ApplyMaterialToPrefab(string prefabName, Material material)
        {
            // ArtImportSetup.BuildVisualPrefab names these "Visual_{id}.prefab".
            var path = $"{VisualPrefabPath}/Visual_{prefabName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[ShaderMaterialSetup] No prefab found at {path} -- run 'Wire Up Imported Art' first.");
                return;
            }

            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[ShaderMaterialSetup] Prefab {prefabName} has no Renderer to apply a material to.");
                return;
            }

            foreach (var renderer in renderers)
            {
                var materials = renderer.sharedMaterials;
                for (var i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }
                renderer.sharedMaterials = materials;
            }

            EditorUtility.SetDirty(prefab);
        }
    }
}

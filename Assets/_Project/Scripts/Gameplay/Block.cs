using UnityEngine;
using GoldAndGoblins.Core;

namespace GoldAndGoblins.Gameplay
{
    public class Block : MonoBehaviour
    {
        public BlockDataSO Data { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }
        public bool IsBroken { get; private set; }
        public bool IsLocked { get; set; }

        private float currentHealth;
        private float maxHealth;
        private double goldReward;
        private GameObject spawnedVisual;

        public void Setup(BlockDataSO data, int row, int col, int depth)
        {
            Data = data;
            Row = row;
            Col = col;
            IsBroken = false;
            gameObject.SetActive(true);

            maxHealth = data.HealthForDepth(depth);
            currentHealth = maxHealth;
            goldReward = data.GoldForDepth(depth);

            SpawnVisual(data);
        }

        private void SpawnVisual(BlockDataSO data)
        {
            if (spawnedVisual != null) Destroy(spawnedVisual);

            if (data != null && data.visualPrefab != null)
            {
                spawnedVisual = Instantiate(data.visualPrefab, transform);
                spawnedVisual.transform.localPosition = Vector3.zero;
                spawnedVisual.transform.localRotation = Quaternion.identity;
                spawnedVisual.transform.localScale = Vector3.one;
                // KayKit meshes are often far larger than one grid cell — without this,
                // a single block can fill the whole camera as a brown blur.
                FitLocalBounds(spawnedVisual, targetSize: 1.0f);
            }

            if (spawnedVisual == null || !HasAnyRenderer(spawnedVisual))
            {
                if (spawnedVisual != null) Destroy(spawnedVisual);
                spawnedVisual = CreateFallbackCube(data != null ? data.kind : BlockKind.Dirt);
            }
        }

        private static bool HasAnyRenderer(GameObject go) =>
            go != null && go.GetComponentInChildren<Renderer>() != null;

        // Scale the visual so its largest axis fits roughly one cell.
        private static void FitLocalBounds(GameObject go, float targetSize)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers == null || renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var max = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (max < 0.0001f) return;

            var scale = targetSize / max;
            go.transform.localScale = go.transform.localScale * scale;

            // Re-center after scaling (world bounds → local offset).
            renderers = go.GetComponentsInChildren<Renderer>();
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var worldOffset = go.transform.position - bounds.center;
            go.transform.position += worldOffset;
        }

        private GameObject CreateFallbackCube(BlockKind kind)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "FallbackVisual";
            cube.transform.SetParent(transform, false);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = Vector3.one * 0.95f;

            // Block already has the tap collider — remove the primitive's.
            var col = cube.GetComponent<Collider>();
            if (col != null) Destroy(col);

            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Shared material instance so we don't leak per-block materials forever in editor play.
                renderer.sharedMaterial = FallbackMaterialFor(kind);
            }

            return cube;
        }

        private static Material FallbackMaterialFor(BlockKind kind)
        {
            // Simple URP/Built-in compatible color materials created once per kind.
            if (fallbackMats == null) fallbackMats = new Material[8];
            var index = Mathf.Clamp((int)kind, 0, fallbackMats.Length - 1);
            if (fallbackMats[index] != null) return fallbackMats[index];

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader);
            mat.color = ColorForKind(kind);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", ColorForKind(kind));
            fallbackMats[index] = mat;
            return mat;
        }

        private static Material[] fallbackMats;

        private static Color ColorForKind(BlockKind kind)
        {
            switch (kind)
            {
                case BlockKind.Dirt: return new Color(0.55f, 0.35f, 0.18f);
                case BlockKind.Stone: return new Color(0.45f, 0.45f, 0.48f);
                case BlockKind.OreCommon: return new Color(0.72f, 0.45f, 0.2f);
                case BlockKind.OreRare: return new Color(0.85f, 0.7f, 0.2f);
                case BlockKind.KeyBlock: return new Color(0.9f, 0.8f, 0.2f);
                case BlockKind.ChestBlock: return new Color(0.55f, 0.3f, 0.1f);
                case BlockKind.GoblinBlock: return new Color(0.35f, 0.55f, 0.3f);
                default: return Color.magenta;
            }
        }

        public void ApplyDamage(float damage)
        {
            if (IsBroken || IsLocked) return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Break();
            }
        }

        private void Break()
        {
            IsBroken = true;
            EventBus.Publish(new BlockBrokenEvent(Row, Col, goldReward));
            Economy.CurrencyManager.Instance.AddGold(goldReward);
            Cleanup();
        }

        // Used by systems (goblin combat, chest-unlock) that grant their own reward
        // and just need the block cleared afterward without double-paying gold.
        public void ForceBreakNoReward()
        {
            if (IsBroken) return;
            IsBroken = true;
            EventBus.Publish(new BlockBrokenEvent(Row, Col, 0));
            Cleanup();
        }

        private void Cleanup()
        {
            if (spawnedVisual != null) Destroy(spawnedVisual);
            gameObject.SetActive(false);
        }

        public float HealthFraction => maxHealth <= 0 ? 0 : Mathf.Clamp01(currentHealth / maxHealth);
    }
}

using System.Collections;
using UnityEngine;
using GoldAndGoblins.Core;

namespace GoldAndGoblins.Gameplay
{
    // Light juice on the mining loop: camera shake when a block breaks or a goblin
    // goes down. Wired by ProjectBootstrapper; safe to leave unassigned (no-ops).
    public class FeedbackManager : GoldAndGoblins.Utils.Singleton<FeedbackManager>
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float shakeDuration = 0.12f;
        [SerializeField] private float shakeMagnitude = 0.08f;

        private Vector3 cameraRestPosition;
        private Coroutine shakeRoutine;

        protected override void Awake()
        {
            base.Awake();
            if (worldCamera == null) worldCamera = Camera.main;
            if (worldCamera != null) cameraRestPosition = worldCamera.transform.localPosition;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<BlockBrokenEvent>(OnBlockBroken);
            EventBus.Subscribe<GoblinDefeatedEvent>(OnGoblinDefeated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<BlockBrokenEvent>(OnBlockBroken);
            EventBus.Unsubscribe<GoblinDefeatedEvent>(OnGoblinDefeated);
        }

        private void OnBlockBroken(BlockBrokenEvent evt) => Shake();
        private void OnGoblinDefeated(GoblinDefeatedEvent evt) => Shake();

        public void Shake()
        {
            if (worldCamera == null || shakeDuration <= 0f || shakeMagnitude <= 0f) return;
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            var elapsed = 0f;
            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var offset = Random.insideUnitSphere * shakeMagnitude;
                offset.z = 0f;
                worldCamera.transform.localPosition = cameraRestPosition + offset;
                yield return null;
            }

            worldCamera.transform.localPosition = cameraRestPosition;
            shakeRoutine = null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameDevStudio.Events;

namespace GameDevStudio.UI
{
    /// <summary>
    /// Displays toast-style notification banners at the top of the screen.
    /// Notifications auto-dismiss after a few seconds.
    /// </summary>
    public class NotificationUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        public GameObject NotificationPrefab;
        public Transform  NotificationContainer;
        [Tooltip("Seconds before auto-dismiss.")]
        public float DisplayDuration = 4f;
        [Tooltip("Max simultaneous notifications shown.")]
        public int MaxVisible = 3;

        // ── Colors per severity ───────────────────────────────────────────────
        public Color InfoColor    = new Color(0.2f, 0.6f, 1f);
        public Color SuccessColor = new Color(0.2f, 0.8f, 0.3f);
        public Color WarningColor = new Color(1f,   0.7f, 0.1f);
        public Color DangerColor  = new Color(0.9f, 0.2f, 0.2f);

        // ── Queue ─────────────────────────────────────────────────────────────
        private readonly Queue<NotificationEvent> _queue = new();
        private int _activeCount;

        // ── Unity lifecycle ───────────────────────────────────────────────────
        private void OnEnable()
        {
            GameEventBus.Subscribe<NotificationEvent>(OnNotification);
        }

        private void OnDisable()
        {
            GameEventBus.Unsubscribe<NotificationEvent>(OnNotification);
        }

        // ── Event handler ─────────────────────────────────────────────────────
        private void OnNotification(NotificationEvent evt)
        {
            if (_activeCount < MaxVisible)
                ShowNotification(evt);
            else
                _queue.Enqueue(evt);
        }

        // ── Display ───────────────────────────────────────────────────────────
        private void ShowNotification(NotificationEvent evt)
        {
            if (NotificationPrefab == null || NotificationContainer == null) return;

            _activeCount++;
            var go = Instantiate(NotificationPrefab, NotificationContainer);

            // Set text
            var label = go.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = evt.Message;

            // Set background colour
            var img = go.GetComponent<Image>();
            if (img != null) img.color = evt.Severity switch
            {
                NotificationSeverity.Info    => InfoColor,
                NotificationSeverity.Success => SuccessColor,
                NotificationSeverity.Warning => WarningColor,
                NotificationSeverity.Danger  => DangerColor,
                _                            => InfoColor
            };

            // Close button
            var closeBtn = go.GetComponentInChildren<Button>();
            if (closeBtn != null) closeBtn.onClick.AddListener(() => DismissNotification(go));

            StartCoroutine(AutoDismiss(go, DisplayDuration));
        }

        private IEnumerator AutoDismiss(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            DismissNotification(go);
        }

        private void DismissNotification(GameObject go)
        {
            if (go == null) return;
            Destroy(go);
            _activeCount--;

            if (_queue.Count > 0)
                ShowNotification(_queue.Dequeue());
        }
    }
}

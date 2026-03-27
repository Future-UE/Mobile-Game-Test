using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameDevStudio.Core;
using GameDevStudio.Data;

namespace GameDevStudio.UI
{
    /// <summary>
    /// Presents a random/triggered event to the player with choice buttons.
    /// </summary>
    public class EventUI : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Content")]
        public TMP_Text TitleText;
        public TMP_Text DescriptionText;
        public TMP_Text CategoryText;

        [Header("Choice Buttons")]
        public Button Choice1Button;
        public Button Choice2Button;
        public Button Choice3Button;
        public Button OKButton;   // Shown for info-only events

        [Header("Choice Labels")]
        public TMP_Text Choice1Label;
        public TMP_Text Choice2Label;
        public TMP_Text Choice3Label;

        // ── State ─────────────────────────────────────────────────────────────
        private RandomEventData _currentEvent;

        // ── Public API ────────────────────────────────────────────────────────
        public void RefreshEvent()
        {
            _currentEvent = GameManager.Instance.Events.PeekNextEvent();
            if (_currentEvent == null)
            {
                UIManager.Instance.HideEventPanel();
                return;
            }

            if (TitleText       != null) TitleText.text       = _currentEvent.Title;
            if (DescriptionText != null) DescriptionText.text = _currentEvent.Description;
            if (CategoryText    != null) CategoryText.text    = _currentEvent.Category;

            bool hasChoices = _currentEvent.Choices != null && _currentEvent.Choices.Length > 0;

            if (OKButton      != null) OKButton.gameObject.SetActive(!hasChoices);
            OKButton?.onClick.RemoveAllListeners();
            OKButton?.onClick.AddListener(() => DismissEvent());

            SetupChoiceButton(Choice1Button, Choice1Label, 0, hasChoices);
            SetupChoiceButton(Choice2Button, Choice2Label, 1, hasChoices);
            SetupChoiceButton(Choice3Button, Choice3Label, 2, hasChoices);
        }

        private void SetupChoiceButton(Button btn, TMP_Text label, int idx, bool hasChoices)
        {
            if (btn == null) return;
            bool visible = hasChoices
                        && _currentEvent.Choices != null
                        && idx < _currentEvent.Choices.Length;

            btn.gameObject.SetActive(visible);
            btn.onClick.RemoveAllListeners();

            if (!visible) return;

            var choice = _currentEvent.Choices[idx];
            if (label != null) label.text = choice.Label;
            btn.onClick.AddListener(() => OnChoiceSelected(idx));
        }

        private void OnChoiceSelected(int idx)
        {
            if (_currentEvent == null) return;
            if (_currentEvent.Choices == null || idx >= _currentEvent.Choices.Length) return;

            var ev     = GameManager.Instance.Events.DequeueEvent();
            var choice = _currentEvent.Choices[idx];
            GameManager.Instance.Events.ResolveChoice(ev, choice);

            TryShowNextEvent();
        }

        private void DismissEvent()
        {
            GameManager.Instance.Events.DequeueEvent();
            TryShowNextEvent();
        }

        private void TryShowNextEvent()
        {
            if (GameManager.Instance.Events.HasPendingEvent)
                RefreshEvent();
            else
                UIManager.Instance.HideEventPanel();
        }
    }
}

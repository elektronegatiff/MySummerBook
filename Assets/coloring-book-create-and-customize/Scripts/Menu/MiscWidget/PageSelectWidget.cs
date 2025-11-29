using System;
using HootyBird.ColoringBook.Repositories;
using HootyBird.ColoringBook.Serialized;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.ColoringBook.Menu
{
    /// <summary>
    /// Tek bir sayfa kartý UI elementi.
    /// Kilitli/Açýk durumu ve ilerleme gösterir.
    /// </summary>
    public class PageSelectWidget : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RawImage thumbnailImage;
        [SerializeField] private Image progressFill;
        [SerializeField] private GameObject lockOverlay;
        [SerializeField] private GameObject completedCheckmark;
        [SerializeField] private TMPro.TextMeshProUGUI pageNumberText;
        [SerializeField] private Button selectButton;

        [Header("Visual Settings")]
        [SerializeField] private Color lockedTintColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color unlockedTintColor = Color.white;
        [SerializeField] private Color completedProgressColor = Color.green;
        [SerializeField] private Color inProgressColor = Color.yellow;

        private int pageIndex;
        private bool isUnlocked;
        private bool isCompleted;

        public event Action<int> OnPageSelected;

        private void Awake()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectButtonClicked);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(OnSelectButtonClicked);
        }

        public void Initialize(int index, ColoringBookDataBase pageData)
        {
            pageIndex = index;

            if (thumbnailImage != null && pageData?.Texture != null)
                thumbnailImage.texture = pageData.Texture;

            if (pageNumberText != null)
                pageNumberText.text = (index + 1).ToString();

            RefreshState();
        }

        public void RefreshState()
        {
            var progressManager = BookProgressManager.Instance;
            if (progressManager == null) return;

            isUnlocked = progressManager.IsPageUnlocked(pageIndex);
            isCompleted = progressManager.IsPageCompleted(pageIndex);
            float progress = progressManager.GetPageProgress(pageIndex);

            UpdateVisuals(isUnlocked, isCompleted, progress);
        }

        private void UpdateVisuals(bool unlocked, bool completed, float progress)
        {
            if (lockOverlay != null)
                lockOverlay.SetActive(!unlocked);

            if (completedCheckmark != null)
                completedCheckmark.SetActive(completed);

            if (thumbnailImage != null)
                thumbnailImage.color = unlocked ? unlockedTintColor : lockedTintColor;

            if (progressFill != null)
            {
                progressFill.fillAmount = progress;
                progressFill.color = completed ? completedProgressColor : inProgressColor;
                progressFill.gameObject.SetActive(unlocked && !completed);
            }

            if (selectButton != null)
                selectButton.interactable = unlocked;
        }

        private void OnSelectButtonClicked()
        {
            if (!isUnlocked)
            {
                Debug.Log($"Sayfa {pageIndex + 1} kilitli!");
                return;
            }

            OnPageSelected?.Invoke(pageIndex);
        }
    }
}
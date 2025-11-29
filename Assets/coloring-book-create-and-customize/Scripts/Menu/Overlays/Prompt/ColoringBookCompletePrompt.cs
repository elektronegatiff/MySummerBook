using HootyBird.ColoringBook.Services;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static System.Net.Mime.MediaTypeNames;

namespace HootyBird.ColoringBook.Menu.Overlays
{
    public class ColoringBookCompletePrompt : PromptOverlay
    {
        [Header("Next Page")]
        [SerializeField]
        private Button nextPageButton;
        [SerializeField]
        private TMP_Text nextPageButtonText;

        private System.Action onNextPageClicked;

        protected override void Awake()
        {
            base.Awake();

            if (nextPageButton != null)
            {
                nextPageButton.onClick.AddListener(OnNextPageButtonClicked);
            }
        }

        public override void Open()
        {
            base.Open();

            // Next page butonunu göster/gizle
            if (nextPageButton != null)
            {
                nextPageButton.gameObject.SetActive(onNextPageClicked != null);
            }
        }

        /// <summary>
        /// Next Page callback'ini ayarla. Null ise buton gizlenir.
        /// </summary>
        public void SetNextPageCallback(System.Action onNextPage)
        {
            onNextPageClicked = onNextPage;

            if (nextPageButton != null)
            {
                nextPageButton.gameObject.SetActive(onNextPage != null);
            }
        }

        private void OnNextPageButtonClicked()
        {
            AudioService.Instance.PlaySfx("menu-click", .4f);
            onNextPageClicked?.Invoke();
        }
    }
}
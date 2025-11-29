using System.Collections.Generic;
using HootyBird.ColoringBook.Repositories;
using HootyBird.ColoringBook.Serialized;
using UnityEngine;

namespace HootyBird.ColoringBook.Menu
{
    /// <summary>
    /// Sayfa seçim overlay'i.
    /// Tüm kitap sayfalarýný grid þeklinde gösterir.
    /// </summary>
    public class PageSelectOverlay : MenuOverlay
    {
        [Header("Page Select Settings")]
        [SerializeField] private PageSelectWidget pageWidgetPrefab;
        [SerializeField] private Transform pagesContainer;
        [SerializeField] private Category bookCategory;

        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI titleText;
        [SerializeField] private TMPro.TextMeshProUGUI progressText;

        private List<PageSelectWidget> pageWidgets = new List<PageSelectWidget>();
        private BookProgressManager progressManager;

        protected override void Awake()
        {
            base.Awake();
            progressManager = BookProgressManager.Instance;
        }

        private void OnEnable()
        {
            if (progressManager != null)
            {
                progressManager.OnPageUnlocked += OnPageStateChanged;
                progressManager.OnPageCompleted += OnPageStateChanged;
            }
        }

        private void OnDisable()
        {
            if (progressManager != null)
            {
                progressManager.OnPageUnlocked -= OnPageStateChanged;
                progressManager.OnPageCompleted -= OnPageStateChanged;
            }
        }

        public override void Open()
        {
            base.Open();

            InitializePages();
            RefreshAllPages();
            UpdateProgressText();
        }

        private void InitializePages()
        {
            if (pageWidgets.Count > 0) return;

            if (bookCategory?.ColoringBooks == null || pageWidgetPrefab == null || pagesContainer == null)
                return;

            for (int i = 0; i < bookCategory.ColoringBooks.Count; i++)
            {
                var pageData = bookCategory.ColoringBooks[i];

                var widget = Instantiate(pageWidgetPrefab, pagesContainer);
                widget.Initialize(i, pageData);
                widget.OnPageSelected += OnPageSelected;

                pageWidgets.Add(widget);
            }
        }

        private void RefreshAllPages()
        {
            foreach (var widget in pageWidgets)
                widget.RefreshState();
        }

        private void UpdateProgressText()
        {
            if (progressText == null || progressManager == null) return;

            int completedCount = 0;
            int totalPages = progressManager.TotalPages;

            for (int i = 0; i < totalPages; i++)
            {
                if (progressManager.IsPageCompleted(i))
                    completedCount++;
            }

            progressText.text = $"Ýlerleme: {completedCount} / {totalPages}";
        }

        private void OnPageSelected(int pageIndex)
        {
            var transitionController = FindObjectOfType<PageTransitionController>();
            if (transitionController != null)
                transitionController.LoadPage(pageIndex);

            Close();
        }

        private void OnPageStateChanged(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < pageWidgets.Count)
                pageWidgets[pageIndex].RefreshState();

            UpdateProgressText();
        }
    }
}
using HootyBird.ColoringBook.Gameplay;
using HootyBird.ColoringBook.Menu.Widgets;
using HootyBird.ColoringBook.Repositories;
using HootyBird.ColoringBook.Services;
using HootyBird.ColoringBook.Services.SaveLoad;
using HootyBird.ColoringBook.Tools;
using UnityEngine;

namespace HootyBird.ColoringBook.Menu.Overlays
{
    public class GameplayOverlay : MenuOverlay
    {
        [SerializeField]
        private ColoringBookView coloringBookView;
        [SerializeField]
        private ProgressBarWidget progressBar;
        [SerializeField]
        private BrushSizeWidget brushSizeWidget;
        [SerializeField]
        private ZoomController zoomController; // YENÝ

        private bool complete;

        protected override void Awake()
        {
            base.Awake();

            coloringBookView.OnCompletePercentUpdated += OnColoringBookCompletePercentUpdate;
            coloringBookView.OnDataSet += OnColoringBookDataSet;

            brushSizeWidget.OnValueChanged += OnBrushSizeChanged;
            brushSizeWidget.SetSliderMinMax(
                Settings.InternalAppSettings.MinBrushSize,
                Settings.InternalAppSettings.MaxBrushSize);
        }

        public void OnApplicationQuit()
        {
            if (coloringBookView.ColoringBookData != null)
            {
                SaveGame();
            }
        }

        public void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (coloringBookView.ColoringBookData != null)
                {
                    SaveGame();
                }
            }
        }

        private void OnBrushSizeChanged(float size)
        {
            TextureDrawService.Instance.BrushSize = size;
        }

        private void OnColoringBookDataSet()
        {
            complete = false;
            progressBar.SetValue(0f);

            // Yeni sayfa yüklendiðinde zoom'u resetle
            ResetZoom();

            switch (coloringBookView.ColoringStyle)
            {
                case ColoringBookView.RegionColoringStyle.FreeDrawing:
                    brushSizeWidget.gameObject.SetActive(true);
                    brushSizeWidget.SetSliderValue(Settings.InternalAppSettings.DefaultBrushSize);
                    break;

                default:
                    brushSizeWidget.gameObject.SetActive(false);
                    break;
            }
        }

        public override void OnBack()
        {
            // Zoom'u resetle
            ResetZoom();

            SaveGame();

            MainMenuController mainMenuContoller =
                MenuController.GetMenuController<MainMenuController>(Settings.InternalAppSettings.MainMenuControllerName);

            mainMenuContoller.SetActive(true);
        }

        private void SaveGame()
        {
            if (Settings.InternalAppSettings.SaveProgress)
            {
                if (coloringBookView.CompletePercent < 1f)
                {
                    coloringBookView.StopColoringBookUpdates();
                    SaveLoadService.SaveColoringBook(coloringBookView);
                }
            }
        }

        private void OnColoringBookCompletePercentUpdate(float percentValue)
        {
            if (complete)
            {
                return;
            }

            progressBar.SetValue(percentValue);

            UpdateBookProgress(percentValue);

            if (percentValue == 1f)
            {
                complete = true;

                CompleteCurrentPage();

                ShowCompletionPrompt();

                SaveLoadService.ClearSavedColoringBook(coloringBookView);
            }
        }

        private void UpdateBookProgress(float progress)
        {
            if (BookProgressManager.Instance == null) return;
            if (coloringBookView.ColoringBookData == null) return;

            string pageId = coloringBookView.ColoringBookData.Name;
            BookProgressManager.Instance.UpdatePageProgress(pageId, progress);
        }

        private void CompleteCurrentPage()
        {
            if (BookProgressManager.Instance == null) return;
            if (coloringBookView.ColoringBookData == null) return;

            string pageId = coloringBookView.ColoringBookData.Name;
            BookProgressManager.Instance.CompletePage(pageId);
        }

        private void ShowCompletionPrompt()
        {
            ColoringBookCompletePrompt prompt = MenuController.GetOverlay<ColoringBookCompletePrompt>();

            string currentPageId = coloringBookView.ColoringBookData.Name;
            bool hasNextPage = CheckHasNextPage();

            prompt.CloseOnReject();
            prompt.SetButtonsEvents(null, () => {
                GoToMainMenu();
            });

            prompt.SetNextPageCallback(hasNextPage ? () => {
                LoadNextPage();
            }
            : null);

            MenuController.OpenOverlay(prompt);
        }

        private bool CheckHasNextPage()
        {
            if (BookProgressManager.Instance == null) return false;
            if (coloringBookView.ColoringBookData == null) return false;

            string currentPageId = coloringBookView.ColoringBookData.Name;
            int currentIndex = BookProgressManager.Instance.GetPageIndex(currentPageId);
            int totalPages = BookProgressManager.Instance.TotalPages;

            return currentIndex + 1 < totalPages;
        }

        private void LoadNextPage()
        {
            if (BookProgressManager.Instance == null) return;
            if (coloringBookView.ColoringBookData == null) return;

            string currentPageId = coloringBookView.ColoringBookData.Name;
            int currentIndex = BookProgressManager.Instance.GetPageIndex(currentPageId);
            int nextIndex = currentIndex + 1;

            if (nextIndex < BookProgressManager.Instance.TotalPages)
            {
                var nextPageData = BookProgressManager.Instance.GetPageData(nextIndex);

                if (nextPageData != null)
                {
                    // Zoom'u resetle
                    ResetZoom();

                    // Mevcut sayfayý temizle
                    coloringBookView.ReleaseCurrentColoringBookAssets();

                    // Yeni sayfayý yükle
                    nextPageData.InitializeAssets();
                    coloringBookView.LoadColoringBook(nextPageData);

                    complete = false;

                    MenuController.GoBack();

                    Debug.Log($"[GameplayOverlay] Sonraki sayfa yüklendi: {nextPageData.Name}");
                }
            }
        }

        private void GoToMainMenu()
        {
            // Zoom'u resetle
            ResetZoom();

            MainMenuController mainMenuContoller =
                MenuController.GetMenuController<MainMenuController>(Settings.InternalAppSettings.MainMenuControllerName);

            mainMenuContoller.SetActive(true);
        }

        private void ResetZoom()
        {
            if (zoomController != null)
            {
                zoomController.ResetZoom();
            }
        }
    }
}
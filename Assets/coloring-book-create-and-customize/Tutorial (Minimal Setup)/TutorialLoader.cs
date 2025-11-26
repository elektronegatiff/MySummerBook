using HootyBird.ColoringBook.Gameplay;
using HootyBird.ColoringBook.Menu.Widgets;
using HootyBird.ColoringBook.Serialized;
using HootyBird.ColoringBook.Services.SaveLoad;
using HootyBird.ColoringBook.Tools;
using UnityEngine;
using static HootyBird.ColoringBook.Gameplay.ColoringBookView;
using HootyBird.ColoringBook.Services;
using System.Collections;

namespace HootyBird.ColoringBook.Tutorial
{
    public class TutorialLoader : MonoBehaviour
    {
        [Header("Coloring Book Settings")]
        [SerializeField]
        private ColoringBookView coloringBookView;
        [SerializeField]
        private ImagesColoringBookData coloringBookData;
        [SerializeField]
        private RegionColoringStyle coloringStyle = RegionColoringStyle.FreeDrawing;
        [SerializeField]
        private bool tryLoadSavedData = true;

        [Space(10f)]
        [Header("UI controls")]
        [SerializeField]
        private ProgressBarWidget progressBar;
        [SerializeField]
        private BrushSizeWidget brushSizeWidget;

        private bool complete;

        protected void Awake()
        {
            coloringBookView.OnCompletePercentUpdated += OnColoringBookCompletePercentUpdate;
            coloringBookView.OnDataSet += OnColoringBookDataSet;

            brushSizeWidget.OnValueChanged += OnBrushSizeChanged;
            brushSizeWidget.SetSliderMinMax(
                Settings.InternalAppSettings.MinBrushSize,
                Settings.InternalAppSettings.MaxBrushSize);
        }

        private IEnumerator Start()
        {
            yield return null; 

            coloringBookData.InitializeAssets();

            coloringBookView.ColoringStyle = coloringStyle;
            coloringBookView.LoadColoringBook(coloringBookData);

            if (tryLoadSavedData)
            {
                SaveLoadService.LoadColoringBookFromSavedData(coloringBookView);
            }
        }

        public void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (tryLoadSavedData && coloringBookView.CompletePercent < 1f && coloringBookView.ColoringBookData != null)
                {
                    coloringBookView.StopColoringBookUpdates();
                    SaveLoadService.SaveColoringBook(coloringBookView);
                }
            }
        }

        public void OnApplicationQuit()
        {
            if (tryLoadSavedData && coloringBookView.CompletePercent < 1f && coloringBookView.ColoringBookData != null)
            {
                SaveLoadService.SaveColoringBook(coloringBookView);
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

            switch (coloringBookView.ColoringStyle)
            {
                case RegionColoringStyle.FreeDrawing:
                    brushSizeWidget.gameObject.SetActive(true);
                    brushSizeWidget.SetSliderValue(Settings.InternalAppSettings.DefaultBrushSize);

                    break;

                default:
                    brushSizeWidget.gameObject.SetActive(false);

                    break;
            }
        }

        private void OnColoringBookCompletePercentUpdate(float percentValue)
        {
            if (complete)
            {
                return;
            }

            progressBar.SetValue(percentValue);

            if (percentValue == 1f)
            {
                complete = true;

                // Clear saved data.
                SaveLoadService.ClearSavedColoringBook(coloringBookView);
            }
        }
    }
}

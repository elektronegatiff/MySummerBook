using HootyBird.ColoringBook.Data;
using HootyBird.ColoringBook.Menu.Overlays;
using HootyBird.ColoringBook.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.ColoringBook.Menu.Widgets
{
    [RequireComponent(typeof(Button))]
    public class ColoringBookWidget : MonoBehaviour
    {
        [SerializeField]
        private RawImage graphics;
        [SerializeField]
        private TMP_Text nameLabel;

        [Header("Lock System")]
        [SerializeField]
        private GameObject lockOverlay;
        [SerializeField]
        private GameObject completedIcon;

        [Header("Progress Bar")]
        [SerializeField]
        private GameObject progressBarObject;  // "Progress Bar - Green" ana objesi
        [SerializeField]
        private RectTransform progressBarFill; // "Bar" objesi (RectTransform scale ile)
        [SerializeField]
        private TMP_Text progressText;         // "Text" - yüzde göstermek için

        [Header("Visual Settings")]
        [SerializeField]
        private Color lockedTintColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField]
        private Color unlockedTintColor = Color.white;

        private AspectRatioFitter aspectRatioFitter;
        private Button button;
        private MenuOverlay menuOverlay;
        private IColoringBookData coloringBookData;
        private int pageIndex;
        private bool isUnlocked = true;

        private void Awake()
        {
            menuOverlay = GetComponentInParent<MenuOverlay>();
            aspectRatioFitter = graphics.GetComponent<AspectRatioFitter>();
            button = GetComponent<Button>();
            button.onClick.AddListener(OpenBookButtonClick);
        }

        public void SetData(IColoringBookData coloringBookData)
        {
            SetData(coloringBookData, 0);
        }

        public void SetData(IColoringBookData coloringBookData, int index)
        {
            this.coloringBookData = coloringBookData;
            this.pageIndex = index;

            nameLabel.text = coloringBookData.Name;
            graphics.texture = coloringBookData.Texture;
            aspectRatioFitter.aspectRatio = (float)coloringBookData.Texture.width / coloringBookData.Texture.height;

            RefreshState();
        }

        public void RefreshState()
        {
            if (coloringBookData == null) return;

            var progressManager = BookProgressManager.Instance;

            if (progressManager == null)
            {
                SetVisualState(true, false, 0f);
                return;
            }

            isUnlocked = progressManager.IsPageUnlocked(coloringBookData.Name);
            bool isCompleted = progressManager.IsPageCompleted(coloringBookData.Name);
            float progress = progressManager.GetPageProgress(coloringBookData.Name);

            SetVisualState(isUnlocked, isCompleted, progress);
        }

        private void SetVisualState(bool unlocked, bool completed, float progress)
        {
            isUnlocked = unlocked;

            // Lock overlay - sadece kilitliyse göster
            if (lockOverlay != null)
            {
                lockOverlay.SetActive(!unlocked);
            }

            // Completed icon - sadece tamamlandýysa göster
            if (completedIcon != null)
            {
                completedIcon.SetActive(completed);
            }

            // Progress bar:
            // - Kilitliyse: GÝZLE
            // - Tamamlandýysa: GÝZLE  
            // - Açýk ve baþlanmýþ ama bitmemiþse (0 < progress < 1): GÖSTER
            bool showProgressBar = unlocked && !completed && progress > 0f && progress < 1f;

            if (progressBarObject != null)
            {
                progressBarObject.SetActive(showProgressBar);
            }

            // Progress fill - Scale X ile (0-1 arasý)
            if (progressBarFill != null)
            {
                progressBarFill.localScale = new Vector3(progress, 1f, 1f);
            }

            // Progress text (opsiyonel - "50%" gibi)
            if (progressText != null)
            {
                int percent = Mathf.RoundToInt(progress * 100f);
                progressText.text = $"{percent}%";
            }

            // Thumbnail tint
            if (graphics != null)
            {
                graphics.color = unlocked ? unlockedTintColor : lockedTintColor;
            }

            // Button interactable
            if (button != null)
            {
                button.interactable = unlocked;
            }
        }

        private void OpenBookButtonClick()
        {
            if (!isUnlocked)
            {
                Debug.Log($"Sayfa kilitli: {coloringBookData.Name}");
                return;
            }

            ColoringStyleSelectPrompt styleSelectPrompt = menuOverlay.MenuController.GetOverlay<ColoringStyleSelectPrompt>();
            styleSelectPrompt.SetColoringBookToLoad(coloringBookData);
            menuOverlay.MenuController.OpenOverlay(styleSelectPrompt);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using HootyBird.ColoringBook.Gameplay;
using HootyBird.ColoringBook.Services;


namespace HootyBird.ColoringBook.Menu.Widgets
{
    public class BrushPanel : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private BrushCategoryTab categoryTabPrefab;
        [SerializeField] private BrushWidget brushWidgetPrefab;

        [Header("Containers")]
        [SerializeField] private Transform categoryTabsParent;
        [SerializeField] private Transform brushesParent;

        [Header("Categories")]
        [SerializeField] private List<BrushCategory> categories;

        private List<BrushCategoryTab> categoryTabs = new List<BrushCategoryTab>();
        private List<BrushWidget> brushWidgets = new List<BrushWidget>();
        private BrushWidget currentBrush;

        private void Start()
        {
            InitializeCategories();
            if (categories.Count > 0)
            {
                SelectCategory(0);
            }
        }

        private void InitializeCategories()
        {
            // Clear existing tabs
            foreach (Transform child in categoryTabsParent)
            {
                Destroy(child.gameObject);
            }
            categoryTabs.Clear();

            // Create category tabs
            for (int i = 0; i < categories.Count; i++)
            {
                var tab = Instantiate(categoryTabPrefab, categoryTabsParent);
                tab.SetData(categories[i].categoryName, i);
                tab.OnTabClicked += SelectCategory;
                categoryTabs.Add(tab);
            }
        }

        private void SelectCategory(int index)
        {
            // Update tab visuals
            for (int i = 0; i < categoryTabs.Count; i++)
            {
                categoryTabs[i].SetSelected(i == index);
            }

            // Clear existing brushes
            foreach (Transform child in brushesParent)
            {
                Destroy(child.gameObject);
            }
            brushWidgets.Clear();

            // Create brush widgets
            foreach (var brushData in categories[index].brushes)
            {
                var widget = Instantiate(brushWidgetPrefab, brushesParent);
                widget.SetData(brushData);
                widget.OnClicked += OnBrushWidgetClicked;
                brushWidgets.Add(widget);
            }

            // Select first brush
            if (brushWidgets.Count > 0)
            {
                OnBrushWidgetClicked(brushWidgets[0]);
            }
        }

        private void OnBrushWidgetClicked(BrushWidget widget)
        {
            if (currentBrush != null)
            {
                currentBrush.SetSelected(false);
            }

            currentBrush = widget;
            currentBrush.SetSelected(true);

            // TextureDrawService'e bildir
            TextureDrawService.Instance.SetBrush(widget.BrushData);

            // BrushManager'a bildir
            if (BrushManager.Instance != null)
            {
                BrushManager.Instance.SetBrush(widget.BrushData);
            }
        }
    }

    [Serializable]
    public class BrushCategory
    {
        public string categoryName;
        public List<BrushData> brushes;
    }
}
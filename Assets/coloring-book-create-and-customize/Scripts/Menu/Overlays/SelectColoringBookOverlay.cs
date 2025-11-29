using HootyBird.ColoringBook.Menu.Widgets;
using HootyBird.ColoringBook.Repositories;
using HootyBird.ColoringBook.Serialized;
using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.ColoringBook.Menu.Overlays
{
    public class SelectColoringBookOverlay : MenuOverlay
    {
        [SerializeField]
        private ColoringBookWidget coloringWidgetPrefab;
        [SerializeField]
        private RectTransform coloringWidgetsParent;

        private List<ColoringBookWidget> pageWidgets = new List<ColoringBookWidget>();
        private bool pagesInitialized = false;

        protected override void Start()
        {
            base.Start();
        }

        public override void Open()
        {
            base.Open();

            if (!pagesInitialized)
            {
                InitPages();
                pagesInitialized = true;
            }
            else
            {
                RefreshPages();
            }
        }

        private void InitPages()
        {
            var coloringBooks = DataHandler.Instance.DefaultColoringBooks.ColoringBooks;

            for (int i = 0; i < coloringBooks.Count; i++)
            {
                ColoringBookDataBase bookData = coloringBooks[i];

                if (BookProgressManager.Instance != null)
                {
                    BookProgressManager.Instance.RegisterPage(bookData.Name, i);
                }

                ColoringBookWidget widget = Instantiate(coloringWidgetPrefab, coloringWidgetsParent);
                widget.SetData(bookData, i);
                pageWidgets.Add(widget);
            }
        }

        private void RefreshPages()
        {
            foreach (var widget in pageWidgets)
            {
                widget.RefreshState();
            }
        }
    }
}
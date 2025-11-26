using HootyBird.ColoringBook.Gameplay;
using HootyBird.ColoringBook.Services;
using HootyBird.ColoringBook.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HootyBird.ColoringBook.Menu.Widgets
{
    public class ColorsPanel : MonoBehaviour
    {
        [SerializeField]
        private ColorWidget colorWidgetPrefab;
        [SerializeField]
        private ColoringBookView coloringBookView;
        [SerializeField]
        private Transform colorWidgetsParent;
        [SerializeField]
        private OnColorFilledAction colorFilledAction = OnColorFilledAction.Checkmark;

        private List<ColorWidget> colorWidgets;
        private ColorWidget current;

        public void Awake()
        {
            if (!coloringBookView)
            {
                gameObject.SetActive(false);
                return;
            }

            colorWidgets = new List<ColorWidget>();
            coloringBookView.OnDataSet += OnColoringBookDataSet;
            coloringBookView.OnColorFilled += OnColorFilled;
        }
        // Çocuklar için canlý ve eðlenceli renkler
        private static readonly Color[] fixedColors = new Color[]
        {
    new Color(1f, 0.2f, 0.2f),       // Parlak Kýrmýzý
    new Color(1f, 0.5f, 0f),         // Turuncu
    new Color(1f, 0.9f, 0.1f),       // Parlak Sarý
    new Color(0.5f, 1f, 0.3f),       // Lime Yeþil
    new Color(0.2f, 0.8f, 0.4f),     // Canlý Yeþil
    new Color(0.3f, 0.9f, 0.9f),     // Turkuaz
    new Color(0.3f, 0.6f, 1f),       // Gökyüzü Mavisi
    new Color(0.5f, 0.3f, 1f),       // Mor
    new Color(0.9f, 0.4f, 0.9f),     // Pembe-Mor
    new Color(1f, 0.4f, 0.7f),       // Þeker Pembesi
    new Color(1f, 0.6f, 0.4f),       // Þeftali
    new Color(0.6f, 0.4f, 0.2f),     // Çikolata
    new Color(0.4f, 0.8f, 1f),       // Bebek Mavisi
    new Color(0.9f, 0.7f, 1f),       // Lavanta
    new Color(1f, 1f, 1f),           // Beyaz
    new Color(0.2f, 0.2f, 0.2f),     // Siyah
        };
        private void OnColoringBookDataSet()
        {
            // Reset current one.
            if (current)
            {
                current.SetSelectedState(false);
                current = null;
            }

            IEnumerable<Color> colors = fixedColors;

            // Remove if needed.
            while (colorWidgets.Count > colors.Count())
            {
                Destroy(colorWidgets[0].gameObject);
                colorWidgets.RemoveAt(0);
            }

            // Reset and add new ones.
            for (int index = 0; index < colors.Count(); index++)
            {
                if (index > colorWidgets.Count - 1)
                {
                    colorWidgets.Add(AddNewColorWidget());
                }

                colorWidgets[index].OnReset();
                colorWidgets[index].SetColor(colors.ElementAt(index));
                //colorWidgets[index].SetText($"{index + 1}");
                colorWidgets[index].SetText("");
            }

            // Shuffle.
            if (SettingsService.GetSettingValue(SettingsOptions.ShuffleColors))
            {
                foreach (ColorWidget colorWidget in colorWidgets.OrderBy(value => Random.value))
                {
                    colorWidget.transform.SetAsFirstSibling();
                }
            }
        }

        private ColorWidget AddNewColorWidget()
        {
            ColorWidget colorWidget = Instantiate(colorWidgetPrefab, colorWidgetsParent);
            colorWidget.OnClicked += OnColorWidgetClicked;

            return colorWidget;
        }

        private void OnColorFilled(Color color)
        {
            switch (colorFilledAction)
            {
                case OnColorFilledAction.Hide:
                    foreach (ColorWidget widget in colorWidgets)
                    {
                        if (widget.Color.Compare(color))
                        {
                            widget.gameObject.SetActive(false);
                        }
                    }

                    coloringBookView.SetFillColor(Color.clear);

                    break;

                case OnColorFilledAction.Checkmark:
                    foreach (ColorWidget widget in colorWidgets)
                    {
                        if (widget.Color.Compare(color))
                        {
                            widget.ShowCompleteIcon();
                        }
                    }

                    break;
            }
        }

        private void OnColorWidgetClicked(ColorWidget widget)
        {
            // If clicked color is a current one, return.
            if (current && current == widget)
            {
                return;
            }

            if (current)
            {
                current.SetSelectedState(false);
            }

            current = widget;
            current.SetSelectedState(true);
            coloringBookView.SetFillColor(current.Color);
        }

        /// <summary>
        /// Action invoked when certain color is filled.
        /// </summary>
        public enum OnColorFilledAction
        {
            /// <summary>
            /// Do nothing.
            /// </summary>
            None,

            /// <summary>
            /// Fade out animation.
            /// </summary>
            Hide,

            /// <summary>
            /// Show "complete" checkmark.
            /// </summary>
            Checkmark,
        }
    }
}

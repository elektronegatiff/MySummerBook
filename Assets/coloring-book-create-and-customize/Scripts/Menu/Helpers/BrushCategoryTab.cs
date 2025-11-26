using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HootyBird.ColoringBook.Menu.Widgets
{
    public class BrushCategoryTab : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image background;
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.8f, 1f);
        [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 0.8f);

        private Button button;
        private int index;

        public Action<int> OnTabClicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnTabClicked?.Invoke(index));
            }
        }

        public void SetData(string name, int tabIndex)
        {
            if (nameText != null)
            {
                nameText.text = name;
            }
            index = tabIndex;
        }

        public void SetSelected(bool selected)
        {
            if (background != null)
            {
                background.color = selected ? selectedColor : normalColor;
            }
        }
    }
}
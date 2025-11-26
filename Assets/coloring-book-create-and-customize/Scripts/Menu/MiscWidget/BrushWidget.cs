using HootyBird.ColoringBook.Gameplay;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.ColoringBook.Menu.Widgets
{
    public class BrushWidget : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image borderImage;
        [SerializeField] private Color selectedColor = Color.yellow;

        private Button button;
        private Color defaultBorderColor;

        public BrushData BrushData { get; private set; }
        public Action<BrushWidget> OnClicked;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnClicked?.Invoke(this));
            }
            if (borderImage != null)
            {
                defaultBorderColor = borderImage.color;
            }
        }

        public void SetData(BrushData data)
        {
            BrushData = data;
            if (data.icon != null && iconImage != null)
            {
                iconImage.sprite = data.icon;
            }
        }

        public void SetSelected(bool selected)
        {
            if (borderImage != null)
            {
                borderImage.color = selected ? selectedColor : defaultBorderColor;
            }
        }
    }
}
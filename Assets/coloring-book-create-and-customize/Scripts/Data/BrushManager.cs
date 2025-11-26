using UnityEngine;
using HootyBird.ColoringBook.Gameplay;

namespace HootyBird.ColoringBook.Services
{
    public class BrushManager : MonoBehaviour
    {
        public static BrushManager Instance { get; private set; }
        public Material GetRegionMaterial() => currentBrush?.regionMaterial;
        private BrushData currentBrush;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetBrush(BrushData brush)
        {
            currentBrush = brush;
        }

        public Material GetCurrentRegionMaterial()
        {
            if (currentBrush != null)
                return currentBrush.regionMaterial;
            return null;
        }
    }
}
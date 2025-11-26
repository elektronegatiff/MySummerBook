using UnityEngine;

namespace HootyBird.ColoringBook.Gameplay
{
    [CreateAssetMenu(fileName = "BrushData", menuName = "ColoringBook/Brush Data")]
    public class BrushData : ScriptableObject
    {
        [Header("Basic Settings")]
        public string brushName;
        public BrushType brushType;
        public Sprite icon;

        [Header("Brush Settings")]
        public Material brushMaterial;
        [Range(0.1f, 2f)]
        public float sizeMultiplier = 1f;

        [Header("Region Display")]
        [Tooltip("Boyanan bölgenin görünümü için material (null = normal)")]
        public Material regionMaterial;
    }
}
using HootyBird.ColoringBook.Gameplay;
using System;
using UnityEngine;

namespace HootyBird.ColoringBook.Services.SaveLoad
{
    [Serializable]
    public class SavedRegionData
    {
        [SerializeField]
        private string textureName;
        [SerializeField]
        private int filledPixelCount;
        [SerializeField]
        private string regionMaterialName;
        [SerializeField]
        private Color fillColor;  // YENÝ

        public string TextureName => textureName;
        public int FilledPixelCount => filledPixelCount;
        public string RegionMaterialName => regionMaterialName;
        public Color FillColor => fillColor;  // YENÝ

        public SavedRegionData(RegionDataView view)
        {
            textureName = view.RegionData.Texture.name;
            filledPixelCount = view.FilledPixelsCount;
            fillColor = view.FillColor;  // YENÝ

            if (view.RawImage != null && view.RawImage.material != null)
            {
                regionMaterialName = view.RawImage.material.name;
            }
            else
            {
                regionMaterialName = "";
            }
        }
    }
}
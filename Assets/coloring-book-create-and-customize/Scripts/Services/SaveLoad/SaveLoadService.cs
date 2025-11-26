using HootyBird.ColoringBook.Gameplay;
using HootyBird.ColoringBook.Tools;
using System.IO;
using UnityEngine;

namespace HootyBird.ColoringBook.Services.SaveLoad
{
    public class SaveLoadService
    {
        public static void SaveColoringBook(ColoringBookView view, bool clearBeforeSave = true)
        {
            if (view == null)
            {
                return;
            }

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            SavedColoringBookData dataToSave = new SavedColoringBookData(view);
            if (dataToSave.SavedRegionData.Length == 0)
            {
                Debug.Log($"No regions data to save on given ColoringBookView");
                return;
            }

            string path = Path.Combine(Application.persistentDataPath, Settings.InternalAppSettings.SaveFolderPath);
            CheckFolder(path);

            if (clearBeforeSave)
            {
                ClearSavedColoringBook(view);
            }

            path = GetColoringBookToPath(path, view);
            CheckFolder(path);

            foreach (SavedRegionData regionData in dataToSave.SavedRegionData)
            {
                RegionDataView regionView = view.Regions.Find(region => region.RegionData.Texture.name == regionData.TextureName);
                if (regionView == null)
                {
                    continue;
                }

                if (regionView.Colored)
                {
                    continue;
                }

                Texture2D maskTexture = GetTextureFromMask(regionView.MaskTexture);
                File.WriteAllBytes(
                    Path.Combine(path, $"{regionView.RegionData.Texture.name}.png"),
                    maskTexture.EncodeToPNG());

                Object.Destroy(maskTexture);
            }

            File.WriteAllText(Path.Combine(path, "data.json"), JsonUtility.ToJson(dataToSave));

            Debug.Log($"Saved coloring book progress {view.ColoringBookData.Name} to {path} in {sw.ElapsedMilliseconds}ms");

            Texture2D GetTextureFromMask(RenderTexture renderTexture)
            {
                RenderTexture prev = RenderTexture.active;
                RenderTexture.active = renderTexture;
                Texture2D texture = new Texture2D(
                    renderTexture.width,
                    renderTexture.height,
                    TextureFormat.R8,
                    false);
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                texture.Apply();
                RenderTexture.active = prev;

                return texture;
            }
        }

        public static void LoadColoringBookFromSavedData(ColoringBookView view)
        {
            if (view == null)
            {
                return;
            }

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            string path = Path.Combine(Application.persistentDataPath, Settings.InternalAppSettings.SaveFolderPath);
            if (!Directory.Exists(path))
            {
                return;
            }

            path = GetColoringBookToPath(path, view);
            if (!Directory.Exists(path))
            {
                return;
            }

            SavedColoringBookData savedData = JsonUtility.FromJson<SavedColoringBookData>(
                File.ReadAllText(Path.Combine(path, "data.json")));

            foreach (SavedRegionData regionData in savedData.SavedRegionData)
            {
                RegionDataView regionView = view.Regions.Find(region => region.RegionData.Texture.name == regionData.TextureName);

                if (regionView == null)
                {
                    Debug.Log($"Issue with save file, failed to find region {regionData.TextureName}");
                    continue;
                }

                // RENK YÜKLE
                regionView.SetFillColor(regionData.FillColor);

                regionView.UpdateFilledPixels(regionData.FilledPixelCount);

                if (regionView.Colored)
                {
                    regionView.SetMaskColor(Color.white);
                }
                else
                {
                    Texture2D maskTexture = new Texture2D(
                        regionView.MaskTexture.width,
                        regionView.MaskTexture.height,
                        TextureFormat.R8,
                        false);

                    string maskTexturePath = Path.Combine(path, $"{regionView.RegionData.Texture.name}.png");
                    if (!File.Exists(maskTexturePath))
                    {
                        continue;
                    }

                    maskTexture.LoadImage(File.ReadAllBytes(maskTexturePath));
                    maskTexture.Apply();

                    Graphics.Blit(maskTexture, regionView.MaskTexture);

                    Object.Destroy(maskTexture);
                }

                // MATERIAL YÜKLE
                LoadRegionMaterial(regionView, regionData.RegionMaterialName);

                // Texture güncelle (material güncelleme KAPALI)
                regionView.UpdateRegionTexture(false);
            }

            Debug.Log($"Loaded coloring book progress {view.ColoringBookData.Name} to {path} in {sw.ElapsedMilliseconds}ms");
        }

        // Yeni metod: Material'i yükle
        private static void LoadRegionMaterial(RegionDataView regionView, string materialName)
        {
            if (regionView.RawImage == null)
                return;

            if (string.IsNullOrEmpty(materialName))
            {
                regionView.RawImage.material = null;
                return;
            }

            string cleanName = materialName.Replace(" (Instance)", "");
            Material mat = Resources.Load<Material>($"Materials/Brushes/{cleanName}");

            if (mat != null)
            {
                regionView.RawImage.material = mat;
            }
            else
            {
                regionView.RawImage.material = null;
            }
        }

        public static void ClearSavedColoringBook(ColoringBookView view)
        {
            string path = Path.Combine(Application.persistentDataPath, Settings.InternalAppSettings.SaveFolderPath);
            CheckFolder(path);

            path = GetColoringBookToPath(path, view);

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        private static void CheckFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string GetColoringBookToPath(string path, ColoringBookView view)
        {
            return Path.Combine(
                path,
                $"{view.ColoringBookData.Name}-for-style-{view.ColoringStyle}");
        }
    }
}
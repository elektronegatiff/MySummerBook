using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;
using HootyBird.ColoringBook.Menu.Widgets;
using HootyBird.ColoringBook.Gameplay;

public class BrushCreatorEditor : EditorWindow
{
    private string brushName = "NewBrush";
    private Texture2D patternTexture;
    private Color patternColor = Color.white;
    private float glow = 20f;
    private Sprite icon;

    private string enumFilePath;
    private int selectedCategoryIndex = 0;
    private string[] categoryNames;
    private BrushPanel brushPanel;

    [MenuItem("Tools/Brush Creator")]
    public static void ShowWindow()
    {
        GetWindow<BrushCreatorEditor>("Brush Creator");
    }

    private void OnEnable()
    {
        // BrushType.cs dosyasını otomatik bul
        string[] guids = AssetDatabase.FindAssets("BrushType t:Script");
        if (guids.Length > 0)
        {
            enumFilePath = AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        // Kategorileri yükle
        RefreshCategories();
    }

    private void RefreshCategories()
    {
        // Sahnedeki BrushPanel'i bul
        brushPanel = FindObjectOfType<BrushPanel>();

        if (brushPanel != null)
        {
            // Kategorileri al (SerializedObject ile)
            SerializedObject so = new SerializedObject(brushPanel);
            SerializedProperty categoriesProp = so.FindProperty("categories");

            categoryNames = new string[categoriesProp.arraySize];
            for (int i = 0; i < categoriesProp.arraySize; i++)
            {
                var categoryProp = categoriesProp.GetArrayElementAtIndex(i);
                categoryNames[i] = categoryProp.FindPropertyRelative("categoryName").stringValue;
            }
        }
        else
        {
            categoryNames = new string[] { "BrushPanel bulunamadı!" };
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("🎨 Brush Creator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // Brush Ayarları
        brushName = EditorGUILayout.TextField("Brush Name", brushName);
        icon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);

        GUILayout.Space(10);

        // Pattern Ayarları
        patternTexture = (Texture2D)EditorGUILayout.ObjectField("Pattern Texture", patternTexture, typeof(Texture2D), false);

        if (patternTexture != null)
        {
            patternColor = EditorGUILayout.ColorField("Color", patternColor);
            glow = EditorGUILayout.Slider("Glow", glow, 1f, 50f);
        }

        GUILayout.Space(10);

        // Kategori Seçimi
        GUILayout.Label("📁 Category", EditorStyles.boldLabel);

        if (brushPanel == null)
        {
            EditorGUILayout.HelpBox("Sahnede BrushPanel bulunamadı!\nBrush oluşturulacak ama kategoriye eklenmeyecek.", MessageType.Warning);
            if (GUILayout.Button("🔄 Refresh"))
            {
                RefreshCategories();
            }
        }
        else
        {
            selectedCategoryIndex = EditorGUILayout.Popup("Category", selectedCategoryIndex, categoryNames);
        }

        GUILayout.Space(10);

        // Mevcut Enum'lar
        GUILayout.Label("📋 Mevcut Brush Types", EditorStyles.boldLabel);
        var existingTypes = Enum.GetNames(typeof(BrushType));
        EditorGUILayout.HelpBox(string.Join(", ", existingTypes), MessageType.None);

        bool isNewType = !existingTypes.Contains(brushName);
        if (isNewType && !string.IsNullOrEmpty(brushName))
        {
            EditorGUILayout.HelpBox($"'{brushName}' yeni bir type! Enum'a eklenecek.", MessageType.Info);
        }

        GUILayout.Space(20);

        // Create Button
        GUI.enabled = !string.IsNullOrEmpty(brushName) && IsValidEnumName(brushName);
        if (GUILayout.Button("✨ Create Brush", GUILayout.Height(40)))
        {
            CreateBrush();
        }
        GUI.enabled = true;

        if (!IsValidEnumName(brushName) && !string.IsNullOrEmpty(brushName))
        {
            EditorGUILayout.HelpBox("Brush name geçerli bir enum adı olmalı (boşluk, özel karakter yok)", MessageType.Error);
        }
    }

    private bool IsValidEnumName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        if (char.IsDigit(name[0])) return false;
        return name.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    private void CreateBrush()
    {
        CreateFolders();

        // 1. Enum'a ekle
        AddToEnumIfNeeded();

        // 2. Material oluştur
        Material mat = null;
        if (patternTexture != null)
        {
            mat = CreateMaterial();
        }

        // 3. BrushData oluştur
        BrushData brushData = CreateBrushData(mat);

        // 4. Kategoriye ekle
        if (brushPanel != null && brushData != null)
        {
            AddToCategory(brushData);
        }

        AssetDatabase.Refresh();

        string categoryMsg = brushPanel != null ? $"• '{categoryNames[selectedCategoryIndex]}' kategorisine eklendi\n" : "";

        EditorUtility.DisplayDialog("✅ Başarılı",
            $"'{brushName}' brush oluşturuldu!\n\n" +
            "• Enum güncellendi\n" +
            "• BrushData oluşturuldu\n" +
            (mat != null ? "• Material oluşturuldu\n" : "") +
            categoryMsg,
            "OK");
    }

    private void AddToEnumIfNeeded()
    {
        var existingTypes = Enum.GetNames(typeof(BrushType));
        if (existingTypes.Contains(brushName))
        {
            return;
        }

        if (!File.Exists(enumFilePath))
        {
            Debug.LogError($"Enum dosyası bulunamadı: {enumFilePath}");
            return;
        }

        string content = File.ReadAllText(enumFilePath);

        // Son enum değerini bul
        int enumStart = content.IndexOf('{') + 1;
        int enumEnd = content.IndexOf('}');
        string enumContent = content.Substring(enumStart, enumEnd - enumStart);

        // Yeni değer ekle
        string trimmed = enumContent.TrimEnd();
        if (!trimmed.EndsWith(","))
        {
            int lastChar = trimmed.Length - 1;
            while (lastChar >= 0 && char.IsWhiteSpace(trimmed[lastChar])) lastChar--;
            if (lastChar >= 0 && trimmed[lastChar] != ',')
            {
                trimmed = trimmed.Substring(0, lastChar + 1) + ",";
            }
        }
        trimmed += $"\n        {brushName}";

        string newContent = content.Substring(0, enumStart) + trimmed + "\n    " + content.Substring(enumEnd);
        File.WriteAllText(enumFilePath, newContent);

        Debug.Log($"'{brushName}' enum'a eklendi!");
    }

    private void CreateFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Materials"))
            AssetDatabase.CreateFolder("Assets/Resources", "Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Materials/Brushes"))
            AssetDatabase.CreateFolder("Assets/Resources/Materials", "Brushes");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Brushes"))
            AssetDatabase.CreateFolder("Assets/Resources", "Brushes");
    }

    private Material CreateMaterial()
    {
        Shader shader = Shader.Find("AllIn1SpriteShader/AllIn1SpriteShader");
        if (shader == null)
        {
            Debug.LogError("AllIn1SpriteShader bulunamadı!");
            return null;
        }

        Material mat = new Material(shader);
        mat.SetTexture("_OverlayTex", patternTexture);
        mat.SetColor("_OverlayColor", patternColor);
        mat.SetFloat("_OverlayGlow", glow);
        mat.SetFloat("_OverlayBlend", 1f);
        mat.EnableKeyword("OVERLAY_ON");

        string path = $"Assets/Resources/Materials/Brushes/Mat_{brushName}.mat";
        AssetDatabase.CreateAsset(mat, path);
        AssetDatabase.SaveAssets();
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }

    private BrushData CreateBrushData(Material mat)
    {
        var brush = ScriptableObject.CreateInstance<BrushData>();
        brush.brushName = brushName;
        brush.icon = icon;
        brush.sizeMultiplier = 1f;
        brush.regionMaterial = mat;

        // BrushType ayarla
        try
        {
            if (Enum.IsDefined(typeof(BrushType), brushName))
            {
                brush.brushType = (BrushType)Enum.Parse(typeof(BrushType), brushName);
            }
        }
        catch
        {
            Debug.LogWarning($"BrushType '{brushName}' henüz compile olmadı. Manuel ayarla.");
        }

        string path = $"Assets/Resources/Brushes/Brush_{brushName}.asset";
        AssetDatabase.CreateAsset(brush, path);
        AssetDatabase.SaveAssets();

        Selection.activeObject = brush;
        return AssetDatabase.LoadAssetAtPath<BrushData>(path);
    }

    private void AddToCategory(BrushData brushData)
    {
        SerializedObject so = new SerializedObject(brushPanel);
        SerializedProperty categoriesProp = so.FindProperty("categories");

        if (selectedCategoryIndex < categoriesProp.arraySize)
        {
            var categoryProp = categoriesProp.GetArrayElementAtIndex(selectedCategoryIndex);
            var brushesProp = categoryProp.FindPropertyRelative("brushes");

            // Yeni brush ekle
            brushesProp.arraySize++;
            var newBrushProp = brushesProp.GetArrayElementAtIndex(brushesProp.arraySize - 1);
            newBrushProp.objectReferenceValue = brushData;

            so.ApplyModifiedProperties();

            // Sahneyi kaydet
            EditorUtility.SetDirty(brushPanel);
            EditorSceneManager.MarkSceneDirty(brushPanel.gameObject.scene);

            Debug.Log($"'{brushName}' → '{categoryNames[selectedCategoryIndex]}' kategorisine eklendi!");
        }
    }
}
using HootyBird.ColoringBook.Data;
using HootyBird.ColoringBook.Serialized;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HootyBird.ColoringBook.Repositories
{


    public class BookProgressManager : MonoBehaviour
    {
        public static BookProgressManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private string saveKey = "MySummerBook_Progress";
        [SerializeField] private bool firstPageUnlockedByDefault = true;
        [SerializeField] private float completionThreshold = 1f;

        [Header("References")]
        [SerializeField] private Category bookCategory;

        public event Action<int> OnPageUnlocked;
        public event Action<int> OnPageCompleted;
        public event Action<int, float> OnPageProgressUpdated;
        public event Action OnAllPagesCompleted;

        private Dictionary<string, PageProgressData> progressData;
        private List<string> pageOrder;
        private int currentPageIndex;

        public int TotalPages => pageOrder?.Count ?? 0;
        public int CurrentPageIndex => currentPageIndex;
        public int LastUnlockedPageIndex => GetLastUnlockedIndex();

        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            progressData = new Dictionary<string, PageProgressData>();
            pageOrder = new List<string>();

            LoadProgress();
        }

        private int GetLastUnlockedIndex()
        {
            for (int i = pageOrder.Count - 1; i >= 0; i--)
            {
                if (pageOrder[i] != null && IsPageUnlocked(pageOrder[i]))
                {
                    return i;
                }
            }
            return 0;
        }

        #region Public API - String Based

        public void RegisterPage(string pageId, int order)
        {
            if (!progressData.ContainsKey(pageId))
            {
                bool shouldUnlock = (order == 0 && firstPageUnlockedByDefault);
                progressData[pageId] = new PageProgressData(pageId, shouldUnlock);
                Debug.Log($"[BookProgress] Sayfa kaydedildi: {pageId}, Sıra: {order}, Açık: {shouldUnlock}");
            }

            while (pageOrder.Count <= order)
            {
                pageOrder.Add(null);
            }

            if (pageOrder[order] == null)
            {
                pageOrder[order] = pageId;
            }

            SaveProgress();
        }

        public bool IsPageUnlocked(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) return false;

            if (progressData.TryGetValue(pageId, out var data))
            {
                return data.isUnlocked;
            }
            return false;
        }

        public bool IsPageCompleted(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) return false;

            if (progressData.TryGetValue(pageId, out var data))
            {
                return data.isCompleted;
            }
            return false;
        }

        public float GetPageProgress(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) return 0f;

            if (progressData.TryGetValue(pageId, out var data))
            {
                return data.completionPercent;
            }
            return 0f;
        }

        public void UpdatePageProgress(string pageId, float progress)
        {
            if (string.IsNullOrEmpty(pageId)) return;

            if (!progressData.TryGetValue(pageId, out var data))
            {
                return;
            }

            data.completionPercent = Mathf.Clamp01(progress);

            int pageIndex = pageOrder.IndexOf(pageId);
            OnPageProgressUpdated?.Invoke(pageIndex, progress);

            if (!data.isCompleted && progress >= completionThreshold)
            {
                CompletePage(pageId);
            }

            SaveProgress();
        }

        public void CompletePage(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) return;

            if (!progressData.TryGetValue(pageId, out var data))
            {
                return;
            }

            if (data.isCompleted) return;

            data.isCompleted = true;
            data.completionPercent = 1f;

            int pageIndex = pageOrder.IndexOf(pageId);
            Debug.Log($"[BookProgress] ✅ Sayfa {pageIndex + 1} ({pageId}) tamamlandı!");
            OnPageCompleted?.Invoke(pageIndex);

            // Sonraki sayfanın kilidini aç
            int nextIndex = pageIndex + 1;
            if (nextIndex < pageOrder.Count && pageOrder[nextIndex] != null)
            {
                UnlockPage(pageOrder[nextIndex]);
            }

            // Tüm sayfalar tamamlandı mı?
            if (progressData.Values.All(p => p.isCompleted))
            {
                Debug.Log("[BookProgress] 🎉 Tüm kitap tamamlandı!");
                OnAllPagesCompleted?.Invoke();
            }

            SaveProgress();
        }

        public void UnlockPage(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) return;

            if (!progressData.TryGetValue(pageId, out var data))
            {
                return;
            }

            if (data.isUnlocked) return;

            data.isUnlocked = true;

            int pageIndex = pageOrder.IndexOf(pageId);
            Debug.Log($"[BookProgress] 🔓 Sayfa {pageIndex + 1} ({pageId}) açıldı!");
            OnPageUnlocked?.Invoke(pageIndex);

            SaveProgress();
        }

        #endregion

        #region Public API - Index Based (PageTransitionController için)

        public bool IsPageUnlocked(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= pageOrder.Count) return false;
            string pageId = pageOrder[pageIndex];
            return IsPageUnlocked(pageId);
        }

        public bool IsPageCompleted(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= pageOrder.Count) return false;
            string pageId = pageOrder[pageIndex];
            return IsPageCompleted(pageId);
        }

        public float GetPageProgress(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= pageOrder.Count) return 0f;
            string pageId = pageOrder[pageIndex];
            return GetPageProgress(pageId);
        }

        public void UpdatePageProgress(int pageIndex, float progress)
        {
            if (pageIndex < 0 || pageIndex >= pageOrder.Count) return;
            string pageId = pageOrder[pageIndex];
            UpdatePageProgress(pageId, progress);
        }

        public bool TrySetCurrentPage(int pageIndex)
        {
            if (!IsPageUnlocked(pageIndex))
            {
                Debug.LogWarning($"[BookProgress] Sayfa {pageIndex} kilitli!");
                return false;
            }

            currentPageIndex = pageIndex;
            SaveProgress();
            return true;
        }

        public ColoringBookDataBase GetPageData(int pageIndex)
        {
            if (bookCategory?.ColoringBooks == null) return null;
            if (pageIndex < 0 || pageIndex >= bookCategory.ColoringBooks.Count) return null;

            return bookCategory.ColoringBooks[pageIndex];
        }

        public string GetPageId(int pageIndex)
        {
            if (pageIndex < 0 || pageIndex >= pageOrder.Count) return null;
            return pageOrder[pageIndex];
        }

        public int GetPageIndex(string pageId)
        {
            return pageOrder.IndexOf(pageId);
        }

        #endregion

        #region Reset

        public void ResetAllProgress()
        {
            PlayerPrefs.DeleteKey(saveKey);

            foreach (var kvp in progressData)
            {
                int index = pageOrder.IndexOf(kvp.Key);
                kvp.Value.isUnlocked = (index == 0 && firstPageUnlockedByDefault);
                kvp.Value.isCompleted = false;
                kvp.Value.completionPercent = 0f;
            }

            currentPageIndex = 0;
            SaveProgress();
            Debug.Log("[BookProgress] İlerleme sıfırlandı!");
        }

        #endregion

        #region Save/Load

        private void LoadProgress()
        {
            string json = PlayerPrefs.GetString(saveKey, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var saveData = JsonUtility.FromJson<BookProgressSaveData>(json);
                    if (saveData?.pages != null)
                    {
                        foreach (var page in saveData.pages)
                        {
                            progressData[page.pageId] = page;
                        }
                        currentPageIndex = saveData.currentPageIndex;
                    }
                    Debug.Log($"[BookProgress] İlerleme yüklendi: {progressData.Count} sayfa");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[BookProgress] Yükleme hatası: {e.Message}");
                }
            }
        }

        private void SaveProgress()
        {
            var saveData = new BookProgressSaveData
            {
                pages = progressData.Values.ToArray(),
                currentPageIndex = currentPageIndex
            };

            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString(saveKey, json);
            PlayerPrefs.Save();
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Debug - Print Progress")]
        private void DebugPrintProgress()
        {
            Debug.Log("=== Book Progress ===");
            for (int i = 0; i < pageOrder.Count; i++)
            {
                string pageId = pageOrder[i];
                if (pageId != null && progressData.TryGetValue(pageId, out var data))
                {
                    string status = data.isCompleted ? "✅" : (data.isUnlocked ? "🔓" : "🔒");
                    Debug.Log($"Sayfa {i + 1}: {status} {pageId} - {data.completionPercent:P0}");
                }
            }
        }

        [ContextMenu("Debug - Unlock All")]
        private void DebugUnlockAll()
        {
            foreach (var data in progressData.Values)
            {
                data.isUnlocked = true;
            }
            SaveProgress();
            Debug.Log("[BookProgress] Tüm sayfalar açıldı!");
        }

        [ContextMenu("Debug - Reset Progress")]
        private void DebugResetProgress()
        {
            ResetAllProgress();
        }
#endif

        #endregion
    }
}
using System;
using UnityEngine;

namespace HootyBird.ColoringBook.Data
{
    [Serializable]
    public class PageProgressData
    {
        public string pageId;
        public bool isUnlocked;
        public bool isCompleted;
        public float completionPercent;

        public PageProgressData() { }

        public PageProgressData(string id, bool unlocked = false)
        {
            pageId = id;
            isUnlocked = unlocked;
            isCompleted = false;
            completionPercent = 0f;
        }
    }

    [Serializable]
    public class BookProgressSaveData
    {
        public PageProgressData[] pages;
        public int currentPageIndex;
    }
}
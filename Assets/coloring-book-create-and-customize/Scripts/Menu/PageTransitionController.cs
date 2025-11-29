using System;
using System.Collections;
using HootyBird.ColoringBook.Gameplay;
using HootyBird.ColoringBook.Repositories;
using HootyBird.ColoringBook.Serialized;
using UnityEngine;
using EasyTransition;

namespace HootyBird.ColoringBook.Menu
{
    public class PageTransitionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ColoringBookView coloringBookView;

        [Header("Easy Transitions")]
        [SerializeField] private TransitionSettings pageTransition;
        [SerializeField] private float transitionDelay = 0f;



        public event Action OnPageTransitionStarted;
        public event Action OnPageTransitionCompleted;
        public event Action OnBookCompleted;

        private BookProgressManager progressManager;
        private AudioSource audioSource;
        private bool isTransitioning;
        private int currentPageIndex;
        private int targetPageIndex;

        private void Start()
        {
            progressManager = BookProgressManager.Instance;
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            if (coloringBookView != null)
                coloringBookView.OnCompletePercentUpdated += OnProgressUpdated;

            if (progressManager != null)
            {
                progressManager.OnPageCompleted += OnPageCompleted;
                progressManager.OnAllPagesCompleted += OnAllPagesCompleted;
                currentPageIndex = progressManager.CurrentPageIndex;
            }

            UpdatePageNumberUI();

         
        }

        private void OnDestroy()
        {
            if (coloringBookView != null)
                coloringBookView.OnCompletePercentUpdated -= OnProgressUpdated;

            if (progressManager != null)
            {
                progressManager.OnPageCompleted -= OnPageCompleted;
                progressManager.OnAllPagesCompleted -= OnAllPagesCompleted;
            }

            ClearTransitionEvents();
        }

        private void OnProgressUpdated(float progress)
        {
            if (progressManager != null)
                progressManager.UpdatePageProgress(currentPageIndex, progress);
        }

        private void OnPageCompleted(int pageIndex)
        {
            if (pageIndex != currentPageIndex || isTransitioning) return;
          
        }

        private void OnAllPagesCompleted()
        {
            Debug.Log("🎉 Tebrikler! Kitabı tamamladın!");
            OnBookCompleted?.Invoke();
        }

        public void LoadPage(int pageIndex)
        {
            if (progressManager == null) return;

            if (!progressManager.IsPageUnlocked(pageIndex))
            {
                Debug.LogWarning($"Sayfa {pageIndex + 1} kilitli!");
                return;
            }

            LoadPageInternal(pageIndex);
        }

        public void GoToNextPage()
        {
            if (isTransitioning) return;

            int nextIndex = currentPageIndex + 1;

            if (progressManager != null && progressManager.IsPageUnlocked(nextIndex))
            {
                TransitionToPage(nextIndex);
            }
        }

        public void GoToPreviousPage()
        {
            if (isTransitioning) return;

            int prevIndex = currentPageIndex - 1;

            if (prevIndex >= 0)
            {
                TransitionToPage(prevIndex);
            }
        }

        public void OnNextPageButtonClicked()
        {
          

            GoToNextPage();
        }

        private void TransitionToPage(int pageIndex)
        {
            if (pageTransition == null || TransitionManager.Instance() == null)
            {
                LoadPageInternal(pageIndex);
                return;
            }

            isTransitioning = true;
            targetPageIndex = pageIndex;
            OnPageTransitionStarted?.Invoke();

            var transitionManager = TransitionManager.Instance();
            ClearTransitionEvents();
            transitionManager.onTransitionCutPointReached += OnTransitionCutPoint;
            transitionManager.onTransitionEnd += OnTransitionEnd;

            transitionManager.Transition(pageTransition, transitionDelay);
        }

        private void OnTransitionCutPoint()
        {
            var transitionManager = TransitionManager.Instance();
            transitionManager.onTransitionCutPointReached -= OnTransitionCutPoint;

            LoadPageInternal(targetPageIndex);
        }

        private void OnTransitionEnd()
        {
            var transitionManager = TransitionManager.Instance();
            transitionManager.onTransitionEnd -= OnTransitionEnd;

            isTransitioning = false;
            OnPageTransitionCompleted?.Invoke();
        }

        private void ClearTransitionEvents()
        {
            var transitionManager = TransitionManager.Instance();
            if (transitionManager != null)
            {
                transitionManager.onTransitionCutPointReached -= OnTransitionCutPoint;
                transitionManager.onTransitionEnd -= OnTransitionEnd;
            }
        }

    

        private void LoadPageInternal(int pageIndex)
        {
            if (progressManager == null) return;

            if (progressManager.TrySetCurrentPage(pageIndex))
            {
                currentPageIndex = pageIndex;
                var pageData = progressManager.GetPageData(pageIndex);

                if (pageData != null && coloringBookView != null)
                {
                    coloringBookView.ReleaseCurrentColoringBookAssets();
                    pageData.InitializeAssets();
                    coloringBookView.LoadColoringBook(pageData);
                }

                UpdatePageNumberUI();
            }
        }

        private void UpdatePageNumberUI()
        {
          
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

    
    }
}
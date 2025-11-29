using UnityEngine;
using UnityEngine.EventSystems;

namespace HootyBird.ColoringBook.Gameplay
{
    public class ZoomController : MonoBehaviour
    {
        [Header("Zoom Settings")]
        [SerializeField] private float minZoom = 1f;
        [SerializeField] private float maxZoom = 3f;
        [SerializeField] private float zoomSpeed = 0.005f;
        [SerializeField] private float smoothSpeed = 10f;

        [Header("References")]
        [SerializeField] private RectTransform zoomArea;
        [SerializeField] private RectTransform contentTransform;

        private Vector3 originalScale;
        private Vector3 originalPosition;
        private Vector3 targetScale;
        private Vector3 targetPosition;

        private float currentZoom = 1f;
        private bool isPinching = false;
        private float lastPinchDistance = 0f;
        private Vector2 lastPinchCenter;

        private Vector2 minPosition;
        private Vector2 maxPosition;

        private Camera uiCamera;

        // Dýþarýdan eriþim için
        public bool IsPinching => isPinching;
        public bool IsZoomed => currentZoom > minZoom + 0.01f;
        public float CurrentZoom => currentZoom;

        private void Start()
        {
            if (contentTransform == null)
            {
                contentTransform = GetComponent<RectTransform>();
            }

            if (zoomArea == null)
            {
                zoomArea = contentTransform;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                uiCamera = canvas.worldCamera;
            }

            originalScale = contentTransform.localScale;
            originalPosition = contentTransform.localPosition;
            targetScale = originalScale;
            targetPosition = originalPosition;
        }

        private void Update()
        {
            HandleTouchInput();
            HandleMouseInput();
            ApplySmoothTransform();
        }

        private void HandleTouchInput()
        {
            int touchCount = Input.touchCount;

            if (touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                // En az bir parmak resim üzerinde mi?
                bool touch1Over = IsTouchOverZoomArea(touch1.position);
                bool touch2Over = IsTouchOverZoomArea(touch2.position);

                if (!touch1Over && !touch2Over)
                {
                    isPinching = false;
                    return;
                }

                Vector2 pinchCenter = (touch1.position + touch2.position) / 2f;
                float currentPinchDistance = Vector2.Distance(touch1.position, touch2.position);

                if (!isPinching)
                {
                    // Pinch baþladý
                    isPinching = true;
                    lastPinchDistance = currentPinchDistance;
                    lastPinchCenter = pinchCenter;
                }
                else
                {
                    // Zoom
                    float distanceDelta = currentPinchDistance - lastPinchDistance;
                    Zoom(distanceDelta * zoomSpeed);
                    lastPinchDistance = currentPinchDistance;

                    // Pan (2 parmakla sürükleme)
                    if (currentZoom > minZoom)
                    {
                        Vector2 panDelta = pinchCenter - lastPinchCenter;
                        Pan(panDelta);
                    }
                    lastPinchCenter = pinchCenter;
                }
            }
            else
            {
                isPinching = false;
            }
        }

        private void HandleMouseInput()
        {
#if UNITY_EDITOR
            if (!IsTouchOverZoomArea(Input.mousePosition))
            {
                return;
            }

            // Scroll wheel zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                Zoom(scroll);
            }

            // Orta tuþ ile pan
            if (Input.GetMouseButton(2) && currentZoom > minZoom)
            {
                if (Input.GetMouseButtonDown(2))
                {
                    lastPinchCenter = Input.mousePosition;
                }
                else
                {
                    Vector2 delta = (Vector2)Input.mousePosition - lastPinchCenter;
                    Pan(delta);
                    lastPinchCenter = Input.mousePosition;
                }
            }
#endif
        }

        private bool IsTouchOverZoomArea(Vector2 screenPosition)
        {
            if (zoomArea == null) return false;

            return RectTransformUtility.RectangleContainsScreenPoint(
                zoomArea,
                screenPosition,
                uiCamera
            );
        }

        private void Zoom(float delta)
        {
            float prevZoom = currentZoom;
            currentZoom = Mathf.Clamp(currentZoom + delta, minZoom, maxZoom);

            if (Mathf.Approximately(prevZoom, currentZoom)) return;

            targetScale = originalScale * currentZoom;

            if (currentZoom <= minZoom + 0.01f)
            {
                currentZoom = minZoom;
                targetPosition = originalPosition;
                targetScale = originalScale;
            }

            UpdateBounds();
            ClampPosition();
        }

        private void Pan(Vector2 delta)
        {
            if (currentZoom <= minZoom) return;

            targetPosition += new Vector3(delta.x, delta.y, 0f);
            ClampPosition();
        }

        private void UpdateBounds()
        {
            if (zoomArea == null) return;

            float scaledWidth = contentTransform.rect.width * currentZoom;
            float scaledHeight = contentTransform.rect.height * currentZoom;

            float parentWidth = zoomArea.rect.width;
            float parentHeight = zoomArea.rect.height;

            float excessWidth = Mathf.Max(0, (scaledWidth - parentWidth) / 2f);
            float excessHeight = Mathf.Max(0, (scaledHeight - parentHeight) / 2f);

            minPosition = new Vector2(-excessWidth, -excessHeight) + (Vector2)originalPosition;
            maxPosition = new Vector2(excessWidth, excessHeight) + (Vector2)originalPosition;
        }

        private void ClampPosition()
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);
        }

        private void ApplySmoothTransform()
        {
            contentTransform.localScale = Vector3.Lerp(
                contentTransform.localScale,
                targetScale,
                Time.deltaTime * smoothSpeed
            );

            contentTransform.localPosition = Vector3.Lerp(
                contentTransform.localPosition,
                targetPosition,
                Time.deltaTime * smoothSpeed
            );
        }

        public void ResetZoom()
        {
            currentZoom = minZoom;
            targetScale = originalScale;
            targetPosition = originalPosition;
        }
    }
}
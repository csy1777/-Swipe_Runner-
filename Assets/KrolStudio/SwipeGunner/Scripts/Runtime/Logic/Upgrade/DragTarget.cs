using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KrolStudio
{
    public class DragTarget : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] float moveDuration = 0.35f;

        private Transform t;
        private Camera mainCam;
        private Vector3 offset;
        private float fixedY;
        private Plane dragPlane;

        private Vector3 targetPosition, startPosition;
        private float moveTime;

        private bool isDragging;
        public bool IsPointerDown { get; set; }
        public bool IsPointerUp { get; set; }

        public event Action PointerDown;
        public event Action PointerUp;
        public event Action PointerDrag;

        void Start()
        {
            isDragging = false;
            t = transform;
            mainCam = Camera.main;
        }

        void Update()
        {
            if (!isDragging) return;

            moveTime += Time.deltaTime;
            float t01 = Mathf.Clamp01(moveTime / moveDuration);
            float eased = Easing.EaseOutQuart(t01);
            t.position = Vector3.Lerp(startPosition, targetPosition, eased);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsPointerDown) return;
            IsPointerDown = true;

            fixedY = t.position.y;

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            dragPlane = new Plane(Vector3.up, new Vector3(0, fixedY, 0)); // Plane at the object's Y level
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                PointerDown?.Invoke();
                offset = t.position - hitPoint;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            
            if (!IsPointerDown || IsPointerUp) return;
            IsPointerUp = true;
            PointerUp?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsPointerDown || IsPointerUp) return;
            
            isDragging = true;
            PointerDrag?.Invoke();

            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                targetPosition = hitPoint + offset;
                targetPosition.y = t.position.y;

                moveTime = 0f;
                startPosition = t.position;
            }
        }
    }
}
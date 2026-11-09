using UnityEngine;
using System;

namespace KrolStudio
{
    [RequireComponent(typeof(DragTarget), typeof(PartLifter), typeof(CurveMover))]
    public class PartDragHandler : MonoBehaviour
    {
        public event Action PointerDown;
        public event Action PointerUp;
        public event Action PointerDrag;

        private DragTarget dragTarget;
        private PartLifter partLifter;
        [HideInInspector] public CurveMover curveMover;

        private BoardManager boardManager;

        private void Awake()
        {
            partLifter = GetComponent<PartLifter>();
            dragTarget = GetComponent<DragTarget>();
            curveMover = GetComponent<CurveMover>();
        }

        public void Initialize(BoardManager board)
        {
            boardManager = board;
        }

        private void OnEnable()
        {
            dragTarget.PointerDown += OnPointerDownInternal;
            dragTarget.PointerUp += OnPointerUpInternal;
            dragTarget.PointerDrag += OnPointerDragInternal;
        }

        private void OnDisable()
        {
            dragTarget.PointerDown -= OnPointerDownInternal;
            dragTarget.PointerUp -= OnPointerUpInternal;
            dragTarget.PointerDrag -= OnPointerDragInternal;
        }

        private void OnPointerDragInternal()
        {
            PointerDrag?.Invoke();
        }

        private void OnPointerDownInternal()
        {
            curveMover.Cancel();
            boardManager.ShowGarbage();
            partLifter.StartLifting();

            PointerDown?.Invoke();  // Enable the highlight
        }

        private void OnPointerUpInternal()
        {
            partLifter.CancelLifting();
            PointerUp?.Invoke();
        }

        public void ResetPointerFlags()
        {
            dragTarget.IsPointerDown = false;
            dragTarget.IsPointerUp = false;
        }

        public void ReturnToBoard()
        {
            Transform target = boardManager.GetNearestCell(transform.position);
            transform.SetParent(target);
            curveMover.MoveTo(target, Easing.EaseInQuart, ResetPointerFlags);
        }
    }
}
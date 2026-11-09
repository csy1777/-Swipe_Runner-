using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KrolStudio
{
    [RequireComponent(typeof(PartLevelController), typeof(PartVisualsController), typeof(PartDragHandler))]
    public class UpgradePartController : MonoBehaviour
    {
        private PartLevelController levelController;
        private PartVisualsController visualsController;
        private PartDragHandler dragHandler;

        private BoardManager boardManager;
        private AudioManager audioManager;

        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask partLayer;
        [SerializeField] private LayerMask garbageLayer;

        private void Awake()
        {
            levelController = GetComponent<PartLevelController>();
            visualsController = GetComponent<PartVisualsController>();
            dragHandler = GetComponent<PartDragHandler>();
        }

        public void Initialize(BoardManager board, int level, PartType partType, AudioManager audioManager)
        {
            this.boardManager = board;
            this.audioManager = audioManager;

            levelController.Initialize(partType, level);
           
            visualsController.Initialize(levelController.FindCurrentPart(), level);
            visualsController.DisplayLevelIndicator(true);
            visualsController.UpdateLevelIndicator(levelController.Level, levelController.CanUpgrade());

            dragHandler.Initialize(board);
            dragHandler.PointerDown += OnPointerDown;
            dragHandler.PointerUp += OnPointerUp;
            dragHandler.PointerDrag += OnPointerDrag;
        }

        private void OnPointerDown() // -> DragHandler
        {
            EnableBacklightOnBoard();
            boardManager.SetBacklight(levelController.PartType, levelController.Level, true);
        }
       
        private void OnPointerUp() // -> DragHandler
        {
            boardManager.SetBacklight(levelController.PartType, levelController.Level, false);
            dragHandler.ReturnToBoard();
            TryUpgrade();
            TryMerge();
            TryDestroy();
            DisableBacklightOnBoard();
        }

        private void OnPointerDrag() // -> DragHandler
        {
            boardManager.SetReadyForDeletionColor(RaycastUtility.RaycastFromMouse(out List<RaycastResult> results, garbageLayer));
        }


        private void TryUpgrade()
        {
            if (RaycastUtility.RaycastFromMouse(out List<RaycastResult> results, playerLayer))
            {
                foreach (var result in results)
                {
                    var player = result.gameObject.GetComponentInParent<UpgradeLevelController>();

                    if (player != null)
                    {
                        int playerPartLevel = player.GetLevel(levelController.PartType);

                        if(levelController.Level >= playerPartLevel)
                        {
                            if (player.CanUpgradePlayerPart(levelController.PartType)) // If the item on the player can be upgraded
                            {
                                int newLevel = levelController.Level > playerPartLevel ? levelController.Level : levelController.Level + 1;
                                player.SetLevel(newLevel, levelController.PartType);

                                boardManager.PlayUpgradeEffect(levelController.PartType);
                                boardManager.OnSetPartInteractable?.Invoke(true);
                                boardManager.OnMergeSuccess?.Invoke();

                                DestroyPart();

                                audioManager.PlaySound(GameConstants.Sounds.UpgradePart);
                            }
                            else
                            {
                                boardManager.ShowItemMaxLevelMessage();
                            }
                        }

                        return;
                    }
                }
            }
        }

        private void EnableBacklightOnBoard()
        {
            foreach (Transform cell in boardManager.CellTransforms)
            {
                if (cell.childCount == 0) continue;

                var part = cell.GetChild(0).GetComponent<UpgradePartController>();
                if (part != this && part.levelController.PartType == levelController.PartType)
                {
                    part.visualsController.EnableBacklight(levelController.Level == part.levelController.Level && levelController.CanUpgrade());
                }
            }
        }

        private void DisableBacklightOnBoard()
        {
            foreach (Transform cell in boardManager.CellTransforms)
            {
                if (cell.childCount == 0) continue;

                var part = cell.GetChild(0).GetComponent<UpgradePartController>();
                if (part != this && part.levelController.PartType == levelController.PartType)
                {
                    part.visualsController.DisableBacklight();
                }
            }
        }

        private void TryMerge()
        {
            if (RaycastUtility.RaycastFromMouse(out List<RaycastResult> results, partLayer))
            {
                foreach (var result in results)
                {
                    var part = result.gameObject.GetComponent<UpgradePartController>();
                   
                    if (part != null && 
                        part != this && 
                        part.levelController.PartType == levelController.PartType &&
                        part.levelController.Level == levelController.Level && 
                        levelController.CanUpgrade())
                    {
                        audioManager.PlaySound(GameConstants.Sounds.UpgradePart);

                        boardManager.OnSetPartInteractable?.Invoke(true);

                        transform.SetParent(part.transform);
                        dragHandler.curveMover.MoveTo(part.transform, Easing.EaseInQuart, () =>
                        {
                            MergeParts(part, this);
                        });

                        return;
                    }
                }
            }
        }

        private void MergeParts(UpgradePartController a, UpgradePartController b)
        {
            dragHandler.ResetPointerFlags();

            boardManager.AddPart(a.transform.parent, levelController.Level + 1, levelController.PartType);

            Destroy(a.gameObject);
            Destroy(b.gameObject);
        }

        private void TryDestroy()
        {
            if (RaycastUtility.RaycastFromMouse(out List<RaycastResult> results, garbageLayer))
            {
                boardManager.OnSetPartInteractable?.Invoke(true);

                dragHandler.curveMover.MoveTo(boardManager.Garbage, Easing.EaseInQuart, DestroyPart);
                dragHandler.curveMover.ScaleTo(Vector3.zero, Easing.EaseInQuart);
            }
            else
            {
                boardManager.HideGarbage();
            }
        }

        private void DestroyPart()
        {
            Destroy(gameObject);
            boardManager.HideGarbage();
        }

        public void PlayEffect()
        {
            visualsController.PlayEffect();
        }
    }
}
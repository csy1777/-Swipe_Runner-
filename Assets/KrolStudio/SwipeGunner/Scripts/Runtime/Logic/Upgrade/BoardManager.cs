using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class BoardManager : MonoBehaviour, IProgressSaver, IProgressReader
    {
        [SerializeField] List<Transform> cellTransforms;

        [Space]
        [SerializeField] Transform garbage;
        [SerializeField] float garbageDuration;
        [SerializeField] SpriteRenderer garbageBackground;
        [SerializeField] Color readyForDeletionColor;
        [SerializeField] Color notReadyForDeletionColor;

        [Space]
        [SerializeField] float levelDecayExponent = 1.5f;   // Controls the rate at which the chance decreases
        [SerializeField] float minWeight = 0.1f;            // Minimum chance below which it does not decrease

        private IWalletService walletService;
        private IGameFactory gameFactory;
        private ILogService logService;
        private IHudView hudView;
        private IPlayerContext playerContext;
        private UpgradeSystem upgradeSystem;

        private bool shouldDisplayGarbage = true;
        private CtsHelper ctsHelper = new();

        public Action<bool> OnSetPartInteractable;
        public Action OnMergeSuccess;

        public List<Transform> CellTransforms => cellTransforms;
        public Transform Garbage => garbage;

        [Inject]
        void Construct(IPlayerContext playerContext, 
                       IWalletService walletService, 
                       UpgradeSystem upgradeSystem,
                       IGameFactory gameFactory,
                       ILogService logService,
                       IHudView hudView,
                       ISaveLoadService saveLoadService)
        {
            this.playerContext = playerContext;

            this.upgradeSystem = upgradeSystem;
            this.walletService = walletService;
            this.gameFactory = gameFactory;
            this.logService = logService;
            this.hudView = hudView;

            saveLoadService.Register(SaveKey.BoardManager, this);
        }

        public void SetReadyForDeletionColor(bool value)
        {
            garbageBackground.color = value ? readyForDeletionColor : notReadyForDeletionColor;
        }

        public void SetGarbageVisibility(bool value)
        {
            shouldDisplayGarbage = value;
        }

        public void ShowGarbage()
        {
            if (!shouldDisplayGarbage) return;
            SetReadyForDeletionColor(false);
            GarbageScale(0f, 1f);
        }

        public void HideGarbage()
        {
            if (!shouldDisplayGarbage) return;
            GarbageScale(1f, 0f);
        }

        public void EvaluatePartPurchaseAvailability()
        {
            OnSetPartInteractable(walletService.HasEnough(CurrencyType.Coins, upgradeSystem.GetNextUpgradePartCost()));
        }

        public void ShowItemMaxLevelMessage()
        {
            hudView.ShowItemMaxLevelMessage();
        }

        public void PlayUpgradeEffect(PartType partType)
        {
            playerContext.UpgradeLevel.PlayUpgradeEffect(partType);
        }

        public void SetBacklight(PartType partType, int partLevel, bool enable)
        {
            playerContext.UpgradeLevel.SetBacklight(partType, partLevel, enable);
        }

        void GarbageScale(float from, float to)
        {          
            ctsHelper.Restart();
            ScaleOverTime(from, to).Forget();
        }

        private async UniTask ScaleOverTime(float from, float to)
        {
            try 
            { 
                float time = 0f;

                while (time < garbageDuration)
                {
                    //if (ctsHelper.Token.IsCancellationRequested || this == null || transform == null)
                    //    return;

                    float t = time / garbageDuration;
                    float scale = Mathf.Lerp(from, to, Easing.EaseOutQuart(t));
                    garbage.localScale = Vector3.one * scale;
                    time += Time.deltaTime;
                    await UniTask.NextFrame(ctsHelper.Token);
                }

                garbage.localScale = Vector3.one * to;
            }
            catch (System.OperationCanceledException)
            {
            }
            catch (MissingReferenceException ex)
            {
                Debug.LogWarning($"The object was destroyed during animation: {ex.Message}", this);
            }
            catch (NullReferenceException ex)
            {
                Debug.LogWarning($"Missing link during animation: {ex.Message}", this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error: {ex}", this);
            }
        }

        private void OnDestroy()
        {
            ctsHelper.Dispose();
        }

        private bool TryPurchaseUpgradePart()
        {
            if (walletService.TryPurchase(CurrencyType.Coins, upgradeSystem.GetNextUpgradePartCost(), this))
            {
                upgradeSystem.IncreaseUpgradeLevel();
                return true;
            }

            logService.LogWarning("Not enough money");
            return false;
        }

        public void AddPart()
        {
            Transform freeCell = GetFirstFreeCell();

            if (freeCell == null)
            {
                logService.LogWarning("The grid is full, you can't add anything");
                return;
            }

            if (!TryPurchaseUpgradePart())
            {
                OnSetPartInteractable?.Invoke(false);
                return;
            }

            EvaluatePartPurchaseAvailability();

            UpgradePartController newPart = gameFactory.CreatePart();
            newPart.transform.SetParent(freeCell, false);
            newPart.transform.localPosition = Vector3.zero;
            newPart.Initialize(this, 0, GetRandomPartType(), playerContext.AudioManager);

            if (GetFirstFreeCell() == null) // If the last slot is occupied
            {
                OnSetPartInteractable?.Invoke(false);
            }
        }

        public void AddPart(Transform cell, int level, PartType partType)
        {
            UpgradePartController newPart = gameFactory.CreatePart();

            newPart.transform.SetParent(cell, false);
            newPart.transform.localPosition = Vector3.zero;

            newPart.PlayEffect();

            newPart.Initialize(this, level, partType, playerContext.AudioManager);
        }

        public void AddPart(int level, PartType partType)
        {
            Transform freeCell = GetFirstFreeCell();

            if (freeCell == null)
            {
                logService.LogWarning("The grid is full, you can't add anything");
                return;
            }

            if (!TryPurchaseUpgradePart())
            {
                OnSetPartInteractable(false);
                return;
            }

            EvaluatePartPurchaseAvailability();

            UpgradePartController newPart = gameFactory.CreatePart();
            newPart.transform.SetParent(freeCell, false);
            newPart.transform.localPosition = Vector3.zero;
            newPart.Initialize(this, level, partType, playerContext.AudioManager);

            if (GetFirstFreeCell() == null) // If the last slot is occupied
            {
                OnSetPartInteractable(false);
            }
        }

        float GetWeight(int level)
        {
            level = level < 0 ? 1 : level + 1;
            return Mathf.Max(minWeight, 1f / Mathf.Pow(level, levelDecayExponent));
        }

        PartType GetRandomPartType()
        {
            PartType[] values = (PartType[])System.Enum.GetValues(typeof(PartType));
            values = values.Where(v => v != PartType.None).ToArray();

            float[] weights = new float[values.Length];

            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = GetWeight(playerContext.UpgradeLevel.GetLevel(values[i]));
            }

            float totalWeight = weights.Sum();
            float roll = UnityEngine.Random.value * totalWeight;

            for (int i = 0; i < weights.Length; i++)
            {
                if (roll < weights[i])
                    return values[i];
                roll -= weights[i];
            }

            return values[0]; // fallback — Always deterministic
        }

        public Transform GetNearestCell(Vector3 position)
        {
            if (cellTransforms == null || cellTransforms.Count == 0)
                throw new System.InvalidOperationException("cellTransforms is empty");

            Transform nearest = null;
            float minDistanceSqr = float.MaxValue;

            foreach (Transform cell in cellTransforms)
            {
                if (cell.childCount > 0)
                    continue;

                float distSqr = (cell.position - position).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    nearest = cell;
                }
            }

            return nearest;
        }

        private Transform GetFirstFreeCell()
        {
            foreach (Transform cell in cellTransforms)
            {
                if (cell.childCount == 0)
                {
                    return cell;
                }
            }

            return null; // all cells are occupied
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            Dictionary<int, PartSaveData> datas = new();

            int index = 0;

            foreach (Transform cell in cellTransforms)
            {
                if (cell.childCount > 0)
                {
                    var item = cell.GetComponentInChildren<PartLevelController>();
                   
                    PartSaveData partData = new PartSaveData();
                    partData.PartType = item.PartType;
                    partData.Level = item.Level;
                    
                    datas.Add(index, partData);
                }

                index++;
            }

            progress.BoardData = new BoardData(datas);
        }

        public void LoadProgress(PlayerProgress progress)
        {
            Dictionary<int, PartSaveData> savedValues = progress.BoardData.ToDictionary();

            foreach (var item in savedValues)
            {
                AddPart(cellTransforms[item.Key], item.Value.Level, item.Value.PartType);
            }
        }
    }
}
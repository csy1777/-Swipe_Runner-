using System.Collections.Generic;
using System;
using Zenject;

namespace KrolStudio
{
    public class WalletService : IWalletService, IProgressReader, IProgressSaver/*, IInitializable*/
    {
        private readonly ILogService logService;

        private Dictionary<int, long> wallets = new();

        // currency, old value, new value
        public Action<CurrencyType, long, long> OnWalletUpdate { get; set; }

        private Dictionary<CurrencyType, Action<long, long, object>> OnWalletUpdateByType = new();

        [Inject]
        public WalletService(ILogService logService, ISaveLoadService saveLoadService)
        {
            this.logService = logService;

            saveLoadService.Register(SaveKey.WalletService, this);

            Initialize();
        }

        public void Initialize()
        {
            ResetSubscriptions();
            ClearWallets();
        }

        public void SubscribeToWalletUpdate(CurrencyType currencyType, Action<long, long, object> callback)
        {
            if (OnWalletUpdateByType.ContainsKey(currencyType))
                OnWalletUpdateByType[currencyType] += callback;
            else
                OnWalletUpdateByType[currencyType] = callback;
        }

        public void UnsubscribeFromWalletUpdate(CurrencyType currencyType, Action<long, long, object> callback)
        {
            if (OnWalletUpdateByType.TryGetValue(currencyType, out var action))
            {
                action -= callback;

                if (action == null)
                {
                    OnWalletUpdateByType.Remove(currencyType);
                }
                else
                {
                    //Dictionary.TryGetValue(...) returns a copy of the delegate, not a reference.
                    //Changes to action do not automatically change the value in the dictionary.
                    //You must manually write the changed delegate back to the dictionary.
                    OnWalletUpdateByType[currencyType] = action;
                }
            }
        }

        private void ResetSubscriptions()
        {
            OnWalletUpdate = null;
            OnWalletUpdateByType = new();
        }

        private void ClearWallets()
        {
            wallets.Clear();

            foreach (CurrencyType type in Enum.GetValues(typeof(CurrencyType)))
            {
                if(type == CurrencyType.None) continue;
                wallets.Add((int)type, 0);
            }
        }

        public long GetAmount(CurrencyType currency)
        {
            wallets.TryGetValue((int)currency, out long value);
            return value;
        }

        public void RaiseWalletUpdate(CurrencyType currency, object sender = null)
        {
            if (OnWalletUpdateByType.TryGetValue(currency, out var callback))
            {
                long oldValue = wallets[(int)currency];
                callback?.Invoke(oldValue, oldValue, sender);
            }
        }

        public void SetAmount(CurrencyType currency, long amount, object sender = null)
        {
            long oldValue = wallets[(int)currency];

            //if (oldValue != amount)
            {
                wallets[(int)currency] = amount;

                if (OnWalletUpdateByType.TryGetValue(currency, out var callback))
                {
                    callback?.Invoke(oldValue, amount, sender);
                }

                OnWalletUpdate?.Invoke(currency, oldValue, amount);
            }
        }

        public void AddAmount(CurrencyType currency, long amount)
        {
            SetAmount(currency, GetAmount(currency) + amount);
        }

        public bool HasEnough(CurrencyType currency, long requiredAmount)
        {
            return GetAmount(currency) >= requiredAmount;
        }

        public bool TryPurchase(CurrencyType currency, long price, object sender)
        {
            if (!HasEnough(currency, price))
            return false;

            SetAmount(currency, GetAmount(currency) - price, sender);
            return true;
        }

        public void LoadProgress(PlayerProgress progress) // Initialize the class with data from a file
        {
            ClearWallets();

            Dictionary<int, long> savedValues = progress.WalletsData.ToDictionary();

            foreach (var pair in savedValues)
            {
                int currencyKey = pair.Key;
                long currencyAmount = pair.Value;

                if (!Enum.IsDefined(typeof(CurrencyType), currencyKey))
                {
                    logService.LogError($"Unknown currency type code {currencyKey} with amount {currencyAmount} on progress loading found! May be some old currency.");
                    continue;
                }

                CurrencyType currencyType = (CurrencyType)currencyKey;

                if (currencyType == CurrencyType.None)
                {
                    logService.LogError($"None currency type on progress loading found!");
                    continue;
                }

                SetAmount(currencyType, currencyAmount);
            }
        }

        public void UpdateProgress(PlayerProgress progress)
        {
            progress.WalletsData = new WalletsData(wallets);
        }
    }
}
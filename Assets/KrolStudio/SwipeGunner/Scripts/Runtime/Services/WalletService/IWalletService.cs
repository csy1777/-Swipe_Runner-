using System;

namespace KrolStudio
{
    public interface IWalletService
    {
        Action<CurrencyType, long, long> OnWalletUpdate { get; set; }
        void SubscribeToWalletUpdate(CurrencyType currencyType, Action<long, long, object> callback);
        void UnsubscribeFromWalletUpdate(CurrencyType currencyType, Action<long, long, object> callback);
        void RaiseWalletUpdate(CurrencyType currency, object sender);
        long GetAmount(CurrencyType currency);
        void SetAmount(CurrencyType currency, long amount, object sender);
        void AddAmount(CurrencyType currency, long amount);
        bool TryPurchase(CurrencyType currency, long price, object sender);
        bool HasEnough(CurrencyType currency, long requiredAmount);
    }
}
using TMPro;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class MoneyText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI coinsAmountText;

        IWalletService walletService;

        [Inject]
        void Construct(IWalletService walletService)
        {
            this.walletService = walletService;
            //coinsAmountText.text = NumberFormatter.Format(walletService.GetAmount(CurrencyType.Coins));
        }

        void OnEnable()
        {
            walletService.OnWalletUpdate += UpdateAmount;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                walletService.AddAmount(CurrencyType.Coins, 10000000);
            }
        }
#endif

        void OnDisable()
        {
            walletService.OnWalletUpdate -= UpdateAmount;
        }

        public void UpdateAmount(CurrencyType type, long oldValue, long amount)
        {
            coinsAmountText.text = NumberFormatter.Format(amount);
        }
    }
}
using TMPro;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    public class MoneyText : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI coinsAmountText;

        [Tooltip("这个文本显示哪种货币。默认金币。")]
        [SerializeField] CurrencyType currencyType = CurrencyType.Coins;

        IWalletService walletService;

        [Inject]
        void Construct(IWalletService walletService)
        {
            this.walletService = walletService;
        }

        void OnEnable()
        {
            walletService.OnWalletUpdate += UpdateAmount;

            // 首次显示当前值（以前这段被注释掉了，只有货币变化时才刷新）
            UpdateAmount(currencyType, walletService.GetAmount(currencyType), walletService.GetAmount(currencyType));
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
            // OnWalletUpdate 是全局事件：任何货币变化都会进来。
            // 必须按类型过滤，否则加了芯片之后，打怪时金币数字会被芯片数量覆盖。
            if (type != currencyType)
                return;

            coinsAmountText.text = NumberFormatter.Format(amount);
        }
    }
}
using TMPro;
using UnityEngine;
using Zenject;

namespace KrolStudio
{
    /// <summary>
    /// 临时货币显示：把某个货币的当前数量写到 TMP 文本上。
    ///
    /// 定位是"商店 UI 做出来之前的过渡方案"：
    ///   - 只显示数字，没有任何交互；
    ///   - 想在 HUD 上同时显示金币和芯片，就复制两份文本、各挂一个组件、类型不同即可；
    ///   - 商店/最终 HUD 做好后，直接把这个组件从对象上摘掉，不影响其它逻辑。
    ///
    /// 货币变化通过 WalletService 按类型的订阅拿到，不会和别的货币串显示。
    /// </summary>
    public class TemporaryText : MonoBehaviour
    {
        [Tooltip("要显示哪种货币。")]
        [SerializeField] CurrencyType currencyType = CurrencyType.Chips;

        [Tooltip("显示数字的 TMP 文本。")]
        [SerializeField] TextMeshProUGUI valueText;

        IWalletService walletService;

        [Inject]
        void Construct(IWalletService walletService)
        {
            this.walletService = walletService;
        }

        void OnEnable()
        {
            // 先显示当前值（否则要等下一次货币变化才会出现数字）
            Refresh(walletService.GetAmount(currencyType));

            walletService.SubscribeToWalletUpdate(currencyType, OnWalletUpdate);
        }

        void OnDisable()
        {
            walletService.UnsubscribeFromWalletUpdate(currencyType, OnWalletUpdate);
        }

        void OnWalletUpdate(long oldValue, long newValue, object sender)
        {
            Refresh(newValue);
        }

        void Refresh(long amount)
        {
            if (valueText == null)
                return;

            valueText.text = NumberFormatter.Format(amount);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace KrolStudio
{
    public class FireModeSwitchButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TMP_Text modeLabel;      // 显示当前模式名字
        [SerializeField] Image overheatFill;       // 过热条（可选）

        TurretShooting turretShooting;

        [Inject]
        void Construct(TurretShooting turretShooting)
        {
            this.turretShooting = turretShooting;
        }

        void OnEnable()
        {
            button.onClick.AddListener(OnClick);
            UpdateLabel();
        }

        void OnDisable()
        {
            button.onClick.RemoveListener(OnClick);
        }

        void Update()
        {
            // 过热条
            if (overheatFill != null)
            {
                overheatFill.fillAmount = turretShooting.HeatNormalized;
                overheatFill.color = turretShooting.IsOverheated ? Color.red : Color.yellow;
            }
        }

        void OnClick()
        {
            FireMode current = turretShooting.CurrentFireMode;
            FireMode[] modes = (FireMode[])System.Enum.GetValues(typeof(FireMode));
            int nextIndex = ((int)current + 1) % modes.Length;
            turretShooting.SetFireMode(modes[nextIndex]);
            UpdateLabel();
        }

        void UpdateLabel()
        {
            if (modeLabel != null)
            {
                modeLabel.text = turretShooting.CurrentFireMode switch
                {
                    FireMode.Single   => "单发",
                    FireMode.Burst    => "连发",
                    FireMode.Auto     => "自动",
                    FireMode.Overheat => "过热",
                    _                 => "—"
                };
            }
        }
    }
}

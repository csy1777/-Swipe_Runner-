using UnityEngine;

namespace KrolStudio
{

    [CreateAssetMenu(fileName = "FireModeConfig", menuName = "Configs/FireModeConfig")]
    public class FireModeConfig : ScriptableObject
    {
        [Header("初始射击模式")]
        public FireMode defaultFireMode = FireMode.Single;

        [Header("连发模式")]
        [Tooltip("每次按下打几发")]
        public int burstCount = 3;

        [Tooltip("连发时每发之间的间隔（秒）")]
        public float burstInterval = 0.08f;

        [Header("过热模式")]
        [Tooltip("最大热量，到达后无法射击")]
        public float maxHeat = 100f;

        [Tooltip("每发子弹产生的热量")]
        public float heatPerShot = 5f;

        [Tooltip("不射击时每秒散热量")]
        public float coolDownRate = 15f;

        [Tooltip("从过热恢复需要的冷却时间（秒）")]
        public float overheatRecoveryTime = 2f;
    }
}

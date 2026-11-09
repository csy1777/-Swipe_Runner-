
namespace KrolStudio
{
    public class PlayerProgress 
    {
        public int CurrentLevelIndex = 1;
        public WalletsData WalletsData;
        public BoardData BoardData;
        public PlayerData PlayerData;
        public int IncomeLevel = 1;
        public int AddPartlevel = 1;

        public FireMode SelectedFireMode = FireMode.Single;
        // Settings
        public bool Vibrate;
        public float MusicVolume;
        public float SfxVolume;

        public bool IsTutorialShown = false;

        public bool IsForcedAdEnabled = true;
    }
}
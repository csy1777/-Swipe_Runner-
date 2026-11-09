using System;

namespace KrolStudio
{
    public interface IHudView
    {
        void DisplayMeterProgress(bool value);
        void UpdateMeterProgress(MovementTracker tracker);
        void UpdateLevelIndicator(int level);
        void ShowOutOfBulletsMessage();
        void ShowItemMaxLevelMessage();
        void ShowIncomeUpMessage();
    }

    public interface IRocketHud
    {
        bool IsReloading();
        void DisplayRocketLauncher(bool value);
        void ShowRocketLaunchFailureMessage();
        void SubscribeToRocketLaunch(Func<bool> handler);
        void UnsubscribeFromRocketLaunch(Func<bool> handler);
    }

    public interface IOverHeatHudView
    {
        void DisplayOverHeat(bool value);
    }
}
using System;
namespace CritterCamp.Core.Screens.UIScreens {
    public interface IOfflineScreen {
        void ShowControls(bool show);
        void MediaPlayerMuted(bool muted);
        void AppendStatusText(string text);
        void UpdateStatusText(string text);
        void ShowUserInput(bool show);
        void ShowResume(bool show);
        void ShowAdDuplex(bool show);
        void GoToNextScreen(Type screen);
    }
}
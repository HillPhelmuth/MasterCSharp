using System;
using System.Collections.Generic;
using System.Linq;
using BlazorApp.Shared.CodeModels;
using BlazorApp.Shared.UserModels;
using BlazorApp.Shared.VideoModels;

namespace BlazorApp.Shared
{
    public class AppStateService
    {
        public CodeChallenges CodeChallenges { get; private set; }
        public Videos Videos { get; private set; }
        public UserAppData UserAppData { get; private set; }
        public string UserName { get; private set; }
        public CodeOutputModel CodeOutput { get; private set; }
        public bool HasUser { get; private set; }
        public string ShareUser { get; private set; }
        public string OtherUser { get; private set; }
        public string ShareTeam { get; private set; }
        public event Action OnChange;
        public event Action<int> OnTabChange;

        public void SetCodeChallenges(CodeChallenges codeChallenges)
        {
            CodeChallenges = codeChallenges;
            NotifyStateHasChanged();
        }
        public void SetVideos(Videos videos)
        {
            Videos = videos;
            NotifyStateHasChanged();
        }

        public void UpdateShareUser(string userName)
        {
            ShareUser = userName;
            NotifyStateHasChanged();
        }
        public void UpdateShareUser(string shareUser, string otherUser)
        {
            ShareUser = shareUser;
            OtherUser = otherUser;
            NotifyStateHasChanged();
        }
        public void UpdatePrivateUser(string otherUser)
        {
            OtherUser = otherUser;
            NotifyStateHasChanged();
        }
        public void UpdateShareTeam(string teamName)
        {
            ShareTeam = teamName;
            NotifyStateHasChanged();
        }
        public void AddVideo(Video video)
        {
            if (video.VideoSectionID == 0) return;
            var videos = Videos;
            foreach (var section in videos.VideoSections.Where(section => section.ID == video.VideoSectionID))
            {
                section.Videos?.Add(video);
            }
            Videos = videos;
            NotifyStateHasChanged();
        }
        public void UpdateChallenges(Challenge challenge)
        {
            CodeChallenges.Challenges.Add(challenge);
            NotifyStateHasChanged();
        }
        public void UpdateUserName(string name)
        {
            UserName = name;
            HasUser = true;
            NotifyStateHasChanged();
        }

        public void UpdateUserAppData(UserAppData userData)
        {
            UserAppData = userData;
            UserName = userData.Name;
            HasUser = true;
            NotifyStateHasChanged();
        }
        public void UpdateCodeOutput(CodeOutputModel codeOutput)
        {
            foreach (var output in codeOutput.Outputs ?? new List<Output>())
            {
                output.CssClass = output.TestResult ? "testPass" : "testFail";
            }
            CodeOutput = codeOutput;
            Console.WriteLine($"Output State Updated");
            NotifyStateHasChanged();
        }

        public void UpdateTabNavigation(int tab) => OnTabChange?.Invoke(tab);
        private void NotifyStateHasChanged() => OnChange?.Invoke();
    }
}

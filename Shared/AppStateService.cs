using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BlazorApp.Shared.CodeModels;
using BlazorApp.Shared.UserModels;
using BlazorApp.Shared.VideoModels;

namespace BlazorApp.Shared
{
    public class AppStateService : INotifyPropertyChanged
    {
        private CodeChallenges codeChallenges;
        private Videos videos;
        private UserAppData userAppData;
        private string userName;
        private CodeOutputModel codeOutput;
        private string shareUser;
        private string otherUser;
        private string shareTeam;
        private int tabIndex;

        public CodeChallenges CodeChallenges
        {
            get => codeChallenges;
            set { codeChallenges = value; OnPropertyChanged(); }
        }

        public Videos Videos
        {
            get => videos;
            set { videos = value; OnPropertyChanged(); }
        }

        public UserAppData UserAppData
        {
            get => userAppData;
            set { userAppData = value; OnPropertyChanged(); }
        }

        public string UserName
        {
            get => userName;
            set { userName = value; OnPropertyChanged(); }
        }

        public CodeOutputModel CodeOutput
        {
            get => codeOutput;
            set { codeOutput = value; OnPropertyChanged();}
        }

        public bool HasUser { get; set; }

        public string ShareUser
        {
            get => shareUser;
            set { shareUser = value; OnPropertyChanged();}
        }

        public string OtherUser
        {
            get => otherUser;
            set { otherUser = value; OnPropertyChanged();}
        }

        public string ShareTeam
        {
            get => shareTeam;
            set { shareTeam = value; OnPropertyChanged();}
        }

        public int TabIndex
        {
            get => tabIndex;
            set
            {
                var tab = value;
                if (tab < 0 || tab > 4)
                    tabIndex = 0;
                else
                    tabIndex = tab;
                OnPropertyChanged();
            }
        }

        public event Action OnChange;
        public event Action<int> OnTabChange;

        //public void SetCodeChallenges(CodeChallenges codeChallenges)
        //{
        //    CodeChallenges = codeChallenges;
        //    //NotifyStateHasChanged();
        //}
        //public void SetVideos(Videos videos)
        //{
        //    Videos = videos;
        //    //NotifyStateHasChanged();
        //}

        //public void UpdateShareUser(string userName)
        //{
        //    ShareUser = userName;
        //    //NotifyStateHasChanged();
        //}
       
        //public void UpdatePrivateUser(string otherUser)
        //{
        //    OtherUser = otherUser;
        //    //NotifyStateHasChanged();
        //}
        //public void UpdateShareTeam(string teamName)
        //{
        //    ShareTeam = teamName;
        //    NotifyStateHasChanged();
        //}
        public void AddVideo(Video video)
        {
            if (video.VideoSectionID == 0) return;
            var videos = Videos;
            foreach (var section in videos.VideoSections.Where(section => section.ID == video.VideoSectionID))
            {
                section.Videos?.Add(video);
            }
            Videos = videos;
            OnPropertyChanged(nameof(Videos));
        }
        public void UpdateChallenges(Challenge challenge)
        {
            CodeChallenges.Challenges.Add(challenge);
            OnPropertyChanged(nameof(CodeChallenges));
        }
        public void UpdateUserName(string name)
        {
            UserName = name;
            HasUser = true;
        }

        public void UpdateUserAppData(UserAppData userData)
        {
            UserAppData = userData;
            UserName = userData.Name;
            HasUser = true;
        }
        //public void UpdateCodeOutput(CodeOutputModel codeOutput)
        //{
        //    foreach (var output in codeOutput.Outputs ?? new List<Output>())
        //    {
        //        output.CssClass = output.TestResult ? "testPass" : "testFail";
        //    }
        //    CodeOutput = codeOutput;
        //    Console.WriteLine($"Output State Updated");
        //   // NotifyStateHasChanged();
        //}
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void UpdateTabNavigation(int tab) => OnTabChange?.Invoke(tab);
        //private void NotifyStateHasChanged() => OnChange?.Invoke();
    }
}

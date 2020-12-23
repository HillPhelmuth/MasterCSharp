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
    public class AppState : INotifyPropertyChanged
    {
        private CodeChallenges codeChallenges;
        private Videos videos;
        private UserAppData userAppData;
        private string userName;
        private CodeOutputModel codeOutput;
        private string shareUser;
        private string otherUser;
        private int tabIndex;
        private bool hasUser;

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
            set { codeOutput = value; OnPropertyChanged(); }
        }

        public bool HasUser
        {
            get => hasUser;
            set { hasUser = value; OnPropertyChanged(); }
        }

        public string ShareUser
        {
            get => shareUser;
            set { shareUser = value; OnPropertyChanged(); }
        }
        public string OtherUser
        {
            get => otherUser;
            set { otherUser = value; OnPropertyChanged(); }
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

        public void AddVideo(Video video)
        {
            if (video.VideoSectionID == 0) return;
            var vids = Videos;
            foreach (var section in vids.VideoSections.Where(section => section.ID == video.VideoSectionID))
            {
                section.Videos?.Add(video);
            }
            Videos = vids;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

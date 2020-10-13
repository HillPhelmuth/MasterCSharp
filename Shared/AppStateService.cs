using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MasterCSharp.Shared.CodeModels;
using MasterCSharp.Shared.RazorCompileService;
using MasterCSharp.Shared.UserModels;
using MasterCSharp.Shared.VideoModels;

namespace MasterCSharp.Shared
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
        private UserProject activeProject;
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

        public bool HasUser { get; set; }

        public string ShareUser
        {
            get => shareUser;
            set { shareUser = value; OnPropertyChanged(); }
        }
        public UserProject ActiveProject
        {
            get => activeProject;
            set { activeProject = value; OnPropertyChanged(); }
        }

        public bool HasActiveProject => ActiveProject != null || activeProject != null;
        public void SaveFileToProject(ProjectFile file)
        {
            if (ActiveProject == null) return;
            ActiveProject.Files ??= new List<ProjectFile>();
            var alteredFile = ActiveProject.Files.FirstOrDefault(x => x.Path == file.Path);
            if (alteredFile == null)
            {
                ActiveProject.Files?.Add(file);
            }
            else
            {
                alteredFile.Content = file.Content;
            }
            OnPropertyChanged(nameof(ActiveProject));

        }
        public string OtherUser
        {
            get => otherUser;
            set { otherUser = value; OnPropertyChanged(); }
        }

        public string ShareTeam
        {
            get => shareTeam;
            set { shareTeam = value; OnPropertyChanged(); }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}

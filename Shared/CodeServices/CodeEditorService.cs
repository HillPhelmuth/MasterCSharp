using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using MasterCSharp.Shared.RazorCompileService;

namespace MasterCSharp.Shared.CodeServices
{
    public class CodeEditorService : INotifyPropertyChanged
    {
        private string _monacoCode;
        private string _codeSnippet;
        private string _sharedCodeSnippet;
        private string _currentOutput;
        private string _language;
        private ProjectFile _activeProjectFile;
        private List<ProjectFile> _codeFiles;
        public List<ProjectFile> CodeFiles
        {
            get => _codeFiles;
            set { _codeFiles = value; OnPropertyChanged(); }
        }
        public ProjectFile ActiveProjectFile
        {
            get => _activeProjectFile;
            set { _activeProjectFile = value; OnPropertyChanged(); }
        }
        public string Language
        {
            get => _language;
            set { _language = value; OnPropertyChanged(); }
        }
        public string MonacoCode
        {
            get => _monacoCode;
            set { _monacoCode = value; OnPropertyChanged(); }
        }

        public string CodeSnippet
        {
            get => _codeSnippet;
            set { _codeSnippet = value; OnPropertyChanged(); }
        }

        public string SharedCodeSnippet
        {
            get => _sharedCodeSnippet;
            set { _sharedCodeSnippet = value; OnPropertyChanged(); }
        }

        public string CurrentOutput
        {
            get => _currentOutput;
            set { _currentOutput = value; OnPropertyChanged();}
        }

        public void SaveCode(ProjectFile projectFile)
        {
            CodeFiles ??= new List<ProjectFile>();
            if (CodeFiles.All(x => x.Path != projectFile.Path))
            {
                CodeFiles.Add(projectFile);
                OnPropertyChanged(nameof(CodeFiles));
                return;
            }

            foreach (var file in CodeFiles.Where(file => file.Path == projectFile.Path))
            {
                file.Content = projectFile.Content;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Console.WriteLine($"property changed: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

   
}

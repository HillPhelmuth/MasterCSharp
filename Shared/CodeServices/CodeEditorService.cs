using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BlazorApp.Shared.RazorCompileService;


namespace BlazorApp.Shared.CodeServices
{
    public class CodeEditorService : INotifyPropertyChanged
    {
        private string _monacoCode;
        private string _codeSnippet;
        private string _sharedCodeSnippet;
        private string _currentOutput;
        private string _language;
        private CodeFile _activeCodeFile;
        private List<CodeFile> _codeFiles;
        public List<CodeFile> CodeFiles
        {
            get => _codeFiles;
            set { _codeFiles = value; OnPropertyChanged(); }
        }
        public CodeFile ActiveCodeFile
        {
            get => _activeCodeFile;
            set { _activeCodeFile = value; OnPropertyChanged(); }
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

        public void SaveCode(CodeFile codeFile)
        {
            CodeFiles ??= new List<CodeFile>();
            if (CodeFiles.All(x => x.Path != codeFile.Path))
            {
                CodeFiles.Add(codeFile);
                OnPropertyChanged(nameof(CodeFiles));
                return;
            }

            foreach (var file in CodeFiles.Where(file => file.Path == codeFile.Path))
            {
                file.Content = codeFile.Content;
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

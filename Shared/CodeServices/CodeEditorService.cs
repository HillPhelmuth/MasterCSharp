using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace BlazorApp.Shared.CodeServices
{
    public class CodeEditorService : INotifyPropertyChanged
    {
        private string _monacoCode;
        private string _codeSnippet;
        private string _sharedCodeSnippet;
        private string _currentOutput;

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
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

   
}

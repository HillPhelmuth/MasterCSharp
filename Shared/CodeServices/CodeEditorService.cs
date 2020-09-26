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

        public event Func<Task> Evaluate;
        public event Func<Task> OnSnippetChange;
        public event Func<Task> OnSharedSnippetChange; 
        public void UpdateSnippet(string codeSnippet)
        {
            CodeSnippet = codeSnippet;
            NotifyNewSnippet();
            //Console.WriteLine($"Event Fired - Snippet updated to {codeSnippet}");
        }

        public void UpdateShardSnippet(string codeSnippet)
        {
            SharedCodeSnippet = codeSnippet;
            NotifyNewSharedSnippet();
        }

        public void EvaluateCode(string code)
        {
            MonacoCode = code;
            NotifyEvaluate();
        }

        private async void NotifyEvaluate()
        {
            if (Evaluate != null) await Evaluate?.Invoke();
        }
        private async void NotifyNewSnippet()
        {
            if (OnSnippetChange != null) await OnSnippetChange?.Invoke();
        }

        private async void NotifyNewSharedSnippet()
        {
            if (OnSharedSnippetChange != null) await OnSharedSnippetChange?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

      
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

   
}

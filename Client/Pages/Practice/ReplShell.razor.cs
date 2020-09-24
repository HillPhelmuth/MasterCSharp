using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BlazorApp.Shared;
using BlazorApp.Shared.CodeServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using TextCopy;

namespace BlazorApp.Client.Pages.Practice
{
    public partial class ReplShell : ComponentBase, IDisposable
    {
        [Inject]
        protected NavigationManager NavigationManager { get; set; }
        [Inject]
        public CodeEditorService CodeEditorService { get;  set; }
        [Inject]
        protected AppStateService AppStateService { get; set; }
        [Inject]
        public IClipboard Clipboard { get; set; }
        public string InfoOutput { get; set; } = "";
        public string Input { get; set; } = "";
        public List<string> InputList { get; set; } = new List<string>();
        protected CSharpCompilation runningCompilation;
        protected IEnumerable<MetadataReference> References;
        protected object[] submissionStates = { null, null };
        protected int submissionIndex = 0;
        protected List<string> history = new List<string>();
        protected int historyIndex = 0;
        protected string CodeOutput { get; set; }
        [Parameter]
        public EventCallback<string> CodeOutputChanged { get; set; }
        
        protected override Task OnInitializedAsync()
        {

            References = AppStateService.References;
            AppStateService.OnChange += StateHasChanged;
            CodeEditorService.Evaluate += SubmitMonaco;
            return base.OnInitializedAsync();
        }

        public async Task CopyHistoryToClipboard()
        {
            var codeInputs = string.Join("\n", InputList);
            await Clipboard.SetTextAsync(codeInputs);
        }
        protected void OnKeyDown(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "ArrowUp" when historyIndex > 0:
                    historyIndex--;
                    Input = history[historyIndex];
                    break;
                case "ArrowDown" when historyIndex + 1 < history.Count:
                    historyIndex++;
                    Input = history[historyIndex];
                    break;  
                case "Escape":
                    Input = "";
                    historyIndex = history.Count;
                    break;
            }
        }
        public async Task SubmitMonaco()
        {
            var code = CodeEditorService.MonacoCode;
            await RunSubmission(code);
            StateHasChanged();
        }
        public async Task Run(KeyboardEventArgs e)
        {
            if (e.Key != "Enter")
                return;

            var code = Input;
            if (!string.IsNullOrEmpty(code))
                history.Add(code);
            historyIndex = history.Count;
            Input = "";
            //CodeComponentBase.HistoryShared = history;
            await RunSubmission(code);
        }
        public async Task RunSubmission(string code)
        {
            InfoOutput += $@"<br /><span class=""info"">{HttpUtility.HtmlEncode(code)}</span>";
            InputList.Add(code);
            
            var previousOut = Console.Out;
            try
            {
                if (TryCompile(code, out var script, out var errorDiagnostics))
                {
                    var writer = new StringWriter();
                    Console.SetOut(writer);

                    var entryPoint = runningCompilation.GetEntryPoint(CancellationToken.None);
                    var type = script.GetType($"{entryPoint.ContainingNamespace.MetadataName}.{entryPoint.ContainingType.MetadataName}");
                    var entryPointMethod = type.GetMethod(entryPoint.MetadataName);

                    var submission = (Func<object[], Task>)entryPointMethod.CreateDelegate(typeof(Func<object[], Task>));

                    if (submissionIndex >= submissionStates.Length)
                    {
                        Array.Resize(ref submissionStates, Math.Max(submissionIndex, submissionStates.Length * 2));
                    }

                    var returnValue = await (Task<object>)submission(submissionStates);
                    if (returnValue != null)
                    {
                        Console.WriteLine(CSharpObjectFormatter.Instance.FormatObject(returnValue));
                    }

                    CodeOutput = writer.ToString();
                    await CodeOutputChanged.InvokeAsync(CodeOutput);
                    var output = HttpUtility.HtmlEncode(writer.ToString());
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        InfoOutput += $"<br />{output}";
                    }
                }
                else
                {
                    var errorOutput = "";
                    foreach (var diag in errorDiagnostics)
                    {
                        InfoOutput += $@"<br / ><span class=""error"">CompileError: {HttpUtility.HtmlEncode(diag)}</span>";
                        errorOutput += HttpUtility.HtmlEncode(diag);
                    }

                    CodeOutput = $"COMPILE ERROR: {errorOutput}";
                    await CodeOutputChanged.InvokeAsync(CodeOutput);
                }
            }
            catch (Exception ex)
            {
                InfoOutput += $@"<br /><span class=""error"">Catch: {HttpUtility.HtmlEncode(CSharpObjectFormatter.Instance.FormatException(ex))}</span>";
            }
            finally
            {
                Console.SetOut(previousOut);
            }
        }


        //Tries to compile, if successful, it outputs the DLL Assembly. If unsuccessful, it will output the error message
        protected bool TryCompile(string source, out Assembly assembly, out IEnumerable<Diagnostic> errorDiagnostics)
        {
            assembly = null;
            var scriptCompilation = CSharpCompilation.CreateScriptCompilation(
                Path.GetRandomFileName(),
                CSharpSyntaxTree.ParseText(source, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script).WithLanguageVersion(LanguageVersion.Preview)),
                References,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: new[]
                {
                    "System",
                    "System.IO",
                    "System.Collections.Generic",
                    "System.Collections",
                    "System.Console",
                    "System.Diagnostics",
                    "System.Dynamic",
                    "System.Linq",
                    "System.Linq.Expressions",
                    "System.Net.Http",
                    "System.Text",
                    "System.Threading.Tasks"
                }),
                runningCompilation
            );

            errorDiagnostics = scriptCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error);
            if (errorDiagnostics.Any())
            {
                return false;
            }

            using var peStream = new MemoryStream();
            var emitResult = scriptCompilation.Emit(peStream);

            if (!emitResult.Success) return false;
            submissionIndex++;
            runningCompilation = scriptCompilation;
            assembly = Assembly.Load(peStream.ToArray());
            return true;

        }

        public void Dispose()
        {
            Console.WriteLine("ReplShell.razor Disposed");
            AppStateService.OnChange -= StateHasChanged;
            CodeEditorService.Evaluate -= SubmitMonaco;
        }
    }
}

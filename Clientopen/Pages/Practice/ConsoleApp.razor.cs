using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Blazor.ModalDialog;
using MasterSharpOpen.Shared;
using MasterSharpOpen.Shared.CodeModels;
using MasterSharpOpen.Shared.CodeServices;
using MasterSharpOpen.Client.ExtensionMethods;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MasterSharpOpen.Client.Pages.Practice
{
    public partial class ConsoleApp : IDisposable
    {
        public string Output = "";
        protected string CodeSnippet;
        [Inject]
        protected AppStateService AppState { get; set; }
        [Inject]
        protected IJSRuntime JsRuntime { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        public CodeEditorService CodeEditorService { get; set; }
        [Inject]
        public  PublicClient PublicClient { get; set; }
        
       

        private string code;
        bool isCodeCompiling;
        private bool codeReady;
        protected override Task OnInitializedAsync()
        {
            codeReady = false;
            AppState.OnCloseConsole += HandleCloseConsole;
            return base.OnInitializedAsync();
        }

        private void HandleCloseConsole()
        {
            codeReady = false;
            Console.WriteLine("Console Close Handled");
            StateHasChanged();
        }
        private Task BeginConsoleApp()
        {
            CodeSnippet = CodeSnippets.ConsoleInput;
            CodeEditorService.UpdateSnippet(CodeSnippet);
            codeReady = true;
            StateHasChanged();
            return Task.CompletedTask;
        }
        protected async Task OnSubmit(string codeInput)
        {
            var sw = new Stopwatch();
            string userInput = "";
            const string readlinePattern = "Console.ReadLine()";
            var tempCode = codeInput;
            isCodeCompiling = true;
            StateHasChanged();
            if (!codeInput.Contains(readlinePattern))
            {
                sw.Start();
                code = codeInput;
                Output = await PublicClient.SubmitConsole(code);
                sw.Stop();
                Console.WriteLine($"console function: {sw.ElapsedMilliseconds}ms");
                isCodeCompiling = false;
                StateHasChanged();
                return;
            }
            var inputDictionary = new Dictionary<int, DataInputFormStringField>();
            var readLineIndexes = tempCode.AllIndexesOf(readlinePattern);
            var regex = new Regex(Regex.Escape(readlinePattern));
            var inputForm = new ModalDataInputForm("User Inputs", "User console input");
            
            for (int i = 1; i <= readLineIndexes.Count(); i++)
            {
                userInput = "";
                var inputField1 = inputForm.AddStringField($"Input{i}", $"{readlinePattern} {i}", userInput, "The user's input.");
                inputDictionary.Add(i, inputField1);
            }

            if (await inputForm.ShowAsync(ModalService))
            {
                int j = 1;
                tempCode = regex.Replace(tempCode, m =>
                 {
                     var input = inputDictionary[j].Value;
                     Console.WriteLine($"Console.ReadLine() replaced with \"{input}\"");
                     j++;
                     return $"\"{input}\"";
                 });
            }
            sw.Start();
            code = tempCode;
            Output = await PublicClient.SubmitConsole(code);
            sw.Stop();
            Console.WriteLine($"console function: {sw.ElapsedMilliseconds}ms");
            isCodeCompiling = false;
            StateHasChanged();
           
        }

        public void Run()
        {
            //CompilerService.WhenReady(RunInternal);
        }

        //private async Task RunInternal()
        //{
        //    Output = "";

        //    Console.WriteLine("Compiling and Running code");
        //    var sw = Stopwatch.StartNew();

        //    var currentOut = Console.Out;
        //    var writer = new StringWriter();
        //    Console.SetOut(writer);
            
        //    Exception exception = null;
        //    try
        //    {
        //        var (success, asm) = CompilerService.TryCompileConsole(code);
        //        if (success)
        //        {
        //            var entry = asm.EntryPoint;
        //            if (entry.Name == "<Main>") // sync wrapper over async Task Main
        //            {
        //                entry = entry.DeclaringType.GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static); // reflect for the async Task Main
        //            }
        //            var hasArgs = entry.GetParameters().Length > 0;
        //            var result = entry.Invoke(null, hasArgs ? new object[] { new string[0] } : null);
        //            if (result is Task t)
        //            {
        //                await t;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        exception = ex;
        //    }
        //    Output = writer.ToString();
        //    if (exception != null)
        //    {
        //        Output += "\r\n" + exception;
        //    }
        //    Console.SetOut(currentOut);

        //    sw.Stop();
        //    Console.WriteLine("Done in " + sw.ElapsedMilliseconds + "ms");
        //    dialogIsOpen = true;
        //    StateHasChanged();
        //}

        //protected StandaloneEditorConstructionOptions EditorOptionsPuzzle(MonacoEditor editor)
        //{
        //    return new StandaloneEditorConstructionOptions
        //    {
        //        AutomaticLayout = true,
        //        AutoIndent = true,
        //        HighlightActiveIndentGuide = true,
        //        Language = "csharp",
        //        Value = CodeSnippet ?? "private string MyProgram() \n" +
        //            "{\n" +
        //            "    string input = \"this does not\"; \n" +
        //            "    string modify = input + \" suck!\"; \n" +
        //            "    return modify;\n" +
        //            "}\n" +
        //            "return MyProgram();"
        //    };
        //}


        //protected async Task EditorOnDidInit(MonacoEditor editor)
        //{
        //    await Editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (editor, keyCode) =>
        //    {
        //        Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
        //    });
        //}

        //protected void OnContextMenu(EditorMouseEvent eventArg)
        //{
        //    Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        //}
        public void Dispose()
        {
            Console.WriteLine("ConsoleApp.razor Disposed");
            AppState.OnCloseConsole -= HandleCloseConsole;
        }
    }
}

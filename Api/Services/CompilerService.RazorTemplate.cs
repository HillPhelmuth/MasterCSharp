using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;

namespace BlazorApp.Api.Services
{
    public partial class CompilerService
    {
        public string GenerateCodeFromTemplate(string template)
        {
            
            RazorProjectEngine engine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                RazorProjectFileSystem.Create(@"."),
                (builder) =>
                {
                    builder.SetNamespace("MyNamespace");
                });

            string fileName = Path.GetRandomFileName();

            RazorSourceDocument document = RazorSourceDocument.Create(template, fileName);

            RazorCodeDocument codeDocument = engine.Process(
                document,
                null,
                new List<RazorSourceDocument>(),
                new List<TagHelperDescriptor>());

            RazorCSharpDocument razorCSharpDocument = codeDocument.GetCSharpDocument();

            return razorCSharpDocument.GeneratedCode;
        }
    }
}

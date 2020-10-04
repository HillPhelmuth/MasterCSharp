namespace BlazorApp.Shared.RazorCompileService
{
    public class RazorFileModel
    {
        public RazorFileModel(string filePath, string fileContent)
        {
            FilePath = filePath;
            Content = fileContent;
        }
        public RazorFileModel(){}
        public string FilePath { get; set; }

        // Combination of html and razor syntax
        public string Content { get; set; }
    }
}

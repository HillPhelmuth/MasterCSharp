namespace BlazorApp.Shared.RazorCompileService
{
    public static class CoreConstants
    {
        public const string MainComponentFilePath = "__RazorOutput.razor";
        public const string MainComponentDefaultFileContent = @"<h1>Hello World</h1>

@code {

}
";
        public const string MainComponentCodePrefix = "@page \"/__razorOutput\"\n";
        public const string MainUserPagePath = "/__razorOutput";
        
    }
}

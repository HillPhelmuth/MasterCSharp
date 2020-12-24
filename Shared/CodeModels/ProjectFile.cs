namespace BlazorApp.Shared.CodeModels
{
    public class ProjectFile
    {
        public int ID { get; set; }
        public int UserProjectID { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }
    }
}

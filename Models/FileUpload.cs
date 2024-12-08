namespace McqTask.Models
{
    public class FileUpload
    {
        public IFormFile UploadedFile { get; set; }
        public List<int> UnparsedQuestionNumbers { get; set; } = new List<int>();
    }
}

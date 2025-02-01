namespace McqTask.Models
{
    public class FileUpload
    {
        public int ExamId { get; set; }
        public IFormFile UploadedFile { get; set; }
        public List<int> UnparsedQuestionNumbers { get; set; } = new List<int>();
        public bool IsAnswerWithDot { get; set; } // ✅ This is a property

    }
}

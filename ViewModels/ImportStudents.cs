namespace McqTask.ViewModels
{
    public class ImportStudents
    {
        public int GroupId { get; set; }
        public IFormFile UploadedFile { get; set; }
        public List<string> AlreadyExistsStudents { get; set; } = new List<string>();
   
    }
}

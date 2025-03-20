namespace McqTask.ViewModels
{
    public class ExamViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int CategoryId { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsPracticeMode { get; set; }
        public bool IsPublic { get; set; }
        public int ExamTime { get; set; }

        // Stores selected group IDs
        public List<int> ExamGroups { get; set; } = new List<int>();
    }

}

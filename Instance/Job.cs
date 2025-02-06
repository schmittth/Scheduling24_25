namespace Projektseminar.Instance
{
    internal class Job
    {
        //Eigenschaften
        public int Id { get; } //Identifikator jedes Jobs
        public int TotalDuration { get; set; } //Kumulierte Laufzeit aller im job enthaltenen Tasks

        public List<Task> Tasks
        {
            get => tasks;
            set => tasks = value;
        }

        //Variablen
        private List<Task> tasks;

        //Konstruktoren
        public Job(int id, int tasksLength = 0)
        {
            Id = id;
            tasks = new List<Task>();
        }

        //Methoden

    }
}

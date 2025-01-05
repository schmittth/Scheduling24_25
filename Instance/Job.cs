namespace Projektseminar.Instance
{
    internal class Job
    {
        //Eigenschaften
        public int Id { get; }
        public int TotalDuration { get; set; } //Kumulierte Laufzeit aller in ihm enthaltenen Tasks

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

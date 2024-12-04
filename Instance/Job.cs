namespace Projektseminar.Instance
{
    internal class Job
    {
        //Eigenschaften
        public Guid Guid { get; }
        public int Id { get; }

        public int TotalDuration { get; set; }

        public List<Task> Tasks
        {
            get => tasks;
            set => tasks = value;
        }

        //Variablen
        private List<Task> tasks = new List<Task>();

        //Konstruktoren
        public Job(int id)
        {
            Guid = Guid.NewGuid();
            Id = id;

        }

        //Methoden

    }
}

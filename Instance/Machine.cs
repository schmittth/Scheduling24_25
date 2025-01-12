namespace Projektseminar.Instance
{
    internal class Machine
    {
        //Eigenschaften
        public int Id { get; set; }
        public List<Task> Schedule
        {
            get => schedule;
            set => schedule = value;
        }

        public int Load { get; set; }

        //Variablen
        public List<Projektseminar.Instance.Task> schedule;

        //Konstruktoren
        public Machine(int id, int scheduleLength = 0)
        {
            Id = id;
            schedule = new List<Task>(scheduleLength);
        }
    }
}

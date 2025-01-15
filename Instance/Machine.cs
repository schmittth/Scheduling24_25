namespace Projektseminar.Instance
{
    internal class Machine
    {
        //Eigenschaften
        public int Id 
        { 
            get => id; 
            set => id = value; 
        }
        public List<Task> Schedule
        {
            get => schedule;
            set => schedule = value;
        }

        public int Load { get; set; }

        //Variablen
        private int id;
        private List<Projektseminar.Instance.Task> schedule;
        private int load;

        //Konstruktoren
        public Machine(int id, int scheduleLength = 0)
        {
            this.id = id;
            schedule = new List<Task>(scheduleLength);
        }
    }
}

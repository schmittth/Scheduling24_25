namespace Projektseminar.Instance
{
    internal class Machine
    {
        //Eigenschaften
        public int Id  //Identifikator jeder Maschine
        { 
            get => id; 
            set => id = value; 
        }
        public List<Task> Schedule //Ablaufplan jeder Maschine
        {
            get => schedule;
            set => schedule = value;
        }

        public int Load { get; set; } //Load jeder Maschine

        //Variablen
        private int id;
        private List<Task> schedule;
        private int load;

        //Konstruktoren
        public Machine(int id, int scheduleLength = 0)
        {
            this.id = id;
            schedule = new List<Task>(scheduleLength);
        }
    }
}

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

        public List<Task> schedule = new List<Task>();

        //Konstruktoren
        public Machine(int id)
        {
            Id = id;
        }
    }
}

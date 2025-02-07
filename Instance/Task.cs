using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Google.OrTools.Sat;

namespace Projektseminar.Instance
{
    internal class Task : IComparable
    {
        //Eigenschaften
        public int Id { get; set; } //Id ist der Identifier für jeden Task innerhalb seines Jobs
        public Machine Machine { get; set; } //Machine ist die Maschine auf welcher der Task ausgeführt wird
        public int Setup { get; set; } //Setup ist die dem Task vorgelagerte Setupzeit
        public Job Job { get; set; } //Job ist der Auftrag zu welchem der Task gehört
        public int Start  //Start ist die Zeit wann der Task tatsächlich beginnt
        {
            get => start;
            set
            {
                start = value;
                end = value + duration; //Ende wird automatisch kalkuliert
            }
        }
        public int Duration //Duration ist die BEarbeitungszeit des Tasks
        { 
            get => duration; 
        } 
        public int End //End ist die Zeit wann der Task tatsächlich zu Ende ist
        { 
            get => end ; 
        }
        public int Position { get; set; } //Position ist die Position dieses Tasks im aktuellen Maschinenplan
        public int Tail { get; set; } //Tail ist die Länge des längsten Pfades vom aktuellen Task bis zum Ende.
        public Task preMachineTask { get; set; } //preMachineTask ist der vorhergehende Tasks auf der gleichen Maschine
        public Task sucMachineTask { get; set; } //sucMachineTask ist der nachfolgende Task auf der gleichen Maschine
        public Task preJobTask { get; set; } //preJobTask ist der vorhergehende Task im gleichen Job
        public Task sucJobTask { get; set; } //sucJobTask ist der nachfolgende Task im gleichen Job
        public IntVar StartIntVar { get; set; }
        public IntVar EndIntVar { get; set; }
        public IntervalVar DurationIntVar { get; set; }


        //Variablen
        private int start;
        private int duration;
        private int end;

        //Konstruktoren

        //Konstruktor um Task mit grundsätzlichen Werten zu erstellen
        public Task(Machine machine, Job job, int duration, int id)
        {
            Machine = machine;
            Job = job;
            this.duration = duration;
            Id = id;
        }

        public Task(int job, int duration, int id)
        {

        }
        public Task(int duration, int id)
        {
            this.duration = duration;
            Id = id;
        }

        //Methoden

        //Vergleichsmethode für IComparable. Wird für OR Solver benötigt.
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            Task task = obj as Task;

            if (task != null)
            {
                if (Start != task.Start)
                    return Start.CompareTo(task.Start);
                else
                    return Duration.CompareTo(task.Duration);
            }
            else
                throw new ArgumentException("Object is not a Temperature");
        }
    }
}

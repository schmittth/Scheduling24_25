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
        public int Start { get; set; } //Start ist die Zeit wann der Task tatsächlich beginnt
        public int End { get; set; } //End ist die Zeit wann der Task tatsächlich zu Ende ist
        public int Duration { get; set; } //Duration ist die BEarbeitungszeit des Tasks
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

        //Konstruktoren

        //Konstruktor um Task mit grundsätzlichen Werten zu erstellen
        public Task(Machine machine, Job job, int duration, int id)
        {
            Machine = machine;
            Job = job;
            Duration = duration;
            Id = id;
        }

        public Task(int job, int duration, int id)
        {

        }
        public Task(int duration, int id)
        {
            Duration = duration;
            Id = id;
        }

        //Methoden

        //Vergleichsmethode für IComparable
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

using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobShopSchedulingProblemCP.Instance
{
    internal class Task : IComparable
    {
        //Eigenschaften
        public Guid Guid { get; }
        public int Id { get; set; }
        public Machine Machine { get; set; }

        //Tasks können ein vorgelagertes Setup haben.
        public int Setup { get; set; }
        public Job Job { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Duration { get; set; }
        public int Position { get; set; }

        //Release ist früheste Startzeit eines Tasks
        public int Release { get; set; }

        //Tail ist die Länge des längsten Pfades vom aktuellen Task bis zum Ende.
        public int Tail { get; set; }
        public Task PredecessorMachine { get; set; }
        public Task SuccessorMachine { get; set; }
        public Task PredecessorJob { get; set; }
        public Task SuccessorJob { get; set; }


        //Variablen

        //Konstruktoren
        public Task(Machine machine, Job job, int duration, int id)
        {
            Guid = Guid.NewGuid();
            Machine = machine;
            Job = job;
            Duration = duration;
            Id = id;
        }

        public Task(int duration, int id)
        {
            Guid = Guid.NewGuid();
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

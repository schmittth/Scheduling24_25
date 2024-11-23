using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobShopSchedulingProblemCP.Instance
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
            this.Id = id;
            
        }

        //Methoden

    }
}

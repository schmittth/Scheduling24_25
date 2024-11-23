using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace JobShopSchedulingProblemCP.Instance
{
    internal class Machine
    {
        //Eigenschaften
        public Guid Guid { get; }
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
            this.Guid = Guid.NewGuid();
            this.Id = id;
        }
    }
}

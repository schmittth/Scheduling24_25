using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobShopSchedulingProblemCP.Instance
{
    //Setup erbt von Task
    internal class Setup
    {
        //Eigenschaften
        public Guid Guid { get; }
        public Job PrevJob { get; }
        public Job NextJob { get; }
        public Machine Machine { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Duration { get; set; }

        //Konstruktoren
        public Setup (Job prevJob, Job nextJob, int duration, Machine machine)
        {
            Guid = Guid.NewGuid ();
            PrevJob = prevJob;
            NextJob = nextJob;
            Duration = duration;
            Machine = machine;

        }
    }
}

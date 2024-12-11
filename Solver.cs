using Google.OrTools.PDLP;
using Projektseminar.Instance;

namespace Projektseminar
{
    internal class Solver
    {
        public Problem CurrentProblem { get; set; }
        public Problem BestProblem { get; set; }
        public void Log(string instanceName, int seedValue, TimeSpan runtime, string solverType, double coolingFactor = 0, int iterations = 0, string neighborhood = "", string priorityRule = "")
        {

            int minTaskAmount = 0;
            int minTaskTime = 0;
            int maxTaskTime = 0;

            foreach (Job job in BestProblem.Jobs)
            {
                if (minTaskAmount > job.Tasks.Count)
                {
                    minTaskAmount = job.Tasks.Count;
                }

                foreach (Instance.Task task in job.Tasks)
                {
                    if (task.Duration < minTaskTime)
                    {
                        minTaskTime = task.Duration;
                    }
                    if (task.Duration > maxTaskTime)
                    {
                        maxTaskTime = task.Duration;
                    }

                }
            }
            using (StreamWriter sw = File.AppendText((@$"..\..\..\LogFile.csv")))
            {
                sw.WriteLine($"{instanceName};{BestProblem.Jobs.Count};{BestProblem.Machines.Count};{minTaskAmount};{minTaskTime};{maxTaskTime};{solverType};{coolingFactor};{iterations};{neighborhood};{priorityRule};{runtime};{seedValue};{BestProblem.CalculateMakespan}");
            }
        }
    }
}


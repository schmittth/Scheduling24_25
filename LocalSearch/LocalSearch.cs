using Projektseminar.Instance;

namespace Projektseminar.LocalSearch
{
    internal class LocalSearch
    {

        public Problem BestProblem { get; set; }

        public string Neighborhood { get; set; }

        public LocalSearch(Problem problem, string neighborhood)
        {
            BestProblem = problem;
            Neighborhood = neighborhood;
        }


        public Problem DoLocalSearch()
        {
            int makespan;
            do
            {
                Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = BestProblem.GetNeighboorhood(Neighborhood);

                makespan = BestProblem.CalculateMakespan();

                foreach (List<Tuple<Instance.Task, Instance.Task, Machine>> list in dict.Values)
                {
                    Problem newProblem = new Problem(BestProblem);

                    foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in list)
                    {
                        newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                    }

                    int newMakespan = newProblem.CalculateMakespan();

                    if (newMakespan < makespan)
                    {
                        BestProblem = newProblem;
                        makespan = newMakespan;
                    }
                }
            } 
            while (makespan > BestProblem.CalculateMakespan());

            return BestProblem;
            }

        public void Log(string instanceName, int seedValue, TimeSpan runtime, string priorityRule = "")
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
                    sw.WriteLine($"{instanceName};{BestProblem.Jobs.Count};{BestProblem.Machines.Count};{minTaskAmount};{minTaskTime};{maxTaskTime};LocalSearch;;;{Neighborhood};{priorityRule};{runtime};{seedValue}");
                }
            }
        }
    }

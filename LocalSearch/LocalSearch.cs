using Projektseminar.Instance;
using System.Collections.Concurrent;

namespace Projektseminar.LocalSearch
{
    internal class LocalSearch
    {

        public Problem BestProblem { get; set; }

        public LocalSearch(Problem problem)
        {
            BestProblem = problem;
        }


        public Problem DoLocalSearch(string searchMethod)
        {
            for (int i = 0; i < 200; i++)
            {
                Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = BestProblem.GetNeighboorhood(searchMethod);

                int makespan = BestProblem.CalculateMakespan();

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
            
            return BestProblem;
        }
    }
}

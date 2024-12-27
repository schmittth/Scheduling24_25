using Projektseminar.Instance;

namespace Projektseminar.LocalSearch
{
    internal class LocalSearch : Solver
    {
        public string Neighborhood { get; set; }

        public LocalSearch(Problem problem, string neighborhood)
        {
            BestProblem = problem;
            Neighborhood = neighborhood;
        }


        public Problem DoLocalSearch()
        {
            int oldMakespan;
            do
            {
                Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = BestProblem.GetNeighboorhood(Neighborhood);

                oldMakespan = BestProblem.Makespan;

                foreach (List<Tuple<Instance.Task, Instance.Task, Machine>> list in dict.Values)
                {
                    Problem newProblem = new Problem(BestProblem);

                    foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in list)
                    {
                        newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                    }

                    if (newProblem.Makespan < BestProblem.Makespan)
                    {
                        BestProblem = newProblem;
                    }
                }
            }
            while (oldMakespan > BestProblem.Makespan);

            return BestProblem;
        }

    }
}

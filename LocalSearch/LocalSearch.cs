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


        public Problem DoLocalSearch(bool parallelMode, string searchMethod)
        {
            for (int i = 0; i < 200; i++)
            {
                ConcurrentDictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = BestProblem.GetNeighboorhood(searchMethod);
                //Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = BestProblem.GetNeighboorhood(searchMethod);

                int makespan = BestProblem.CalculateMakespan();

                switch (parallelMode)
                {
                    case true:
                        ConcurrentDictionary<int, Tuple<Problem, int>> conDict = new ConcurrentDictionary<int, Tuple<Problem, int>>();

                        conDict.TryAdd(0, Tuple.Create(BestProblem, makespan));

                        Parallel.ForEach(dict.Values, list =>
                        {
                            Problem newProblem = new Problem(BestProblem);

                            foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in list)
                            {
                                newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3, false);
                                conDict.TryAdd(conDict.Last().Key + 1, Tuple.Create(newProblem, newProblem.CalculateMakespan()));

                            }
                        });
                        BestProblem = conDict.First(x => x.Value.Item2 == conDict.Min(x => x.Value.Item2)).Value.Item1;
                        break;

                    case false:
                        foreach (List<Tuple<Instance.Task, Instance.Task, Machine>> list in dict.Values)
                        {
                            Problem newProblem = new Problem(BestProblem);

                            foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in list)
                            {
                                newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3, false);
                            }

                            int newMakespan = newProblem.CalculateMakespan();

                            if (newMakespan < makespan)
                            {
                                BestProblem = newProblem;
                                makespan = newMakespan;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            return BestProblem;
        }
    }
}

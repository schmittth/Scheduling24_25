using JobShopSchedulingProblemCP.Instance;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scheduling24-25-main.MetaHeuristic
{
    internal class SimulatedAnnealing
    {
        //Eigenschaften
        public Problem CurrentProblem { get; set; }
        public Problem BestProblem { get; set; }
        public double Temperature { get; set; }
        public double CoolingFactor { get; set; }
        public int Iterations { get; set; }

        //Variablen

        //Konstruktoren
        public SimulatedAnnealing(Problem problem, int temperature, double coolingFactor, int iterations)
        {
            this.CurrentProblem = problem;
            this.BestProblem = problem;
            this.Temperature = temperature;
            this.CoolingFactor = coolingFactor;
            this.Iterations = iterations;

        }

        //Methoden
        public Problem DoSimulatedAnnealing()
        {
            while (Temperature > 1)
            {
                Console.WriteLine($"Current Temperature Simulated Annealing {Temperature}");

                for (int i = 0; i < Iterations; i++)
                {
                    Console.WriteLine($"Current Iteration Simulated Annealing {i}");
                    CurrentProblem.ProblemAsDiagramm(@$"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\scheduling24-25-main\diagrammAnnealingTemp{Temperature}Iteration{i}.html");

                    Random random = new Random();

                    ConcurrentDictionary<int, List<Tuple<JobShopSchedulingProblemCP.Instance.Task, JobShopSchedulingProblemCP.Instance.Task, Machine>>> dict = CurrentProblem.GetNeighboorhood("N3");
                    List<Tuple<JobShopSchedulingProblemCP.Instance.Task, JobShopSchedulingProblemCP.Instance.Task, Machine>> randomNeighbor;

                    if (dict.Count == 0)
                    {
                        Console.WriteLine("Dict leer");
                    }

                    randomNeighbor = dict[random.Next(1,dict.Count)];
                    
                    Problem newProblem = new Problem(CurrentProblem);

                    foreach (Tuple<JobShopSchedulingProblemCP.Instance.Task, JobShopSchedulingProblemCP.Instance.Task, Machine> tuple in randomNeighbor)
                    {
                        newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                    }

                    if (CurrentProblem.CalculateMakespan() > newProblem.CalculateMakespan())
                    {
                        Console.WriteLine($"Iteration{i} Solution accepted - better than current");
                        CurrentProblem = newProblem;
                        if (BestProblem.CalculateMakespan() > newProblem.CalculateMakespan())
                        {
                            BestProblem = newProblem;
                            Console.WriteLine($"Iteration{i} Solution accepted - better than best");
                        }
                    }
                    else
                    {
                        int delta = newProblem.CalculateMakespan() - CurrentProblem.CalculateMakespan();
                        double exponent = -delta / Temperature;
                        double probability = Math.Pow(Math.E, exponent);
                        if (random.NextDouble() < probability)
                        {
                            CurrentProblem = newProblem;
                            Console.WriteLine($"Iteration{i} Solution accepted - but worse");
                        }
                    }
                }
                Temperature = Temperature * CoolingFactor;
            }
            return BestProblem;
        }
    }
}

using Projektseminar.Instance;
using System.Collections.Concurrent;

namespace Projektseminar.MetaHeuristic
{
    internal class SimulatedAnnealing
    {
        //Eigenschaften
        public Problem CurrentProblem { get; set; }
        public Problem BestProblem { get; set; }
        public double Temperature { get; set; }
        public double CoolingFactor { get; set; }
        public int Iterations { get; set; }
        public string Neighboorhood { get; set; }

        //Variablen

        //Konstruktoren
        public SimulatedAnnealing(Problem problem, int temperature, double coolingFactor, int iterations, string neighboorhood)
        {
            this.CurrentProblem = problem;
            this.BestProblem = problem;
            this.Temperature = temperature;
            this.CoolingFactor = coolingFactor;
            this.Iterations = iterations;
            this.Neighboorhood = neighboorhood;
        }

        //Methoden
        public Problem DoSimulatedAnnealing()
        {
            while (Temperature > 1)
            {
                Console.WriteLine($"Current Temperature Simulated Annealing {Temperature}");

                for (int i = 0; i < Iterations; i++)
                {
                    /*Debug*/
                    //Console.WriteLine($"Current Iteration Simulated Annealing {i}");                  
                    //CurrentProblem.ProblemAsDiagramm(@$"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\Projektseminar\diagrammAnnealingTemp{Temperature}Iteration{i}.html");
                    Random randSeed = new Random();
                    int seedValue = randSeed.Next(0, Int32.MaxValue);

                    Random random = new Random(seedValue);
                    Problem newProblem;

                    Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = CurrentProblem.GetNeighboorhood(Neighboorhood);

                    List<int> invalidNumber = new List<int>();

                    do
                    {
                        newProblem = new Problem(CurrentProblem);

                        int chooseNeighbor;
                        do
                        {
                            chooseNeighbor = random.Next(0, dict.Count);

                            /*Debug*/
                            //Console.WriteLine($"Choose Neigbor {chooseNeighbor}");
                            
                            /*if (invalidNumber.Count == dict.Count)
                            {
                                invalidNumber.Clear();
                                dict = CurrentProblem.GetNeighboorhood("N1");
                                chooseNeighbor = random.Next(0, dict.Count);                             
                            }*/
                        }
                        while (invalidNumber.Contains(chooseNeighbor));


                        List<Tuple<Instance.Task, Instance.Task, Machine>> randomNeighbor = dict[chooseNeighbor];
                        invalidNumber.Add(chooseNeighbor);

                        foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in randomNeighbor)
                        {
                            newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                        }
                    }
                    while (!newProblem.ConfirmFeasability());

                    if (CurrentProblem.CalculateMakespan() > newProblem.CalculateMakespan())
                    {
                        /*Debug*/
                        //Console.WriteLine($"Iteration{i} Solution accepted - better than current");
                        
                        CurrentProblem = newProblem;
                        if (BestProblem.CalculateMakespan() > newProblem.CalculateMakespan())
                        {
                            
                            BestProblem = newProblem;
                            /*Debug*/
                            //Console.WriteLine($"Iteration{i} Solution accepted - better than best");
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
                            /*Debug*/
                            //Console.WriteLine($"Iteration{i} Solution accepted - but worse");
                        }
                    }
                }
                Temperature = Temperature * CoolingFactor;
            }
            return BestProblem;
        }
    }
}

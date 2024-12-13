using Projektseminar.Instance;
using System.Collections.Concurrent;

namespace Projektseminar.MetaHeuristic
{
    internal class SimulatedAnnealing : Solver
    {
        //Eigenschaften
        public double Temperature { get; set; }
        public double CoolingFactor { get; set; }
        public int Iterations { get; set; }
        public string Neighboorhood { get; set; }

        //Variablen

        //Konstruktoren
        public SimulatedAnnealing(Problem problem, double coolingFactor, int iterations, string neighboorhood)
        {
            this.CurrentProblem = problem; //Übergebenes Problem wird als aktuelles Problem gesetzt
            this.BestProblem = problem; //Bei Instanzierung ist bestes Problem = aktuelles Problem
            this.Temperature = 100; //Temperatur hardcoded auf 100
            this.CoolingFactor = coolingFactor; //Abkühlungfaktor wird von Konsole übergeben.
            this.Iterations = iterations; //Anzahl an Iterationen wird von Konsole übergeben.
            this.Neighboorhood = neighboorhood; //Nachbarschaft wird von Konsole übergeben.
        }

        //Methoden
        public Problem DoSimulatedAnnealing(int seedValue)
        {

            Random random = new Random(seedValue);
            Problem newProblem;

            while (Temperature > 1)
            {
                /*Debug*/              
                //CurrentProblem.ProblemAsDiagramm(@$"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\Projektseminar\diagrammAnnealingTemp{Temperature}.html");

                Console.WriteLine($"Current Temperature Simulated Annealing {Temperature}");

                for (int i = 0; i < Iterations; i++)
                {
                    /*Debug*/
                    //Console.WriteLine($"Current Iteration Simulated Annealing {i}");                  
                    //CurrentProblem.ProblemAsDiagramm(@$"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\Projektseminar\diagrammAnnealingTemp{Temperature}Iteration{i}.html");

                    Dictionary<int, List<Tuple<Instance.Task, Instance.Task, Machine>>> dict = CurrentProblem.GetNeighboorhood(Neighboorhood);

                    List<int> invalidNumber = new List<int>();

                    do
                    {
                        newProblem = new Problem(CurrentProblem);

                        int chooseNeighbor;

                            chooseNeighbor = random.Next(0, dict.Count);



                        List<Tuple<Instance.Task, Instance.Task, Machine>> randomNeighbor = dict[chooseNeighbor];

                        foreach (Tuple<Instance.Task, Instance.Task, Machine> tuple in randomNeighbor)
                        {
                            newProblem.SwapTasks(tuple.Item1, tuple.Item2, tuple.Item3);
                        }
                    }
                    while (!newProblem.ConfirmFeasability());

                    /*Debug*/

                    int curMakespan = CurrentProblem.CalculateMakespan();
                    int newMakespan = newProblem.CalculateMakespan();

                    //Console.WriteLine($"New Makespan: {newMakespan}, Old Makespan {curMakespan}");

                    if (curMakespan > newMakespan)
                    {
                        /*Debug*/
                        //Console.WriteLine($"Iteration{i} Solution accepted - better than current");
                        
                        CurrentProblem = newProblem;
                        if (BestProblem.CalculateMakespan() > newMakespan)
                        {
                            
                            BestProblem = newProblem;
                            /*Debug*/
                            //Console.WriteLine($"Iteration{i} Solution accepted - better than best");
                        }
                    }
                    else
                    {
                        int delta = newMakespan - curMakespan;
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

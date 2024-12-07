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

        public void Log(string instanceName, int seedValue)
        {

            int minTaskAmount = BestProblem.Machines.Count;
            int minTaskTime = BestProblem.Horizon;
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
                sw.WriteLine($"{instanceName};{BestProblem.Jobs.Count};{BestProblem.Machines.Count};{minTaskAmount};{minTaskTime};{maxTaskTime};SimulatedAnnealing;{CoolingFactor};{Iterations};{Neighboorhood};{seedValue}");
            }
        }
    }
}

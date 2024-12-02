using Projektseminar.Instance;
using Projektseminar.LocalSearch;
using Projektseminar.MetaHeuristic;
using Projektseminar.OpeningHeuristic;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Projektseminar
{
    public class ScheduleRequestsSat
    {
        //Variablen
        string[] chars = new string[] { "/", "-", "\\", " | " };

        int charCounter = 0;

        //Methoden
        public static void Main(string[] args)
        {
            Importer importer = new Importer();
            string instanceChoice = Dialog.ChooseInstance();
            string blab = "seedValue";
            if (instanceChoice == "Random")
            {
                Tuple <int,int,int,int,int,int> randomSize = Dialog.ChooseRandomInstanceSize();
                importer.ImportRandomInstance(randomSize.Item1, randomSize.Item2, randomSize.Item3, randomSize.Item4, randomSize.Item5, randomSize.Item6);
            }
            else
            {
                importer.ImportInstanceFromFile(instanceChoice);
            }

            Problem problem = importer.GenerateProblem();
            switch (Dialog.ChooseSolver())
            {
                case 1:
                    ORToolsSolver.GoogleOR newSolver = new ORToolsSolver.GoogleOR();
                    newSolver.SolveProblem(problem);
                    break;
                case 2:
                    Giffler_Thompson giffler_Thompson = new Giffler_Thompson(problem,Dialog.ChoosePriorityRule());
                    problem = giffler_Thompson.InitialSolution();
                    problem.ProblemAsDiagramm(@$"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\Projektseminar\diagrammInitial.html");

                    Tuple<int, double, int> simAnnealParams = Dialog.ChooseSimAnnealParameters();
                    SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, simAnnealParams.Item3, Dialog.ChooseNeighboorhood());
                    problem = simAnneal.DoSimulatedAnnealing();
                    problem.ProblemAsDiagramm(@$"G:\SynologyDrive\Studium\Master\2.Semester\Scheduling\Projektseminar\diagramm.html");
                    break;
                case 3:
                    LocalSearch.LocalSearch local_search = new LocalSearch.LocalSearch(problem);
                    problem = local_search.DoLocalSearch(true, Dialog.ChooseNeighboorhood());
                    break;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();


            stopwatch.Stop();
            Console.WriteLine($"Local Search ran {stopwatch.Elapsed.Minutes} Minutes {stopwatch.Elapsed.Seconds} Seconds {stopwatch.Elapsed.Milliseconds} Milliseconds");
        }
    }
}
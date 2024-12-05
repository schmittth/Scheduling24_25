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
            Stopwatch stopwatch = new Stopwatch();

            Importer importer = new Importer();
            string instanceChoice = Dialog.ChooseInstance();
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
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;              
            switch (Dialog.ChooseSolver())
            {
                case 1:
                    stopwatch.Start();

                    ORToolsSolver.GoogleOR newSolver = new ORToolsSolver.GoogleOR();
                    newSolver.SolveProblem(problem);
                    break;
                case 2:
                    stopwatch.Start();

                    Giffler_Thompson giffler_Thompson = new Giffler_Thompson(problem,Dialog.ChoosePriorityRule());
                    problem = giffler_Thompson.InitialSolution();

                    stopwatch.Stop();

                    problem.ProblemAsDiagramm( @"..\diagrammInitial.html");

                    Tuple<int, double, int> simAnnealParams = Dialog.ChooseSimAnnealParameters();

                    stopwatch.Start();

                    SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, simAnnealParams.Item3, Dialog.ChooseNeighboorhood());
                    problem = simAnneal.DoSimulatedAnnealing();
                    problem.ProblemAsDiagramm(@$"..\..\..\diagramm.html");

                    string sFile = System.IO.Path.Combine(sCurrentDirectory, @"..\..\..\diagramm.html");  
                    string sFilePath = Path.GetFullPath(sFile);
                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    try
                    {
                        process.StartInfo.UseShellExecute = true;
                        process.StartInfo.FileName = sFilePath;
                        process.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    break;
                case 3:
                    stopwatch.Start();

                    Giffler_Thompson localgiffler_Thompson = new Giffler_Thompson(problem, Dialog.ChoosePriorityRule());
                    problem = localgiffler_Thompson.InitialSolution();
                    problem.ProblemAsDiagramm(@"..\diagrammInitial.html");

                    LocalSearch.LocalSearch local_search = new LocalSearch.LocalSearch(problem);
                    problem = local_search.DoLocalSearch(Dialog.ChooseNeighboorhood());

                    break;
            }

            stopwatch.Stop();
            Console.WriteLine($"Local Search ran {stopwatch.Elapsed.Minutes} Minutes {stopwatch.Elapsed.Seconds} Seconds {stopwatch.Elapsed.Milliseconds} Milliseconds");
        }
    }
}
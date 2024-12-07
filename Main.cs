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
        List<string> toCSV = new List<string>();

        int charCounter = 0;

        //Methoden
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            int seedValue = 0;

            Importer importer = new Importer();
            string instanceChoice = Dialog.ChooseInstance();
            if (instanceChoice == "Random")
            {
                Tuple <int,int,int,int,int,int> randomSize = Dialog.ChooseRandomInstanceSize();
                importer.ImportRandomInstance(randomSize.Item1, randomSize.Item2, randomSize.Item3, randomSize.Item4, randomSize.Item5, randomSize.Item6);
                seedValue = randomSize.Item3;
            }
            else
            {
                Random randSeed = new Random();
                seedValue = randSeed.Next(0, Int32.MaxValue);
                importer.ImportInstanceFromFile(instanceChoice);
            }

            Problem problem = importer.GenerateProblem();
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory; 
            
            switch (Dialog.ChooseSolver())
            {
                case 1:
                    stopwatch.Start();

                    ORToolsSolver.GoogleOR newSolver = new ORToolsSolver.GoogleOR(problem);
                    newSolver.DoORSolver();
                    newSolver.Log(instanceChoice, seedValue);

                    break;
                case 2:
                    stopwatch.Start();

                    Giffler_Thompson giffler_Thompson = new Giffler_Thompson(problem,Dialog.ChoosePriorityRule());
                    problem = giffler_Thompson.InitialSolution();

                    stopwatch.Stop();

                    problem.ProblemAsDiagramm( @"..\diagrammInitial.html");

                    Tuple<double, int> simAnnealParams = Dialog.ChooseSimAnnealParameters();

                    stopwatch.Start();

                    SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, Dialog.ChooseNeighboorhood());
                    problem = simAnneal.DoSimulatedAnnealing(seedValue);
                    simAnneal.Log(instanceChoice, seedValue);
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

                    LocalSearch.LocalSearch localSearch = new LocalSearch.LocalSearch(problem, Dialog.ChooseNeighboorhood());
                    problem = localSearch.DoLocalSearch();
                    localSearch.Log(instanceChoice, seedValue);

                    break;
            }

            stopwatch.Stop();
            Console.WriteLine($"Local Search ran {stopwatch.Elapsed.Minutes} Minutes {stopwatch.Elapsed.Seconds} Seconds {stopwatch.Elapsed.Milliseconds} Milliseconds");
        }
    }
}
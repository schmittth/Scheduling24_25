using Projektseminar.Algorithms;
using Projektseminar.Instance;
using Projektseminar.Standalone;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Projektseminar
{
    public class ScheduleRequestsSat
    {
        //Variablen

        //Methoden
        public static void Main(string[] args)
        {
            int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; //Generiere sog. Unix-Timestamp.
            int seedValue = 0; //Initialisiere eine Int der den Random-Seed für diese Ausführung entält. Setze auf 0

            string instanceChoice = Dialog.ChooseInstance(); //Bestimme ob randomisierte oder feste Instanz        

            List<string> subFiles = new List<string>();
            int instanceAmount = 0;

            if (instanceChoice.EndsWith(".txt")) //When ein .txt-File selektiert wird nur eine Instanz geladen
            {
                instanceAmount = 1;
            }
            else if (instanceChoice.EndsWith("\\"))
            {
                foreach (string subDir in Directory.GetDirectories(instanceChoice))
                {
                    foreach (string subFile in Directory.GetFiles(subDir, "*.txt", SearchOption.TopDirectoryOnly))
                    {
                        subFiles.Add(subFile);
                    }
                }
                instanceAmount = subFiles.Count;
            }
            else
            {
                subFiles = Directory.GetFiles(instanceChoice, "*.txt", SearchOption.TopDirectoryOnly).ToList();
                instanceAmount = subFiles.Count;
            }

            int solverChoice = Dialog.ChooseSolver(); //Lasse Lösungsansatz auswählen
            string priorityRule = ""; //Initialisiere PriortityRule String
            string neighborhood = ""; //Initialisiere Nachbarschafts String


            if (solverChoice == 2 || solverChoice == 3)
            {
                priorityRule = Dialog.ChoosePriorityRule();
                neighborhood = Dialog.ChooseNeighboorhood();
                //priorityRule = "LTT";
                //neighborhood = "N5";
            }

            Tuple<double, int> simAnnealParams = null;
            if (solverChoice == 2)
            {
                simAnnealParams = Dialog.ChooseSimAnnealParameters();
            }

            //
            for (int instanceCounter = 0; instanceCounter < instanceAmount; instanceCounter++)
            {

                    Random randSeed = new Random();
                    seedValue = randSeed.Next(0, Int32.MaxValue);
                

                Importer importer = new Importer();
                if (instanceChoice.EndsWith(".txt"))
                {
                    importer.ImportInstanceFromFile(instanceChoice);
                }
                else
                {
                    importer.ImportInstanceFromFile(subFiles[instanceCounter]);
                }

                Problem problem = importer.GenerateProblem();

                GifflerThompson gifflerThompson = new GifflerThompson(problem, priorityRule);

                if (solverChoice == 2 || solverChoice == 3)
                {
                    problem = gifflerThompson.InitialSolution();
                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\initialSolution.html", false, seedValue, gifflerThompson.Stopwatch.Elapsed);
                }

                switch (solverChoice) //Switch-Case Anweisungen basierend auf der Solver-Auswahl.
                {
                    //Solver: Google OR-Tools
                    case 1:
                        //Erstelle neues OR-Solver Objekt und löse das Problem.
                        ORToolsSolver.GoogleOR googleor = new ORToolsSolver.GoogleOR(problem);
                        problem = googleor.DoORSolver();

                        googleor.Log(instanceChoice, seedValue, googleor.Stopwatch.Elapsed, "GoogleOR"); //Logge die Ausführung
                        problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\googleOr.html", true, seedValue, googleor.Stopwatch.Elapsed);

                        break;
                    //Solver: Simulated Annealing
                    case 2:
                        SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, neighborhood);
                        problem = simAnneal.DoSimulatedAnnealing(seedValue);

                        simAnneal.Log(instanceChoice, seedValue, simAnneal.Stopwatch.Elapsed, "Simulated Annealing", simAnneal.CoolingFactor, simAnneal.Iterations, simAnneal.Neighborhood, gifflerThompson.PriorityRule);
                        problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\simAnneal.html", true, seedValue, simAnneal.Stopwatch.Elapsed);
                        //problem.ProblemAsFile($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\instanceExport.txt");
                        break;
                    case 3:
                        LocalSearch localSearch = new LocalSearch(problem, neighborhood);
                        problem = localSearch.DoLocalSearch();

                        localSearch.Log(instanceChoice, seedValue, localSearch.Stopwatch.Elapsed, "Local Search", iterations: 0, priorityRule: gifflerThompson.PriorityRule);
                        problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\localSearch.html", true, seedValue, localSearch.Stopwatch.Elapsed);

                        break;
                }
            }
            stopwatch.Stop();
        }
    }
}
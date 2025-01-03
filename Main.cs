﻿using Projektseminar.Instance;
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

        //Methoden
        public static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch(); //Initialisiere eine Stopwatch um die Laufzeit zu messen.
            int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds; //Generiere sog. Unix-Timestamp.
            int seedValue = 0; //Initialisiere eine Int der den Random-Seed für diese Ausführung entält. Setze auf 0
            int seedChoice = 0;

            string instanceChoice = Dialog.ChooseInstance(); //Bestimme ob randomisierte oder feste Instanz        

            Tuple<int, int, int, int, int> randomInstanceSize = null;
            List<string> subFiles = new List<string>();
            int instanceAmount = 0;

            if (instanceChoice == "Random") //Wenn randomisierte Instanz lasse Instanzgröße auswählen
            {
                instanceAmount = Dialog.ChooseInstanceAmount(); //Bestimme wie oft Instanzen generiert werden sollen   
                randomInstanceSize = Dialog.ChooseRandomInstanceSize(); //Größe der randomisierten Instanz auswählen               
            }
            else if (instanceChoice.EndsWith(".txt")) //When ein .txt-File selektiert wird nur eine Instanz geladen
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
            string neighboorhood = ""; //Initialisiere Nachbarschafts String


            if (solverChoice == 2 || solverChoice == 3)
            {
                //priorityRule = Dialog.ChoosePriorityRule();
                priorityRule = "LTT";
                neighboorhood = Dialog.ChooseNeighboorhood();
            }

            Tuple<double, int> simAnnealParams = null;
            if (solverChoice == 2)
            {
                seedChoice = Dialog.SeedAlgorithm();
                simAnnealParams = Dialog.ChooseSimAnnealParameters();
            }

            //
            for (int instanceCounter = 0; instanceCounter < instanceAmount; instanceCounter++)
            {
                if (seedChoice == 0)
                {
                    Random randSeed = new Random();
                    seedValue = randSeed.Next(0, Int32.MaxValue);
                }
                else
                {
                    seedValue = seedChoice;
                }

                Importer importer = new Importer();
                if (instanceChoice == "Random")
                {
                    importer.ImportRandomInstance(randomInstanceSize.Item1, randomInstanceSize.Item2, randomInstanceSize.Item3, randomInstanceSize.Item4, randomInstanceSize.Item5);
                }
                else if (instanceChoice.EndsWith(".txt"))
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
                    problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\initialSolution.html", false, seedValue, stopwatch.Elapsed);
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
                        SimulatedAnnealing simAnneal = new SimulatedAnnealing(problem, simAnnealParams.Item1, simAnnealParams.Item2, neighboorhood);
                        problem = simAnneal.DoSimulatedAnnealing(seedValue);

                        simAnneal.Log(instanceChoice, seedValue, simAnneal.Stopwatch.Elapsed, "Simulated Annealing", simAnneal.CoolingFactor, simAnneal.Iterations, simAnneal.Neighboorhood, gifflerThompson.PriorityRule);
                        problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\simAnneal.html", true, seedValue, simAnneal.Stopwatch.Elapsed);
                        //problem.ProblemAsFile($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\instanceExport.txt");
                        break;
                    case 3:
                        LocalSearch.LocalSearch localSearch = new LocalSearch.LocalSearch(problem, neighboorhood);
                        problem = localSearch.DoLocalSearch();

                        localSearch.Log(instanceChoice, seedValue, stopwatch.Elapsed, "Local Search", iterations: 0, priorityRule: gifflerThompson.PriorityRule);
                        problem.ProblemAsDiagramm($@"..\..\..\Diagramms\{unixTimestamp}\instance{instanceCounter}\localSearch.html", true, seedValue, localSearch.Stopwatch.Elapsed);

                        break;
                }
            }
            stopwatch.Stop();
        }
    }
}
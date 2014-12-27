using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AntColony;
using Presentation.TSP.Properties;

namespace Presentation.TSP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static Random random = new Random(0);

        static int alpha = 3; // influence of pheromone on direction
        static int beta = 2;  // influence of adjacent node distance

        static double rho = 0.01; // pheromone decrease factor
        static double Q = 2.0;   // pheromone increase factor
        private List<List<int>> _dataFromFile;

        public List<List<int>> DataFromFile
        {
            get { return _dataFromFile??(_dataFromFile=new List<List<int>>()); }
            set { _dataFromFile = value; }
        }

        private void loadCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataFromFile.Clear();
            var f = new OpenFileDialog();
            if (f.ShowDialog()==DialogResult.OK)
            {
                var file = f.FileName;
                var reader = new StreamReader(File.OpenRead(@file));
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line != null)
                    {
                        try
                        {
                            var values = line.Split(' ').Select(x => Convert.ToInt32(x)).ToList();
                            DataFromFile.Add(values);
                        }
                        catch (InvalidCastException ex)
                        {                            
                            throw new InvalidCastException(String.Format(Resources.Form1_loadCsvToolStripMenuItem_Click_Invalid_file_format_Line___0_,line));
                        }                                                
                    }
                }
            }

            labelStatus.Text = Resources.Form1_loadCsvToolStripMenuItem_Click_Data_loaded_succesfully;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            try
            {
                int numCities = 10;
                int numAnts = 4;
                int maxTime = 1000;


                logTextBox.Text += String.Format("Number of cities: {0} \n", numCities);
                logTextBox.Text += String.Format("Number of ants: {0} \n", numAnts);                
                logTextBox.Text += String.Format("Maximum time: {0} \n", maxTime);                              
                logTextBox.Text += String.Format("Alpha (pheromone influence): {0} \n", alpha);                              
                logTextBox.Text += String.Format("Beta (local node influence): {0} \n", beta);                              
                logTextBox.Text += String.Format("Rho (pheromone evaporation coefficient): {0} \n", rho.ToString("F2"));        
                logTextBox.Text += String.Format("Q (pheromone deposit factor): {0} \n", Q.ToString("F2"));        
                logTextBox.Text += String.Format("Initialing graph distances \n");

                //int[][] dists = AntColonyProgram.MakeGraphDistancesDemo(numCities); //generate random distances
                if (DataFromFile == null || DataFromFile.Count == 0)
                {
                    logTextBox.Text += "Data from file not loaded \n";
                    logTextBox.Text += "Operation cancelled \n";
                    return;
                }
                    

                int[][] dists = AntColonyProgram.MakeGraphDistances(numCities,DataFromFile);


                logTextBox.Text += String.Format("\n Initialing ants to random trails \n");
 
                int[][] ants = AntColonyProgram.InitAnts(numAnts, numCities); // initialize ants to random trails
               // ShowAnts(ants, dists);

                int[] bestTrail =AntColonyProgram.BestTrail(ants, dists); // determine the best initial trail
                double bestLength =AntColonyProgram.Length(bestTrail, dists); // the length of the best trail

                logTextBox.Text+=("\nBest initial trail length: " + bestLength.ToString("F1") + "\n");
               logTextBox.Text+= AntColonyProgram.Display(bestTrail);

                logTextBox.Text += ("\nInitializing pheromones on trails");
                double[][] pheromones =AntColonyProgram.InitPheromones(numCities);

                int time = 0;
                logTextBox.Text += ("\nEntering UpdateAnts - UpdatePheromones loop\n");
                while (time < maxTime)
                {
                  AntColonyProgram.UpdateAnts(ants, pheromones, dists);
                  AntColonyProgram.UpdatePheromones(pheromones, ants, dists);

                    int[] currBestTrail =AntColonyProgram.BestTrail(ants, dists);
                    double currBestLength =AntColonyProgram.Length(currBestTrail, dists);
                    if (currBestLength < bestLength)
                    {
                        bestLength = currBestLength;
                        bestTrail = currBestTrail;
                        logTextBox.Text += ("\n New best length of " + bestLength.ToString("F1") + " found at time " + time);
                    }
                    ++time;
                }

                logTextBox.Text += ("\nTime complete");

                logTextBox.Text += ("\nBest trail found:");
                logTextBox.Text+= AntColonyProgram.Display(bestTrail);
                logTextBox.Text += ("\nLength of best trail found: " + bestLength.ToString("F1"));

            }
            catch
            {

            }
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            logTextBox.Clear();
        }
    }
}

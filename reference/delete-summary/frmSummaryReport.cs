using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeleteUserInterface
{
    public partial class frmSummaryReport : Form
    {
        public frmSummaryReport()
        {
            InitializeComponent();
        }
        private void btnSummary_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = "animals.txt";

                // If the file doesn't exist, create an empty one so the program doesn't crash
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }

                // Read all lines and remove any blank lines
                List<string> lines = File.ReadAllLines(filePath).Where(line => !string.IsNullOrWhiteSpace(line)).ToList();

                int totalAnimals = lines.Count;
                int totalAge = 0;
                int totalScore = 0;

                // Variables to count each status
                int criticalCount = 0;
                int seriousCount = 0;
                int stableCount = 0;
                int recoveringCount = 0;
                int releaseReadyCount = 0;

                // Loop through every animal in the file
                foreach (string line in lines)
                {        
                    string[] parts = line.Split('|');

                    // Make sure the line has exactly 7 parts before trying to read it
                    if (parts.Length == 7)
                    {
                        // Convert Age and Score to numbers. 
                        // parts[3] is Age, parts[4] is Score
                        if (int.TryParse(parts[3], out int age) && int.TryParse(parts[4], out int score))
                        {
                            totalAge += age;
                            totalScore += score;
                        }

                        // Check the status (parts[5]) and add to the correct counter
                        string status = parts[5];
                        if (status == "Critical") criticalCount++;
                        else if (status == "Serious") seriousCount++;
                        else if (status == "Stable") stableCount++;
                        else if (status == "Recovering") recoveringCount++;
                        else if (status == "Release-Ready") releaseReadyCount++;
                    }
                }

                // Build the text for the summary
                string summary = "BAOBAB RIDGE WILDLIFE REHABILITATION CENTRE ANIMAL SUMMARY REPORT\n";
                summary += "Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\n\n";

                if (totalAnimals == 0)
                {
                    summary += "Total animals: 0\n";
                    summary += "Average age (years): N/A\n";
                    summary += "Average recovery score: N/A\n\n";
                }
                else
                {
                    // Calculate averages
                    double avgAge = (double)totalAge / totalAnimals;
                    double avgScore = (double)totalScore / totalAnimals;

                    summary += "Total animals: " + totalAnimals + "\n";

                    summary += "Average age (years): " + avgAge.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "\n";
                    summary += "Average recovery score: " + avgScore.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "\n\n";
                }

                // Add the status counts (always show all 5, even if 0)
                summary += "Animals per status:\n\n";
                summary += "Critical: " + criticalCount + "\n";
                summary += "Serious: " + seriousCount + "\n";
                summary += "Stable: " + stableCount + "\n";
                summary += "Recovering: " + recoveringCount + "\n";
                summary += "Release-Ready: " + releaseReadyCount + "\n";

                rtbSummary.Text = summary;

                // Save to summary.txt (overwrites the file every time)
                File.WriteAllText("summary.txt", summary);

                MessageBox.Show("Summary report generated and saved to summary.txt", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (IOException)
            {
                MessageBox.Show("Could not generate summary. The animals.txt file might be open in another program.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to write to this folder.", "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBackToMain_Click(object sender, EventArgs e)
        {
            frmDelete deleteForm = new frmDelete();
            deleteForm.Show();
        }
    }
}

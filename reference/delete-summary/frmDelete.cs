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
    public partial class frmDelete : Form
    {
        public frmDelete()
        {
            InitializeComponent();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Check if a row is actually selected
            if (dgvAnimals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an animal from the list to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the ID and Name of the selected animal from the grid
            string selectedID = dgvAnimals.SelectedRows[0].Cells[0].Value.ToString();
            string selectedName = dgvAnimals.SelectedRows[0].Cells[1].Value.ToString();

            // Show the confirmation message exactly as the brief asks
            DialogResult result = MessageBox.Show($"Delete {selectedID} ({selectedName})? This cannot be undone.",
                                                  "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string filePath = "animals.txt";

                    // Read all lines from the file into a list
                    List<string> lines = File.ReadAllLines(filePath).ToList();

                    lines.RemoveAll(line => line.StartsWith(selectedID + "|"));

                    // Write the remaining lines back to the file
                    File.WriteAllLines(filePath, lines);

                    MessageBox.Show("Animal deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    this.Close();
                }
                catch (IOException)
                {
                    MessageBox.Show("The animals.txt file is currently open in another program (like Notepad). Please close it and try again.", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("You do not have permission to modify this file. Check if the file is set to Read-Only.", "Permission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnGoToSummary_Click(object sender, EventArgs e)
        {
            frmSummaryReport summaryForm = new frmSummaryReport();
            summaryForm.Show(); // Show() opens the new form without closing the Delete form
        }

        private void frmDelete_Load(object sender, EventArgs e)
        {

        }
    }
}

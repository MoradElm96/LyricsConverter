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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LyricsConverter.Vistas
{
    public partial class Form1 : Form
    {

        private DataTable dataTable;  
        private int maxProgress;
        private BackgroundWorker csvLoaderWorker;  


        public Form1()
        {
            InitializeComponent();
            InitializeDataTable(); 

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            dataGridView1.AllowUserToOrderColumns = false;

           
            dataGridView1.AllowUserToOrderColumns = false;
            dataGridView1.AllowUserToResizeRows = false;
            this.FormClosing += Form1_FormClosing; 

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            ClearDataTable();
        }


        private void InitializeDataTable()
        {
            dataTable = new DataTable();
            dataGridView1.DataSource = dataTable;

           
            dataGridView1.ReadOnly = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnAbrirCSV_Click(object sender, EventArgs e)
        {
        }

        private async void btnSeleccion_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;

                    // Verifica si la extensión del archivo es .csv
                    if (Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
                    {
                        // Muestra el indicador de carga antes de cargar el archivo CSV
                        progressBar1.Visible = true;

                        await LoadCsvDataAsync(filePath);

                        // Oculta el indicador de carga después de cargar el archivo CSV
                        progressBar1.Visible = false;
                    }
                    else
                    {
                        ShowWarning("El archivo seleccionado no es un archivo CSV válido.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("Error al abrir el archivo CSV: " + ex.Message);
            }
        }

        private async Task LoadCsvDataAsync(string filePath)
        {
            dataTable.Clear();

            using (StreamReader reader = new StreamReader(filePath))
            {
                await reader.ReadLineAsync(); // Omitir la primera línea

                AddColumnIfNotExists("Milisegundos");
                AddColumnIfNotExists("Letra");

                maxProgress = File.ReadLines(filePath).Count() - 1; // Establece el máximo progreso
                int currentProgress = 0; // Inicializa el progreso actual

                while (!reader.EndOfStream)
                {
                    string line = await reader.ReadLineAsync();
                    string[] values = line?.Split(';');

                    if (values != null && values.Length >= 2)
                    {
                        dataTable.Rows.Add(values[0], values[1]); // Agrega solo los campos de milisegundos y letra
                    }

                    currentProgress++; // Incrementa el progreso actual
                    UpdateProgressBar(currentProgress); // Actualiza la barra de progreso
                    await Task.Delay(10); // Pequeña pausa para actualizar la interfaz gráfica
                }
            }

            progressBar1.Value = 0; // Restablece la barra de progreso al finalizar
        }


        private void AddColumnIfNotExists(string columnName)
        {
            if (!dataTable.Columns.Contains(columnName))
            {
                dataTable.Columns.Add(columnName);
            }
        }

        private void btnConverter_Click(object sender, EventArgs e)
        {
            if (dataTable.Rows.Count == 0)
            {
                ShowWarning("No se ha cargado ningún archivo CSV.");
                return;
            }

            try
            {
                ConvertToLrc();
            }
            catch (Exception ex)
            {
                ShowError("Error al convertir a formato LRC: " + ex.Message);
            }
        }

        private void ConvertToLrc()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Archivos LRC (*.lrc)|*.lrc|Todos los archivos (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string outputLrc = saveFileDialog.FileName;
                    GenerateLrcFile(outputLrc);
                    ShowMessage($"Archivo .lrc generado: {outputLrc}", "Conversión Completada");
                }
            }
        }

        private void GenerateLrcFile(string outputLrc)
        {
            using (StreamWriter lrcWriter = new StreamWriter(outputLrc, false, Encoding.UTF8))
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row.ItemArray.Length >= 2)
                    {
                        int milliseconds = Convert.ToInt32(row[0]);
                        string text = row[1].ToString().Trim();
                        string lrcTime = FormatLrcTime(milliseconds);
                        string lrcLine = $"{lrcTime}{text}";
                        lrcWriter.WriteLine(lrcLine);
                    }
                }
            }
        }

        private string FormatLrcTime(int milliseconds)
        {
            int minutes = milliseconds / 60000;
            int seconds = (milliseconds / 1000) % 60;
            milliseconds %= 1000;
            return $"[{minutes:D2}:{seconds:D2}.{milliseconds:D3}]";
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void UpdateProgressBar(int currentProgress)
        {
            int percentage = (int)((double)currentProgress / maxProgress * 100);
            progressBar1.Value = Math.Min(percentage, 100); // Asegura que el valor no exceda el 100%
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
        private void ClearDataTable()
        {
            if (dataTable != null)
            {
                dataTable.Dispose();
                dataTable = null;
            }
        }
    }
}








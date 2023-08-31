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

namespace LyricsConverter.Vistas
{
    public partial class Form1 : Form
    {

        private DataTable dataTable;  // Declarar el DataTable a nivel de formulario

        public Form1()
        {
            InitializeComponent();
            InitializeDataTable();  // Llama a este método en el constructor para crear la estructura de la tabla
        }
        private void InitializeDataTable()
        {
            dataTable = new DataTable();
            dataGridView1.DataSource = dataTable;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnAbrirCSV_Click(object sender, EventArgs e)
        {
        }
        private void btnSeleccion_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos CSV (*.csv)|*.csv|Todos los archivos (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                // Leer el contenido del archivo CSV y cargarlo en el DataTable
                dataTable.Clear();  // Limpiar los datos existentes en la tabla

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string[] columnNames = reader.ReadLine().Split(';');  // Leer y descartar la primera línea (encabezados)

                    // Agregar columnas al DataTable si no existen
                    foreach (string columnName in columnNames)
                    {
                        if (!dataTable.Columns.Contains(columnName))
                        {
                            dataTable.Columns.Add(columnName);
                        }
                    }

                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        string[] values = line.Split(';');  // Usar punto y coma como separador

                        // Agregar fila solo si la cantidad de valores coincide con la cantidad de columnas
                        if (values.Length == dataTable.Columns.Count)
                        {
                            dataTable.Rows.Add(values);
                        }
                    }
                }
            }
        }

        private void btnConverter_Click(object sender, EventArgs e)
        {
            if (dataTable.Rows.Count == 0)
            {
                MessageBox.Show("No se ha cargado ningún archivo CSV.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivos LRC (*.lrc)|*.lrc|Todos los archivos (*.*)|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string outputLrc = saveFileDialog.FileName;

                GenerateLrcFile(outputLrc);

                MessageBox.Show($"Archivo .lrc generado: {outputLrc}", "Conversión Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void GenerateLrcFile(string outputLrc)
        {
            using (StreamWriter lrcWriter = new StreamWriter(outputLrc, false, Encoding.UTF8))
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (row.ItemArray.Length >= 2) // Considera las columnas del archivo CSV (milisegundos, texto)
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


    }
}

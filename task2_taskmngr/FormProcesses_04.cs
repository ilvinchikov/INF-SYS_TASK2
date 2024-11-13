using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

// https://ru.stackoverflow.com/questions/618689/%D0%9F%D0%BE%D0%BB%D1%83%D1%87%D0%B8%D1%82%D1%8C-%D1%81%D1%87%D0%B5%D1%82%D1%87%D0%B8%D0%BA%D0%B8-%D0%BF%D1%80%D0%BE%D0%B8%D0%B7%D0%B2%D0%BE%D0%B4%D0%B8%D1%82%D0%B5%D0%BB%D1%8C%D0%BD%D0%BE%D1%81%D1%82%D0%B8

namespace task2_taskmngr
{
    public partial class FormProcesses_04 : Form
    {
        Process[] processes;    // процессы
        private CancellationTokenSource cantoken;   // токен  для ассинхронного процесса
        public FormProcesses_04()
        {
            InitializeComponent();
        }

        private void FormProcesses_04_Load(object sender, EventArgs e)
        {
            _ = StarterAsync(); // запуск динамической части в ассинхронном режиме
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // если что то изменяется - возвращаем все ячейки в норму
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Visible = true;           // включаем все строки обратно, если текст в поле изменился, чтобы не упустить какую-либо ячейку с новым измененным текстом
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.White;   // возвращаем всем ячейкам стандартный цвет, перед новым поиском
                }
            }
            if (textBox1.Text != null && textBox1.Text.Equals("") == false) Search();       // СТАРТ ПОИСКА, если строка не пуста
        }
        private void FormProcesses_04_FormClosing(object sender, FormClosingEventArgs e)
        {
            cantoken?.Cancel();
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e) // выбор строки
        {
            if (dataGridView1.SelectedRows != null) button1.Enabled = true;
            else button1.Enabled = false;
        }

        // custom code:
        private string RespondingNamed(bool responding)
        {
            if (responding) return "  ";
            else return "Не отвечает";
        }
        private string MachineNameToDefault(string machineName)
        {
            if (machineName.Equals(".")) return Environment.MachineName;
            else return machineName;
        }
        private void Search()
        {
            bool WordIsFinded = false;          // true = если найдено совпадение в ячейке
            dataGridView1.CurrentCell = null;   // для того чтобы не выдавало ошибку при сокрытии ячеек
            for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    if (dataGridView1.Rows[i].Cells[j].Value != null && dataGridView1.Rows[i].Cells[j].Value.ToString().ToLower().Contains(textBox1.Text.ToLower())) // проверка ячеек
                    {
                        WordIsFinded = true;
                        dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.FromArgb(255, 201, 201); // красим ячейку в красный если в ней содержится искомый текст
                    }
                }
                if (!WordIsFinded)
                {
                    dataGridView1.Rows[i].Visible = false;  // скрываем ячейку если не нашли искомый текст
                }
                WordIsFinded = false;                       // сброс переменной
            }
        }
        private async Task StarterAsync()
        {
            cantoken = new CancellationTokenSource();
            var token = cantoken.Token;
            await UpdaterTableAsync(token);
        }
        public async Task UpdaterTableAsync(CancellationToken token)
        {
            string selectedIDProcess = "";
            int savedRowPosition;
            DataGridViewColumn oldSelectedColumn = null;
            string oldOrderOfColumn = "";
            while (true)
            {
                // сохраняем состояние предыдущей табл.
                oldSelectedColumn = dataGridView1.SortedColumn;
                oldOrderOfColumn = dataGridView1.SortOrder.ToString();
                savedRowPosition = dataGridView1.FirstDisplayedScrollingRowIndex;
                if (dataGridView1.SelectedRows.Count > 0) selectedIDProcess = dataGridView1.SelectedRows[0].Cells[2].Value.ToString(); // сохраняем позицию пользователя в таблице по ID процесса
                // обнуляем и получаем новые данные
                dataGridView1.Rows.Clear(); // очищаем старые данные из табл.
                processes = Process.GetProcesses(); // получаем свежые данные по процессам
                // заносим данные в табл.
                foreach (var element in processes)
                {
                    double timeCPUUsage;
                    try
                    {
                        timeCPUUsage = Math.Round(element.TotalProcessorTime.TotalSeconds, 2);
                    }
                    catch (Exception)
                    {
                        timeCPUUsage = -1.0;
                        label2.Text = "Примечание: значение \"-1\" означает, что отсутсвуют права доступа для просмотра. Решение: запустите программу от имени администратора.";
                    }
                    dataGridView1.Rows.Add(element.ProcessName, RespondingNamed(element.Responding), element.Id,
                        MachineNameToDefault(element.MachineName), element.BasePriority, element.MainWindowTitle, timeCPUUsage,
                        (element.VirtualMemorySize64 / 1024), (element.WorkingSet64 / 1024));

                }
                textBox1_TextChanged(null, null);   // возвращаем сортировку по поиску
                // возвращаем ранее убранную сортировку
                if (oldSelectedColumn != null)
                {
                    switch (oldOrderOfColumn)
                    {
                        case "Ascending":
                            dataGridView1.Sort(oldSelectedColumn, ListSortDirection.Ascending);
                            break;
                        default:
                            dataGridView1.Sort(oldSelectedColumn, ListSortDirection.Descending);
                            break;
                    }
                }
                // возвращаем пользователя на ранюю позицию
                if (savedRowPosition >= 0) dataGridView1.FirstDisplayedScrollingRowIndex = savedRowPosition;
                if (!selectedIDProcess.Equals(""))
                {
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        if (dataGridView1.Rows[i].Cells[2].Value.ToString().Equals(selectedIDProcess))
                        {
                            dataGridView1.Rows[i].Selected = true;
                            selectedIDProcess = ""; // обнуление переменной
                            break;
                        }
                    }
                }
                await Task.Delay(5000, token);
            }
        }

        private void button1_Click(object sender, EventArgs e)  // СНЯТЬ ПРОЦЕСС
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                Process prc = Process.GetProcessById(int.Parse(dataGridView1.SelectedRows[0].Cells[2].Value.ToString()));
                prc.Kill();
            } 
        }
    }
}

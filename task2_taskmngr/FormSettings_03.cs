using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace task2_taskmngr
{
    public partial class FormSettings_03 : Form
    {
        SeriesChartType ChartType = SeriesChartType.StackedArea100; // тип диаграммы ещё не выбран
        Color Color1 = Color.Empty; // цвет1 ещё не выбран
        Color Color2 = Color.Empty; // цвет2 ещё не выбран
        
        private FormApparat_02 form2;
        public FormSettings_03(FormApparat_02 form2)
        {
            InitializeComponent();
            this.form2 = form2;
        }

        private void FormSettings_03_Load(object sender, EventArgs e)
        {
            switch (form2.mode)
            {
                case 1: // режим логов и аппаратной части
                    // подстраиваем настройки под форму
                    label1.Text = "· Датчик логирования";
                    label4.Location = label2.Location;
                    textBox1.Location = comboBox2.Location;
                    comboBox1.Items.RemoveAt(comboBox1.Items.Count - 1);    // убираем диски из выбора, т.к они для dashboard
                    // откл. видимость для ненужных параметров
                    OffVisibilityObjects(new Label[] { label2, label3, label6 }, new Button[] { button3, button4, button5, button6 });
                    break;
                case 2: // режим dashboard
                    break;
                default:    // исключения
                    this.Close();
                    break;
            }
        }

        

        private void button1_Click(object sender, EventArgs e) // СОХРАНИТЬ
        {
            ClassSettingsProgramm classSettings = new ClassSettingsProgramm();
            if (textBox1.Text.ToString().Trim().Equals("")) textBox1.Text = "0";

            // запись макс.числа логов
            if (UInt64.TryParse(textBox1.Text, out _))  
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        // значения в форме
                        form2.MaxSizeLogsCPU = Int64.Parse(textBox1.Text.Trim());
                        break;
                    case 1:
                        // значения в форме
                        form2.MaxSizeLogsRAM = Int64.Parse(textBox1.Text.Trim());
                        break;
                    case 2:
                        // значения в форме
                        form2.MaxSizeLogsGPU = Int64.Parse(textBox1.Text.Trim());     
                        break;
                    default:
                        break;
                }
                // запись новых настроек в файл
                classSettings.UpdateSettings(comboBox1.SelectedIndex, "monitoring_logs_size", UInt64.Parse(textBox1.Text.Trim()));
            }
            else MessageBox.Show("Число должно быть больше или равно нулю.", "Ошибка");

            // запись типа диаграмммы
            if (ChartType != SeriesChartType.StackedArea100)
            {
                classSettings.UpdateSettings(comboBox1.SelectedIndex, "dashboard_chart_type", ChartType);
            }
            // запись цвета 1
            if (Color1 != Color.Empty)
            {
                classSettings.UpdateSettings(comboBox1.SelectedIndex, "dashboard_color_1", Color1.ToArgb());
            }
            // запись цвета 2
            if (Color2 != Color.Empty)
            {
                classSettings.UpdateSettings(comboBox1.SelectedIndex, "dashboard_color_2", Color2.ToArgb());
            }
            MessageBox.Show("Изменения успешно сохранены.\nПерезайдите на форму.", "Сохранение");
        }
   
        private void button2_Click(object sender, EventArgs e) // ОТМЕНА
        {
            this.Close();
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e) // выбор графика/датчика
        {
            EnableOrDisableElements(); // включаем/отключаем элементы
            ClassSettingsProgramm classSettings = new ClassSettingsProgramm();
            var settings = classSettings.CheckSettings();
            // число значений логов
            textBox1.Text = settings[comboBox1.SelectedIndex].monitoring_logs_size.ToString();
            // тип графика
            ChartType = settings[comboBox1.SelectedIndex].dashboard_chart_type;
            switch (settings[comboBox1.SelectedIndex].dashboard_chart_type.ToString())
            {
                case "Line":
                    comboBox2.SelectedItem = "Линейный";
                    break;
                case "Spline":
                    comboBox2.SelectedItem = "Линейный (сглаж.)";
                    break;
                case "StepLine":
                    comboBox2.SelectedItem = "Ступенчатый";
                    break;
                case "Point":
                    comboBox2.SelectedItem = "Точки";
                    break;
                case "Area":
                    comboBox2.SelectedItem = "Линейный с областями";
                    break;
                case "SplineArea":
                    comboBox2.SelectedItem = "Линейный с областями (сглаж.)";
                    break;
                case "StackedArea":
                    comboBox2.SelectedItem = "Нормированный с областями";
                    break;
                case "Pie":
                    comboBox2.SelectedItem = "Круговой";
                    break;
                case "Doughnut":
                    comboBox2.SelectedItem = "Кольцевой";
                    break;
                case "Column":
                    comboBox2.SelectedItem = "Столбчатый (вертикальный)";
                    break;
                case "Bar":
                    comboBox2.SelectedItem = "Столбчатый (горизонтальный)";
                    break;
                default:
                    break;
            }
            // цвета
            Color1 = Color.FromArgb(settings[comboBox1.SelectedIndex].dashboard_color_1);
            Color2 = Color.FromArgb(settings[comboBox1.SelectedIndex].dashboard_color_2);
        }
        private void button5_Click(object sender, EventArgs e)  // цвет 1
        {
            Color1 = ColorSelected(Color1);
        }
        private void button6_Click(object sender, EventArgs e)  // цвет 2
        {
            Color2 = ColorSelected(Color2);
        }
        private void button3_Click(object sender, EventArgs e)  // кнопка "сделать прозрачным цвет 1" 
        {
            Color1 = Color.Transparent;
        }

        private void button4_Click(object sender, EventArgs e) // кнопка "сделать прозрачным цвет 2" 
        {
            Color2 = Color.Transparent;
        }
        private void comboBox2_SelectionChangeCommitted(object sender, EventArgs e) // выбор типа графика
        {
            switch (comboBox2.SelectedItem)
            {
                case "Линейный":
                    ChartType = SeriesChartType.Line;
                    break;
                case "Линейный (сглаж.)":
                    ChartType = SeriesChartType.Spline;
                    break;
                case "Ступенчатый":
                    ChartType = SeriesChartType.StepLine;
                    break;
                case "Точки":
                    ChartType = SeriesChartType.Point;
                    break;
                case "Линейный с областями":
                    ChartType = SeriesChartType.Area;
                    break;
                case "Линейный с областями (сглаж.)":
                    ChartType = SeriesChartType.SplineArea;
                    break;
                case "Нормированный с областями":
                    ChartType = SeriesChartType.StackedArea;
                    break;
                case "Круговой":
                    ChartType = SeriesChartType.Pie;
                    break;
                case "Кольцевой":
                    ChartType = SeriesChartType.Doughnut;
                    break;
                case "Столбчатый (вертикальный)":
                    ChartType = SeriesChartType.Column;
                    break;
                case "Столбчатый (горизонтальный)":
                    ChartType = SeriesChartType.Bar;
                    break;
                default:
                    break;
            }
        }

        // custom code:
        private void EnableOrDisableElements()
        {
            if (comboBox1.SelectedIndex == 3) // для дисков - логи не пишем
            {
                textBox1.Visible = false;
                label4.Visible = false;
            }
            textBox1.Enabled = true;
            comboBox2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
            button6.Enabled = true;
            button1.Enabled = true;
        }

        private void OffVisibilityObjects(Label[] objects, Button[] objects2)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].Visible = false;
            }
            for (int i = 0; i < objects2.Length; i++)
            {
                objects2[i].Visible = false;
            }
        }

        public Color ColorSelected(Color colored)
        {
            colorDialog1.FullOpen = true;
            colorDialog1.Color = colored;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                colored = colorDialog1.Color;
            }
            return colored;        
        }    
    }
}

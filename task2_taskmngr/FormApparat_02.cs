using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

// https://powershell.one/wmi/root/cimv2

namespace task2_taskmngr
{
    public partial class FormApparat_02 : Form
    {
        public byte mode;                           //  1 - режим аппаратной части и логов // 2 - режим dashboard
        public long MaxSizeLogsCPU;                 // макс.логов у CPU
        public long MaxSizeLogsRAM;                 // макс.логов у RAM
        public long MaxSizeLogsGPU;                 // макс.логов у GPU
        private double RamSize = 0;                 // макс.объём.оперативки
        private CancellationTokenSource cantoken;   // токен  для ассинхронного процесса
        List<ClassTimeMetrics> MetricsCPU_Time = new List<ClassTimeMetrics>();
        List<ClassTimeMetrics> MetricsRAM_Time = new List<ClassTimeMetrics>();
        List<ClassTimeMetrics> MetricsGPU_Time = new List<ClassTimeMetrics>();
        
        public FormApparat_02()
        {
            InitializeComponent();
        }
        
        private void FormApparat_02_Load(object sender, EventArgs e)
        {
            if (this.Name.Contains("mode=")) mode = (byte)Int64.Parse(this.Name[5].ToString()); // настройка режима  
            SettingSetupLogs(); // установка настроек логов из файла

            // оперативная память
            using (ManagementObjectSearcher searcherRAM = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject queryObj in searcherRAM.Get())
                {
                    if (mode == 1) label10.Text = Math.Round(System.Convert.ToDouble(queryObj["TotalVisibleMemorySize"]) / 1024 / 1024, 2).ToString() + " GB | (" + Math.Round(System.Convert.ToDouble(queryObj["TotalVisibleMemorySize"]) / 1024 / 1024, 0).ToString() + " GB)";   // ёмк в ГБ    
                    RamSize = Math.Round(System.Convert.ToDouble(queryObj["TotalVisibleMemorySize"]) / 1024, 2);                            // для расчетов
                }
            }
            switch (mode)
            {
                case 1: // режим аппаратной части
                    // -------------- статическая часть
                    // процессор
                    using (ManagementObjectSearcher searcherProcessor = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
                    {
                        foreach (ManagementObject queryObj in searcherProcessor.Get())
                        {
                            label8.Text = queryObj["Name"].ToString();              // назв
                            label7.Text = queryObj["NumberOfCores"].ToString();     // колво ядер
                            label17.Text = Double.Parse(queryObj["MaxClockSpeed"].ToString()) / 1000 + " GHz";   // макс частота
                        }
                    }
                    // видеоадаптер
                    using (ManagementObjectSearcher searcherGPU = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController"))
                    {
                        foreach (ManagementObject queryObj in searcherGPU.Get())
                        {
                            label26.Text = queryObj["Name"].ToString();
                            label27.Text = queryObj["DriverVersion"].ToString();
                        }
                    }
                    // диск
                    using (ManagementObjectSearcher searcherVolume = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Volume"))
                    {
                        foreach (ManagementObject queryObj in searcherVolume.Get())
                        {
                            label23.Text += "· Название диска:\n· Общий объём:\n· Используемый объём:\n· Свободный объём:\n------\n";
                            label20.Text += queryObj["Label"] + " (" + queryObj["Name"] + ")\n"
                                + Math.Round(System.Convert.ToDouble(queryObj["Capacity"]) / 1024 / 1024 / 1024, 2) + " GB\n"
                                + Math.Round((System.Convert.ToDouble(queryObj["Capacity"]) - System.Convert.ToDouble(queryObj["FreeSpace"])) / 1024 / 1024 / 1024, 2) + " GB\n"
                                + Math.Round(System.Convert.ToDouble(queryObj["FreeSpace"]) / 1024 / 1024 / 1024, 2) + " GB\n" + "------\n";
                        }
                    }
                    break;
                case 2: // режим DASHBOARD
                    // отключаем элементы
                    OffVisibilityLabels(new Label[] { label1, label2, label6, label9, label30, label8, label7, label17, 
                        label31, label21, label14, label13, label15, label10, label12, label16, label22, label29, label28, 
                        label25, label26, label27, label24, label32, label18, label19, label34, label35, label33, label5 });
                    OffVisibilityListBoxs(new ListBox[] { listBox1, listBox2, listBox3 });
                    pictureBox3.Visible = false;
                    // редактируем элементы
                    // ------ картинки
                    pictureBox1.Location = new Point(27, 13);
                    pictureBox2.Location = new Point(27, 301);
                    pictureBox4.Location = new Point(508, 13);
                    // ------ тексты
                    label3.Location = new Point(69, 21);
                    label3.Text = "Процессор (CPU): загруженность";
                    label4.Location = new Point(69, 309);
                    label4.Text = "Оперативная память (RAM): занято";
                    label11.Location = new Point(550, 21);
                    label11.Text = "Графический процессор (GPU): выделено";
                    // ------ кнопки
                    button1.Location = new Point(1092, 12);
                    button1.Anchor = (AnchorStyles.Top | AnchorStyles.Right);
                    // создаем чарты-графики без циклов

                    // процессор
                    Chart chart_CPU_Total_Percent = new Chart();
                    SettingSetupChart(chart_CPU_Total_Percent, 12, 43, "%", SeriesChartType.Line, -1.0, 0);  // Line, Spline, StepLine, Point, Area, SplineArea, StackedArea  
                    chart_CPU_Total_Percent.DataSource = MetricsCPU_Time;
                    chart_CPU_Total_Percent.ChartAreas[0].AxisY.Maximum = 100; // указываем максимум на графике
                    // опер.память
                    Chart chart_RAM_Used_MB = new Chart();
                    SettingSetupChart(chart_RAM_Used_MB, 12, 331, "MB", SeriesChartType.Line, -1.0, 1);
                    chart_RAM_Used_MB.DataSource = MetricsRAM_Time;
                    chart_RAM_Used_MB.ChartAreas[0].AxisY.Maximum = RamSize; // указываем максимум на графике          
                    // видеоадаптер
                    Chart chart_GPU_Dedicated_MB = new Chart();
                    SettingSetupChart(chart_GPU_Dedicated_MB, 493, 43, "MB", SeriesChartType.Line, -1.0, 2);
                    chart_GPU_Dedicated_MB.DataSource = MetricsGPU_Time;
                    // создаем чарты-графики с циклами

                    // диск
                    using (ManagementObjectSearcher searcherVolume = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Volume"))
                    {
                        int loc_x_chartDisk = 12;
                        int loc_x_picBoxDisk = 27;
                        int loc_x_textDisk = 69;
                        int loc_y_chartDisk = chart_RAM_Used_MB.Location.Y;
                        int loc_y_picBoxDisk = pictureBox2.Location.Y;
                        int loc_y_textDisk = label4.Location.Y;
                        int counter = 1;
                        foreach (ManagementObject queryObj in searcherVolume.Get())
                        {
                            if (queryObj["Name"].ToString().Length < 4) // оставляем только физические накопители
                            {     
                                if ((counter+2) % 2 == 0) // формируем в таблицу 2 на X
                                {
                                    loc_x_chartDisk = 12;
                                    loc_x_picBoxDisk = 27;
                                    loc_x_textDisk = 69;
                                    loc_y_chartDisk += chart_RAM_Used_MB.Location.Y- chart_CPU_Total_Percent.Location.Y;
                                    loc_y_picBoxDisk += pictureBox2.Location.Y - pictureBox1.Location.Y;
                                    loc_y_textDisk += label4.Location.Y - label3.Location.Y;
                                }
                                else
                                {
                                    loc_x_chartDisk += 481;
                                    loc_x_picBoxDisk += 481;
                                    loc_x_textDisk += 481;
                                }
                                this.Controls.Add(new PictureBox()
                                {
                                    Image = Properties.Resources.hdd,
                                    Size = new Size(36, 36),
                                    Location = new Point(loc_x_picBoxDisk, loc_y_picBoxDisk),
                                    SizeMode = PictureBoxSizeMode.Zoom,
                                });
                                // добавляем текст к чарту с именем диска 
                                this.Controls.Add(new Label()
                                {
                                    Text = "Диск: " + queryObj["Label"] + " (" + queryObj["Name"] + ")",
                                    Size = new Size(250, 19),
                                    Font = new Font("Microsoft YaHei", 11.25f, FontStyle.Bold),
                                    Location = new Point(loc_x_textDisk, loc_y_textDisk)
                                });
                                // добавляем чарт о диске
                                SettingSetupChart(new Chart(), loc_x_chartDisk, loc_y_chartDisk, "MB", SeriesChartType.Pie,
                                    Math.Round(Convert.ToDouble(queryObj["FreeSpace"])*100/ Convert.ToDouble(queryObj["Capacity"]), 2), 3);   // Pie, Doughnut
                                counter++;
                            }
                        }
                    }
                    
                    break;
                default:    // исключения
                    break;
            }

            _ = StarterAsync(); // запуск динамической части в ассинхронном режиме

            Console.WriteLine("дошел_лоад");

        }
   
        private void FormApparat_02_FormClosing(object sender, FormClosingEventArgs e)
        {
            cantoken?.Cancel();
        }
        
        private void button1_Click(object sender, EventArgs e) // кнопка НАСТРОЙКИ
        {
            FormSettings_03 form3 = new FormSettings_03(this);
            form3.ShowDialog();
        }

        // custom code:
        private async Task DynamicStats(CancellationToken token) 
        {
            double cpuValue;
            double ramValue;
            double gpuValue;
            Console.WriteLine("async_task_start");
            // -------------- динамическая часть (обновляемые данные)
            using (PerformanceCounter cpu = new PerformanceCounter("Процессор", "% загруженности процессора", "_Total")) // значения из "Системный монитор"
            {
                using (PerformanceCounter ram_available = new PerformanceCounter("Память", "Доступно МБ"))
                {
                    PerformanceCounterCategory category = new PerformanceCounterCategory("GPU Adapter Memory");
                    string VideoAdapter = "";
                    foreach (string videoAdapter_Test in category.GetInstanceNames())
                    {
                        using (PerformanceCounter gpuTest = new PerformanceCounter("GPU Adapter Memory", "Dedicated Usage", videoAdapter_Test))
                        {
                            if (gpuTest.NextValue() > 0)
                            {
                                VideoAdapter = videoAdapter_Test;
                                break;
                            }
                        }     
                    }
                    using (PerformanceCounter gpu = new PerformanceCounter("GPU Adapter Memory", "Dedicated Usage", VideoAdapter))
                    {

                        while (true)
                        {
                            cpuValue = cpu.NextValue();
                            ramValue = ram_available.NextValue();
                            gpuValue = gpu.NextValue();
                            // процессор
                            label31.Text = (Math.Floor(cpuValue * 100) / 100).ToString() + "%";
                            if (MaxSizeLogsCPU > 0) listBox1.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " = " + label31.Text); // логи
                            if (listBox1.Items.Count > MaxSizeLogsCPU) listBox1.Items.RemoveAt(0);
                            
                                
                            // опер.память
                            if (RamSize != 0) label12.Text = RamSize - ramValue + " MB"; // занято RAM
                            if (MaxSizeLogsRAM > 0) listBox2.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " = " + label12.Text); // логи
                            if (listBox2.Items.Count > MaxSizeLogsRAM) listBox2.Items.RemoveAt(0);
                            // видеоадаптер
                            label24.Text = Math.Round(gpuValue / 1024 / 1024, 2).ToString() + " MB";
                            if (MaxSizeLogsGPU > 0) listBox3.Items.Add(DateTime.Now.ToString("HH:mm:ss") + " = " + label24.Text); // логи
                            if (listBox3.Items.Count > MaxSizeLogsGPU) listBox3.Items.RemoveAt(0);
                            // остальное
                            if (mode == 1)
                            {
                                // опер. память
                                label16.Text = ram_available.NextValue().ToString() + " MB";                    // свободно RAM
                            }
                            else
                            {
                                // процессор
                                MetricsCPU_Time.Add(new ClassTimeMetrics(Math.Round(cpuValue,1), DateTime.Now.ToString("HH:mm:ss")));
                                if (MetricsCPU_Time.Count > MaxSizeLogsCPU) MetricsCPU_Time.RemoveAt(0);
                                // опер. память - занято
                                MetricsRAM_Time.Add(new ClassTimeMetrics(Math.Round(RamSize - ramValue, 1), DateTime.Now.ToString("HH:mm:ss")));
                                if (MetricsRAM_Time.Count > MaxSizeLogsRAM) MetricsRAM_Time.RemoveAt(0);
                                // видеоадаптер
                                MetricsGPU_Time.Add(new ClassTimeMetrics(Math.Round(gpuValue / 1024 / 1024, 1), DateTime.Now.ToString("HH:mm:ss")));
                                if (MetricsGPU_Time.Count > MaxSizeLogsGPU) MetricsGPU_Time.RemoveAt(0);
                                foreach (var chart in this.Controls.OfType<Chart>())
                                {
                                    chart.DataBind();
                                }
                            }
                            CreatingLogsToFile();   // логи в файл
                            await Task.Delay(5000, token); // кулдаун
                        }
                    }
                }
            }
        }
        private async Task StarterAsync()
        {
            cantoken = new CancellationTokenSource();
            var token = cantoken.Token;
            await DynamicStats(token);
        }

        private void SettingSetupChart(Chart chart, int loc_x, int loc_y, string namelegend, SeriesChartType linetype, double Disk_series_X, byte mode)
        {
            ClassSettingsProgramm classSettings = new ClassSettingsProgramm();
            var settings = classSettings.CheckSettings();
            // настройка графика // indexes "mode": 0 - CPU, 1 - RAM, 2 - GPU, 3 - DISK

            // добавление легенды
            Legend legend = new Legend
            {
                BackColor = Color.Transparent
            };
            chart.Legends.Add(legend);
            // добавление графика
            ChartArea chartarea = new ChartArea
            {
                Name = "ChartArea",
                BackColor = Color.Transparent
            };
            chart.ChartAreas.Add(chartarea);
            // добавление значений на график
            Series series = new Series
            {
                Name = namelegend,
                Color = Color.FromArgb(settings[mode].dashboard_color_1),
                IsValueShownAsLabel = true,
                XValueMember = "Time",
                YValueMembers = "Value",
                ChartType = settings[mode].dashboard_chart_type
            };
            chart.Series.Add(series);
            // настраиваем чарты про диск
            if (Disk_series_X > 0) 
            {
                series.Points.AddXY("% свободно", Disk_series_X);
                series.Points.AddXY("% занято", 100 - Disk_series_X);
                series.Color = Color.Transparent;           // оставляем прозрачным серию
                chartarea.BackColor = Color.Transparent;    // зону чарта оставляем прозрачным
                // задаем цвета долям
                chart.Series[0].Points[0].Color = Color.FromArgb(settings[mode].dashboard_color_1);
                chart.Series[0].Points[1].Color = Color.FromArgb(settings[mode].dashboard_color_2);
            }
            else  // настраиваем остальные чарты  
            {
                chartarea.Axes[0].MajorGrid.LineColor = Color.FromArgb(settings[mode].dashboard_color_2); // настраиваем цвет сетки (решетки)
                chartarea.Axes[1].MajorGrid.LineColor = Color.FromArgb(settings[mode].dashboard_color_2);
            }
            // общие настройки чарта
            chart.Location = new Point(loc_x, loc_y);
            chart.Size = new Size(475, 257);
            chart.BackColor = Color.Transparent;
            // добавление на форму
            this.Controls.Add(chart);
        }

        private void SettingSetupLogs()
        {
            ClassSettingsProgramm classSettings = new ClassSettingsProgramm();
            var settings = classSettings.CheckSettings();
            if (settings != null)
            {
                // логирование
                MaxSizeLogsCPU = (long)settings[0].monitoring_logs_size;
                if (MaxSizeLogsCPU == 0)
                {
                    label19.Visible = false;
                    listBox1.Visible = false;
                }
                MaxSizeLogsRAM = (long)settings[1].monitoring_logs_size;
                if (MaxSizeLogsRAM == 0)
                {
                    label34.Visible = false;
                    listBox2.Visible = false;
                }
                MaxSizeLogsGPU = (long)settings[2].monitoring_logs_size;
                if (MaxSizeLogsGPU == 0)
                {
                    label35.Visible = false;
                    listBox3.Visible = false;
                }    
            }
        }
        private void OffVisibilityLabels(Label[] labels)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].Visible = false;
            }
        }
        private void OffVisibilityListBoxs(ListBox[] listboxes)
        {
            for (int i = 0; i < listboxes.Length; i++)
            {
                listboxes[i].Visible = false;
            }
        }
        private void CreatingLogsToFile()
        {
            string logs_to_file = "";
            if (MaxSizeLogsCPU > 0)
            {
                logs_to_file += "CPU Logs:\n";
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    logs_to_file += listBox1.Items[i] + "\n";
                }
            }
            if (MaxSizeLogsRAM > 0)
            {
                logs_to_file += "RAM Logs:\n";
                for (int i = 0; i < listBox2.Items.Count; i++)
                {
                    logs_to_file += listBox2.Items[i] + "\n";
                }
            }
            if (MaxSizeLogsGPU > 0)
            {
                logs_to_file += "GPU Logs:\n";
                for (int i = 0; i < listBox2.Items.Count; i++)
                {
                    logs_to_file += listBox2.Items[i] + "\n";
                }
            }
            File.WriteAllText("last_logs.txt", logs_to_file);
        }
    }
}

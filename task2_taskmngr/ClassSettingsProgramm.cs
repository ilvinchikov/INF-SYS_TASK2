using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace task2_taskmngr
{
    class ClassSettingsProgramm
    {
        //  indexes: 0 - CPU, 1 - RAM, 2 - GPU, 3 - DISK
        public ulong monitoring_logs_size { get; set; }
        public SeriesChartType dashboard_chart_type { get; set; }
        public int dashboard_color_1 { get; set; }
        public int dashboard_color_2 { get; set; }

        public ClassSettingsProgramm() { }
        public ClassSettingsProgramm(ulong monitoring_logs_size, SeriesChartType dashboard_chart_type, int dashboard_color_1, int dashboard_color_2)
        {
            this.monitoring_logs_size = monitoring_logs_size;
            this.dashboard_chart_type = dashboard_chart_type;
            this.dashboard_color_1 = dashboard_color_1;
            this.dashboard_color_2 = dashboard_color_2;
        }
        public List<ClassSettingsProgramm> CheckSettings()
        {
            if (File.Exists("settings_taskmngr.json"))
            {
                using (StreamReader reader = new StreamReader("settings_taskmngr.json"))
                {
                    string jsonString = reader.ReadToEnd();
                    try
                    {
                        var jsonList = JsonSerializer.Deserialize<List<ClassSettingsProgramm>>(jsonString);  // десериализация (конвертация) файла с помощью system.text.json
                        for (int i = 0; i < jsonList.Count; i++)
                        {
                            if (!(UInt64.TryParse(jsonList[i].monitoring_logs_size.ToString().Trim(), out _)))
                            {
                                WriteNewSettings();         // сброс настроек по умолч.
                                return CheckSettings();
                            }
                        }
                        return jsonList;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Ошибка");
                        reader.Close();
                        // сброс настроек по умолч.
                        WriteNewSettings();
                        return CheckSettings();
                    }
                }
            }
            else
            {
                WriteNewSettings();
                return CheckSettings();
            }
        }
        public void WriteNewSettings()
        {
            var options = new JsonSerializerOptions()   // настройки json 
            {

                IncludeFields = true,   // чтобы вообще работало с листом (изначально не записываются данные в json)
                WriteIndented = true,   // для красивого вида в документе (изначально всё в одной строке)
            };
            List<ClassSettingsProgramm> listjson = new List<ClassSettingsProgramm>();
            for (int i=0; i<4; i++) // 0 - CPU, 1 - RAM, 2 - GPU, 3 - DISKS
            {
                if (i != 3) listjson.Add(new ClassSettingsProgramm(10, SeriesChartType.Line, -65536, 16777215));    // для динамических графиков
                else listjson.Add(new ClassSettingsProgramm(0, SeriesChartType.Pie, -7063020, -40427));             // для статических графиков
            }
            File.WriteAllText("settings_taskmngr.json", JsonSerializer.Serialize(listjson, options)); // сериализация(конвертация)
        }
        public void UpdateSettings(int index, string settingName, object NewValue)
        {
            var settings = CheckSettings();
            if (settings != null)
            {
                switch (settingName)
                {
                    case "monitoring_logs_size":
                        settings[index].monitoring_logs_size = (ulong)NewValue;
                        break;
                    case "dashboard_chart_type":
                        settings[index].dashboard_chart_type = (SeriesChartType)NewValue;
                        break;
                    case "dashboard_color_1":
                        settings[index].dashboard_color_1 = (int)NewValue;
                        break;
                    case "dashboard_color_2":
                        settings[index].dashboard_color_2 = (int)NewValue;
                        break;
                    default:
                        Console.WriteLine("Invalid setting name.");
                        break;
                }
                var options = new JsonSerializerOptions()   // настройки json 
                {

                    IncludeFields = true,   // чтобы вообще работало с листом (изначально не записываются данные в json)
                    WriteIndented = true,   // для красивого вида в документе (изначально всё в одной строке)
                };
                File.WriteAllText("settings_taskmngr.json", JsonSerializer.Serialize(settings, options)); // сериализация(конвертация)
            }
        }
    }
}

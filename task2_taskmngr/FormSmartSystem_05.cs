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
using System.Windows.Forms;

// источники:
// https://powershell.one/wmi/root/cimv2/win32_volume
// https://stackoverflow.com/questions/8079325/how-to-read-harddisk-temperature
// https://wutils.com/wmi/root/wmi/msstoragedriver_atapismartdata/
// https://vasters.com/archive/Reading-ATAPI-SMART-Data-From-Drives-Using-NET-Temperature-Anyone.html
// https://bva.dyndns.info/wiki/tasks/smart
// https://winitpro.ru/index.php/2021/05/26/opredelit-sostoyanie-diska-windows-smart/

namespace task2_taskmngr
{
    public partial class FormSmartSystem_05 : Form
    {

        byte modeLanguage = 0;  // 0 - RUS // 1 - ENG
        
        List<ClassConnectionDisks> ConnectionDisks = new List<ClassConnectionDisks>();
        public FormSmartSystem_05()
        {
            InitializeComponent();
        }

        private void FormSmartSystem_05_Load(object sender, EventArgs e)
        {
            int loc_x_textDisk = 63; // +0
            int loc_y_textDisk = 92; // +318
            int loc_x_picbox = 21; // +0
            int loc_y_picbox = 84; // +318
            int loc_x_dataGridView = 21; // +0
            int loc_y_dataGridView = 133; // +318
            ConnectionsBetweenLogicalAndPhysicalDisks(); 
            using (ManagementObjectSearcher searcherVolume = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Volume"))
            {
                foreach (ManagementObject queryObj in searcherVolume.Get())
                {
                    if (queryObj["Name"].ToString().Length < 4) // оставляем только физические накопители
                    {
                        try
                        {
                            // добавляем текст
                            this.Controls.Add(new Label()
                            {
                                Text = "Диск: " + queryObj["Label"] + " (" + queryObj["Name"] + ")",
                                Size = new Size(250, 19),
                                Font = new Font("Microsoft YaHei", 11.25f, FontStyle.Bold),
                                Location = new Point(loc_x_textDisk, loc_y_textDisk)
                            });
                            loc_y_textDisk += 318;

                            // добавляем картинку
                            this.Controls.Add(new PictureBox()
                            {
                                Image = Properties.Resources.volume,
                                Size = new Size(36, 36),
                                Location = new Point(loc_x_picbox, loc_y_picbox),
                                SizeMode = PictureBoxSizeMode.Zoom,
                            });
                            loc_y_picbox += 318;
                            // добавляем таблицу
                            string[] columnID = { "columnAttr", "columnValue", "columnCurrent", "columnWorst", "columnThreshold", "columnStatus",
                                "Атрибут", "Значение", "Текущее", "Худшее", "Порог", "Статус"};
                            DataGridView table = new DataGridView()
                            {
                                Size = new Size(1053, 262),
                                Location = new Point(loc_x_dataGridView, loc_y_dataGridView),
                                //Anchor = (AnchorStyles.Left | AnchorStyles.Right),
                                MultiSelect = false,
                                ReadOnly = true,
                                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                                AllowUserToAddRows = false,
                                AllowUserToDeleteRows = false,
                                AllowDrop = false,
                                EditMode = DataGridViewEditMode.EditProgrammatically
                            };
                            for (int i=0; i<(columnID.Length/2); i++)
                            {
                                table.Columns.Add(columnID[i], columnID[i + (columnID.Length / 2)]);
                            }
                            loc_y_dataGridView += 318;
                            // данные в таблицу
                            string[] FirstAttributes = {"Имя логического диска", "Общий объём (GB)", "Используемый объём (GB)", "Свободный объём (GB)", "Серийный номер", "Файловая система", "Тип диска", "Размер блока (B)",
                                "Volume Name", "Capacity (GB)", "Usage Space (GB)", "Free Space (GB)", "Volume Serial Number ", "File System", "Drive Type", "Block Size (B)"};
                            object[] FirstValues = { queryObj["Label"] + " (" + queryObj["Name"] + ")", Math.Round(Convert.ToDouble(queryObj["Capacity"]) / 1024 / 1024 / 1024, 2), Math.Round((Convert.ToDouble(queryObj["Capacity"]) - System.Convert.ToDouble(queryObj["FreeSpace"])) / 1024 / 1024 / 1024, 2), Math.Round(Convert.ToDouble(queryObj["FreeSpace"]) / 1024 / 1024 / 1024, 2), queryObj["SerialNumber"], queryObj["FileSystem"], DriveTypeWIM32(queryObj["DriveType"]), queryObj["BlockSize"] };
                            for (int i=0; i<(FirstAttributes.Length/2); i++)
                            {
                                if (modeLanguage == 0) table.Rows.Add(FirstAttributes[i], FirstValues[i], "", "", "", "");
                                else table.Rows.Add(FirstAttributes[i + (FirstAttributes.Length / 2)], FirstValues[i], "", "", "", "");
                            }
                            // часть 2 - // SMART данные
                            SmartInformationReturn(queryObj["Name"].ToString().Replace("\"", ""), table);
                            this.Controls.Add(table); // добавляем таблицу
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("Ошибка: " + err +"\nВозможное решение: запустите программу от имени администратора.");
                        }
                    }
                }
            } 
        }
        private void button1_Click(object sender, EventArgs e)  // кнопка RUS 
        {
            modeLanguage = 0;
            ReloadForm();    // перезагрузка формы
        }
        private void button2_Click(object sender, EventArgs e)  // кнопка ENG 
        {
            modeLanguage = 1;
            ReloadForm();    // перезагрузка формы
        }
        // custom code:
        private void SmartInformationReturn(string str, DataGridView table) // SMART-данные
        {
            int index = 0;
            ClassReturnSmart SmartInfo = new ClassReturnSmart();
            var drives = SmartInfo.GetSmartInfo();
            if (drives != null)
            {
                foreach (var drive in drives)
                {
                    if (str.Contains(ConnectionDisks[index].LogicalDisk) && ConnectionDisks[index].PhysycalDisk.Trim().Equals(drive.Value.DeviceID.Trim()))
                    {
                        // данные диска (доп.)
                        string[] SecondAttributes = {"Серийный номер накопителя", "Модель накопителя", "Тип накопителя", "Статус накопителя", "ID устройства",
                                "Disk Serial Number", "Disk Model", "Disk Type", "Disk Status", "Disk Device ID"};
                        object[] SecondValues = { drive.Value.Serial, drive.Value.Model, drive.Value.Type, SMARTSystemErrors(drive.Value.Status), drive.Value.DeviceID };
                        for (int i = 0; i < (SecondAttributes.Length / 2); i++)
                        {
                            if (modeLanguage == 0) table.Rows.Add(SecondAttributes[i], SecondValues[i], "", "", "", "");
                            else table.Rows.Add(SecondAttributes[i + (SecondAttributes.Length / 2)], SecondValues[i], "", "", "", "");
                        }

                        // SMART-аттрибуты диска
                        foreach (var attr in drive.Value.Attributes)
                        {
                            if (attr.Value.HasData)
                            {
                                table.Rows.Add(CutterWordToCurLang(attr.Value.Attribute), attr.Value.Data, attr.Value.Current, attr.Value.Worst, attr.Value.Threshold, SMARTSystemErrors(attr.Value.Status));
                            }
                        }
                    }
                    else index++;
                }
            }
            else
            {
                table.Rows.Add("Ошибка: Не удалось заполучить SMART-данные о диске");
            }
        }
        private string CutterWordToCurLang(string strToCut) // обрезаем строку под конкретный язык (см. класс ClassSmartInfo)
        {
            if (modeLanguage == 0) return strToCut.Substring(strToCut.IndexOf('|')+2, (strToCut.Length-strToCut.IndexOf('|'))-2);
            else return strToCut.Substring(0, strToCut.IndexOf('|'));
        }
        private string SMARTSystemErrors(bool status)
        {
            switch (modeLanguage)
            {
                case 1:
                    if (!status) return "BAD";
                    else return "OK";
                default:
                    if (!status) return "Ужас";
                    else return "OK";
            }
        }
        private string DriveTypeWIM32(object value)
        {
            switch (Int32.Parse(value.ToString()))
            {
                case 0:
                    return "Unknown";
                case 1:
                    return "No Root Directory";
                case 2:
                    return "Removable Disk";
                case 3:
                    return "Local Disk";
                case 4:
                    return "Network Drive";
                case 5:
                    return "Compact Disk";
                case 6:
                    return "RAM Disk";
                default:
                    return value.ToString();
            }
        }

        
        private void ReloadForm()
        {
            foreach (var table in this.Controls.OfType<DataGridView>())
            {
                table.Dispose();
            }
            foreach (var picBox in this.Controls.OfType<PictureBox>())
            {
                if (!picBox.Name.Equals(pictureBox3.Name)) picBox.Dispose(); // удаляем все картинки, кроме основной
            }
            foreach (var label in this.Controls.OfType<Label>())
            {
                if (!label.Name.Equals(label5.Name)) label.Dispose(); // удаляем все тексты, кроме основного
            }
            FormSmartSystem_05_Load(null, null);    // перезагрузка формы
        }
        private void ConnectionsBetweenLogicalAndPhysicalDisks()
        {
            // получаем все физические диски
            using (ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
            {
                using(ManagementObjectCollection disks = diskSearcher.Get())
                {
                    // получаем все логические диски
                    using (ManagementObjectSearcher volumeSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Volume"))
                    {
                        using (ManagementObjectCollection volumes = volumeSearcher.Get())
                        {
                            // физические диски
                            foreach (ManagementObject disk in disks)
                            {
                                string diskDeviceID = disk["DeviceID"].ToString();
                                // получаем связи между диском и его разделами
                                using(ManagementObjectSearcher partitionSearcher = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{diskDeviceID}'}} WHERE AssocClass=Win32_DiskDriveToDiskPartition"))
                                {
                                    using(ManagementObjectCollection partitions = partitionSearcher.Get())
                                    {
                                        // разделы
                                        foreach (ManagementObject partition in partitions)
                                        {
                                            string partitionDeviceID = partition["DeviceID"].ToString();
                                            // получаем связи между разделом и логическими дисками
                                            using (ManagementObjectSearcher logicalDiskSearcher = new ManagementObjectSearcher($"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partitionDeviceID}'}} WHERE AssocClass=Win32_LogicalDiskToPartition"))
                                            {
                                                using(ManagementObjectCollection logicalDisks = logicalDiskSearcher.Get())
                                                {
                                                    // Для каждого логического диска
                                                    foreach (ManagementObject volume in logicalDisks)
                                                    {
                                                        string volumeDeviceID = volume["DeviceID"].ToString();
                                                        ConnectionDisks.Add(new ClassConnectionDisks(volumeDeviceID, diskDeviceID));
                                                    }
                                                }
                                            }     
                                        }
                                    }
                                } 
                            }
                        }
                    }
                }
            }   
        }
    }
}
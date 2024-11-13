using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Windows.Forms;

// источники:
// https://www.bestor.spb.ru/v3/faq?cat_id=1037&subcat_id=1114&faq_id=1139
// http://know24.net/blog/C%2BWMI%2BHDD%2BSMART%2BInformation.aspx
// http://harddiscsentinel.helpmax.net/ru/%D0%BF%D1%80%D0%B8%D0%BB%D0%BE%D0%B6%D0%B5%D0%BD%D0%B8%D0%B5/%D0%B0%D1%82%D1%80%D0%B8%D0%B1%D1%83%D1%82%D1%8B-s-m-a-r-t/
namespace task2_taskmngr
{
    public class ClassSmartInfo
    {   
        // информация по диску:
        public string Model { get; set; }   // модель
        public bool Status { get; set; }    // статус: ок/есть ошибки
        public string Type { get; set; }    // тип
        public string DeviceID { get; set; }    // ID устройства 
        public string Serial { get; set; }      // серийный номер
        public Dictionary<int, SmartAttribute> Attributes = new Dictionary<int, SmartAttribute>() {           // smart информация
                {0x00, new SmartAttribute("Invalid                         | Недействительно")},
                {0x01, new SmartAttribute("Raw read error rate             | Ошибок при чтении из-за аппаратных неполадок диска")},
                {0x02, new SmartAttribute("Throughput performance          | Рабочая эффективность")},
                {0x03, new SmartAttribute("Spinup time                     | Время раскрутки шпинделя")},
                {0x04, new SmartAttribute("Start/Stop count                | Запусков/остановок шпинделя")},
                {0x05, new SmartAttribute("Reallocated sector count        | Переназначенных секторов")},
                {0x06, new SmartAttribute("Read channel margin             | Запас канала чтения")},
                {0x07, new SmartAttribute("Seek error rate                 | Ошибок позиционирования магнитных головок")},
                {0x08, new SmartAttribute("Seek timer performance          | Сред.производительность операции позиционирования магнитными головками")},
                {0x09, new SmartAttribute("Power-on hours count            | Часы работы")},
                {0x0A, new SmartAttribute("Spinup retry count              | Повторных попыток раскрутить шпиндель после неудачи")},
                {0x0B, new SmartAttribute("Calibration retry count         | Повторов запросов калибровки")},
                {0x0C, new SmartAttribute("Power cycle count               | Полных циклов «включение-отключение»")},
                {0x0D, new SmartAttribute("Soft read error rate            | Ошибок чтения из-за программ")},
                {0xB8, new SmartAttribute("End-to-End error                | Ошибок при передачи данных через кэш диска")},
                {0xBE, new SmartAttribute("Airflow Temperature             | Температура воздуха")},
                {0xBF, new SmartAttribute("G-sense error rate              | Ошибок из-за ударных нагрузок")},
                {0xC0, new SmartAttribute("Power-off retract count         | Циклов отключения питания или аварийного втягивания")},
                {0xC1, new SmartAttribute("Load/Unload cycle count         | Циклов перемещения БМГ в парковочную зону/в рабочее положение")},
                {0xC2, new SmartAttribute("HDD temperature                 | Температура")},
                {0xC3, new SmartAttribute("Hardware ECC recovered          | Исправлено ошибок средствами ECC (аппаратно)")},
                {0xC4, new SmartAttribute("Reallocation count              | Операций переназначения секторов")},
                {0xC5, new SmartAttribute("Current pending sector count    | Секторов-кандидатов на переназначение в резервную область")},
                {0xC6, new SmartAttribute("Offline scan uncorrectable count| Неисправимые ошибки секторов")},
                {0xC7, new SmartAttribute("UDMA CRC error rate             | Ошибок UDMA CRC")},
                {0xC8, new SmartAttribute("Write error rate                | Ошибок при записи")},
                {0xC9, new SmartAttribute("Soft read error rate            | Ошибок при чтении по вине программного обеспечения")},
                {0xCA, new SmartAttribute("Data Address Mark errors        | Ошибок DAM (зависит от поставщика")},
                {0xCB, new SmartAttribute("Run out cancel                  | Ошибок ECC")},
                {0xCC, new SmartAttribute("Soft ECC correction             | Исправлено ошибок средствами ECC (программно)")},
                {0xCD, new SmartAttribute("Thermal asperity rate (TAR)     | Проблем из-за высокой температурой")},
                {0xCE, new SmartAttribute("Flying height                   | Высота поднятия головки над поверхностью диска")},
                {0xCF, new SmartAttribute("Spin high current               | Значение набора оборотов")},
                {0xD0, new SmartAttribute("Spin buzz                       | Циклов набора оборотов")},
                {0xD1, new SmartAttribute("Offline seek performance        | Производительность позиционирования магнитными головками во время оффлайн-операций")},
                {0xDC, new SmartAttribute("Disk shift                      | Дистанция смещения блока дисков относительно шпинделя")},
                {0xDD, new SmartAttribute("G-sense error rate              | Ошибок из-за ударных нагрузок")},
                {0xDE, new SmartAttribute("Loaded hours                    | Время БМГ между выгрузкой на раб.область диска из парк.области и загрузкой этого блока на парк. область.")},
                {0xDF, new SmartAttribute("Load/unload retry count         | Попыток выгрузок/загрузок БМГ на/из парк. область после неудачи")},
                {0xE0, new SmartAttribute("Load friction                   | Сила трения БМГ при его загрузке из парк.области")},
                {0xE1, new SmartAttribute("Load/Unload cycle count         | Циклов перемещения блока магнитных головок в парк.зону/рабоч.положение")},
                {0xE2, new SmartAttribute("Load-in time                    | Время загрузки магнитных головок из парк.области на раб.поверхность диска")},
                {0xE3, new SmartAttribute("Torque amplification count      | Попыток скомпенсировать вращающий момент")},
                {0xE4, new SmartAttribute("Power-off retract count         | Циклов отключения питания")},
                {0xE6, new SmartAttribute("GMR head amplitude              | Амплитуда «дрожания»")},
                {0xE7, new SmartAttribute("Temperature                     | Другая температура")},
                {0xF0, new SmartAttribute("Head flying hours               | Время позиционирования головки")},
                {0xFA, new SmartAttribute("Read error retry rate           | Ошибок при чтении сектора с поверхности диска")},
            };
    }

    public class SmartAttribute
    {
        // данные аттрибута
        public string Attribute { get; set; }   // название
        public int Current { get; set; }        // текущее (значение)
        public int Worst { get; set; }          // худшее (значение)
        public int Threshold { get; set; }      // порог (как в crystal disk info)
        public int Data { get; set; }           // данные
        public bool Status { get; set; }          // статус
        // конструкторы
        public SmartAttribute() { }             // безпараметрический конструктор
        public SmartAttribute(string attributeName) // конструктор с параметрами
        {
            this.Attribute = attributeName;
        }
        public bool HasData         // проверка данных
        {
            get
            {
                // если нет информации об атрибуте - отказываем показывать
                if (Current == 0 && Worst == 0 && Threshold == 0 && Data == 0) return false;    
                return true;
            }
        }
    }

    public class ClassReturnSmart
    {
        public Dictionary<int, ClassSmartInfo> GetSmartInfo()   // возврат собранных SMART-данных
        {
            
            try
            {
                // получаем список дисков на компьютере: HDD / CDROM / SSD                  
                var dicDrives = new Dictionary<int, ClassSmartInfo>();
                int indx = 0;
                using (ManagementObjectSearcher wdSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    // получаем информацию об модели и интерфейсе
                    foreach (ManagementObject drive in wdSearcher.Get())
                    {
                        var hdd = new ClassSmartInfo
                        {
                            Model = drive["Model"].ToString().Trim(),
                            Type = drive["InterfaceType"].ToString().Trim(),
                            DeviceID = drive["DeviceID"].ToString().Trim()
                        };
                        dicDrives.Add(indx, hdd);
                        indx++;
                    }
                }
                using (ManagementObjectSearcher pmsearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia"))
                {
                    // получаем серийный номер диска
                    indx = 0;
                    foreach (ManagementObject drive in pmsearcher.Get())
                    {
                        // поскольку все физ.диски будут возвращены, нужно будет выйти после извлечения данных
                        if (indx >= dicDrives.Count) break;
                        dicDrives[indx].Serial = drive["SerialNumber"] == null ? "None" : drive["SerialNumber"].ToString().Trim();
                        indx++;
                    }
                }
                // получаем wmi доступ к диску
                // powershell query запрос: (Get-WmiObject -Namespace root\wmi –Class MSStorageDriver_FailurePredictStatus)
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_DiskDrive")
                    {
                        // получаем статус диска
                        Scope = new ManagementScope(@"\root\wmi"),   
                        Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictStatus")
                    })
                {
                    indx = 0;
                    foreach (ManagementObject drive in searcher.Get())
                    {
                        dicDrives[indx].Status = (bool)drive.Properties["PredictFailure"].Value == false;
                        indx++;
                    }

                    // получаем флаги атрибутов SMART из VendorSpecific от MSStorageDriver_FailurePredictData
                    searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictData");
                    indx = 0;
                    foreach (ManagementObject data in searcher.Get())
                    {
                        byte[] bytes = (byte[])data.Properties["VendorSpecific"].Value;
                        for (int i = 0; i < 30; i++)
                        {
                            try
                            {
                                int id = bytes[i * 12 + 2];
                                int flags = bytes[i * 12 + 4]; // наименьший значительный статуса байта , +3 значительных байта, но т.к не юзается -> игнорируется.
                                bool failureImminent = (flags & 0x1) == 0x1;
                                int value = bytes[i * 12 + 5];
                                int worst = bytes[i * 12 + 6];
                                int vendordata = BitConverter.ToInt32(bytes, i * 12 + 7);
                                if (id == 0) continue;

                                var attr = dicDrives[indx].Attributes[id];
                                attr.Current = value;
                                attr.Worst = worst;
                                attr.Data = vendordata;
                                attr.Status = failureImminent == false;
                            }
                            catch
                            {
                                // ключ не существует в коллекции атрибутов (атрибут отсутствует в словаре атрибутов)
                            }
                        }
                        indx++;
                    }

                    // получаем пороговые значения для каждого атрибута
                    searcher.Query = new ObjectQuery("Select * from MSStorageDriver_FailurePredictThresholds");
                    indx = 0;
                    foreach (ManagementObject data in searcher.Get())
                    {
                        byte[] bytes = (byte[])data.Properties["VendorSpecific"].Value;
                        for (int i = 0; i < 30; ++i)
                        {
                            try
                            {

                                int id = bytes[i * 12 + 2];
                                int thresh = bytes[i * 12 + 3];
                                if (id == 0) continue;

                                var attr = dicDrives[indx].Attributes[id];
                                attr.Threshold = thresh;
                            }
                            catch
                            {
                                // ключ не существует в коллекции атрибутов(атрибут отсутствует в словаре атрибутов)
                            }
                        }
                        indx++;
                    }
                }   
                return dicDrives; // возврат словаря со всеми данными
            }
            catch (ManagementException err)
            {
                MessageBox.Show("Произошла ошибка при запросе данных WMI: " + err.Message + "\nПримечание: Если отказано в доступе, попробуйте запустить программу от имени администратора.");
                return null;
            }
        }
    }
}

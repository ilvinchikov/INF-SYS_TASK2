using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// источники:
// https://student-labs.kernel-dev.ru/InformationSystems/lab2.html
// https://www.cyberforum.ru/csharp-net/thread564926.html?ysclid=m25u17ft2l666813248
// https://powershell.one/wmi/root/cimv2
// https://yandex.ru/video/preview/16330731365587899759
// https://www.youtube.com/watch?v=MskGt9SCibo&ab_channel=EETechStuff

namespace task2_taskmngr
{
    public partial class FormMain_01 : Form
    {
        private Form form = null;   // дочерняя форма, которая будет подгружаться в панель
        public FormMain_01()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button4_Click(null, null); // при загрузке показываем форму с процессами
        }
        private void button1_Click(object sender, EventArgs e)  // кнопка DASHBOARD
        {
            LoadForms(new FormApparat_02(), 2);
            PanelShow.HorizontalScroll.Maximum = 0;
            PanelShow.AutoScroll = false;
            PanelShow.VerticalScroll.Visible = false;
            PanelShow.AutoScroll = true;
        }

        private void button2_Click(object sender, EventArgs e) // кнопка аппарат.часть
        {
            LoadForms(new FormApparat_02(), 1);
            PanelShow.HorizontalScroll.Maximum = 0;
            PanelShow.AutoScroll = false;
            PanelShow.VerticalScroll.Visible = false;
            PanelShow.AutoScroll = true;
        }

        

        private void button4_Click(object sender, EventArgs e) // кнопка процессы
        {
            LoadForms(new FormProcesses_04(), 0);
            PanelShow.HorizontalScroll.Maximum = 0;
            PanelShow.AutoScroll = false;
            PanelShow.VerticalScroll.Visible = false;
            PanelShow.AutoScroll = true;
        }
        private void button3_Click(object sender, EventArgs e)  // SMART
        {
            LoadForms(new FormSmartSystem_05(), 0);
            PanelShow.HorizontalScroll.Maximum = 0;
            PanelShow.AutoScroll = false;
            PanelShow.VerticalScroll.Visible = false;
            PanelShow.AutoScroll = true;
        }

        public void LoadForms(object TypeForm, byte mode)
        {
            if (PanelShow.Controls.Count > 0)
            {
                // очищаем предыдущие формы в панели
                PanelShow.Controls.Remove(form);
                // закрываем форму, освобождаем память
                form.Close();
                form = null;
            }
            form = TypeForm as Form;
            // свойства формы
            form.TopLevel = false;      // задаем свойство отсутствия верхнего уровня формы
            form.TopMost = true;        // задаем свойство переднего плана
            form.FormBorderStyle = FormBorderStyle.None;
            PanelShow.Controls.Add(form);   // добавляем форму в панель
            PanelShow_Resize(null, null);   // меняем размер дочерней формы под панель   
            if (mode > 0) form.Name = "mode="+mode; 
            form.Show();
        }

        private void PanelShow_Resize(object sender, EventArgs e)
        {
            // изменяем размер дочерней формы под панель
            foreach (Control item in PanelShow.Controls)
            {
                Form frm = (Form)item;
                frm.Width = PanelShow.Width;
                frm.Height = PanelShow.Height;
            }
        }

        
    }
}

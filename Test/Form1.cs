using System;
using System.IO;
using System.Data;
using System.Windows.Forms;
using System.Text;

namespace Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            FormClosed += new FormClosedEventHandler(CloseForm);
            dataGridView1.AllowUserToAddRows = false;
            dataGridView2.AllowUserToAddRows = false;
            //dataGridView2.CellValueChanged += DataGridView2_CellValueChanged;
            //dataGridView2.RowsRemoved += DataGridView2_RowsRemoved;
            openFileDialog1.AddExtension = true;
            saveFileDialog1.AddExtension = true;
            openFileDialog1.Filter = "База данных (*.db)|*.db";
            saveFileDialog1.Filter = "База данных (*.db)|*.db";
        }

        // Бессмысленная проверка на наличие в папке библиотеки System.Data.SQLite.dll
        // Если она не найдена, то выходим из приложения
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists("System.Data.SQLite.dll"))
            {
                string message = "Библиотека System.Data.SQLite.dll не найдена.";
                WriteLog(message);
                Application.Exit();
            }

            dbconnect.Message += (msg) => WriteLog(msg);
        }

        private void CloseForm(object sender, FormClosedEventArgs e)
        {
            // Если приложение закрывается и соединение было открыто, то закрываем соединение
            if (connect != null) connect.CloseConnect();
        }

        dbconnect connect;              // Объект класса dbconnect для соединения с БД
        DataSet DTables;
        DialogResult dialog;            // DialogResult для подтверждения удаления выделенной строки

        // Запись сообщений в Log файл и даты сообщения или ошибки
        private void WriteLog(string Message)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("Log.txt", true))
                {
                    sw.WriteLine(DateTime.Now);
                    sw.WriteLine(Message);
                }
            }
            catch (Exception exc) { MessageBox.Show(exc.Message); }
        }


        // Выбрать базу данных, загрузить и развернуть.
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            string path = openFileDialog1.FileName;

            // Если нажали отмена и путь пустой или выбрали файл другого типа, то сообщаем
            // об этом и выходим без продолжения.
            if (string.IsNullOrEmpty(path) || !Path.GetExtension(path).Equals(".db"))
            {
                string msg = "База данных не выбрана.";
                MessageBox.Show(msg);
                WriteLog(msg);
                return;
            }

            // Если все ОК, то создаем новое подключение и открываем соединение.
            connect = new dbconnect(path);
            DTables = new DataSet();
            connect.GetDataSet(DTables, "SELECT * FROM Documents; SELECT * FROM Positions");

            DTables.Tables[0].TableName = "Documents";
            DTables.Tables[1].TableName = "Positions";

            dataGridView1.DataSource = DTables.Tables["Documents"];
            dataGridView2.DataSource = DTables.Tables["Positions"];

            if (dataGridView1.Columns.Contains("Amount")) dataGridView1.Columns["Amount"].ReadOnly = true;
            if (dataGridView2.Columns.Contains("ID")) dataGridView2.Columns["ID"].Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (connect == null || DTables == null) return;
            DataRow row = DTables.Tables["Positions"].NewRow();
            DTables.Tables["Positions"].Rows.Add(row);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            dialog = MessageBox.Show(string.Format("Удалить строки?"), "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.No) return;
            foreach (DataGridViewRow row in dataGridView2.SelectedRows) dataGridView2.Rows.Remove(row);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (connect == null || DTables == null) return;
            connect.UpdateTable(DTables, "Documents");
            connect.UpdateTable(DTables, "Positions");

            string query = "UPDATE DOCUMENTS SET AMOUNT = (SELECT SUM(AMOUNT) FROM POSITIONS WHERE POSITIONS.NUMBER = DOCUMENTS.NUMBER);";
            connect.ExecuteQuery(query);

            connect.GetTable(DTables.Tables["Documents"]);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (connect == null || DTables == null) return;
            DataRow row = DTables.Tables["Documents"].NewRow();
            row["Date"] = DateTime.Now.ToShortDateString();
            row["Amount"] = 0;
            row["Number"] = 0;

            DTables.Tables["Documents"].Rows.Add(row);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            dialog = MessageBox.Show(string.Format("Удалить строку?"), "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialog == DialogResult.No) return;
            foreach (DataGridViewRow row in dataGridView1.SelectedRows) dataGridView1.Rows.Remove(row);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (connect == null || DTables == null) return;
            connect.UpdateTable(DTables, "Documents");
        }

        // Создать новуб базу данных.
        private void button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            string path = saveFileDialog1.FileName;
            if (string.IsNullOrEmpty(path))
            {
                // Если нажали отмену и путь оказался пустым, то сообщаем об этом и не продолжаем.
                string msg = "Путь для сохранения базы данных не был выбран.";
                WriteLog(msg);
                return;
            }
            dbconnect.CreateDataBase(path);
        }
        private void button9_Click(object sender, EventArgs e)
        {
            char del = (char)822;
            if (textBox1.Text == "" || textBox1.Text.IndexOf(del) > -1) return;
            StringBuilder sb = new StringBuilder(del);
            foreach (char c in textBox1.Text) { sb.Append(c); sb.Append(del); }
            textBox1.Text = sb.ToString();
        }
    }
}
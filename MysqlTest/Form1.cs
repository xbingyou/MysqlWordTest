using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MysqlTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //要插入列的类型
            var col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.DataPropertyName = "id";
            col.Name = "id";
            col.HeaderText = "显示编号";
            dgvDataTotal.Columns.Insert(0, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.DataPropertyName = "size";
            col.Name = "size";
            col.HeaderText = "显示名称";
            dgvDataTotal.Columns.Insert(1, col);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataSet objDs = MySqlDBManager.GetInstance().ExcuteQuerySQL("select * from mobile","mobile");
            if(objDs != null && objDs.Tables.Count>0)
            {
                DataRow objDr;
                List<mobile> lst = new List<mobile>();
                mobile mobile;
                for (int i = 0; i < objDs.Tables[0].Rows.Count; i++)
                {
                    objDr = objDs.Tables[0].Rows[i];
                    mobile = new mobile();
                    mobile.id = objDr["id"].ToString();
                    mobile.size = objDr["size"].ToString();
                    lst.Add(mobile);
                }
                dgvDataTotal.AutoGenerateColumns = false;
                dgvDataTotal.DataSource = lst;
            }    
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int nCount = MySqlDBManager.GetInstance().ExcuteSQL("insert into mobile (id,size) values(5,889)");
            this.label1.Text = nCount.ToString();
            MessageBox.Show(nCount.ToString());

        }
    }
}

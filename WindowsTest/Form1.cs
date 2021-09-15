using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsTest
{
    public partial class Form1 : Form
    {
        List<ReportDetail> lstData;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ReportDetail.CreateReportDBTable();
            InitDgv();
        }
        private void InitDgv()
        {
            //dgvDataTotal.AutoGenerateColumns = false;
            dgvDataTotal.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;
            dgvDataTotal.DefaultCellStyle.Font = new Font("微软雅黑", 15);
            dgvDataTotal.DefaultCellStyle.SelectionBackColor = Color.DarkOrange;
            dgvDataTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDataTotal.RowTemplate.Height = 40;
            dgvDataTotal.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;


            DataGridViewColumn objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            //objColumn.Width = 180;
            objColumn.DataPropertyName = "TestType";
            objColumn.HeaderText = "检测类型";
            dgvDataTotal.Columns.Insert(0, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            //objColumn.Width = 180;
            objColumn.DataPropertyName = "TestCode";
            objColumn.HeaderText = "实验编号";
            dgvDataTotal.Columns.Insert(1, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            //objColumn.Width = 180;
            objColumn.DataPropertyName = "PetName";
            objColumn.HeaderText = "宠物昵称";
            dgvDataTotal.Columns.Insert(2, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            //objColumn.Width = 180;
            objColumn.DataPropertyName = "PetType";
            objColumn.HeaderText = "宠物种类";
            dgvDataTotal.Columns.Insert(3, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            //objColumn.Width = 180;
            objColumn.DataPropertyName = "PetGender";
            objColumn.HeaderText = "宠物性别";
            dgvDataTotal.Columns.Insert(4, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            //objColumn.Width = 180;
            objColumn.DataPropertyName = "PetAge";
            objColumn.HeaderText = "宠物年龄";
            dgvDataTotal.Columns.Insert(5, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            //objColumn.Width = 180;
            objColumn.DataPropertyName = "OwnerName";
            objColumn.HeaderText = "宠主姓名";
            dgvDataTotal.Columns.Insert(6, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            //objColumn.Width = 180;
            objColumn.DataPropertyName = "OwnerContact";
            objColumn.HeaderText = "宠主电话";
            dgvDataTotal.Columns.Insert(7, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            objColumn.Width = 120;
            objColumn.DataPropertyName = "TestDoctor";
            objColumn.HeaderText = "检验医师";
            dgvDataTotal.Columns.Insert(8, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            objColumn.Width = 120;
            objColumn.DataPropertyName = "MainDoctor";
            objColumn.HeaderText = "主治医师";
            dgvDataTotal.Columns.Insert(9, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            objColumn.Width = 180;
            objColumn.DataPropertyName = "HospitalName";
            objColumn.HeaderText = "医院名称与地址";
            dgvDataTotal.Columns.Insert(10, objColumn);

            objColumn = new DataGridViewColumn();
            objColumn.CellTemplate = new DataGridViewTextBoxCell();
            objColumn.DataPropertyName = "ReportId";
            objColumn.Name = "ReportId";
            dgvDataTotal.Columns.Insert(11, objColumn);
            dgvDataTotal.Columns[11].Visible = false;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            MySqlDBManager.GetInstance().CloseConn();
        }
        //查询
        private void button1_Click(object sender, EventArgs e)
        {
            dgvDataTotal.AutoGenerateColumns = false;
            lstData = ReportDetail.GetReportLst();
            paging1.InitValue();
            DgvDataBind();
            paging1.BindEvent();
        }
        ReportDetail objReport = new ReportDetail();

        //添加
        private void button2_Click(object sender, EventArgs e)
        {
            objReport.ReportTitle = "猫免疫系统";
            objReport.PetName = "小于";
            objReport.PetType = "猫";
            objReport.PetGender = "雌";
            objReport.PetAge = "一年六";
            objReport.OwnerName = "小歪";
            objReport.OwnerContact = "0396-85868885";
            objReport.TestCode = "Q358655";
            objReport.TestDoctor = "凌肖";
            objReport.MainDoctor = "凌宇";
            objReport.HospitalName = "凌天医院";
            objReport.TestTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            objReport.ReportTime = DateTime.Now.ToString("yyyyMMdd");
            objReport.TestType = "SNP检测";
            objReport.Insert();
        }
        //删除
        private void button3_Click(object sender, EventArgs e)
        {
            this.label1.Text = dateTimePicker1.Text + "\r\n" + dateTimePicker2.Text +
                "\r\n" + dateTimePicker1.Value + "\r\n" + dateTimePicker2.Value;
            if (int.TryParse(dgvDataTotal.CurrentRow.Cells["ReportId"].Value.ToString(), out int nId))
            {
                objReport.ReportId = nId;
                objReport.Delete();
            }
        }
        private void paging1_EventPaging(EventArgs e)
        {
            DgvDataBind();
        }
        private void DgvDataBind()
        {
            if (lstData != null || lstData.Count > 0)
            {
                paging1.TotalCount = lstData.Count;
                List<ReportDetail> lstClone = new List<ReportDetail>();
                int nStartPos = 0; //当前页面开始记录行
                int nEndPos = 0; //当前页面结束记录行
                if (paging1.CurrentPage == paging1.PageCount)
                    nEndPos = paging1.TotalCount;
                else
                    nEndPos = paging1.PageSize * paging1.CurrentPage;
                nStartPos = paging1.CurrentRow;
                //从元数据源复制记录行
                for (int i = nStartPos; i < nEndPos; i++)
                {
                    if (i < lstData.Count)
                    {
                        lstClone.Add(lstData[i]);
                        paging1.CurrentRow++;
                    }
                }                
                dgvDataTotal.DataSource = lstClone;
            }
            else
            {
                return;
            }
        }

        private void dgvDataTotal_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                //this.label1.Text = dgvDataTotal.CurrentRow.Cells["ReportId"].Value.ToString();

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //WordEx.MainWord();
            WordHelper.CreatWordByReport();
        }
        void A()
        {

            //要插入列的类型
            var col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.DataPropertyName = "id";
            col.Name = "colName";
            col.HeaderText = "显示编号";
            dgvDataTotal.Columns.Insert(0, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.DataPropertyName = "size";
            col.Name = "coName";
            col.HeaderText = "显示名称";
            dgvDataTotal.Columns.Insert(1, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.Name = "cName";
            col.HeaderText = "显示内容";
            dgvDataTotal.Columns.Insert(2, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.Name = "Name";
            col.HeaderText = "显示日期";
            dgvDataTotal.Columns.Insert(3, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.Name = "ame";
            col.HeaderText = "显示送货地址";
            dgvDataTotal.Columns.Insert(4, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.Name = "me";
            col.HeaderText = "显示详情";
            dgvDataTotal.Columns.Insert(5, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.Name = "aae";
            col.HeaderText = "显示内容详情页面";
            dgvDataTotal.Columns.Insert(6, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.Name = "ace";
            col.HeaderText = "显示备注";
            dgvDataTotal.Columns.Insert(7, col);

            col = new DataGridViewColumn();
            col.CellTemplate = new DataGridViewTextBoxCell();
            col.Name = "aced";
            col.HeaderText = "显示上虞";
            dgvDataTotal.Columns.Insert(8, col);

            for (int i = 0; i < 100; i++)    //组织指定数据行
            {
                DataGridViewRow dr = new DataGridViewRow();
                dgvDataTotal.Rows.Add(dr);           //增加行
                for (int j = 0; j < dgvDataTotal.ColumnCount - 1; j++) //部分列
                {
                    if (j == 3)                //特殊数据列。此处可据列的ValueType属性来处理不同类型数据及格式
                        dgvDataTotal.Rows[i].Cells[j].Value = true;
                    else
                        dgvDataTotal.Rows[i].Cells[j].Value = "Cell" + i + "_" + j;
                }
                dgvDataTotal.Rows[i].Cells[dgvDataTotal.ColumnCount - 1].Value = dgvDataTotal.ColumnCount - 1; //最后一列
            }
        }
    }
}

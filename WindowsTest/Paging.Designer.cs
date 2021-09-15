
namespace WindowsTest
{
    partial class Paging
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lbTotalCount = new System.Windows.Forms.Label();
            this.lbPage = new System.Windows.Forms.Label();
            this.btnLastPage = new System.Windows.Forms.PictureBox();
            this.btnNextPage = new System.Windows.Forms.PictureBox();
            this.btnPreviousPage = new System.Windows.Forms.PictureBox();
            this.btnHomePage = new System.Windows.Forms.PictureBox();
            this.cbPerPage = new System.Windows.Forms.ComboBox();
            this.lbPerPage = new System.Windows.Forms.Label();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnLastPage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnNextPage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPreviousPage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHomePage)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.lbTotalCount);
            this.flowLayoutPanel1.Controls.Add(this.lbPage);
            this.flowLayoutPanel1.Controls.Add(this.btnLastPage);
            this.flowLayoutPanel1.Controls.Add(this.btnNextPage);
            this.flowLayoutPanel1.Controls.Add(this.btnPreviousPage);
            this.flowLayoutPanel1.Controls.Add(this.btnHomePage);
            this.flowLayoutPanel1.Controls.Add(this.cbPerPage);
            this.flowLayoutPanel1.Controls.Add(this.lbPerPage);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(658, 61);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // lbTotalCount
            // 
            this.lbTotalCount.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbTotalCount.AutoSize = true;
            this.lbTotalCount.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbTotalCount.Location = new System.Drawing.Point(561, 6);
            this.lbTotalCount.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lbTotalCount.Name = "lbTotalCount";
            this.lbTotalCount.Size = new System.Drawing.Size(94, 25);
            this.lbTotalCount.TabIndex = 5;
            this.lbTotalCount.Text = "总计2120";
            this.lbTotalCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbPage
            // 
            this.lbPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbPage.AutoSize = true;
            this.lbPage.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbPage.Location = new System.Drawing.Point(467, 6);
            this.lbPage.Name = "lbPage";
            this.lbPage.Size = new System.Drawing.Size(91, 25);
            this.lbPage.TabIndex = 4;
            this.lbPage.Text = "第1/23页";
            // 
            // btnLastPage
            // 
            this.btnLastPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnLastPage.BackgroundImage = global::WindowsTest.Properties.Resources.Last_Page;
            this.btnLastPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLastPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLastPage.Location = new System.Drawing.Point(428, 4);
            this.btnLastPage.Name = "btnLastPage";
            this.btnLastPage.Size = new System.Drawing.Size(33, 30);
            this.btnLastPage.TabIndex = 3;
            this.btnLastPage.TabStop = false;
            this.btnLastPage.Click += new System.EventHandler(this.btnLastPage_Click);
            this.btnLastPage.MouseEnter += new System.EventHandler(this.btnHomePage_MouseEnter);
            this.btnLastPage.MouseLeave += new System.EventHandler(this.btnHomePage_MouseLeave);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnNextPage.BackgroundImage = global::WindowsTest.Properties.Resources.Next_Page;
            this.btnNextPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnNextPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnNextPage.Location = new System.Drawing.Point(389, 4);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(33, 30);
            this.btnNextPage.TabIndex = 2;
            this.btnNextPage.TabStop = false;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            this.btnNextPage.MouseEnter += new System.EventHandler(this.btnHomePage_MouseEnter);
            this.btnNextPage.MouseLeave += new System.EventHandler(this.btnHomePage_MouseLeave);
            // 
            // btnPreviousPage
            // 
            this.btnPreviousPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnPreviousPage.BackgroundImage = global::WindowsTest.Properties.Resources.Previous_Page;
            this.btnPreviousPage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPreviousPage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPreviousPage.Location = new System.Drawing.Point(350, 4);
            this.btnPreviousPage.Name = "btnPreviousPage";
            this.btnPreviousPage.Size = new System.Drawing.Size(33, 30);
            this.btnPreviousPage.TabIndex = 1;
            this.btnPreviousPage.TabStop = false;
            this.btnPreviousPage.Click += new System.EventHandler(this.btnPreviousPage_Click);
            this.btnPreviousPage.MouseEnter += new System.EventHandler(this.btnHomePage_MouseEnter);
            this.btnPreviousPage.MouseLeave += new System.EventHandler(this.btnHomePage_MouseLeave);
            // 
            // btnHomePage
            // 
            this.btnHomePage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnHomePage.BackgroundImage = global::WindowsTest.Properties.Resources.Home_Page;
            this.btnHomePage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnHomePage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnHomePage.Location = new System.Drawing.Point(311, 4);
            this.btnHomePage.Name = "btnHomePage";
            this.btnHomePage.Size = new System.Drawing.Size(33, 30);
            this.btnHomePage.TabIndex = 0;
            this.btnHomePage.TabStop = false;
            this.btnHomePage.Click += new System.EventHandler(this.btnHomePage_Click);
            this.btnHomePage.MouseEnter += new System.EventHandler(this.btnHomePage_MouseEnter);
            this.btnHomePage.MouseLeave += new System.EventHandler(this.btnHomePage_MouseLeave);
            // 
            // cbPerPage
            // 
            this.cbPerPage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPerPage.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbPerPage.FormattingEnabled = true;
            this.cbPerPage.Items.AddRange(new object[] {
            "20",
            "40",
            "60",
            "80"});
            this.cbPerPage.Location = new System.Drawing.Point(242, 3);
            this.cbPerPage.Margin = new System.Windows.Forms.Padding(0, 3, 6, 3);
            this.cbPerPage.Name = "cbPerPage";
            this.cbPerPage.Size = new System.Drawing.Size(60, 32);
            this.cbPerPage.TabIndex = 7;
            this.cbPerPage.SelectedIndexChanged += new System.EventHandler(this.cbPerPage_SelectedIndexChanged);
            // 
            // lbPerPage
            // 
            this.lbPerPage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lbPerPage.AutoSize = true;
            this.lbPerPage.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbPerPage.Location = new System.Drawing.Point(189, 6);
            this.lbPerPage.Name = "lbPerPage";
            this.lbPerPage.Size = new System.Drawing.Size(50, 25);
            this.lbPerPage.TabIndex = 6;
            this.lbPerPage.Text = "每页";
            // 
            // Paging
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "Paging";
            this.Size = new System.Drawing.Size(658, 61);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnLastPage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnNextPage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPreviousPage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHomePage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.PictureBox btnHomePage;
        private System.Windows.Forms.PictureBox btnPreviousPage;
        private System.Windows.Forms.PictureBox btnNextPage;
        private System.Windows.Forms.PictureBox btnLastPage;
        private System.Windows.Forms.Label lbPage;
        private System.Windows.Forms.Label lbTotalCount;
        private System.Windows.Forms.Label lbPerPage;
        private System.Windows.Forms.ComboBox cbPerPage;
    }
}

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
    public delegate void EventPagingHandler(EventArgs e);
    public partial class Paging : UserControl
    {
        public Paging()
        {
            InitializeComponent();
        }
        public event EventPagingHandler EventPaging;

        #region 公开属性
        private int _currentRow = 0;
        /// <summary>
        /// 翻页当前行(从0开始)
        /// </summary>
        public int CurrentRow
        {
            get
            {
                return _currentRow;
            }
            set
            {
                _currentRow = value;
            }
        }

        private int _pageSize = 20;
        /// <summary>
        /// 每页显示记录数(默认20)
        /// </summary>
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value > 0)
                {
                    _pageSize = value;
                }
                else
                {
                    _pageSize = 20;
                }
                this.cbPerPage.Text = _pageSize.ToString();
            }
        }
        private int _currentPage = 1;
        /// <summary>
        /// 当前页
        /// </summary>
        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                if (value > 0)
                {
                    _currentPage = value;
                }
                else
                {
                    _currentPage = 1;
                }
            }
        }

        private int _totalCount = 0;
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount
        {
            get
            {
                return _totalCount;
            }
            set
            {
                if (value >= 0)
                {
                    _totalCount = value;
                }
                else
                {
                    _totalCount = 0;
                }
                this.lbTotalCount.Text = "总计" + this._totalCount.ToString();
                CalculatePageCount();
            }
        }
        private int _pageCount = 0;
        /// <summary>
        /// 页数
        /// </summary>
        public int PageCount
        {
            get
            {
                return _pageCount;
            }
            set
            {
                if (value >= 0)
                {
                    _pageCount = value;
                }
                else
                {
                    _pageCount = 0;
                }
                this.lbPage.Text = string.Format("第{0}/{1}页", this.CurrentPage, this.PageCount);
            }
        }
        #endregion

        /// <summary>
        /// 计算页数
        /// </summary>
        private void CalculatePageCount()
        {
            if (this.TotalCount > 0)
            {
                this.PageCount = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(this.TotalCount) / Convert.ToDouble(this.PageSize)));
            }
            else
            {
                this.PageCount = 0;
            }
        }
        /// <summary>
        /// 获取显示记录区间（格式如：1-50）
        /// </summary>
        /// <returns></returns>
        private string GetRecordRegion()
        {
            if (this.PageCount == 1) //只有一页
            {
                return "1-" + this.TotalCount.ToString();
            }
            else  //有多页
            {
                if (this.CurrentPage == 1) //当前显示为第一页
                {
                    return "1-" + this.PageSize;
                }
                else if (this.CurrentPage == this.PageCount) //当前显示为最后一页
                {
                    return ((this.CurrentPage - 1) * this.PageSize + 1) + "-" + this.TotalCount;
                }
                else //中间页
                {
                    return ((this.CurrentPage - 1) * this.PageSize + 1) + "-" + this.CurrentPage * this.PageSize;
                }
            }
        }
        /// <summary>
        /// 初始化数据绑定
        /// </summary>
        public void InitValue()
        {
            PageSize = 20;
            CurrentPage = 1;
            CurrentRow = 0;
        }
        /// <summary>
        /// 数据绑定
        /// </summary>
        public void Bind()
        {
            if (this.EventPaging != null)
                this.EventPaging(new EventArgs());
            BindEvent();
        }
        public void BindEvent()
        {
            if (this.CurrentPage > this.PageCount)
                this.CurrentPage = this.PageCount;
            this.lbTotalCount.Text = "总计" + this.TotalCount;
            this.lbPage.Text = string.Format("第{0}/{1}页", this.CurrentPage, this.PageCount);

            if (this.CurrentPage == 1)
            {
                this.btnHomePage.Enabled = false;
                this.btnPreviousPage.Enabled = false;
            }
            else
            {
                this.btnHomePage.Enabled = true;
                this.btnPreviousPage.Enabled = true;
            }
            if (this.CurrentPage == this.PageCount)
            {
                this.btnNextPage.Enabled = false;
                this.btnLastPage.Enabled = false;
            }
            else
            {
                this.btnNextPage.Enabled = true;
                this.btnLastPage.Enabled = true;
            }
            if (this.TotalCount == 0)
            {
                this.btnHomePage.Enabled = false;
                this.btnPreviousPage.Enabled = false;
                this.btnNextPage.Enabled = false;
                this.btnLastPage.Enabled = false;
            }
        }

        private void btnHomePage_Click(object sender, EventArgs e)
        {
            this.CurrentPage = 1;
            this.CurrentRow = 0;
            this.Bind();
        }

        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            this.CurrentPage -= 1;
            CurrentRow = PageSize * (CurrentPage - 1);
            this.Bind();
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            this.CurrentPage += 1;
            CurrentRow = PageSize * (CurrentPage - 1);
            this.Bind();
        }

        private void btnLastPage_Click(object sender, EventArgs e)
        {
            CurrentPage = this.PageCount;
            CurrentRow = PageSize * (PageCount - 1);
            this.Bind();
        }

        private void cbPerPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            int.TryParse(cbPerPage.Text, out int nPageSize);
            PageSize = nPageSize;
            CurrentPage = 1;
            CurrentRow = 0;
            this.Bind();
        }

        private void btnHomePage_MouseEnter(object sender, EventArgs e)
        {
            PictureBox btn = sender as PictureBox;
            switch (btn.Name)
            {
                case "btnHomePage":
                    btn.BackgroundImage = WindowsTest.Properties.Resources.Home_Page_Click;
                    break;
                case "btnPreviousPage":
                    btn.BackgroundImage = WindowsTest.Properties.Resources.Previous_Page_Click;
                    break;
                case "btnNextPage":
                    btn.BackgroundImage = WindowsTest.Properties.Resources.Next_Page_Click;
                    break;
                case "btnLastPage":
                    btn.BackgroundImage = WindowsTest.Properties.Resources.Last_Page_Click;
                    break;
                default:
                    break;
            }
        }

        private void btnHomePage_MouseLeave(object sender, EventArgs e)
        {
            PictureBox btn = sender as PictureBox;
            switch (btn.Name)
            {
                case "btnHomePage":
                    btn.BackgroundImage = WindowsTest.Properties.Resources.Home_Page;
                    break;
                case "btnPreviousPage":
                    btn.BackgroundImage = WindowsTest.Properties.Resources.Previous_Page;
                    break;
                case "btnNextPage":
                    btn.BackgroundImage = WindowsTest.Properties.Resources.Next_Page;
                    break;
                case "btnLastPage":
                    btn.BackgroundImage = WindowsTest.Properties.Resources.Last_Page;
                    break;
                default:
                    break;
            }
        }
    }
}

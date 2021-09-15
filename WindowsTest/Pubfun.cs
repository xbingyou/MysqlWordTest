using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WindowsTest
{
    public class Pubfun
    {
        public static bool CheckDBTableExist(string strTableName)
        {
            string strSQL = string.Format("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE UPPER(TABLE_NAME)='{0}'", strTableName.ToUpper());
            DataSet objDs = MySqlDBManager.GetInstance().ExcuteQuerySQL(strSQL, strTableName);
            if (objDs == null || objDs.Tables.Count <= 0)
            {
                return false;
            }
            int nRows = objDs.Tables[0].Rows.Count;
            if (nRows > 0)
            {
                // 数据表已经存在
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

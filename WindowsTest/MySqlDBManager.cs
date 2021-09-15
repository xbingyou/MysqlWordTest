using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WindowsTest
{
    public class MySqlDBManager
    {
        private MySqlConnection myConn = null;
        private MySqlCommand cmd = null;
        private static MySqlDBManager myDBManager = null;
        public static string ConnectionString = "Database='sys';Data Source='localhost';User Id='root';Password='root';charset='utf8';pooling=true";

        public static MySqlDBManager GetInstance()
        {
            if (myDBManager == null)
                myDBManager = new MySqlDBManager();
            return myDBManager;
        }
        private MySqlDBManager()
        {
            GetConnection();
        }
        /// <summary>
        /// 建立数据库连接
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            lock (this)
            {
                myConn = new MySqlConnection(ConnectionString);
                myConn.Open();
            }
            return myConn;
        }
        /// <summary>
        /// 关闭数据库
        /// </summary>
        public void CloseConn()
        {
            try
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                if (myConn != null && myConn.State != ConnectionState.Closed)
                {
                    myConn.Close();
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (myConn != null)
                {
                    myConn.ConnectionString = "";
                    myConn.Dispose();
                    myConn = null;
                }
            }
        }

        /// <summary>
        /// 执行非查询性SQL
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="blInsert">是否是插入命令</param>
        /// <returns>插入时返回自增ID值</returns>
        public int ExcuteSQL(string strSQL, bool blInsert = false, MySqlParameter[] cmdParms = null)
        {
            int nCount = -1;
            try
            {
                if (myConn == null || myConn.State != ConnectionState.Open)
                {
                    GetConnection();
                }
                if (cmd == null)
                {
                    cmd = myConn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                }
                cmd.CommandText = strSQL;
                if (cmdParms != null)
                {
                    foreach (MySqlParameter parm in cmdParms)
                        cmd.Parameters.Add(parm);
                }
                nCount = cmd.ExecuteNonQuery();
                if (blInsert)
                {
                    //插入时，返回自增ID值
                    if (nCount >= 0)
                        nCount = int.Parse(cmd.LastInsertedId.ToString());
                }
            }
            catch (Exception)
            {
                nCount = -2;
                CloseConn();
            }
            return nCount;
        }

        /// <summary>
        /// 执行查询性SQL
        /// </summary>
        public DataSet ExcuteQuerySQL(String strSQL, String strTableName, MySqlParameter[] cmdParms = null)
        {
            DataSet dsObj = new DataSet();
            try
            {
                if (myConn == null || myConn.State != ConnectionState.Open)
                {
                    GetConnection();
                }
                if (cmd == null)
                {
                    cmd = myConn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                }
                cmd.CommandText = strSQL;
                if (cmdParms != null)
                {
                    foreach (MySqlParameter parm in cmdParms)
                        cmd.Parameters.Add(parm);
                }
                MySqlDataAdapter MyDataAdapter = new MySqlDataAdapter();
                MyDataAdapter.SelectCommand = cmd;// 设置OleDbDataAdapte对象的SelectCommand属性
                MyDataAdapter.Fill(dsObj, strTableName);
                MyDataAdapter.Dispose();
            }
            catch (Exception)
            {
                CloseConn();
            }
            return dsObj;
        }

    }
}

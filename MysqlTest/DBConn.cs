using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MysqlTest
{
    public class DBConn
    {
        public static MySqlConnection conn;//定义一个MySqlConnection类型的公共变量conn，用于判断数据库是否连接成功
        public static string sqlcon = "Database='sys';Data Source='localhost';User Id='root';Password='root';charset='utf8';pooling=true";

        #region 建立数据库连接
        /// <summary>
        /// 建立数据库连接
        /// </summary>
        /// <returns></returns>
        public static MySqlConnection GetConn()
        {
            conn = new MySqlConnection(sqlcon);
            conn.Open();
            return conn;
        }
        #endregion

        #region 关闭数据库
        /// <summary>
        /// 关闭数据库
        /// </summary>
        public void CloseConn()
        {
            if (conn.State == ConnectionState.Open)   //判断是否打开与数据库的连接
            {
                conn.Close();   //关闭数据库的连接
                conn.Dispose();   //释放My_con变量的所有空间
            }
        }
        #endregion

        #region  读取指定表中的信息
        /// <summary>
        /// 读取指定表中的信息.
        /// </summary>
        /// <param name="sqlStr">SQL语句</param>
        /// <returns>返回bool型</returns>
        public MySqlDataReader GetComand(string sqlStr)
        {
            GetConn();   //打开与数据库的连接
            MySqlCommand comand = conn.CreateCommand();//创建一个SqlCommand对象，用于执行SQL语句
            comand.CommandText = sqlStr;    //获取指定的SQL语句
            MySqlDataReader myRead = comand.ExecuteReader(); //执行SQL语名句，生成一个MySqlDataReader对象
            return myRead;
        }
        #endregion

        #region 执行MySqlCommand命令
        /// <summary>
        /// 执行SqlCommand
        /// </summary>
        /// <param name="sqlStr">SQL语句</param>
        public void GetSqlCom(string sqlStr)
        {
            GetConn();   //打开与数据库的连接
            MySqlCommand sqlCom = new MySqlCommand(sqlStr, conn); //创建一个MySqlCommand对象，用于执行SQL语句
            sqlCom.ExecuteNonQuery();   //执行SQL语句
            sqlCom.Dispose();   //释放所有空间
            CloseConn();    //调用con_close()方法，关闭与数据库的连接
        }
        #endregion

        #region  创建DataSet对象
        /// <summary>
        /// 创建一个DataSet对象
        /// </summary>
        /// <param name="sqlStr">SQL语句</param>
        /// <param name="tableName">表名</param>
        /// <returns>返回DataSet对象</returns>
        public DataSet GetDataSet(string sqlStr, string tableName)
        {
            GetConn();   //打开与数据库的连接
            MySqlDataAdapter SQLda = new MySqlDataAdapter(sqlStr, conn);  //创建一个SqlDataAdapter对象，并获取指定数据表的信息
            DataSet myDataSet = new DataSet(); //创建DataSet对象
            SQLda.Fill(myDataSet, tableName);  //通过MySqlDataAdapter对象的Fill()方法，将数据表信息添加到DataSet对象中
            CloseConn();    //关闭数据库的连接
            return myDataSet;  //返回DataSet对象的信息

        }
        #endregion
    }
}

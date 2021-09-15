
using System;
using System.Collections;
using System.Text;
using System.Data;
using System.Threading;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace ZjbBase.DBManager
{
    /// <summary>
    /// MySqlDBManager 的摘要说明。
    /// </summary>
    public class MySqlDBManagerd : BaseDBManager
    {
        // 数据库连接池
        IList _lstUseConnection = new List<MySqlConnection>();
        IList _lstWaitConnection = new List<MySqlConnection>();
        IList<DateTime> _lstWaitConnectionTime = new List<DateTime>(); // 记录等待队列中的连接的最后使用时间（每隔5分钟左右重连一次）
        IList _lstTransConnection = new List<MySqlConnection>();

        Hashtable _hasConnUseCount = new Hashtable(); // 连接使用次数
        Hashtable _hasConnCommand = new Hashtable(); // Connect对应的Command

        private static int _nCommandTimeout = 60; // 连接执行超时时间（秒） add by zhangjb 20151202 for i单车执行超时

        /// <summary>
        /// 等待队列重建（清理超时连接，每隔5分钟）
        /// </summary>
        private bool _bWaitConnectionRebulid = false;
        private System.Timers.Timer timerWaitConnectionRebulid = new System.Timers.Timer(300000);
        private void StartWaitConnectionRebulid()
        {
            if (_bWaitConnectionRebulid == false)
            {
                _bWaitConnectionRebulid = true;
                LogInfo.OutputInfoLog("MySqlDB#" + this._nPos.ToString() + "等待队列定时清理程序启动！");
                timerWaitConnectionRebulid.Elapsed += new System.Timers.ElapsedEventHandler(WaitConnectionRebulid);
                timerWaitConnectionRebulid.AutoReset = false;//设置是执行一次（false）还是一直执行(true)； 
                timerWaitConnectionRebulid.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件； 
            }
        }

        private void WaitConnectionRebulid(object source, System.Timers.ElapsedEventArgs e)
        {
            timerWaitConnectionRebulid.Enabled = false;

            while (_lstWaitConnection.Count > 0)
            {
                lock (this)
                {
                    int nSize = this._lstWaitConnectionTime.Count;
                    DateTime dtTime;
                    DateTime dtTimeNow = DateTime.Now;
                    MySqlConnection myConn;

                    LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：等待队列清理程序执行！等待队列连接数=" + _lstWaitConnection.Count.ToString());

                    try
                    {
                        for (int i = 0; i < nSize; i++)
                        {
                            dtTime = this._lstWaitConnectionTime[0].AddMinutes(5);
                            if (dtTime.CompareTo(dtTimeNow) < 0)
                            {
                                // 等待的连接已经超过5分钟没有使用
                                myConn = (MySqlConnection)this._lstWaitConnection[0];

                                this._lstWaitConnectionTime.RemoveAt(0);
                                this._lstWaitConnection.RemoveAt(0);
                                LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：等待队列连接超时，移出队列！");

                                if (myConn != null)
                                {
                                    IDbCommand cmd = this._hasConnCommand[myConn] as IDbCommand;
                                    if (cmd != null)
                                    {
                                        cmd.Dispose();
                                    }
                                    this._hasConnCommand.Remove(myConn);

                                    if (myConn != null)
                                    {
                                        _hasConnUseCount.Remove(myConn);
                                    }

                                    if (myConn.State != ConnectionState.Closed)
                                    {
                                        myConn.Close();
                                        LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：等待队列连接超时，成功关闭！");
                                    }

                                    myConn.ConnectionString = "";
                                    myConn.Dispose();
                                    myConn = null;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex1)
                    {
                        LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex1.ToString());
                    }
                }
                Thread.Sleep(300000); // 等待5分钟
            }
            timerWaitConnectionRebulid.Enabled = true;
        }

        /// <summary>
        /// 设置数据库连接的字符集
        /// </summary>
        /// <param name="conn"></param>
        private void SetDBCharSet(MySqlConnection conn, BaseTransaction objTrans)
        {
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                string strSQL = "SET NAMES " + BaseParamsManager.GetParamsInstance(this._nPos).CharSet;
                MySqlCommand cmd = new MySqlCommand(strSQL, conn);

                if (objTrans != null && objTrans.DBTransaction != null)
                {
                    cmd.Transaction = objTrans.DBTransaction as MySqlTransaction;
                }
                cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                LogInfo.GetInstance().WriteErrorLog(ex.ToString());
            }
        }

        /// <summary>
        /// 获取数据库DataSource
        /// </summary>
        /// <returns></returns>
        public override string GetDataSource(params bool[] bParamsIsTrans)
        {
            MySqlConnection conn = null;
            string strDataSource = "";
            try
            {
                conn = (MySqlConnection)GetConnection();
                //MySqlConnection conn = new MySqlConnection(BaseParamsManager.GetParamsInstance(this._nPos).ConnectionString);
                if (conn != null)
                {
                    strDataSource = conn.DataSource;
                }
            }
            catch (Exception ex)
            {
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString());
            }
            finally
            {
                CloseConnection(conn);
            }

            return strDataSource;
        }

        /// <summary>
        /// 获取数据库DataBase
        /// </summary>
        /// <returns></returns>
        public override string GetDataBase(params bool[] bParamsIsTrans)
        {
            MySqlConnection conn = null;
            string strDataBase = "";
            try
            {
                conn = (MySqlConnection)GetConnection();
                if (conn != null)
                {
                    strDataBase = conn.Database;
                }
            }
            catch (Exception ex)
            {
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString());
            }
            finally
            {
                CloseConnection(conn);
            }

            return strDataBase;
        }

        /// <summary>
        /// 得到用户名
        /// </summary>
        /// <param name="bParamsIsTrans"></param>
        /// <returns></returns>
        public override string GetDBUserID(params bool[] bParamsIsTrans)
        {
            MySqlConnectionStringBuilder s = new MySqlConnectionStringBuilder(BaseParamsManager.GetParamsInstance(this._nPos).ConnectionString);
            return s.UserID;
        }

        /// <summary>
        /// 得到用户密码
        /// </summary>
        /// <param name="bParamsIsTrans"></param>
        /// <returns></returns>
        public override string GetDBUserPwd(params bool[] bParamsIsTrans)
        {
            MySqlConnectionStringBuilder s = new MySqlConnectionStringBuilder(BaseParamsManager.GetParamsInstance(this._nPos).ConnectionString);
            return s.Password;
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <returns></returns>
        public override IDbConnection GetConnection(params bool[] bParamsIsTrans)
        {
            MySqlConnection myConn = null;
            try
            {
                // 事务标识
                bool bIsTrans = false;
                if (bParamsIsTrans.Length > 0)
                {
                    bIsTrans = bParamsIsTrans[0];
                }

                // 等待队列中有，直接从等待队列中获取
                lock (this)
                {
                    if (this._lstWaitConnection.Count > 0)
                    {
                        myConn = (MySqlConnection)this._lstWaitConnection[0];
                        if (this._lstWaitConnectionTime.Count > 0)
                        {
                            this._lstWaitConnectionTime.RemoveAt(0);
                        }
                        this._lstWaitConnection.RemoveAt(0);
                        //this._lstWaitConnection.Remove(myConn);
                        if (myConn != null && myConn.ConnectionString.Length <= 0)
                        {
                            myConn = null;
                        }
                        else if (myConn != null)
                        {
                            this._lstUseConnection.Add(myConn);
                            _hasConnUseCount[myConn] = 1;
                            if (bIsTrans) { this._lstTransConnection.Add(myConn); } // 事务连接
                        }
                    }
                }

                // 等待队列里已经没有可用的连接时，如果连接数小于规定最大连接数，则新建一个连接
                if (myConn == null)
                {
                    lock (this)
                    {
                        // 如果连接数小于规定最大连接数，则新建一个连接
                        if (this._lstUseConnection.Count + this._lstWaitConnection.Count < BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize)
                        {
                            myConn = new MySqlConnection(BaseParamsManager.GetParamsInstance(this._nPos).ConnectionString);
                            this._lstUseConnection.Add(myConn);
                            if (bIsTrans) { this._lstTransConnection.Add(myConn); } // 事务连接
                            _hasConnUseCount[myConn] = 1;
                            if (BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize > 0)
                            {
                                // 记录数据库连接数占用比例
                                BaseDBManager.g_hasDBConnectUsedPercent[this._nPos] = (this._lstWaitConnection.Count + this._lstUseConnection.Count) * 100.0 / BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize;
                            }
                            LogInfo.OutputDebugLog(BaseGlobalData.g_strAppPath + ":" + string.Format("MySqlDB#{0}数据库连接：如果连接数小于规定最大连接数，则新建一个连接。等待队列连接数={1}；正在使用连接数={2}；最大连接数={3}",
                                this._nPos.ToString(), this._lstWaitConnection.Count.ToString(), this._lstUseConnection.Count.ToString(), BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize.ToString()));
                        }
                    }
                }

                // 等待队列用完，新建连接数也用完，则等待可用连接（最大等待5分钟）
                int nWaitTotalCount = 0;
                while (myConn == null && nWaitTotalCount <= 600)
                {
                    if (nWaitTotalCount == 20)
                    {
                        //LogInfo.OutputInfoLog("数据库连接数不足，等待时间超过10秒！");
                        LogInfo.OutputErrorLog(string.Format(BaseGlobalData.g_strAppPath + ":MySQLDB#{0}数据库连接：连接数满，且没有已用连接可用，等待时间超过10秒。等待队列连接数={1}；正在使用连接数={2}；最大连接数={3}",
                                this._nPos.ToString(), this._lstWaitConnection.Count.ToString(), this._lstUseConnection.Count.ToString(), BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize.ToString()));
                    }
                    else if (nWaitTotalCount >= 600)
                    {
                        //LogInfo.OutputInfoLog("数据库连接数不足，等待时间超过5分钟！");
                        LogInfo.OutputErrorLog(string.Format(BaseGlobalData.g_strAppPath + ":MySQLDB#{0}数据库连接：连接数满，且没有已用连接可用，等待时间超过5分钟。等待队列连接数={1}；正在使用连接数={2}；最大连接数={3}",
                                this._nPos.ToString(), this._lstWaitConnection.Count.ToString(), this._lstUseConnection.Count.ToString(), BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize.ToString()));
                    }
                    nWaitTotalCount++;

                    int nWaitCount = 0;
                    while (nWaitCount < 50 && this._lstWaitConnection.Count <= 0)
                    {
                        Thread.Sleep(10);
                        nWaitCount++;
                    }

                    lock (this)
                    {
                        // 如果等待队列有已释放的连接，则使用
                        if (this._lstWaitConnection.Count > 0)
                        {
                            myConn = (MySqlConnection)this._lstWaitConnection[0];
                            if (this._lstWaitConnectionTime.Count > 0)
                            {
                                this._lstWaitConnectionTime.RemoveAt(0);
                            }
                            this._lstWaitConnection.RemoveAt(0);
                            //this._lstWaitConnection.Remove(myConn);
                            this._lstUseConnection.Add(myConn);
                            _hasConnUseCount[myConn] = 1;
                            if (bIsTrans) { this._lstTransConnection.Add(myConn); } // 事务连接
                        }
                        //// 否则使用已用队列
                        //else if (this._lstUseConnection.Count > 0)
                        //{
                        //    int nCount = this._lstUseConnection.Count;
                        //    int nMinUseCount = 99999999;
                        //    int nConnUseCount = 99999999;
                        //    MySqlConnection connTmp = null;

                        //    // 寻找使用次数最小的连接
                        //    for (int i = 0; i < nCount; i++)
                        //    {
                        //        if (this._lstUseConnection[i] != null
                        //            && this._lstTransConnection.Contains(this._lstUseConnection[i]) == false)
                        //        {
                        //            if (_hasConnUseCount.ContainsKey(this._lstUseConnection[i]) == false)
                        //            {
                        //                connTmp = (MySqlConnection)this._lstUseConnection[i]; ;
                        //                _hasConnUseCount[connTmp] = 1;
                        //                nConnUseCount = 1;
                        //                break;
                        //            }

                        //            nConnUseCount = BasePubfun.ConvertToInt32(_hasConnUseCount[this._lstUseConnection[i]], nMinUseCount + 1);
                        //            if (nConnUseCount >= nMinUseCount)
                        //            {
                        //                continue;
                        //            }

                        //            nMinUseCount = nConnUseCount;
                        //            connTmp = (MySqlConnection)this._lstUseConnection[i];
                        //        }
                        //    }

                        //    // 使用重用次数最小的连接
                        //    if (connTmp != null)
                        //    {
                        //        myConn = connTmp;
                        //        if (bIsTrans) { this._lstTransConnection.Add(myConn); } // 事务连接
                        //        nConnUseCount = nConnUseCount + 1;
                        //        _hasConnUseCount[myConn] = nConnUseCount;// 记录该连接被使用的次数
                        //        LogInfo.OutputInfoLog(string.Format(BaseGlobalData.g_strAppPath + ":MySqlDB#{0}数据库连接：连接数满，使用已用的连接，重用次数={1}。等待队列连接数={2}；正在使用连接数={3}；最大连接数={4}",
                        //        this._nPos.ToString(), nConnUseCount.ToString(), this._lstWaitConnection.Count.ToString(), this._lstUseConnection.Count.ToString(), BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize.ToString()));
                        //    }
                        //    else
                        //    {
                        //        LogInfo.OutputInfoLog(string.Format(BaseGlobalData.g_strAppPath + ":MySqlDB#{0}数据库连接：连接数满，且没有已用连接可用。等待队列连接数={1}；正在使用连接数={2}；最大连接数={3}",
                        //        this._nPos.ToString(), this._lstWaitConnection.Count.ToString(), this._lstUseConnection.Count.ToString(), BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize.ToString()));
                        //    }
                        //}
                    }
                }

                if (myConn != null && myConn.State != ConnectionState.Open)
                {
                    try
                    {
                        myConn.Open();
                    }
                    catch (MySqlException oex)
                    {
                        LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + oex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString());
            }
            return myConn;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="myConn"></param>
        public override void CloseConnection(IDbConnection myConn)
        {
            try
            {
                // 如果不使用等待队列，则直接关闭
                if (BaseParamsManager.GetParamsInstance(this._nPos).UseWaitList == false)
                {
                    Disconnect(myConn, null);
                    return;
                }

                lock (this)
                {
                    if (myConn == null || myConn.ConnectionString.Length == 0)
                    {
                        return;
                    }

                    if (myConn != null && BasePubfun.ConvertToInt32(_hasConnUseCount[myConn], 0) > 1)
                    {
                        _hasConnUseCount[myConn] = BasePubfun.ConvertToInt32(_hasConnUseCount[myConn], 1) - 1;
                    }
                    else
                    {
                        this._lstUseConnection.Remove(myConn);
                        this._lstTransConnection.Remove(myConn);
                        if (myConn != null && myConn.ConnectionString.Length > 0)
                        {
                            this._lstWaitConnection.Add(myConn);
                            this._lstWaitConnectionTime.Add(DateTime.Now);
                            _hasConnUseCount[myConn] = 0;

                            // 启动等待队列清理程序
                            StartWaitConnectionRebulid();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString());
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="myConn"></param>
        internal override void Disconnect(IDbConnection myConn, Exception ex)
        {
            if (ex != null)
            {
                LogInfo.OutputErrorLog("MySqlDB#" + this._nPos.ToString() + "数据库操作出错：" + ex.ToString());
            }
            //else
            //{
            //    LogInfo.OutputErrorLog("MySqlDB#" + this._nPos.ToString() + "数据库操作出错！");
            //}

            try
            {
                lock (this)
                {
                    this._lstUseConnection.Remove(myConn);

                    int nPos = this._lstWaitConnection.IndexOf(myConn);
                    if (nPos >= 0 && nPos < this._lstWaitConnectionTime.Count)
                    {
                        this._lstWaitConnectionTime.RemoveAt(nPos);
                    }
                    this._lstWaitConnection.Remove(myConn);
                    this._lstTransConnection.Remove(myConn);
                }

                IDbCommand cmd = this._hasConnCommand[myConn] as IDbCommand;
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                this._hasConnCommand.Remove(myConn);

                if (myConn != null)
                {
                    _hasConnUseCount.Remove(myConn);
                    //        _hasConnUseCount[myConn] = 0;
                }

                if (myConn != null && myConn.State != ConnectionState.Closed)
                {
                    myConn.Close();
                    //LogInfo.OutputDebugLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：成功关闭连接#" + myConn.ConnectionString);
                }
            }
            catch (Exception ex1)
            {
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex1.ToString());
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
        /// 退出系统或者出错时断开连接
        /// </summary>
        public override void CloseAllConnection()
        {
            try
            {
                lock (this)
                {
                    int intUseCount = this._lstUseConnection.Count;
                    int intWaitCount = this._lstWaitConnection.Count;
                    LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：关闭所有连接，等待队列连接数：" +
                                intWaitCount.ToString() + "；正在使用连接数：" + intUseCount.ToString());
                    for (int i = 0; i < intUseCount; i++)
                    {
                        IDbConnection myConn = (IDbConnection)this._lstUseConnection[i];
                        LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：正在关闭连接#" + i.ToString());
                        if (myConn.State != ConnectionState.Closed)
                        {
                            myConn.Close();
                            myConn.Dispose();
                            LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：成功关闭连接#" + i.ToString());
                        }
                    }
                    for (int i = 0; i < intWaitCount; i++)
                    {
                        IDbConnection myConn = (IDbConnection)this._lstWaitConnection[i];
                        LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：正在关闭连接#" + i.ToString());
                        if (myConn.State != ConnectionState.Closed)
                        {
                            myConn.Close();
                            myConn.Dispose();
                            LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：成功关闭连接#" + i.ToString());
                        }
                    }
                    this._lstUseConnection.Clear();
                    this._lstWaitConnection.Clear();
                    this._lstWaitConnectionTime.Clear();
                    this._lstTransConnection.Clear();
                    _hasConnUseCount.Clear();
                }
            }
            catch (Exception ex)
            {
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString());
            }
        }

        /// <summary>
        /// 执行非查询性SQL，带连接，用于事务处理
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public override int ExceuteSQL(string strSQL, IDbConnection objConn, BaseTransaction objTrans)
        {
            int nCount = -1;
            MySqlCommand cmd = null;
            MySqlConnection conn = null;
            try
            {
                conn = (MySqlConnection)objConn;
                
                SetDBCharSet(conn, objTrans);

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _nCommandTimeout; // 设置超时时间为60秒 add by zhangjb 20151202 for i单车查询超时
                    _hasConnCommand[conn] = cmd;
                }
                cmd.CommandText = strSQL;

                if (objTrans != null)
                {
                    cmd.Transaction = objTrans.DBTransaction as MySqlTransaction;
                }
                nCount = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                nCount = -2;//
                if (objTrans == null)
                {
                    Disconnect(conn, ex);
                }
                else
                {
                    objTrans.ConnError = true;
                    objTrans.ConnErrorEx = ex;
                }
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            return nCount;
        }

        /// <summary>
        /// 执行非查询性SQL，带连接，用于事务处理（add by zhangjb 2015-12-25）
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public override int ExceuteSQLBindParams(string strSQL, Hashtable hasParams, IDbConnection objConn, BaseTransaction objTrans)
        {
            int nCount = -1;
            MySqlCommand cmd = null;
            MySqlConnection conn = null;
            try
            {
                conn = (MySqlConnection)objConn;

                SetDBCharSet(conn, objTrans);

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    _hasConnCommand[conn] = cmd;
                }

                // 设置Command参数
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                foreach (string strParamsName in hasParams.Keys)
                {
                    cmd.Parameters.AddWithValue(strParamsName, hasParams[strParamsName]);
                }

                if (objTrans != null)
                {
                    cmd.Transaction = objTrans.DBTransaction as MySqlTransaction;
                }
                nCount = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                nCount = -2;//
                if (objTrans == null)
                {
                    Disconnect(conn, ex);
                }
                else
                {
                    objTrans.ConnError = true;
                    objTrans.ConnErrorEx = ex;
                }
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            return nCount;
        }

        /// <summary>
        /// 执行非查询性SQL，带连接，用于事务处理
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public override int ExceuteSQLTran(System.Collections.Generic.List<string> lstSQL)
        {
            BaseTransaction objTrans = new BaseTransaction();

            try
            {
                objTrans.BeginTransaction();
                for (int i = 0; i < lstSQL.Count; i++)
                {
                    string strsql = lstSQL[i];
                    if (strsql.Trim().Length > 1)
                    {
                        objTrans.ExceuteSQL(strsql);
                    }
                }

                objTrans.Commit();
                return 1;
            }
            catch (Exception ex)
            {
                LogInfo.OutputErrorLog("Mysql执行事务失败：" + ex.Message);
                objTrans.Rollback();
                return -1;
            }
        }

        /// <summary>
        /// 执行SQL查询并返回DataSet(用于事务处理)
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public override DataSet ExcecuteQuerySQL(String strSQL, String strTableName, IDbConnection objConn, BaseTransaction objTrans)
        {
            DataSet MyDataSet = new DataSet(); // '定义DataSet 
            MySqlConnection conn = null;
            MySqlCommand cmd = null;
            try
            {
                conn = (MySqlConnection)objConn;

                SetDBCharSet(conn, objTrans);

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _nCommandTimeout; // 设置超时时间为60秒 add by zhangjb 20151202 for i单车查询超时
                    _hasConnCommand[conn] = cmd;
                }

                cmd.CommandText = strSQL;

                if (objTrans != null)
                {
                    cmd.Transaction = objTrans.DBTransaction as MySqlTransaction;
                }
                MySqlDataAdapter MyDataAdapter = new MySqlDataAdapter();
                MyDataAdapter.SelectCommand = cmd;// 设置OleDbDataAdapte对象的SelectCommand属性
                MyDataAdapter.Fill(MyDataSet, strTableName);

                MyDataAdapter.Dispose();
            }
            catch (Exception ex)
            {
                if (objTrans == null)
                {
                    Disconnect(conn, ex);
                }
                else
                {
                    objTrans.ConnError = true;
                    objTrans.ConnErrorEx = ex;
                }
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            return MyDataSet;
        }

        /// <summary>
        /// 执行SQL查询并返回DataSet(绑定变量 add by zhangjb 2015-12-25）
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public override DataSet ExcecuteQuerySQLBindParams(String strSQL, Hashtable hasParams, String strTableName, IDbConnection objConn, BaseTransaction objTrans)
        {
            DataSet MyDataSet = new DataSet(); // '定义DataSet 
            MySqlConnection conn = null;
            MySqlCommand cmd = null;
            try
            {
                // 获取数据库连接和Command对象
                conn = (MySqlConnection)objConn;

                SetDBCharSet(conn, null);

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    _hasConnCommand[conn] = cmd;
                }

                // 设置Command参数
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                foreach (string strParamsName in hasParams.Keys)
                {
                    cmd.Parameters.AddWithValue(strParamsName, hasParams[strParamsName]);
                }
                //cmd.Prepare();

                // 执行检索
                if (objTrans != null)
                {
                    cmd.Transaction = objTrans.DBTransaction as MySqlTransaction;
                }
                MySqlDataAdapter MyDataAdapter = new MySqlDataAdapter();
                MyDataAdapter.SelectCommand = cmd;// 设置OleDbDataAdapte对象的SelectCommand属性
                MyDataAdapter.Fill(MyDataSet, strTableName);

                // 释放资源
                MyDataAdapter.Dispose();
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                if (objTrans == null)
                {
                    Disconnect(conn, ex);
                }
                else
                {
                    objTrans.ConnError = true;
                    objTrans.ConnErrorEx = ex;
                }
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            return MyDataSet;
        }

        /// <summary>
        /// 执行非查询性SQL
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public override int ExceuteSQL(string strSQL)
        {
            int nCount;
            nCount = -1;

            MySqlConnection conn = (MySqlConnection)GetConnection();
            try
            {
                nCount = ExceuteSQL(strSQL, conn, null);
            }
            catch (Exception ex)
            {
                nCount = -2;
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return nCount;
        }

        /// <summary>
        /// 执行SQL查询并返回DataSet
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public override DataSet ExcecuteQuerySQL(String strSQL, String strTableName)
        {
            DataSet MyDataSet = new DataSet(); // '定义DataSet 
            MySqlConnection conn = (MySqlConnection)GetConnection();
            try
            {
                MyDataSet = ExcecuteQuerySQL(strSQL, strTableName, conn, null);
            }
            catch (Exception ex)
            {
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return MyDataSet;
        }

        /// <summary>
        /// 执行SQL查询并返回MySQLDataReader(操作BLOB)
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public override MySqlDataReader ExceuteQuerySQL_Reader(String strSQL)
        {
            MySqlConnection conn = null;
            MySqlDataReader MyReader = null;
            MySqlCommand cmd = null;
            try
            {
                // 获取数据库连接和Command对象
                conn = (MySqlConnection)GetConnection();

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _nCommandTimeout; // 设置超时时间为60秒 add by zhangjb 20151202 for i单车查询超时
                    _hasConnCommand[conn] = cmd;
                }

                // 设置Command参数
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();

                // 执行SQL
                SetDBCharSet(conn, null);
                MyReader = (MySqlDataReader)cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            }
            catch (Exception ex)
            {
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return MyReader;
        }

        /// <summary>
        /// 执行非查询SQL（绑定变量方式）
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="hasParams"></param>
        /// <returns></returns>
        public override int ExceuteSQLBindParams(string strSQL, Hashtable hasParams)
        {
            int nCount = -1;
            MySqlCommand cmd = null;
            MySqlConnection conn = null;
            try
            {
                // 获取数据库连接和Command对象
                conn = (MySqlConnection)GetConnection();

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _nCommandTimeout; // 设置超时时间为60秒 add by zhangjb 20151202 for i单车查询超时
                    _hasConnCommand[conn] = cmd;
                }

                // 设置Command参数
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                foreach (string strParamsName in hasParams.Keys)
                {
                    cmd.Parameters.AddWithValue(strParamsName, hasParams[strParamsName]);
                }
                //cmd.Prepare();

                // 执行SQL
                nCount = cmd.ExecuteNonQuery();

                // 清空资源
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                nCount = -2;
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return nCount;
        }

        /// <summary>
        /// 执行SQL查询并返回DataSet(绑定变量)
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="hasParams"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public override DataSet ExcecuteQuerySQLBindParams(String strSQL, Hashtable hasParams, String strTableName)
        {
            DataSet MyDataSet = new DataSet(); // '定义DataSet 
            MySqlCommand cmd = null;
            MySqlConnection conn = null;
            try
            {
                // 获取数据库连接和Command对象
                conn = (MySqlConnection)GetConnection();

                SetDBCharSet(conn, null);

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _nCommandTimeout; // 设置超时时间为60秒 add by zhangjb 20151202 for i单车查询超时
                    _hasConnCommand[conn] = cmd;
                }

                // 设置Command参数
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                foreach (string strParamsName in hasParams.Keys)
                {
                    cmd.Parameters.AddWithValue(strParamsName, hasParams[strParamsName]);
                }
                //cmd.Prepare();

                // 执行检索
                MySqlDataAdapter MyDataAdapter = new MySqlDataAdapter();
                MyDataAdapter.SelectCommand = cmd;// 设置OleDbDataAdapte对象的SelectCommand属性
                MyDataAdapter.Fill(MyDataSet, strTableName);

                // 释放资源
                MyDataAdapter.Dispose();
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "：" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return MyDataSet;
        }
    }
}
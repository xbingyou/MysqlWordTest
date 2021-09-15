
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
    /// MySqlDBManager ��ժҪ˵����
    /// </summary>
    public class MySqlDBManagerd : BaseDBManager
    {
        // ���ݿ����ӳ�
        IList _lstUseConnection = new List<MySqlConnection>();
        IList _lstWaitConnection = new List<MySqlConnection>();
        IList<DateTime> _lstWaitConnectionTime = new List<DateTime>(); // ��¼�ȴ������е����ӵ����ʹ��ʱ�䣨ÿ��5������������һ�Σ�
        IList _lstTransConnection = new List<MySqlConnection>();

        Hashtable _hasConnUseCount = new Hashtable(); // ����ʹ�ô���
        Hashtable _hasConnCommand = new Hashtable(); // Connect��Ӧ��Command

        private static int _nCommandTimeout = 60; // ����ִ�г�ʱʱ�䣨�룩 add by zhangjb 20151202 for i����ִ�г�ʱ

        /// <summary>
        /// �ȴ������ؽ�������ʱ���ӣ�ÿ��5���ӣ�
        /// </summary>
        private bool _bWaitConnectionRebulid = false;
        private System.Timers.Timer timerWaitConnectionRebulid = new System.Timers.Timer(300000);
        private void StartWaitConnectionRebulid()
        {
            if (_bWaitConnectionRebulid == false)
            {
                _bWaitConnectionRebulid = true;
                LogInfo.OutputInfoLog("MySqlDB#" + this._nPos.ToString() + "�ȴ����ж�ʱ�������������");
                timerWaitConnectionRebulid.Elapsed += new System.Timers.ElapsedEventHandler(WaitConnectionRebulid);
                timerWaitConnectionRebulid.AutoReset = false;//������ִ��һ�Σ�false������һֱִ��(true)�� 
                timerWaitConnectionRebulid.Enabled = true;//�Ƿ�ִ��System.Timers.Timer.Elapsed�¼��� 
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

                    LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "���ȴ������������ִ�У��ȴ�����������=" + _lstWaitConnection.Count.ToString());

                    try
                    {
                        for (int i = 0; i < nSize; i++)
                        {
                            dtTime = this._lstWaitConnectionTime[0].AddMinutes(5);
                            if (dtTime.CompareTo(dtTimeNow) < 0)
                            {
                                // �ȴ��������Ѿ�����5����û��ʹ��
                                myConn = (MySqlConnection)this._lstWaitConnection[0];

                                this._lstWaitConnectionTime.RemoveAt(0);
                                this._lstWaitConnection.RemoveAt(0);
                                LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "���ȴ��������ӳ�ʱ���Ƴ����У�");

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
                                        LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "���ȴ��������ӳ�ʱ���ɹ��رգ�");
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
                        LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex1.ToString());
                    }
                }
                Thread.Sleep(300000); // �ȴ�5����
            }
            timerWaitConnectionRebulid.Enabled = true;
        }

        /// <summary>
        /// �������ݿ����ӵ��ַ���
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
        /// ��ȡ���ݿ�DataSource
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
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString());
            }
            finally
            {
                CloseConnection(conn);
            }

            return strDataSource;
        }

        /// <summary>
        /// ��ȡ���ݿ�DataBase
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
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString());
            }
            finally
            {
                CloseConnection(conn);
            }

            return strDataBase;
        }

        /// <summary>
        /// �õ��û���
        /// </summary>
        /// <param name="bParamsIsTrans"></param>
        /// <returns></returns>
        public override string GetDBUserID(params bool[] bParamsIsTrans)
        {
            MySqlConnectionStringBuilder s = new MySqlConnectionStringBuilder(BaseParamsManager.GetParamsInstance(this._nPos).ConnectionString);
            return s.UserID;
        }

        /// <summary>
        /// �õ��û�����
        /// </summary>
        /// <param name="bParamsIsTrans"></param>
        /// <returns></returns>
        public override string GetDBUserPwd(params bool[] bParamsIsTrans)
        {
            MySqlConnectionStringBuilder s = new MySqlConnectionStringBuilder(BaseParamsManager.GetParamsInstance(this._nPos).ConnectionString);
            return s.Password;
        }

        /// <summary>
        /// ��ȡ���ݿ�����
        /// </summary>
        /// <returns></returns>
        public override IDbConnection GetConnection(params bool[] bParamsIsTrans)
        {
            MySqlConnection myConn = null;
            try
            {
                // �����ʶ
                bool bIsTrans = false;
                if (bParamsIsTrans.Length > 0)
                {
                    bIsTrans = bParamsIsTrans[0];
                }

                // �ȴ��������У�ֱ�Ӵӵȴ������л�ȡ
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
                            if (bIsTrans) { this._lstTransConnection.Add(myConn); } // ��������
                        }
                    }
                }

                // �ȴ��������Ѿ�û�п��õ�����ʱ�����������С�ڹ涨��������������½�һ������
                if (myConn == null)
                {
                    lock (this)
                    {
                        // ���������С�ڹ涨��������������½�һ������
                        if (this._lstUseConnection.Count + this._lstWaitConnection.Count < BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize)
                        {
                            myConn = new MySqlConnection(BaseParamsManager.GetParamsInstance(this._nPos).ConnectionString);
                            this._lstUseConnection.Add(myConn);
                            if (bIsTrans) { this._lstTransConnection.Add(myConn); } // ��������
                            _hasConnUseCount[myConn] = 1;
                            if (BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize > 0)
                            {
                                // ��¼���ݿ�������ռ�ñ���
                                BaseDBManager.g_hasDBConnectUsedPercent[this._nPos] = (this._lstWaitConnection.Count + this._lstUseConnection.Count) * 100.0 / BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize;
                            }
                            LogInfo.OutputDebugLog(BaseGlobalData.g_strAppPath + ":" + string.Format("MySqlDB#{0}���ݿ����ӣ����������С�ڹ涨��������������½�һ�����ӡ��ȴ�����������={1}������ʹ��������={2}�����������={3}",
                                this._nPos.ToString(), this._lstWaitConnection.Count.ToString(), this._lstUseConnection.Count.ToString(), BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize.ToString()));
                        }
                    }
                }

                // �ȴ��������꣬�½�������Ҳ���꣬��ȴ��������ӣ����ȴ�5���ӣ�
                int nWaitTotalCount = 0;
                while (myConn == null && nWaitTotalCount <= 600)
                {
                    if (nWaitTotalCount == 20)
                    {
                        //LogInfo.OutputInfoLog("���ݿ����������㣬�ȴ�ʱ�䳬��10�룡");
                        LogInfo.OutputErrorLog(string.Format(BaseGlobalData.g_strAppPath + ":MySQLDB#{0}���ݿ����ӣ�������������û���������ӿ��ã��ȴ�ʱ�䳬��10�롣�ȴ�����������={1}������ʹ��������={2}�����������={3}",
                                this._nPos.ToString(), this._lstWaitConnection.Count.ToString(), this._lstUseConnection.Count.ToString(), BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize.ToString()));
                    }
                    else if (nWaitTotalCount >= 600)
                    {
                        //LogInfo.OutputInfoLog("���ݿ����������㣬�ȴ�ʱ�䳬��5���ӣ�");
                        LogInfo.OutputErrorLog(string.Format(BaseGlobalData.g_strAppPath + ":MySQLDB#{0}���ݿ����ӣ�������������û���������ӿ��ã��ȴ�ʱ�䳬��5���ӡ��ȴ�����������={1}������ʹ��������={2}�����������={3}",
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
                        // ����ȴ����������ͷŵ����ӣ���ʹ��
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
                            if (bIsTrans) { this._lstTransConnection.Add(myConn); } // ��������
                        }
                        //// ����ʹ�����ö���
                        //else if (this._lstUseConnection.Count > 0)
                        //{
                        //    int nCount = this._lstUseConnection.Count;
                        //    int nMinUseCount = 99999999;
                        //    int nConnUseCount = 99999999;
                        //    MySqlConnection connTmp = null;

                        //    // Ѱ��ʹ�ô�����С������
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

                        //    // ʹ�����ô�����С������
                        //    if (connTmp != null)
                        //    {
                        //        myConn = connTmp;
                        //        if (bIsTrans) { this._lstTransConnection.Add(myConn); } // ��������
                        //        nConnUseCount = nConnUseCount + 1;
                        //        _hasConnUseCount[myConn] = nConnUseCount;// ��¼�����ӱ�ʹ�õĴ���
                        //        LogInfo.OutputInfoLog(string.Format(BaseGlobalData.g_strAppPath + ":MySqlDB#{0}���ݿ����ӣ�����������ʹ�����õ����ӣ����ô���={1}���ȴ�����������={2}������ʹ��������={3}�����������={4}",
                        //        this._nPos.ToString(), nConnUseCount.ToString(), this._lstWaitConnection.Count.ToString(), this._lstUseConnection.Count.ToString(), BaseParamsManager.GetParamsInstance(this._nPos).DBConnectSize.ToString()));
                        //    }
                        //    else
                        //    {
                        //        LogInfo.OutputInfoLog(string.Format(BaseGlobalData.g_strAppPath + ":MySqlDB#{0}���ݿ����ӣ�������������û���������ӿ��á��ȴ�����������={1}������ʹ��������={2}�����������={3}",
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
                        LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + oex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString());
            }
            return myConn;
        }

        /// <summary>
        /// �ر�����
        /// </summary>
        /// <param name="myConn"></param>
        public override void CloseConnection(IDbConnection myConn)
        {
            try
            {
                // �����ʹ�õȴ����У���ֱ�ӹر�
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

                            // �����ȴ������������
                            StartWaitConnectionRebulid();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString());
            }
        }

        /// <summary>
        /// �Ͽ�����
        /// </summary>
        /// <param name="myConn"></param>
        internal override void Disconnect(IDbConnection myConn, Exception ex)
        {
            if (ex != null)
            {
                LogInfo.OutputErrorLog("MySqlDB#" + this._nPos.ToString() + "���ݿ��������" + ex.ToString());
            }
            //else
            //{
            //    LogInfo.OutputErrorLog("MySqlDB#" + this._nPos.ToString() + "���ݿ��������");
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
                    //LogInfo.OutputDebugLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "���ɹ��ر�����#" + myConn.ConnectionString);
                }
            }
            catch (Exception ex1)
            {
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex1.ToString());
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
        /// �˳�ϵͳ���߳���ʱ�Ͽ�����
        /// </summary>
        public override void CloseAllConnection()
        {
            try
            {
                lock (this)
                {
                    int intUseCount = this._lstUseConnection.Count;
                    int intWaitCount = this._lstWaitConnection.Count;
                    LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "���ر��������ӣ��ȴ�������������" +
                                intWaitCount.ToString() + "������ʹ����������" + intUseCount.ToString());
                    for (int i = 0; i < intUseCount; i++)
                    {
                        IDbConnection myConn = (IDbConnection)this._lstUseConnection[i];
                        LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "�����ڹر�����#" + i.ToString());
                        if (myConn.State != ConnectionState.Closed)
                        {
                            myConn.Close();
                            myConn.Dispose();
                            LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "���ɹ��ر�����#" + i.ToString());
                        }
                    }
                    for (int i = 0; i < intWaitCount; i++)
                    {
                        IDbConnection myConn = (IDbConnection)this._lstWaitConnection[i];
                        LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "�����ڹر�����#" + i.ToString());
                        if (myConn.State != ConnectionState.Closed)
                        {
                            myConn.Close();
                            myConn.Dispose();
                            LogInfo.OutputInfoLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "���ɹ��ر�����#" + i.ToString());
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
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString());
            }
        }

        /// <summary>
        /// ִ�зǲ�ѯ��SQL�������ӣ�����������
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
                    cmd.CommandTimeout = _nCommandTimeout; // ���ó�ʱʱ��Ϊ60�� add by zhangjb 20151202 for i������ѯ��ʱ
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
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            return nCount;
        }

        /// <summary>
        /// ִ�зǲ�ѯ��SQL�������ӣ�����������add by zhangjb 2015-12-25��
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

                // ����Command����
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
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            return nCount;
        }

        /// <summary>
        /// ִ�зǲ�ѯ��SQL�������ӣ�����������
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
                LogInfo.OutputErrorLog("Mysqlִ������ʧ�ܣ�" + ex.Message);
                objTrans.Rollback();
                return -1;
            }
        }

        /// <summary>
        /// ִ��SQL��ѯ������DataSet(����������)
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public override DataSet ExcecuteQuerySQL(String strSQL, String strTableName, IDbConnection objConn, BaseTransaction objTrans)
        {
            DataSet MyDataSet = new DataSet(); // '����DataSet 
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
                    cmd.CommandTimeout = _nCommandTimeout; // ���ó�ʱʱ��Ϊ60�� add by zhangjb 20151202 for i������ѯ��ʱ
                    _hasConnCommand[conn] = cmd;
                }

                cmd.CommandText = strSQL;

                if (objTrans != null)
                {
                    cmd.Transaction = objTrans.DBTransaction as MySqlTransaction;
                }
                MySqlDataAdapter MyDataAdapter = new MySqlDataAdapter();
                MyDataAdapter.SelectCommand = cmd;// ����OleDbDataAdapte�����SelectCommand����
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
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            return MyDataSet;
        }

        /// <summary>
        /// ִ��SQL��ѯ������DataSet(�󶨱��� add by zhangjb 2015-12-25��
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public override DataSet ExcecuteQuerySQLBindParams(String strSQL, Hashtable hasParams, String strTableName, IDbConnection objConn, BaseTransaction objTrans)
        {
            DataSet MyDataSet = new DataSet(); // '����DataSet 
            MySqlConnection conn = null;
            MySqlCommand cmd = null;
            try
            {
                // ��ȡ���ݿ����Ӻ�Command����
                conn = (MySqlConnection)objConn;

                SetDBCharSet(conn, null);

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    _hasConnCommand[conn] = cmd;
                }

                // ����Command����
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                foreach (string strParamsName in hasParams.Keys)
                {
                    cmd.Parameters.AddWithValue(strParamsName, hasParams[strParamsName]);
                }
                //cmd.Prepare();

                // ִ�м���
                if (objTrans != null)
                {
                    cmd.Transaction = objTrans.DBTransaction as MySqlTransaction;
                }
                MySqlDataAdapter MyDataAdapter = new MySqlDataAdapter();
                MyDataAdapter.SelectCommand = cmd;// ����OleDbDataAdapte�����SelectCommand����
                MyDataAdapter.Fill(MyDataSet, strTableName);

                // �ͷ���Դ
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
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            return MyDataSet;
        }

        /// <summary>
        /// ִ�зǲ�ѯ��SQL
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
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return nCount;
        }

        /// <summary>
        /// ִ��SQL��ѯ������DataSet
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public override DataSet ExcecuteQuerySQL(String strSQL, String strTableName)
        {
            DataSet MyDataSet = new DataSet(); // '����DataSet 
            MySqlConnection conn = (MySqlConnection)GetConnection();
            try
            {
                MyDataSet = ExcecuteQuerySQL(strSQL, strTableName, conn, null);
            }
            catch (Exception ex)
            {
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return MyDataSet;
        }

        /// <summary>
        /// ִ��SQL��ѯ������MySQLDataReader(����BLOB)
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
                // ��ȡ���ݿ����Ӻ�Command����
                conn = (MySqlConnection)GetConnection();

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _nCommandTimeout; // ���ó�ʱʱ��Ϊ60�� add by zhangjb 20151202 for i������ѯ��ʱ
                    _hasConnCommand[conn] = cmd;
                }

                // ����Command����
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();

                // ִ��SQL
                SetDBCharSet(conn, null);
                MyReader = (MySqlDataReader)cmd.ExecuteReader(CommandBehavior.SequentialAccess);
            }
            catch (Exception ex)
            {
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return MyReader;
        }

        /// <summary>
        /// ִ�зǲ�ѯSQL���󶨱�����ʽ��
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
                // ��ȡ���ݿ����Ӻ�Command����
                conn = (MySqlConnection)GetConnection();

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _nCommandTimeout; // ���ó�ʱʱ��Ϊ60�� add by zhangjb 20151202 for i������ѯ��ʱ
                    _hasConnCommand[conn] = cmd;
                }

                // ����Command����
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                foreach (string strParamsName in hasParams.Keys)
                {
                    cmd.Parameters.AddWithValue(strParamsName, hasParams[strParamsName]);
                }
                //cmd.Prepare();

                // ִ��SQL
                nCount = cmd.ExecuteNonQuery();

                // �����Դ
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                nCount = -2;
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return nCount;
        }

        /// <summary>
        /// ִ��SQL��ѯ������DataSet(�󶨱���)
        /// </summary>
        /// <param name="strSQL"></param>
        /// <param name="hasParams"></param>
        /// <param name="strTableName"></param>
        /// <returns></returns>
        public override DataSet ExcecuteQuerySQLBindParams(String strSQL, Hashtable hasParams, String strTableName)
        {
            DataSet MyDataSet = new DataSet(); // '����DataSet 
            MySqlCommand cmd = null;
            MySqlConnection conn = null;
            try
            {
                // ��ȡ���ݿ����Ӻ�Command����
                conn = (MySqlConnection)GetConnection();

                SetDBCharSet(conn, null);

                cmd = _hasConnCommand[conn] as MySqlCommand;
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = _nCommandTimeout; // ���ó�ʱʱ��Ϊ60�� add by zhangjb 20151202 for i������ѯ��ʱ
                    _hasConnCommand[conn] = cmd;
                }

                // ����Command����
                cmd.CommandText = strSQL;
                cmd.Parameters.Clear();
                foreach (string strParamsName in hasParams.Keys)
                {
                    cmd.Parameters.AddWithValue(strParamsName, hasParams[strParamsName]);
                }
                //cmd.Prepare();

                // ִ�м���
                MySqlDataAdapter MyDataAdapter = new MySqlDataAdapter();
                MyDataAdapter.SelectCommand = cmd;// ����OleDbDataAdapte�����SelectCommand����
                MyDataAdapter.Fill(MyDataSet, strTableName);

                // �ͷ���Դ
                MyDataAdapter.Dispose();
                cmd.Parameters.Clear();
            }
            catch (Exception ex)
            {
                Disconnect(conn, ex);
                LogInfo.OutputErrorLog(BaseGlobalData.g_strAppPath + ":MySqlDB#" + this._nPos.ToString() + "��" + ex.ToString() + ";\r\nSQL=" + strSQL);
            }
            finally
            {
                CloseConnection(conn);
            }
            return MyDataSet;
        }
    }
}
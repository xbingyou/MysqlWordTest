using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace WindowsTest
{
    public class ReportDetail:IReport
    {
        #region 字段
        //表名
        public static string TableName = "report_detail";
        //字段
        public static string report_id = "report_id";//报告编号（自增）
        public static string report_title = "report_title";//报告标题
        public static string report_time = "report_time";//报告时间
        public static string pet_name = "pet_name";//宠物昵称
        public static string pet_type = "pet_type";//宠物种类
        public static string pet_gender = "pet_gender";//宠物性别
        public static string pet_age = "pet_age";//宠物年龄
        public static string owner_name = "owner_name";//宠主姓名
        public static string owner_contact = "owner_contact";//宠主电话
        public static string test_code = "test_code";//实验编号
        public static string test_project = "test_project";//检测项目
        public static string test_time = "test_time";//检测时间
        public static string test_doctor = "test_doctor";//检测医师
        public static string main_doctor = "main_doctor";//主治医师
        public static string hospital_name = "hospital_name";//医院名称
        public static string hospital_contact = "hospital_contact";//医院电话
        public static string hospital_address = "hospital_address";//医院地址
        public static string test_type = "test_type";//检测类型

        //属性
        public int ReportId { get; set; }//报告编号（自增）
        public string ReportTitle { get; set; }//报告标题
        public string ReportTime { get; set; }//报告时间
        public string PetName { get; set; }//宠物昵称
        public string PetType { get; set; }//宠物种类
        public string PetGender { get; set; }//宠物性别
        public string PetAge { get; set; }//宠物年龄
        public string OwnerName { get; set; }//宠主姓名
        public string OwnerContact { get; set; }//宠主电话
        public string TestCode { get; set; }//实验编号
        public string TestProject { get; set; }//检测项目
        public string TestTime { get; set; }//检测时间
        public string TestDoctor { get; set; }//检测医师
        public string MainDoctor { get; set; }//主治医师
        public string HospitalName { get; set; }//医院名称
        public string HospitalContact { get; set; }//医院电话
        public string HospitalAddress { get; set; }//医院地址
        public string TestType { get; set; }//检测类型

        #endregion

        #region 表操作
        /// <summary>
        /// 创建表
        /// </summary>
        /// <returns></returns>
        public static bool CreateReportDBTable()
        {
            try
            {
                if (Pubfun.CheckDBTableExist(TableName))
                    return true;
                StringBuilder strSQL = new StringBuilder();
                strSQL.Append("create table ");
                strSQL.Append(ReportDetail.TableName).Append("( ");
                strSQL.Append(ReportDetail.report_id).Append(" int primary key not null auto_increment,");
                strSQL.Append(ReportDetail.report_title).Append(" varchar(64),");
                strSQL.Append(ReportDetail.report_time).Append(" varchar(32),");
                strSQL.Append(ReportDetail.pet_name).Append(" varchar(32),");
                strSQL.Append(ReportDetail.pet_type).Append(" varchar(16),");
                strSQL.Append(ReportDetail.pet_gender).Append(" varchar(16),");
                strSQL.Append(ReportDetail.pet_age).Append(" varchar(32),");
                strSQL.Append(ReportDetail.owner_name).Append(" varchar(32),");
                strSQL.Append(ReportDetail.owner_contact).Append(" varchar(16),");
                strSQL.Append(ReportDetail.test_code).Append(" varchar(32),");
                strSQL.Append(ReportDetail.test_project).Append(" varchar(128),");
                strSQL.Append(ReportDetail.test_time).Append(" varchar(32),");
                strSQL.Append(ReportDetail.test_doctor).Append(" varchar(32),");
                strSQL.Append(ReportDetail.main_doctor).Append(" varchar(32),");
                strSQL.Append(ReportDetail.hospital_name).Append(" varchar(64),");
                strSQL.Append(ReportDetail.hospital_contact).Append(" varchar(16),");
                strSQL.Append(ReportDetail.hospital_address).Append(" varchar(128),");
                strSQL.Append(ReportDetail.test_type).Append(" varchar(64)");
                strSQL.Append(" )");
                int nRet = MySqlDBManager.GetInstance().ExcuteSQL(strSQL.ToString());
                if (nRet >= 0)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 删除
        /// </summary>
        public bool Delete()
        {
            string strDeleteSQL = "delete from " + ReportDetail.TableName +
                        " where " + ReportDetail.report_id + "=" + Convert.ToString(this.ReportId) + "";
            int nRet = MySqlDBManager.GetInstance().ExcuteSQL(strDeleteSQL);
            if (nRet >= 0)
                return true;
            return false;
        }
        /// <summary>
        /// 插入
        /// </summary>
        public bool Insert()
        {
            StringBuilder strSQL = new StringBuilder();
            strSQL.Append("insert into ").Append(ReportDetail.TableName).Append(" (");
            strSQL.Append(ReportDetail.report_title).Append(",");
            strSQL.Append(ReportDetail.report_time).Append(",");
            strSQL.Append(ReportDetail.pet_name).Append(",");
            strSQL.Append(ReportDetail.pet_type).Append(",");
            strSQL.Append(ReportDetail.pet_gender).Append(",");
            strSQL.Append(ReportDetail.pet_age).Append(",");
            strSQL.Append(ReportDetail.owner_name).Append(",");
            strSQL.Append(ReportDetail.owner_contact).Append(",");
            strSQL.Append(ReportDetail.test_code).Append(",");
            strSQL.Append(ReportDetail.test_project).Append(",");
            strSQL.Append(ReportDetail.test_time).Append(",");
            strSQL.Append(ReportDetail.test_doctor).Append(",");
            strSQL.Append(ReportDetail.main_doctor).Append(",");
            strSQL.Append(ReportDetail.hospital_name).Append(",");
            strSQL.Append(ReportDetail.hospital_contact).Append(",");
            strSQL.Append(ReportDetail.hospital_address).Append(",");
            strSQL.Append(ReportDetail.test_type);
            strSQL.Append(") values (");
            strSQL.Append("'").Append(ReportTitle).Append("',");
            strSQL.Append("'").Append(ReportTime).Append("',");
            strSQL.Append("'").Append(PetName).Append("',");
            strSQL.Append("'").Append(PetType).Append("',");
            strSQL.Append("'").Append(PetGender).Append("',");
            strSQL.Append("'").Append(PetAge).Append("',");
            strSQL.Append("'").Append(OwnerName).Append("',");
            strSQL.Append("'").Append(OwnerContact).Append("',");
            strSQL.Append("'").Append(TestCode).Append("',");
            strSQL.Append("'").Append(TestProject).Append("',");
            strSQL.Append("'").Append(TestTime).Append("',");
            strSQL.Append("'").Append(TestDoctor).Append("',");
            strSQL.Append("'").Append(MainDoctor).Append("',");
            strSQL.Append("'").Append(HospitalName).Append("',");
            strSQL.Append("'").Append(HospitalContact).Append("',");
            strSQL.Append("'").Append(HospitalAddress).Append("',");
            strSQL.Append("'").Append(TestType).Append("'");
            strSQL.Append(")");
            int nRet = MySqlDBManager.GetInstance().ExcuteSQL(strSQL.ToString(), true);
            if (nRet >= 0)
                return true;
            return false;
        }

        #endregion


        #region 表查询


        public static List<ReportDetail> GetReportLst()
        {
            List<ReportDetail> lstData = new List<ReportDetail>();
            string strSQL = "select * from " + ReportDetail.TableName;
            DataSet objDs = MySqlDBManager.GetInstance().ExcuteQuerySQL(strSQL, ReportDetail.TableName);
            if (objDs == null || objDs.Tables.Count <= 0)
            {
                return lstData;
            }
            int nCount = objDs.Tables[0].Rows.Count;
            ReportDetail objReportDetail;
            for (int i = 0; i < nCount; i++)
            {
                DataRow objDr = objDs.Tables[0].Rows[i];
                objReportDetail = new ReportDetail();
                objReportDetail.ReportId = int.Parse(objDr[ReportDetail.report_id].ToString());
                objReportDetail.ReportTitle = objDr[ReportDetail.report_title].ToString();
                objReportDetail.ReportTime = objDr[ReportDetail.report_time].ToString();
                objReportDetail.PetName = objDr[ReportDetail.pet_name].ToString();
                objReportDetail.PetType = objDr[ReportDetail.pet_type].ToString();
                objReportDetail.PetGender = objDr[ReportDetail.pet_gender].ToString();
                objReportDetail.PetAge = objDr[ReportDetail.pet_age].ToString();
                objReportDetail.PetAge = objDr[ReportDetail.pet_age].ToString();
                objReportDetail.OwnerName = objDr[ReportDetail.owner_name].ToString();
                objReportDetail.OwnerContact = objDr[ReportDetail.owner_contact].ToString();
                objReportDetail.TestCode = objDr[ReportDetail.test_code].ToString();
                objReportDetail.TestProject = objDr[ReportDetail.test_project].ToString();
                objReportDetail.TestTime = objDr[ReportDetail.test_time].ToString();
                objReportDetail.TestDoctor = objDr[ReportDetail.test_doctor].ToString();
                objReportDetail.MainDoctor = objDr[ReportDetail.main_doctor].ToString();
                objReportDetail.HospitalName = objDr[ReportDetail.hospital_name].ToString();
                objReportDetail.HospitalContact = objDr[ReportDetail.hospital_contact].ToString();
                objReportDetail.HospitalAddress = objDr[ReportDetail.hospital_address].ToString();
                objReportDetail.TestType = objDr[ReportDetail.test_type].ToString();
                lstData.Add(objReportDetail);
            }
            return lstData;
        }

        #endregion
    }
}

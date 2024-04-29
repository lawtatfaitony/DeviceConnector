using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Common
{
    public partial class CommonBase
    {
        /// <summary>    
        /// 将泛型集合类转换成DataTable    
        /// </summary>    
        /// <typeparam name="T">集合项类型</typeparam>    
        /// <param name="list">集合</param>    
        /// <param name="propertyName">需要返回的列的列名</param>    
        /// <returns>数据集(表)</returns>    
        public static DataTable ToDataTable<T>(IList<T> list, params string[] propertyName)
        {
            List<string> propertyNameList = new List<string>();
            if (propertyName != null)
                propertyNameList.AddRange(propertyName);
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (propertyNameList.Count == 0)
                    {
                        result.Columns.Add(pi.Name, pi.PropertyType);
                    }
                    else
                    {
                        if (propertyNameList.Contains(pi.Name))
                            result.Columns.Add(pi.Name, pi.PropertyType);
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        if (propertyNameList.Count == 0)
                        {
                            object obj = pi.GetValue(list[i], null);
                            tempList.Add(obj);
                        }
                        else
                        {
                            if (propertyNameList.Contains(pi.Name))
                            {
                                object obj = pi.GetValue(list[i], null);
                                tempList.Add(obj);
                            }
                        }
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }
        /// <summary>
        /// NoLock Invoke DB
        /// </summary>
        /// <param name="d"></param>
        public void NoLockInvokeDB(EFdelegate d)
        {
            var transactionOptions = new System.Transactions.TransactionOptions();
            transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
            using (var transactionScope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Required, transactionOptions))
            {
                d();
                transactionScope.Complete();
            }
        }
        public delegate void EFdelegate();
    }

    /// <summary>
    /// //解决字段空的问题。Time=System.Nullable
    /// </summary>
    public class ListToDataTable
    {
        public static DataTable ToDataTable<T>(IList<T> list)
        {
            return ToDataTable(list, null);
        }

        //将list转换成DataTable
        public static DataTable ToDataTable<T>(IList<T> list, params string[] propertyName)
        {
            List<string> propertyNameList = new List<string>();
            if (propertyName != null)
            {
                propertyNameList.AddRange(propertyName);
            }
            DataTable result = new DataTable();
            if (list.Count > 0)
            {
                PropertyInfo[] propertys = list[0].GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    if (propertyNameList.Count == 0)
                    {
                        //if (DBNull.Value.Equals(pi.PropertyType))
                        //{
                        //   // pi.PropertyType = DateTime;
                        //}
                        Type colType = pi.PropertyType;
                        if (colType.IsGenericType && colType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            colType = colType.GetGenericArguments()[0];
                        }
                        result.Columns.Add(pi.Name, colType);
                        //result.Columns.Add(pi.Name, pi.PropertyType);
                    }
                    else
                    {
                        if (propertyNameList.Contains(pi.Name))
                        {
                            result.Columns.Add(pi.Name, pi.PropertyType);
                        }
                    }
                }
                for (int i = 0; i < list.Count; i++)
                {
                    ArrayList tempList = new ArrayList();
                    foreach (PropertyInfo pi in propertys)
                    {
                        if (propertyNameList.Count == 0)
                        {
                            object obj = pi.GetValue(list[i], null);
                            tempList.Add(obj);
                        }
                        else
                        {
                            if (propertyNameList.Contains(pi.Name))
                            {
                                object obj = pi.GetValue(list[i], null);
                                tempList.Add(obj);
                            }
                        }
                    }
                    object[] array = tempList.ToArray();
                    result.LoadDataRow(array, true);
                }
            }
            return result;
        }
    }

    public class DataTableConvert
    {
        /// <summary>
        /// DataTable转换List<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> TableToList<T>(DataTable dt)
        {
            List<T> ret = new List<T>();
            Type type = typeof(T);
            List<string> lstColumns = new List<string>();
            foreach (DataRow dr in dt.Rows)
            {
                PropertyInfo[] proInfo = type.GetProperties();
                T entity = Activator.CreateInstance<T>();
                foreach (PropertyInfo p in proInfo)
                {
                    if (!dt.Columns.Contains(p.Name) || dr[p.Name] == null || dr[p.Name] == DBNull.Value)
                        continue;
                    if (p.PropertyType == typeof(DateTime) && Convert.ToDateTime(dr[p.Name]) < Convert.ToDateTime("1753-01-01"))
                        continue;
                    try
                    {
                        object obj = Convert.ChangeType(dr[p.Name], p.PropertyType);
                        p.SetValue(entity, obj, null);
                    }
                    catch
                    {

                    }
                }
                ret.Add(entity);
            }
            return ret;
        }

        /// <summary>
        /// List<T>转换DataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static DataTable ListToTable<T>(List<T> list)
        {
            Type type = typeof(T);
            PropertyInfo[] proInfo = type.GetProperties();
            DataTable dt = new DataTable();
            foreach (PropertyInfo p in proInfo)
            {
                //类型存在Nullable<Type>时，需要进行以下处理，否则异常
                Type t = p.PropertyType;
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    t = t.GetGenericArguments()[0];
                dt.Columns.Add(p.Name, t);
            }
            foreach (T t in list)
            {
                DataRow dr = dt.NewRow();
                foreach (PropertyInfo p in proInfo)
                {
                    object obj = p.GetValue(t);
                    if (obj == null) continue;
                    if (p.PropertyType == typeof(DateTime) && Convert.ToDateTime(obj) < Convert.ToDateTime("1753-01-01"))
                        continue;
                    dr[p.Name] = obj;
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static T TableRowToEntity<T>(DataTable dt, int rowIndex)
        {
            Type type = typeof(T);
            T entity = Activator.CreateInstance<T>();
            if (dt == null) return entity;
            DataRow dr = dt.Rows[rowIndex];
            PropertyInfo[] proInfo = type.GetProperties();
            foreach (PropertyInfo p in proInfo)
            {
                if (!dt.Columns.Contains(p.Name) || dr[p.Name] == null || dr[p.Name] == DBNull.Value)
                    continue;
                if (p.PropertyType == typeof(DateTime) && Convert.ToDateTime(dr[p.Name]) < Convert.ToDateTime("1753-01-01"))
                    continue;
                try
                {
                    object obj = Convert.ChangeType(dr[p.Name], p.PropertyType);
                    p.SetValue(entity, obj, null);
                }
                catch
                {

                }
            }
            return entity;
        }
    }
}



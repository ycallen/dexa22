using DM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace DAL
{
    public class GetCandidates
    {
        public DataTable Retrieve(Dictionary<int, List<Triple>> dict, Dictionary<string, List<Triple>> variablesDic)
        {
            using (SqlConnection cn = new SqlConnection("Data Source=COM123769;Initial Catalog=tempdb;Integrated Security=True"))
            {
                cn.Open();
                SqlTransaction transaction = cn.BeginTransaction();
                SqlCommand cmd = null;
                StringBuilder sb = new StringBuilder();
                sb.Append("Select * from ");

                foreach (var key in dict.Keys)
                {
                    cmd = new SqlCommand(string.Format("create table #triples_{0} (subject varchar(MAX), predicate varchar(MAX), object varchar(MAX))", key), cn, transaction);
                    cmd.ExecuteNonQuery();
                    
                    DataTable tbl = new DataTable();
                    tbl.Columns.Add(new DataColumn("subject", typeof(string)));
                    tbl.Columns.Add(new DataColumn("predicate", typeof(string)));
                    tbl.Columns.Add(new DataColumn("object", typeof(string)));

                    for (int i = 0; i < dict[key].Count; i++)
                    {
                        DataRow dr = tbl.NewRow();
                        dr["subject"] = dict[key][i].Subject.Value;
                        dr["predicate"] = dict[key][i].Predicate.Value;
                        dr["object"] = dict[key][i].Object.Value;
                        tbl.Rows.Add(dr);
                    }

                    SqlBulkCopy objbulk = new SqlBulkCopy(cn, SqlBulkCopyOptions.Default, transaction);

                    objbulk.ColumnMappings.Add("subject", "subject");
                    objbulk.ColumnMappings.Add("predicate", "predicate");
                    objbulk.ColumnMappings.Add("object", "object");

                    objbulk.DestinationTableName = string.Format("#triples_{0}", key);
                    objbulk.WriteToServer(tbl);
                    sb.Append(string.Format("#triples_{0},", key));
                }
                sb.Length--;

                foreach (var key in variablesDic.Keys)
                {
                    if(variablesDic[key].Count == 1)
                    {
                        continue;
                    }

                    var first = variablesDic[key][0];
                    var firstVariableField = first.GetVariableField(key).ToString().ToLower();
                    var firstIndex = first.Index;
                    for (int i = 1; i < variablesDic[key].Count; i++)
                    {
                        var variableField = variablesDic[key][i].GetVariableField(key).ToString().ToLower();
                        sb.Append(String.Format(" and #triples_{0}.{1} = #triples_{2}.{3} ",
                          firstIndex  , firstVariableField, variablesDic[key][i].Index, variableField));
                    }
                }

                var select = sb.ToString();
                var regex = new Regex(Regex.Escape("and"));
                select = regex.Replace(select, "where", 1);

                
                cmd = new SqlCommand(select, cn, transaction);

                DataTable dt = new DataTable();
                using (SqlDataAdapter a = new SqlDataAdapter(cmd))
                {
                    a.Fill(dt);
                }

                transaction.Commit();
                return dt;
            }
        }
    }
}

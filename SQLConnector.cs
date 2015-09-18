using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SearchBackend
{
    // The purpose of this class is to connect to a SQL database and push keywords and page rank data
    public class SQLConnector
    {
        private SqlConnection sq;
        public SQLConnector(string ConnectionString)
        {
            sq = new SqlConnection(ConnectionString);
        }

        public void AddKeywords(string url, Dictionary<string, int> keywords)
        {
            string SQL = "INSERT INTO Keywords(url,keyword,k_count,GUID) VALUES";
            foreach (var hash in keywords)
            {
                SQL += String.Format(" ('{0}', '{1}', {2}, '{3}'),", url, hash.Key, hash.Value, url + "_" + hash.Key);
            }

            // Remove last comma and add a semi colon.
            SQL = SQL.Substring(0, SQL.Length - 1) + ';';

            if (keywords.Count > 0)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(SQL, sq);

                    lock (sq)
                    {
                        sq.Open();
                        cmd.CommandType = System.Data.CommandType.Text;
                        cmd.ExecuteNonQuery();
                        sq.Close();
                    }
                }
                catch
                {
                    return;
                }
                finally
                {
                    sq.Close();
                }
            }
        }
    }
}

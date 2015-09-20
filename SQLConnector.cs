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
        private static string connection;
        public SQLConnector(string ConnectionString)
        {
            connection = ConnectionString;
            //sq = new SqlConnection(ConnectionString);
        }

        public void AddKeywordsAndRank(string url, Dictionary<string, int> keywords, int pagerank)
        {
            SqlConnection sq = new SqlConnection(connection);

            string SQL = "INSERT INTO Keywords(url,keyword,k_count,GUID) VALUES";
            StringBuilder sb = new StringBuilder(SQL);
            int i = 1;
            if (keywords.Count > 0)
            {
                foreach (var hash in keywords)
                {
                    // Flush the next thousand rows (SQL limitation only allows 1,000 inserts per INSERT INTO)
                    if ((i % 1000 == 0) || (i == keywords.Count))
                    {
                        try
                        {
                            SQL = sb.ToString();
                            // Remove last comma and add a semi colon.
                            SQL = SQL.Substring(0, SQL.Length - 1) + ';';

                            SqlCommand cmd = new SqlCommand(SQL, sq);
                            //SqlCommand cmdRank = new SqlCommand(SQLRank, sq);

                            sq.Open();
                            cmd.CommandType = System.Data.CommandType.Text;
                            cmd.ExecuteNonQuery();

                            //cmdRank.CommandType = System.Data.CommandType.Text;
                            //cmdRank.ExecuteNonQuery();
                            sq.Close();
                        }
                        finally
                        {
                            sq.Close();
                        }
                        SQL = "INSERT INTO Keywords(url,keyword,k_count,GUID) VALUES";
                        sb = new StringBuilder(SQL);
                    }
                    else
                    {
                        sb.Append(String.Format(" ('{0}', '{1}', {2}, '{3}'),", url, hash.Key, hash.Value, url + "_" + hash.Key));
                    }
                    i++;
                }
            }
        }

        // Method for benchmarking SQL performance
        public void TEST_Rank(string url, int count)
        {
            SqlConnection sq = new SqlConnection(connection);
            string SQLRank = String.Format("INSERT INTO Page_Rank(url,P_rank) VALUES ('{0}', {1});", url, count);

            try
            {
                SqlCommand cmdRank = new SqlCommand(SQLRank, sq);

                sq.Open();
                cmdRank.CommandType = System.Data.CommandType.Text;
                cmdRank.ExecuteNonQuery();
                sq.Close();
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

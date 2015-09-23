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
        }

        public void AddKeywordsAndRank(string url, Dictionary<string, int> keywords, int pagerank)
        {
            string SQLRank = String.Format("INSERT INTO Page_Rank(url,P_rank) VALUES ('{0}', {1});", url, pagerank);
            string SQL = SQLRank + "INSERT INTO Keywords(url,keyword,k_count,date,GUID) VALUES";

            StringBuilder sb = new StringBuilder(SQL);
            int i = 1;
            if (keywords.Count > 0)
            {
                using (SqlConnection sq = new SqlConnection(connection))
                {
                    sq.Open();
                    foreach (var hash in keywords)
                    {
                        // Flush the next thousand rows (SQL limitation only allows 1,000 inserts per INSERT INTO)
                        if ((i % 1000 == 0) || (i == keywords.Count))
                        {
                            // Convert string builder object to string
                            SQL = sb.ToString();

                            // Remove last comma and add a semi colon.
                            SQL = SQL.Substring(0, SQL.Length - 1) + ';';

                            SqlCommand cmd = new SqlCommand(SQL, sq);
                            SqlCommand cmdRank = new SqlCommand(SQLRank, sq);

                            cmd.CommandType = System.Data.CommandType.Text;
                            cmd.ExecuteNonQuery();

                            SQL = "INSERT INTO Keywords(url,keyword,k_count,date,GUID) VALUES";
                            sb = new StringBuilder(SQL);
                        }
                        else
                        {
                            sb.Append(String.Format(" ('{0}', '{1}', {2}, '{3}', '{4}'),", url, hash.Key, hash.Value, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), url + "_" + hash.Key));
                        }
                        i++;
                    }
                }
                    
            }
        }

        public Dictionary<string, int> GetVisited()
        {
            Dictionary<string, int> ret = new Dictionary<string, int>();
            string SQL = "select * from Page_rank;";
            using (SqlConnection sq = new SqlConnection(connection))
            {
                SqlCommand cmd = new SqlCommand(SQL, sq);
                cmd.CommandType = System.Data.CommandType.Text;
                sq.Open();

                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        string url = reader["url"].ToString();
                        if (!ret.ContainsKey(url))
                        {
                            ret.Add(url, (int) reader["P_rank"]);
                        }
                    }
                }
            }

            return ret;
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

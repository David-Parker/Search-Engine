using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using SearchBackend.Exceptions;

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

        // This method will get overloaded with complexity, but SqlConnection.Open() is too expensive to open seperately in multiple methods
        public void BulkInsert(string url, Dictionary<string, int> keywords, int pagerank, Dictionary<string,bool> ranks)
        {
            DataTable table = new DataTable();

            DataRow row;

            // URL Column
            DataColumn url_column;
            url_column = new DataColumn();
            url_column.DataType = Type.GetType("System.String");
            url_column.ColumnName = "url";
            table.Columns.Add(url_column);

            // Keyword Column
            DataColumn keyword_column;
            keyword_column = new DataColumn();
            keyword_column.DataType = Type.GetType("System.String");
            keyword_column.ColumnName = "keyword";
            table.Columns.Add(keyword_column);

            // K_count Column
            DataColumn k_count_column;
            k_count_column = new DataColumn();
            k_count_column.DataType = Type.GetType("System.Int32");
            k_count_column.ColumnName = "k_count";
            table.Columns.Add(k_count_column);

            // Date column
            DataColumn date_column;
            date_column = new DataColumn();
            date_column.DataType = Type.GetType("System.DateTime");
            date_column.ColumnName = "date";
            table.Columns.Add(date_column);

            // GUID column
            DataColumn guid_column;
            guid_column = new DataColumn();
            guid_column.DataType = Type.GetType("System.String");
            guid_column.ColumnName = "guid";
            table.Columns.Add(guid_column);

            foreach(var hash in keywords)
            {
                row = table.NewRow();
                row["url"] = url;
                row["keyword"] = hash.Key;
                row["k_count"] = hash.Value;
                row["date"] = DateTime.Now;
                row["guid"] = url + "_" + hash.Key;
                table.Rows.Add(row);
            }

            using (SqlConnection sq = new SqlConnection(connection))
            {
                sq.Open();

                // Add the pagerank for the current page
                this.AddRank(url, pagerank, sq);

                // Update ranks for other pages
                this.UpdateRanks(ranks, sq, url);

                using (SqlBulkCopy sbc = new SqlBulkCopy(sq))
                {
                    sbc.DestinationTableName = "dbo.Keywords";
                    sbc.WriteToServer(table);
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

        // Assumes that the connection is open
        private void UpdateRanks(Dictionary<string, bool> ranks, SqlConnection sq, string currentUrl)
        {
            if(sq.State == ConnectionState.Closed)
            {
                throw new SQLConnectionException("Connection cannot be closed in UpdateRanks.");
            }

            string SQL = "update page_rank set p_rank = p_rank + 1 where url = '{0}';";
            StringBuilder sb = new StringBuilder();

            foreach (var rank in ranks)
            {
                // Don't double dip for the current url that is being added
                if (rank.Key != currentUrl)
                {
                    sb.Append(string.Format(SQL, rank.Key));
                }
            }

            SQL = sb.ToString();

            // Don't run a query for an empty string
            if (SQL == "") return;

            SqlCommand cmd = new SqlCommand(SQL, sq);

            cmd.CommandType = System.Data.CommandType.Text;
            cmd.ExecuteNonQuery();
        }

        // Assumes that the connection is open
        private void AddRank(string url, int pagerank, SqlConnection sq)
        {
            if (sq.State == ConnectionState.Closed)
            {
                throw new SQLConnectionException("Connection cannot be closed in AddRank.");
            }

            string SQL = String.Format("INSERT INTO Page_Rank(url,P_rank) VALUES ('{0}', {1});", url, pagerank);

            SqlCommand cmd = new SqlCommand(SQL, sq);

            cmd.CommandType = System.Data.CommandType.Text;
            cmd.ExecuteNonQuery();
        }
    }
}

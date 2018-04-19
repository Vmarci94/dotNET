using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADOExample
{
    class Program : IDisposable
    {

        private static string CONN_STRING = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MyLitleDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private SqlConnection sqlConnection = null;

        Program() {
            sqlConnection = new SqlConnection(CONN_STRING);
        }


        static void Main(string[] args)
        {
            Program instance = null;
            try
            {
                instance = new Program();
                //instance.InsertShipper("Super Shipper", "49-98562");
                //instance.UpdateProducts(5, "Mosogatógép", 2000);
                instance.ListTabelAllRows("Products");
                //instance.GetShippers();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally {
                instance.Dispose(); //Vagy tegyük a Program példányosítását (és így az egészet) using blockba :)
            }

            Console.ReadKey();
        }

        public void ReconnectDatabase() {
            if(sqlConnection != null)
                sqlConnection.Close();
            sqlConnection.Open();
        }

        private void UpdateProducts(int productID, string productsName, int price) {
            /*
                https://docs.microsoft.com/en-us/sql/relational-databases/stored-procedures/view-the-definition-of-a-stored-procedure?view=sql-server-2017
             */
            using (SqlConnection connection = new SqlConnection(CONN_STRING)) //Példa kedvéért direkt újat hozok létre. A Using gondoskodik a Dispose/Close Hívásról
                using (SqlCommand command = new SqlCommand("Product_Update", connection)) {
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ProductID", productID);
                command.Parameters.AddWithValue("@ProductName", productsName);
                command.Parameters.AddWithValue("@UnitPrice", price);
                connection.Open();
                int updatedRows = command.ExecuteNonQuery();
                Console.WriteLine($"{updatedRows}, rows updated.");
            }
        }

        public void ListTabelAllRows(String table) {
            string SQLcommand = "SELECT * FROM " + table;
            using (SqlConnection connection = new SqlConnection(CONN_STRING))
                using (SqlCommand command = new SqlCommand(SQLcommand, connection)) {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            StringBuilder stringBuilder = new StringBuilder();
                            for (int i = 0; i < reader.VisibleFieldCount; ++i) {
                                stringBuilder.Append(reader[i] + ", ");
                            }
                            Console.WriteLine(stringBuilder);
                        }    
                    }
                }
        }

        private void GetShippers() {
            SqlDataReader reader = null;
            SqlCommand sqlCommand = null;
            try {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("SELECT ShipperID, CompanyName, Phone FROM Shippers", sqlConnection);
                Console.WriteLine("{0,-10}{1,-20}{2,-20}", "ShipperID", "CompanyName", "Phone");
                Console.WriteLine(new string('-', 60));
                reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    while (reader.Read())
                        Console.WriteLine($@"{
                            reader["ShipperID"],-10
                            }{
                            reader["CompanyName"],-20
                            }{ reader["Phone"],-20
                            }");
                }


            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
            finally{
                //Valójában a "Close" --> "Dispose"-t hív :)
                if (reader != null) reader.Close();
                sqlCommand.Dispose(); //ezt nem szabad elfelejteni meghívni!
            }
        }

        private void InsertShipper(string companyName, string phone) {
            try
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand /*A using megoldja a Dispose hívást, vagy a close-t. De pont ezért, az itt példányosító osztályoknak muszáj implementálniuk az IDispose interfészt*/
                    = new SqlCommand("INSERT INTO Shippers(CompanyName, Phone) VALUES(@name,@phone)", sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@name", companyName);
                    sqlCommand.Parameters.AddWithValue("@phone", phone);
                    int affectedRows = sqlCommand.ExecuteNonQuery();
                    Console.WriteLine($"{affectedRows} rows affected.");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally {
                if (sqlConnection != null) sqlConnection.Close(); //Ugye Dispose...

            }
        }

        public void Dispose()
        {
            if (sqlConnection != null) sqlConnection.Close();
        }
    }
}

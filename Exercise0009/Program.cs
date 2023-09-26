using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Exercise0009
{
    internal class Program
    {

        static string connectionString = @"Server=localhost\SQLEXPRESS;Database=orders;Trusted_Connection=True;";

        static void Main(string[] args)
        {
            if (Login())
            {
                return;
            }
            Menu();
        }

        static ConsoleKeyInfo GetInput()
        {
            var res = Console.ReadKey();
            Console.WriteLine();
            return res;
        }

        static void Menu()
        {
            while (true)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("0- Esci dal programma\n1- Cambia utente\n2- Crea utente\n3- Visualizza ordini\n4- Visualizza dettagli ordine\n5- Fai ordine");
                    switch(GetInput().KeyChar)
                    {
                        case '0': //return
                            return;

                        case '1': //login
                            Login();
                            break;

                        case '2': // crea utente
                            if (InsertUser())
                            {
                                Console.WriteLine("Utente creato con successo");
                            }
                            else
                            {
                                Console.WriteLine("Creazione Utente fallita");
                            }
                            Thread.Sleep(1000);
                            break;

                        case '3': // Visualizza ordine
                            Console.Clear();
                            WriteRecord(GetOrders());
                            Console.ReadKey();
                            break;

                        case '4': // Dettagli ordine
                            Console.Clear();
                            Console.WriteLine("Inserisci id");
                            try
                            {
                                var i = int.Parse(Console.ReadLine());
                                Console.Clear();
                                WriteOrderSpecifics(GetOrderSpecifics(i));
                            }
                            catch(FormatException)
                            {
                                Console.WriteLine("Valore inserito non è del tipo corretto");
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                Console.WriteLine("Valore inserito non è presente nel db");
                            }
                            Console.ReadKey();
                            break;

                        case '5': // Fai ordine
                            try
                            {
                                Console.WriteLine("Inserisci Customer");
                                var customer = GetCustomer(Console.ReadLine())["customer"];
                                bool continua = true;
                                while (continua) 
                                {

                                    
                                    while (continua)
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Vuoi continuare? (y/n)");
                                        switch(GetInput().Key)
                                        {
                                            case ConsoleKey.Y:
                                                break;
                                            case ConsoleKey.N:
                                                continua = false;
                                                break;
                                        }
                                    }
                                
                                }
                            }
                            catch(InvalidOperationException)
                            {
                                Console.WriteLine("Valore inserito non presente nel DB");
                            }

                            break;

                        default:
                            break;
                    }
                }
                catch
                {

                }
            }
        }

        static bool InsertUser()
        {
            bool res = true;

            try
            {
                using(var connection = new SqlConnection(connectionString))
                {

                    connection.Open();

                    #region Ask for input
                    Console.WriteLine("Inserisci Utente");
                    var user = new SqlParameter("@username", System.Data.SqlDbType.NVarChar,25);
                    user.Value = Console.ReadLine();

                    Console.WriteLine("Inserisci Password");
                    var psw = new SqlParameter("@psw", System.Data.SqlDbType.NVarChar, 25);
                    psw.Value = Console.ReadLine();
                    #endregion

                    #region Parametric query
                    var query = new SqlCommand("insert into users values(@username, @psw)", connection);

                    query.Parameters.Add(user);
                    query.Parameters.Add(psw);
                    #endregion

                    query.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                res = false;
            }

            return res;
        }

        static bool Login()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {

                    Console.Clear();

                    connection.Open();

                    {
                        var query = new SqlCommand("select count(*) from users", connection);

                        if ((int)query.ExecuteScalar() == 0)
                        {
                            Console.WriteLine("Inizializzazione utente admin");
                            query.CommandText = "insert into users values('admin', 'admin')";
                            query.ExecuteNonQuery();
                        }
                    }

                    while (true)
                    {

                        Console.Clear();

                        #region Ask for input
                        Console.WriteLine("Inserisci Utente");
                        var user = new SqlParameter("@user", Console.ReadLine());

                        Console.WriteLine("Inserisci Password");
                        var psw = new SqlParameter("@psw", Console.ReadLine());
                        #endregion

                        #region Parametric query
                        var query = new SqlCommand("select count(*) from users where username = @user and psw = @psw", connection);
                        query.Parameters.Add(user);
                        query.Parameters.Add(psw); 
                        #endregion

                        if ((int)query.ExecuteScalar() == 1)
                        {
                            Console.WriteLine("Login effettuato con successo");
                            Thread.Sleep(1000);
                            return false;
                        }
                        else
                        {
                            Console.WriteLine("Username o Password errati");
                            Thread.Sleep(1000);

                            bool x = true;
                            while (x)
                            {

                                Console.Clear();

                                Console.WriteLine("Vuoi continuare? (y/n)");
                                switch (GetInput().Key)
                                {
                                    case ConsoleKey.N:
                                        return true;
                                    
                                    case ConsoleKey.Y:
                                        {
                                            x = false; 
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return true;
            }
        }

        static List<Dictionary<string,object>> GetOrders()
        {
            var res = new List<Dictionary<string,object>>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = new SqlCommand("select a.*, sum(b.price*qty) as tot from orders as a\r\nleft join orderitems as b\r\n\ton a.orderid = b.orderid\r\ngroup by a.orderid, a.customer, a.orderdate", connection);

                using (var reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new Dictionary<string,object>();
                        item["id"] = reader["orderid"];
                        item["customer"] = reader["customer"];
                        item["order date"] = reader["orderdate"];
                        item["total"] = reader["tot"];
                        res.Add(item);
                    }
                }
            }

            return res;
        }

        static List<Dictionary<string, object>> GetOrderSpecifics(int id)
        {
            var res = new List<Dictionary<string, object>>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = new SqlCommand("select a.customer,a.orderdate,b.item,b.qty,b.price from orders as a left join orderitems as b on a.orderid = b.orderid where a.orderid = @orderid", connection);
                var param = new SqlParameter("@orderid", id);

                query.Parameters.Add(param);

                using (var reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new Dictionary<string, object>();
                        item["customer"] = reader["customer"];
                        item["order date"] = reader["orderdate"];
                        item["item"] = reader["item"];
                        item["quantity"] = reader["qty"];
                        item["price"] = reader["price"];
                        res.Add(item);
                    }
                }
            }

            return (res.Count > 0) ? res : throw new ArgumentOutOfRangeException();
        }

        static void WriteRecord(ICollection<Dictionary<string,object>> records)
        {
            foreach (var record in records)
            {
                foreach (var key in record.Keys)
                {
                    Console.WriteLine($"{key} = {record[key]}");
                }
                Console.WriteLine();
            }
        }

        static void WriteOrderSpecifics(ICollection<Dictionary<string, object>> records)
        {
            Console.WriteLine($"customer = {records.ElementAt(0)["customer"]}");
            Console.WriteLine($"order date = {records.ElementAt(0)["order date"]}");
            Console.WriteLine($"-----------------------------------------");

            foreach (var record in records)
            {
                Console.WriteLine($"item = {record["item"]}");
                Console.WriteLine($"quantity = {record["quantity"]}");
                Console.WriteLine($"price = {record["price"]}");
                Console.WriteLine();
            }
        }

        static Dictionary<string,object> GetCustomer(string customer)
        {
            var res = new Dictionary<string, object>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = new SqlCommand("select * from customers where customer = @customer", connection);

                query.Parameters.Add(new SqlParameter("@customer", customer));

                using(var reader = query.ExecuteReader())
                {
                    reader.Read();
                    res["customer"] = reader["customer"];
                    res["country"] = reader["country"];
                }
            }
            return res;
        }

        static Dictionary<string, object> GetItem(string item)
        {
            var res = new Dictionary<string, object>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var query = new SqlCommand("select * from items where item = @item", connection);

                query.Parameters.Add(new SqlParameter("@item", item));

                using (var reader = query.ExecuteReader())
                {
                    reader.Read();
                    res["item"] = reader["item"];
                    res["color"] = reader["color"];
                }
            }
            return res;
        }

        static void x(string connectionString, string table)
        {

            table = table.Replace(" ","");
            table = table.Replace(Environment.NewLine,"");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                var query = new SqlCommand($"select * from {table}", connection);
                
                using (var reader = query.ExecuteReader())
                {
                    var x = reader.GetSchemaTable();
                    foreach (var item in x.Columns)
                    {
                        Console.WriteLine(item);
                    }

                    Console.WriteLine();
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["customer"]}");
                    }
                }

            }
        }

    }
}

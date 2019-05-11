using HslCommunication;
using HslCommunication.Profinet.Siemens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace R_And_W_PLC
{
    public partial class Form_PLC : Form
    {
        public Form_PLC()
        {
            InitializeComponent();
            // 启动线程
            thread = new Thread(new ThreadStart(ReadMethod));
            thread.IsBackground = true;
            thread.Start();
        }
        private Thread thread;
        private bool readEnable = false;
        private bool writeEnable = false;
        SiemensS7Net plc = new SiemensS7Net(SiemensPLCS.S1200, "127.0.0.1");
        private void ReadMethod()
        {
            //SiemensS7Net siemens = new SiemensS7Net(SiemensPLCS.S1200, "127.0.0.1");
            plc.SetPersistentConnection();         // 设置长连接
            while (true)
            {
                Thread.Sleep(1000);  // 决定了一秒读1次
                if (writeEnable)
                {
                    Random rnd = new Random();
                    short wriNum = (short)rnd.Next();
                    // 启动了读操作
                    OperateResult write = plc.Write("M100", wriNum);
                    // 跨UI更新界面
                    Invoke(new Action(() =>
                    {
                        if (write.IsSuccess)
                        {
                            textBox2.Text = wriNum.ToString();


                        }
                        else
                        {
                            textBox2.Text = "写入失败";
                        }
                    }));
                    if (readEnable)
                {
                    // 启动了读操作
                    OperateResult<short> read = plc.ReadInt16("M100");
                    // 跨UI更新界面
                    Invoke(new Action(() =>
                    {
                        if (read.IsSuccess)
                        {
                            textBox1.Text = read.Content.ToString();
                        }
                        else
                        {
                            textBox1.Text = "读取失败";
                        }
                    }));

                }
                    //Thread.Sleep(1000);






                }


            }
        }

        
        private void Plc_read_Click(object sender, EventArgs e)
        {
               // 访问的是本机的服务器信息
            OperateResult<int> read = plc.ReadInt32("M100");                     // 对应PLC是 MW100
            if (read.IsSuccess)
            {
                textBox1.Text = read.Content.ToString();
            }
            else
            {
                MessageBox.Show("Read Failed:" + read.ToMessageShowString());     // 读取失败时显示的信息
            }
        }

        private void Write_plc_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();

            
            OperateResult write = plc.Write("M100", rnd.Next());
            textBox2.Text = rnd.Next().ToString();
            if (write.IsSuccess)
            {
               // MessageBox.Show("success");// success
            }
            else
            {
                MessageBox.Show("WRITE Failed:" + write.ToMessageShowString()); // failed
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            readEnable = true;
            writeEnable = true;
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            readEnable = false;
            writeEnable = false;
        }

        private void But_sql_Click(object sender, EventArgs e)
        {
            string connectionString = GetConnectionString();
            ConnectToData(connectionString);
    //        try
    //        {
    //            CreateCommand("select * from ning", "Data Source=SUYONGBIN;Initial Catalog=login;Integrated Security=True");
    //        }
    //        catch (Exception ex)
    //        {

    //            MessageBox.Show(ex.ToString());
    //        }
    //    }
    //    private static void CreateCommand(string queryString,
    //string connectionString)
    //    {
    //        using (SqlConnection connection = new SqlConnection(
    //                   connectionString))
    //        {
    //            SqlCommand command = new SqlCommand(queryString, connection);
    //            command.Connection.Open();
    //            command.ExecuteNonQuery();
    //        }
        }
        private static void ConnectToData(string connectionString)
        {

           // Create a SqlConnection to the Northwind database.
            using (SqlConnection connection =
                       new SqlConnection(connectionString))
            {
                //Create a SqlDataAdapter for the Suppliers table.
                SqlDataAdapter adapter = new SqlDataAdapter();

                // A table mapping names the DataTable.
                adapter.TableMappings.Add("Table", "Suppliers");

                // Open the connection.
                connection.Open();
                MessageBox.Show("The SqlConnection is open.");

                // Create a SqlCommand to retrieve Suppliers data.
                SqlCommand command = new SqlCommand(
                    "SELECT SupplierID, CompanyName FROM dbo.Suppliers;",
                    connection);
                command.CommandType = CommandType.Text;

                // Set the SqlDataAdapter's SelectCommand.
                adapter.SelectCommand = command;

                // Fill the DataSet.
                DataSet dataSet = new DataSet("Suppliers");
                adapter.Fill(dataSet);

                // Create a second Adapter and Command to get
                // the Products table, a child table of Suppliers. 
                SqlDataAdapter productsAdapter = new SqlDataAdapter();
                productsAdapter.TableMappings.Add("Table", "Products");

                SqlCommand productsCommand = new SqlCommand(
                    "SELECT ProductID, SupplierID FROM dbo.Products;",
                    connection);
                productsAdapter.SelectCommand = productsCommand;

                // Fill the DataSet.
                productsAdapter.Fill(dataSet);

                // Close the connection.
                connection.Close();
                MessageBox.Show("The SqlConnection is closed.");

                // Create a DataRelation to link the two tables
                // based on the SupplierID.
                DataColumn parentColumn =
                    dataSet.Tables["Suppliers"].Columns["SupplierID"];
                DataColumn childColumn =
                    dataSet.Tables["Products"].Columns["SupplierID"];
                DataRelation relation =
                    new System.Data.DataRelation("SuppliersProducts",
                    parentColumn, childColumn);
                dataSet.Relations.Add(relation);
                MessageBox.Show(
                    "The {0} DataRelation has been created.",
                    relation.RelationName);
            }
        }
        static private string GetConnectionString()
        {
            // To avoid storing the connection string in your code, 
            // you can retrieve it from a configuration file.
            return "Data Source=(local);Initial Catalog=login;"
                + "Integrated Security=SSPI";
        }
        public static SqlDataAdapter CreateCustomerAdapter(
    SqlConnection connection)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();

            // Create the SelectCommand.
            SqlCommand command = new SqlCommand("SELECT * FROM Customers " +
                "WHERE Country = @Country AND City = @City", connection);

            // Add the parameters for the SelectCommand.
            command.Parameters.Add("@Country", SqlDbType.NVarChar, 15);
            command.Parameters.Add("@City", SqlDbType.NVarChar, 15);

            adapter.SelectCommand = command;

            // Create the InsertCommand.
            command = new SqlCommand(
                "INSERT INTO Customers (CustomerID, CompanyName) " +
                "VALUES (@CustomerID, @CompanyName)", connection);

            // Add the parameters for the InsertCommand.
            command.Parameters.Add("@CustomerID", SqlDbType.NChar, 5, "CustomerID");
            command.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 40, "CompanyName");

            adapter.InsertCommand = command;

            // Create the UpdateCommand.
            command = new SqlCommand(
                "UPDATE Customers SET CustomerID = @CustomerID, CompanyName = @CompanyName " +
                "WHERE CustomerID = @oldCustomerID", connection);

            // Add the parameters for the UpdateCommand.
            command.Parameters.Add("@CustomerID", SqlDbType.NChar, 5, "CustomerID");
            command.Parameters.Add("@CompanyName", SqlDbType.NVarChar, 40, "CompanyName");
            SqlParameter parameter = command.Parameters.Add(
                "@oldCustomerID", SqlDbType.NChar, 5, "CustomerID");
            parameter.SourceVersion = DataRowVersion.Original;

            adapter.UpdateCommand = command;

            // Create the DeleteCommand.
            command = new SqlCommand(
                "DELETE FROM Customers WHERE CustomerID = @CustomerID", connection);

            // Add the parameters for the DeleteCommand.
            parameter = command.Parameters.Add(
                "@CustomerID", SqlDbType.NChar, 5, "CustomerID");
            parameter.SourceVersion = DataRowVersion.Original;

            adapter.DeleteCommand = command;

            return adapter;
        }
        private static DataSet SelectRows(DataSet dataset,
    string connectionString, string queryString)
        {
            using (SqlConnection connection =
                new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = new SqlCommand(
                    queryString, connection);
                adapter.Fill(dataset);
                return dataset;
            }
        }
        private void Form_PLC_Load(object sender, EventArgs e)
        {
            // TODO: 这行代码将数据加载到表“loginDataSet.Products”中。您可以根据需要移动或删除它。
            this.productsTableAdapter.Fill(this.loginDataSet.Products);
            // TODO: 这行代码将数据加载到表“loginDataSet.Suppliers”中。您可以根据需要移动或删除它。
            this.suppliersTableAdapter.Fill(this.loginDataSet.Suppliers);

        }
    }
}

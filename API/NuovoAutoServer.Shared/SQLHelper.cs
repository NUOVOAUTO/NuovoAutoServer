using Microsoft.Data.SqlClient;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Shared
{
    public sealed class SqlHelper
    {


        // Since this class provides only static methods, make the default constructor private to prevent
        // instances from being created with "new SqlHelper()"
        public SqlHelper() { }

        private static string _connectionString = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");


        /// <summary>
        /// This method is used to attach array of SqlParameters to a SqlCommand.
        ///
        /// This method will assign a value of DbNull to any parameter with a direction of
        /// InputOutput and a value of null.
        ///
        /// This behavior will prevent default values from being used, but
        /// this will be the less common case than an intended pure output parameter (derived as InputOutput)
        /// where the user provided no input value.
        /// </summary>
        /// <param name="command">The command to which the parameters will be added</param>
        /// <param name="commandParameters">An array of SqlParameters to be added to command</param>
        public static void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (SqlParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        // Check for derived output value with no value assigned
                        if ((p.Direction == ParameterDirection.InputOutput ||
                            p.Direction == ParameterDirection.Input) &&
                            (p.Value == null))
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }


        public static async Task<string> ExecuteStoreProcedureAsync(string commandText, Dictionary<string, object> parameters, int timeout = 0)
        {
            string jsonResult = string.Empty;
            SqlDataReader dataReader;
            IList<SqlParameter> sqlParameters = new List<SqlParameter>();

            if (parameters != null)
            {
                foreach (var item in parameters)
                {
                    sqlParameters.Add(new SqlParameter(item.Key, item.Value));
                }
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand())
                {
                    if (timeout > 0)
                        command.CommandTimeout = timeout;

                    connection.Open();
                    // Associate the connection with the command
                    command.Connection = connection;
                    // Set the command text (stored procedure name or SQL statement)
                    command.CommandText = commandText;
                    // Set the command type
                    command.CommandType = CommandType.StoredProcedure;
                    // Attach the command parameters if they are provided
                    if (sqlParameters.Any())
                    {
                        AttachParameters(command, sqlParameters.ToArray());
                    }

                    dataReader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);


                    dataReader.Read();
                    if (dataReader.HasRows)
                        jsonResult = dataReader[0].ToString();


                    // Detach the SqlParameters from the command object, so they can be used again.
                    bool canClear = true;
                    foreach (SqlParameter commandParameter in command.Parameters)
                    {
                        if (commandParameter.Direction != ParameterDirection.Input)
                            canClear = false;
                    }

                    if (canClear)
                    {
                        command.Parameters.Clear();
                    }
                    connection.Close();
                }
            }

            return jsonResult;
        }

        public static async Task<DataTableCollection> ExecuteStoreProcedureAsync<T>(string commandText, Dictionary<string, object> parameters) where T : DataTable
        {
            DataSet dataset;
            DataTableCollection dataTableCollection;

            IList<SqlParameter> sqlParameters = new List<SqlParameter>();
            foreach (var item in parameters)
            {
                sqlParameters.Add(new SqlParameter(item.Key, item.Value));
            }

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Create a command and prepare it for execution
                using (SqlCommand command = new SqlCommand())
                {
                    connection.Open();
                    // Associate the connection with the command
                    command.Connection = connection;
                    // Set the command text (stored procedure name or SQL statement)
                    command.CommandText = commandText;
                    // Set the command type
                    command.CommandType = CommandType.StoredProcedure;
                    // Attach the command parameters if they are provided
                    if (sqlParameters.Any())
                    {
                        AttachParameters(command, sqlParameters.ToArray());
                    }
                    // Create the DataAdapter & DataSet
                    using (SqlDataAdapter da = new SqlDataAdapter(command))
                    {
                        dataset = new DataSet();
                        // Fill the DataSet using default values for DataTable names, etc
                        da.Fill(dataset);

                        // *** CUSTOM PROCESSING CODE BEGIN
                        dataTableCollection = dataset.Tables;
                        // *** CUSTOM PROCESSING CODE END

                        // Detach the SqlParameters from the command object, so they can be used again
                        command.Parameters.Clear();

                        connection.Close();
                    }
                }
            }

            return dataTableCollection;
        }

    }
}

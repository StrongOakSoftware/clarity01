using Clarity01.Data;
using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SQLitePCL;
using System.Dynamic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Clarity01.Components.Pages
{
    public partial class Display_Data_01
    {
        [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = default!;

        public List<string> columnNames = new List<string>();
        public IEnumerable<dynamic> results = new List<dynamic>();
        public List<Dictionary<string, object>> rowsAsDictionaries = new List<Dictionary<string, object>>();

        public Dictionary<string, object>? clickedItem = new Dictionary<string, object>()
        {
            { "Id", 0 }
        };

        public bool EditMode { get; set; } = false;


        protected override async Task OnInitializedAsync()
        {
            // 1. Create a context instance using the factory
            using var context = await DbFactory.CreateDbContextAsync();

            var connection = context.Database.GetDbConnection();

            columnNames = await GetColumnNames(connection);
            rowsAsDictionaries = await GetRowsAsDictionaries(connection);

            // 2. Call Dapper on that connection


            //var items = new List<string> { "Id", "Field2", "Field3", "Field4" };
            //var dynamicList = items.Select(item =>
            //{
            //    dynamic exp = new ExpandoObject();
            //        exp.Name = item;                
            //    return (ExpandoObject)exp;
            //}).ToList();

            // 2. Load the data asynchronously into your list

        }

        private async Task<List<string>> GetColumnNames(System.Data.Common.DbConnection connection)
        {
            var pragmaResults = await connection.QueryAsync("PRAGMA table_info(table_1_data);");

            // Extract just the 'name' property from each DapperRow
            List<string> columnNames = pragmaResults.Select(row => (string)row.name).ToList();

            return columnNames;
        }

        private async Task<List<Dictionary<string, object>>> GetRowsAsDictionaries(System.Data.Common.DbConnection connection)
        {
            // Added 'var' here assuming 'results' wasn't declared at the class level
            var results = await connection.QueryAsync("SELECT * FROM Table_1_Data");
            var rowsList = results.ToList();

            // 1. Create a list to hold all the row dictionaries
            var listOfDictionaries = new List<Dictionary<string, object>>();

            foreach (var row in rowsList)
            {
                // 2. Cast the Dapper row to an IDictionary
                var rowDict = (IDictionary<string, object>)row;

                // 3. Create a new concrete Dictionary from the Dapper dictionary 
                // and add it to our list
                listOfDictionaries.Add(new Dictionary<string, object>(rowDict));
            }

            // 4. Return the populated list
            return listOfDictionaries;
        }
        private void HandleRowClick(Dictionary<string, object> clickedRow)
        {
            // Your logic here
            // Example: var id = clickedRow["Id"];
            clickedItem = clickedRow;
            EditMode = true;
        }

        private async Task HandleValidSubmit() {
            using var context = await DbFactory.CreateDbContextAsync();

            var connection = context.Database.GetDbConnection();

            // 1. Separate the ID from the updates
            int id = clickedItem.Where(kvp => kvp.Key == "Id").Select(kvp => Convert.ToInt32(kvp.Value)).FirstOrDefault();
            var fieldsToUpdate = clickedItem.Where(kvp => kvp.Key != "Id");
            

            // 2. Build the SQL string: "SET Field1 = @Field1, Field2 = @Field2..."
            var setClause = string.Join(", ", fieldsToUpdate.Select(kvp => $"{kvp.Key} = @{kvp.Key}"));
            var sql = $"UPDATE Table_1_Data SET {setClause} WHERE Id = @Id";

            // 3. Add parameters dynamically
            var parameters = new DynamicParameters();
            parameters.Add("Id", id);
            foreach (var kvp in fieldsToUpdate)
            {
                parameters.Add(kvp.Key, kvp.Value);
            }

            // 4. Execute
            await connection.ExecuteAsync(sql, parameters);
            EditMode = false;

            StateHasChanged();
        }

    }

}

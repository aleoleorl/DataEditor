using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DataEditor.Models;
using DataEditor.Models.Enums;

namespace DataEditor.Support
{
    public class DatabaseHelper
    {
        private const string DatabaseFileName = "database.db";
        private const string ConnectionString = $"Data Source={DatabaseFileName}";

        public static void InitializeDatabase()
        {
            if (!File.Exists(DatabaseFileName))
            {
                CreateDatabase();
            }
        }

        private static void CreateDatabase()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var createModesTable = @"
                CREATE TABLE Modes (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    MaxBottleNumber INTEGER,
                    MaxUsedTips INTEGER
                );";

                var createStepsTable = @"
                CREATE TABLE Steps (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    ModeId INTEGER,
                    Timer INTEGER,
                    Destination TEXT,
                    Speed INTEGER,
                    Type TEXT,
                    Volume INTEGER,
                    FOREIGN KEY (ModeId) REFERENCES Modes(ID)
                );";

                var createUsersTable = @"
                CREATE TABLE Users (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Password TEXT NOT NULL
                );";

                using (var command = new SqliteCommand(createModesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SqliteCommand(createStepsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SqliteCommand(createUsersTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(ConnectionString);
        }

        public static ObservableCollection<Mode> LoadModes()
        {
            var modes = new ObservableCollection<Mode>();

            using (var connection = GetConnection())
            {
                connection.Open();
                var selectCommand = "SELECT ID, Name, MaxBottleNumber, MaxUsedTips FROM Modes";
                using (var command = new SqliteCommand(selectCommand, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var mode = new Mode
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            MaxBottleNumber = reader.GetInt32(2),
                            MaxUsedTips = reader.GetInt32(3)
                        };
                        modes.Add(mode);
                    }
                }
            }

            using (var connection = GetConnection())
            {
                connection.Open();
                var selectCommand = "SELECT MAX(ID) FROM Modes";
                using (var command = new SqliteCommand(selectCommand, connection))
                {
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        Mode.SetNextID(Convert.ToInt32(result) + 1);
                    }
                    else
                    {
                        Mode.SetNextID(1);
                    }
                }
            }

            return modes;
        }

        public static void SaveModeChanges(Mode item, DBOperation operation)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    if (operation == DBOperation.New)
                    {
                        AddNewRowToTable(connection, item, transaction);
                    }
                    else if (operation == DBOperation.Delete)
                    {
                        DeleteRowFromTable(connection, item, transaction);
                    }
                    else if (operation == DBOperation.Modify)
                    {
                        UpdateRowInTable(connection, item, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        private static void AddNewRowToTable(SqliteConnection connection, Mode item, SqliteTransaction transaction)
        {
            var insertCommand = @"
                INSERT INTO Modes (Name, MaxBottleNumber, MaxUsedTips)
                VALUES (@Name, @MaxBottleNumber, @MaxUsedTips)"
            ;

            using (var command = new SqliteCommand(insertCommand, connection, transaction))
            {
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@MaxBottleNumber", item.MaxBottleNumber);
                command.Parameters.AddWithValue("@MaxUsedTips", item.MaxUsedTips);
                command.ExecuteNonQuery();
            }
        }

        private static void DeleteRowFromTable(SqliteConnection connection, Mode item, SqliteTransaction transaction)
        {
            var deleteCommand = "DELETE FROM Modes WHERE ID = @ID";

            using (var command = new SqliteCommand(deleteCommand, connection, transaction))
            {
                command.Parameters.AddWithValue("@ID", item.ID);
                command.ExecuteNonQuery();
            }
        }

        private static void UpdateRowInTable(SqliteConnection connection, Mode item, SqliteTransaction transaction)
        {
            var updateCommand = @"
                UPDATE Modes
                SET Name = @Name, MaxBottleNumber = @MaxBottleNumber, MaxUsedTips = @MaxUsedTips
                WHERE ID = @ID";

            using (var command = new SqliteCommand(updateCommand, connection, transaction))
            {
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@MaxBottleNumber", item.MaxBottleNumber);
                command.Parameters.AddWithValue("@MaxUsedTips", item.MaxUsedTips);
                command.Parameters.AddWithValue("@ID", item.ID);
                command.ExecuteNonQuery();
            }
        }


        public static ObservableCollection<Step> LoadSteps()
        {
            var steps = new ObservableCollection<Step>();

            using (var connection = GetConnection())
            {
                connection.Open();
                var selectCommand = "SELECT ID, ModeId, Timer, Destination, Speed, Type, Volume FROM Steps";
                using (var command = new SqliteCommand(selectCommand, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var step = new Step
                        {
                            ID = reader.GetInt32(0),
                            ModeId = reader.GetInt32(1),
                            Timer = reader.GetInt32(2),
                            Destination = reader.GetString(3),
                            Speed = reader.GetInt32(4),
                            Type = reader.GetString(5),
                            Volume = reader.GetInt32(6)
                        };
                        steps.Add(step);
                    }
                }
            }

            using (var connection = GetConnection())
            {
                connection.Open();
                var selectCommand = "SELECT MAX(ID) FROM Steps";
                using (var command = new SqliteCommand(selectCommand, connection))
                {
                    var result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        Step.SetNextID(Convert.ToInt32(result) + 1);
                    }
                    else
                    {
                        Step.SetNextID(1);
                    }
                }
            }

            return steps;
        }

        public static void SaveStepChanges(Step item, DBOperation operation)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    if (operation == DBOperation.New)
                    {
                        AddNewRowToTableStep(connection, item, transaction);
                    }
                    else if (operation == DBOperation.Delete)
                    {
                        DeleteRowFromTableStep(connection, item, transaction);
                    }
                    else if (operation == DBOperation.Modify)
                    {
                        UpdateRowInTableStep(connection, item, transaction);
                    }

                    transaction.Commit();
                }
            }
        }

        private static void AddNewRowToTableStep(SqliteConnection connection, Step item, SqliteTransaction transaction)
        {
            var insertCommand = @"
                INSERT INTO Steps (ModeId, Timer, Destination, Speed, Type, Volume)
                VALUES (@ModeId, @Timer, @Destination, @Speed, @Type, @Volume)";

            using (var command = new SqliteCommand(insertCommand, connection, transaction))
            {
                command.Parameters.AddWithValue("@ModeId", item.ModeId);
                command.Parameters.AddWithValue("@Timer", item.Timer);
                command.Parameters.AddWithValue("@Destination", item.Destination);
                command.Parameters.AddWithValue("@Speed", item.Speed);
                command.Parameters.AddWithValue("@Type", item.Type);
                command.Parameters.AddWithValue("@Volume", item.Volume);
                command.ExecuteNonQuery();
            }
        }

        private static void DeleteRowFromTableStep(SqliteConnection connection, Step item, SqliteTransaction transaction)
        {
            var deleteCommand = "DELETE FROM Steps WHERE ID = @ID";

            using (var command = new SqliteCommand(deleteCommand, connection, transaction))
            {
                command.Parameters.AddWithValue("@ID", item.ID);
                command.ExecuteNonQuery();
            }
        }

        private static void UpdateRowInTableStep(SqliteConnection connection, Step item, SqliteTransaction transaction)
        {
            var updateCommand = @"
                UPDATE Steps
                SET ModeId = @ModeId, Timer = @Timer, Destination = @Destination, Speed = @Speed, Type = @Type, Volume = @Volume
                WHERE ID = @ID";

            using (var command = new SqliteCommand(updateCommand, connection, transaction))
            {
                command.Parameters.AddWithValue("@ModeId", item.ModeId);
                command.Parameters.AddWithValue("@Timer", item.Timer);
                command.Parameters.AddWithValue("@Destination", item.Destination);
                command.Parameters.AddWithValue("@Speed", item.Speed);
                command.Parameters.AddWithValue("@Type", item.Type);
                command.Parameters.AddWithValue("@Volume", item.Volume);
                command.Parameters.AddWithValue("@ID", item.ID);
                command.ExecuteNonQuery();
            }
        }

        public static LoginResult ValidateUser(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                return LoginResult.InvalidInput;
            }
                        
            using (var connection = GetConnection())
            {
                connection.Open();
                var selectUserCommand = "SELECT Password FROM Users WHERE Name = @Name";
                using (var command = new SqliteCommand(selectUserCommand, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    var result = command.ExecuteScalar();

                    if (result == null)
                    {
                        return LoginResult.NoUser;
                    }

                    var storedPassword = result.ToString();
                    var hashedPassword = PasswordHelper.HashPassword(password);
                    if (storedPassword == hashedPassword)
                    {
                        return LoginResult.OK;
                    }
                    else
                    {
                        return LoginResult.WrongPassword;
                    }
                }
            }
        }
        public static bool IsValidPassword(string password)
        {
            if (password.Length < 6)
                return false;

            bool hasLetter = false;
            bool hasDigit = false;

            foreach (char c in password)
            {
                if (char.IsLetter(c))
                    hasLetter = true;
                if (char.IsDigit(c))
                    hasDigit = true;
            }

            return hasLetter && hasDigit;
        }

        public static bool CreateUser(string name, string password)
        {
            var hashedPassword = PasswordHelper.HashPassword(password);

            using (var connection = GetConnection())
            {
                connection.Open();
                var selectCommand = "SELECT COUNT(*) FROM Users WHERE Name = @Name";
                using (var command = new SqliteCommand(selectCommand, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    var count = Convert.ToInt32(command.ExecuteScalar());
                    if (count > 0)
                    {
                        return false;
                    }
                }

                var insertCommand = "INSERT INTO Users (Name, Password) VALUES (@Name, @Password)";
                using (var command = new SqliteCommand(insertCommand, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.ExecuteNonQuery();
                    return true;
                }
            }
        }
    }
}
using Npgsql;
using System.Data;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace General;


/// <summary>
/// # Кратко о классах в этом файле
/// 
/// * DbHelperForUsers -- это класс для управления базой данных для заданий, где нужно добавить пользователя или вести учет пользователей (добавлять дату, время входа выхода)
///     Вроде в этом классе есть все методы для заданий такого типа. Если что можно добавить еще, опираясь на другие методы. 
///     
/// * DbHelperForCalc -- это класс для управления базой данных для заданий, где нужно сделать вычисление и сохранить, полученный результат в бд 
/// 
/// # Почему классы имеют приписку static?
/// Так мы запрещаем создавать еще одни класс для работы с бд и реализуем часть паттерна Singleton
/// 
/// </summary>

public static class DbHelperForUsers
{
    private static string connString = "";      // строка пустая, т.к. при еще не вызванном методе CreateDataBase (т.е. в еще не созданной базе данных) можно вызывать другие методы типа AddUser



    public static void CreateDataBase(string dbname)
    {
        connString = "Server=localhost;Port=5432;User Id=postgres;Password=admin;Database=postgres;";

        // Используем using, т.к. не хотим вручную закрывать соединения ,команды и т.д.
        // EXPL: Здесь мы создаем базу данных, если она еще не создана. Если создана, то просто вылетает ошибка, которая никак не влияет на работу программы
        using (var connection = new NpgsqlConnection(connString))
        {
            // EXPL: Комадна в строковом формате, которую потом передадим для выполнения бдшке
            using (var cmd = new NpgsqlCommand($"" +
                $"CREATE DATABASE {dbname} " +
                $"WITH OWNER = postgres " +
                $"ENCODING = 'UTF-8' " +
                $"CONNECTION LIMIT = -1;" +
                $"", connection))
            {
                connection.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Npgsql.PostgresException)
                { Debug.WriteLine("Все хорошо, бд уже создана"); }

            }
            connection.Close();
        }

        // EXPL: Данные, по которым мы подключаемся к базе данных
        connString = $"Server=localhost;Port=5432;User Id=postgres;Password=admin;Database={dbname};";

        // EXPL: Создаем две таблицы users, visitinfo (по названию таблицы понятно, что они делают)
        using (var connection = new NpgsqlConnection(connString))
        {
            connection.Open();

            // думаю как-то переделать эту бдшку
            string tableCommand1 = "CREATE TABLE IF NOT EXISTS users" +
                "(" +
                "name varchar(50) NOT NULL," +
                "login varchar(50) NOT NULL PRIMARY KEY," +
                "password varchar(50)" +
                ");";

            string tableCommand2 = "CREATE TABLE IF NOT EXISTS visitinfo" +
                "(" +
                "id serial PRIMARY KEY NOT NULL," +
                "login varchar(50) REFERENCES users (login)," +
                "logintime timestamp," +
                "exittime timestamp" +
                ");";

            using (var cmd = new NpgsqlCommand(tableCommand1 + tableCommand2, connection))
            {
                cmd.ExecuteNonQuery();
            }

            connection.Close();
        }
    }


    public static bool AddUser(User user)                                  // TODO: Rename to Add or AddToDb
    {
        // EXPL: Если существует, то ...
        if (IsExisting(user))
            return false;


        using (var connection = new NpgsqlConnection(connString))
        {
            // QUESTION: Что такое @ в команде снизу?
            string insertCommand = "INSERT INTO users(name, login, password) VALUES (@name, @login, @password)";

            using (var cmd = new NpgsqlCommand(insertCommand, connection))
            {
                connection.Open();

                // ЧТо-то связанное с @
                cmd.Parameters.AddWithValue("name", user.Name);
                cmd.Parameters.AddWithValue("login", user.Login);
                cmd.Parameters.AddWithValue("password", user.Password);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Npgsql.PostgresException)
                {
                    Debug.WriteLine($"Что-то случилось с базой данной при добавлении нового пользователя {user.Name} с логином {user.Login}");
                    return false;
                }
            }

            connection.Close();
        }
        return true;
    }

    // EXPL: Добавляем в бд информацию о времени входа пользователя
    public static void AddVisitInfo(User user, DateTime logintime, DateTime exittime)
    {
        // EXPL: ...
        if (!IsExisting(user)) return;

        using (var connection = new NpgsqlConnection(connString))
        {
            connection.Open();

            // QUESTION: ...?
            var cmd = new NpgsqlCommand("INSERT INTO visitinfo(login, logintime, exittime) " +
                                        "VALUES (@login, @logintime, @exittime)", connection);

            cmd.Parameters.AddWithValue("login", user.Login);
            cmd.Parameters.AddWithValue("logintime", logintime);
            cmd.Parameters.AddWithValue("exittime", exittime);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Npgsql.PostgresException)
            {
                Debug.WriteLine("Что-то случилось с базой данных при добавлении информации о входе и выходе пользователя из приложения");
            }

            connection.Close();
        }
    }

    public static bool IsExisting(User user)
    {
        //string res = "";
        bool res = false;

        using (var connection = new NpgsqlConnection(connString))
        {
            connection.Open();


            // EXPL: ВМессто @ можно использовать ф строки (как в питоне). Так менее безопастно
            string selectcmd = $"SELECT * FROM users u " +
                $"WHERE u.login = '{user.Login}' AND u.password = '{user.Password}'";

            var cmd = new NpgsqlCommand(selectcmd, connection);

            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res = reader.HasRows;
                        //res = reader.GetString(0);
                    }
                }
            }
            catch (Npgsql.PostgresException)
            {
                Debug.WriteLine("Что-то случилось с базой данной при проверке на наличие пользователя в базе данных");
            }

            connection.Close();
        }


        //return !string.IsNullOrEmpty(res);
        return res;
    }


    public static User GetUserByLogin(string login)
    {
        User user = new();  // Создание пользователя

        using (var connection = new NpgsqlConnection(connString))
        {
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT * from users u WHERE u.login = '{login}'";

            try
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // EXPL: Пересоздание пользователя, но уже с заполнеными данными
                        user = new User
                        {
                            Name = reader.GetString(0),
                            Login = (string)reader["login"],
                            Password = reader.GetString(reader.GetOrdinal("password"))
                        };
                    }
                }
            }
            catch (Npgsql.PostgresException) { }


            connection.Close();
        }


        return user;
    }
}

// ===================================================================================================

public static class DbHelperForCalc
{
    private static string connString = "";



    public static void CreateDataBase(string dbname)
    {
        connString = "Server=localhost;Port=5432;User Id=postgres;Password=admin;Database=postgres;";

        using (var connection = new NpgsqlConnection(connString))
        {
            using (var cmd = new NpgsqlCommand($"" +
                $"CREATE DATABASE {dbname} " +
                $"WITH OWNER = postgres " +
                $"ENCODING = 'UTF-8' " +
                $"CONNECTION LIMIT = -1;" +
                $"", connection))
            {
                connection.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Npgsql.PostgresException)
                { Debug.WriteLine("Все хорошо, бд уже создана"); }

            }
        }

        connString = $"Server=localhost;Port=5432;User Id=postgres;Password=admin;Database={dbname};";

        using (var connection = new NpgsqlConnection(connString))
        {
            connection.Open();

            string tableCommand = "CREATE TABLE IF NOT EXISTS results " +
                "(" +
                "id serial PRIMARY KEY NOT NULL," +
                "a double precision NOT NULL," +
                "b double precision NOT NULL," +
                "result double precision NOT NULL" +
                ");";


            using (var cmd = new NpgsqlCommand(tableCommand, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }


    public static bool SaveResult(double a, double b, double result)
    {
        // сделать в виде bool
        if (string.IsNullOrEmpty(connString))
            return false;

        using (var connection = new NpgsqlConnection(connString))
        {
            connection.Open();

            string insertcmd = "INSERT INTO results (a, b, result) VALUES (@a, @b, @result)";

            using (var cmd = new NpgsqlCommand(insertcmd, connection))
            {
                cmd.Parameters.AddWithValue("a", a);
                cmd.Parameters.AddWithValue("b", b);
                cmd.Parameters.AddWithValue("result", result);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Npgsql.PostgresException)
                {
                    Debug.WriteLine("Что-то случилось при сохранении результата");
                    return false;
                }

            }
        }

        return true;
    }
}
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Tires
{
    class Program
    {
        static bool isAuth = false;
        static string path = "";
        static Memento memento;
        static void Main(string[] args)
        {
            try
            {
                hangeState();
                while (login())
                {
                    database();
                    options();
                }
            }
            catch (Exception ex)
            {
                print($"Błąd aplikacji: \n{ex.Message}", ConsoleColor.Red);
            }
            Console.ReadLine();
        }

        //Metody pomocnicze w aplikacji klinckiej
        #region Helpers
        //State
        static void hangeState()
        {
            Context c = new Context(new BigState());
            c.Change(); // Ustawia wielkość konsoli na starcie na BigState
            print("----Aby zmienić rozmiar konsoli [M], jeśli chcesz przejść dalej wciśnij inny klawisz.");
            var readKey = new ConsoleKey(); 
            while (readKey != ConsoleKey.Enter)
            {
                readKey = decisionKey();
                switch (readKey)
                {
                    case ConsoleKey.M:
                        c.Change(); // Zmienia stan
                        break;                 
                    default:
                        return; 
                }
            }
        }
        //Memento
        static void revertFromMemento()
        {
            if (memento != null && File.Exists(path))
            {
                try
                {
                    File.WriteAllBytes(path, memento.Data);
                    print();
                    print("----Przywrócono poprzedni stan bazy----", ConsoleColor.Green);
                }
                catch (Exception ex)
                {
                    print(ex.Message, ConsoleColor.Red);
                }

            }
        }
        //Metody print z użyciem dekoratora
        static void print(string message = "", ConsoleColor color = ConsoleColor.White)
        {
            switch (color)
            {
                case ConsoleColor.Green:
                    new PrinterDecorated(color).Print(message);
                    break;
                case ConsoleColor.Red:
                    new PrinterDecorated(color).Print(message);
                    break;
                case ConsoleColor.Blue:
                    new PrinterDecorated(color).Print(message);
                    break;
                case ConsoleColor.White:
                    new Printer().Print(message);
                    break;
                default:
                    new Printer().Print("----Nieobsługiwany kolor!----");
                    break;
            }
        }
        static void print(List<string[]> list, ConsoleColor color = ConsoleColor.White)
        {
            switch (color)
            {
                case ConsoleColor.Green:
                    new PrinterDecorated(color).Print(list);
                    break;
                case ConsoleColor.Red:
                    new PrinterDecorated(color).Print(list);
                    break;
                case ConsoleColor.Blue:
                    new PrinterDecorated(color).Print(list);
                    break;
                case ConsoleColor.White:
                    new Printer().Print(list);
                    break;
                default:
                    new Printer().Print("----Nieobsługiwany kolor!----");
                    break;
            }
        }
        private static ConsoleKey decisionKey()
        {
            ConsoleKey readKey = Console.ReadKey().Key;
            Console.Write("\b \b");
            return readKey;
        }
        private static bool login()
        {
            while (!isAuth)
            {
                print("----Podaj nazwę użytkownika----");
                var name = Console.ReadLine();
                print("----Podaj hasło oraz wciśnij Enter----");
                var pwd = getPassword();
                // Sprawdzamy dostęp za pomocą Proxy
                isAuth = new ProxyPromisions(name, pwd).IsAuthenticated();
                if (isAuth)
                {
                    print("----Logowanie OK----", ConsoleColor.Green);
                    break;
                }
                else
                {
                    print("----Błędny login lub hasło, spróbuj ponownie----", ConsoleColor.Red);
                }
            }
            return isAuth;
        }
        private static void logout()
        {
            isAuth = false;
            path = "";
            //Singleton usuwamy instancję, aby przy logowaniu nadać nowe uprawnienia.
            UserType.RemoveInstance();
            //Memento czyścimy obiekt
            memento = null;
            print();
            print("----Wylogowano----", ConsoleColor.Green);
            print();
            Main(null); // Powrót do początku.
        }

        //Pomocnik dla hasła, gwiazdki zamiast znaków itp.
        static string getPassword()
        {
            string pwd = "";
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    print();
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.Substring(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    pwd += i.KeyChar;
                    Console.Write("*");
                }
            }
            return pwd;
        }
        // Obsługa bazy danych
        private static void database()
        {
            if (isAuth)
            {
                print();
                print("----Baza danych:");
                print();
                if (UserType.GetUserType().IsAdmin) //Tylko admin może tworzyć bazę, uprawnienia istnieją w Singletonie
                    print("----Utwórz nową--------[A]");
                print("----Otwórz istniejącą--[B]");
                print();
                var readKey = decisionKey();
                switch (readKey)
                {
                    case ConsoleKey.A:
                        createDB();
                        break;
                    case ConsoleKey.B:
                        openDB();
                        break;
                    case ConsoleKey.Escape:
                        logout();
                        break;
                    default:
                        print("Wybrano nieobsługiwany przycisk.", ConsoleColor.Red);
                        break;
                }
                if (path != "" && File.Exists(path))
                {
                    memento = new Memento(path, File.ReadAllBytes(path)); // Pierwsza inicjacja memento
                }
            }
        }
        //Pomocnik do otwierania bazy
        private static void openDB()
        {
            if (isAuth)
            {
                while (path.Length == 0)
                {
                    print("----Podaj ścieżkę----");
                    path = Console.ReadLine();
                    if (!File.Exists(path))
                    {
                        path = "";
                        print("----Plik nie istnieje lub nie masz dostępu. Wróć [Backspace], wyloguj [Esc]");
                        var readKey = decisionKey();
                        switch (readKey)
                        {
                            case ConsoleKey.Backspace:
                                database();
                                break;
                            case ConsoleKey.Escape:
                                logout();
                                break;
                            default:
                                print("Wybrano nieobsługiwany przycisk.", ConsoleColor.Red);
                                break;
                        }
                    }
                    else if (!path.EndsWith(".csv") && !path.EndsWith(".db"))
                    {
                        path = "";
                        print("----Nieobsługiwany typ bazy.", ConsoleColor.Red);
                    }
                    else
                        print($"----Zapamiętano ścieżkę {path}", ConsoleColor.Green);
                }
            }
        }
        //Pomocnik do tworzenia bazy - używa Buildera
        private static void createDB()
        {
            if (isAuth)
            {
                if (UserType.GetUserType().IsAdmin)
                {
                    print("----Jeśli chcesz wprowadzić własną ścieżkę wciśnij [D], jeśli chcesz pozostawić domyślną ścieżkę wciśnij [F]");
                    var readKey = decisionKey();
                    BuildDirector director = null;
                    switch (readKey)
                    {
                        case ConsoleKey.D:
                            bool pathNotOK = true;
                            while (pathNotOK)
                            {
                                print("----Podaj ścieżkę----");
                                path = Console.ReadLine();
                                if (path.EndsWith(".db"))
                                {
                                    director = new BuildDirector(new SQLiteBuilder());
                                    director.AddDbPath(path);
                                    director.Construct();
                                    pathNotOK = false;
                                }
                                else if (path.EndsWith(".csv"))
                                {
                                    director = new BuildDirector(new CSVBuilder());
                                    director.AddDbPath(path);
                                    director.Construct();
                                    pathNotOK = false;
                                }
                                else
                                {
                                    print("----Ścieżka nie wskazuje na plik bazy danych, spróbuj ponownie [F], użyj domyślnej [G]----");
                                    readKey = decisionKey();
                                    switch (readKey)
                                    {
                                        case ConsoleKey.F:
                                            break;
                                        case ConsoleKey.G:
                                            director = new BuildDirector(new SQLiteBuilder());
                                            director.AddDbPath("");
                                            director.Construct();
                                            if (director.IsSuccess())
                                                path = director.InstancePath();
                                            pathNotOK = false;
                                            break;
                                        case ConsoleKey.Backspace:
                                            database();
                                            break;
                                        case ConsoleKey.Escape:
                                            logout();
                                            break;
                                        default:
                                            print("Wybrano nieobsługiwany przycisk.", ConsoleColor.Red);
                                            database();
                                            break;
                                    }
                                }
                            }
                            if (director.IsSuccess())
                                print($"----Utworzono bazę: {path}", ConsoleColor.Green);
                            break;
                        case ConsoleKey.F:
                            director = new BuildDirector(new SQLiteBuilder());
                            director.AddDbPath("");
                            director.Construct();
                            path = director.InstancePath();
                            print($"----Zapamiętano ścieżkę {path}", ConsoleColor.Green);
                            break;
                        case ConsoleKey.Escape:
                            logout();
                            break;
                        default:
                            print("Wybrano nieobsługiwany przycisk.", ConsoleColor.Red);
                            database();
                            break;
                    }
                }
            }
        }
        //Głowne opcje prgramu.
        private static void options()
        {
            while (isAuth)
            {
                print();
                print("----Wybierz co chcesz zrobić:");
                print();
                print("----Lista klientów----[Q]");
                print("----Nowy klient-------[W]");
                print("----Lista zamówień----[E]");
                print("----Nowe zamówienie---[R]");
                print("----Lista usług-------[T]");
                if (UserType.GetUserType().IsAdmin)
                    print("----Nowa usługa-------[Y]");
                print("----Cofnij------------[P]");
                print("----Wyloguj-----------[Esc]");               
                var readKey = decisionKey();
                try
                {
                    switch (readKey)
                    {
                        case ConsoleKey.Q:
                            print();
                            print("----Lista Klinetow:", ConsoleColor.Green);
                            print(Factory.GetEditorObject(path).Get(tables.Customers), ConsoleColor.Blue);
                            print();
                            break;
                        case ConsoleKey.W:
                            memento.SetNewData(path);
                            print();
                            print($"----{Factory.GetEditorObject(path).Add(tables.Customers, new Customer().NewArray()/*Strategia*/)}----");                            
                            goto case ConsoleKey.Q;
                        case ConsoleKey.E:
                            print();
                            print("----Lista Zamówień:", ConsoleColor.Green);
                            print(Factory.GetEditorObject(path).Get(tables.Transactions), ConsoleColor.Blue);
                            print();
                            break;
                        case ConsoleKey.R:
                            print();
                            print("----Lista Klinetow:", ConsoleColor.Green);
                            print(Factory.GetEditorObject(path).Get(tables.Customers), ConsoleColor.Blue);
                            print();
                            print("----Lista Usług:", ConsoleColor.Green);
                            print(Factory.GetEditorObject(path).Get(tables.Services), ConsoleColor.Blue);
                            print();
                            memento.SetNewData(path);
                            print($"----{Factory.GetEditorObject(path).Add(tables.Transactions, new Transaction().NewArray()/*Strategia*/)}----");
                            goto case ConsoleKey.E;
                        case ConsoleKey.T:
                            print();
                            print("----Lista Usług:", ConsoleColor.Green);
                            print(Factory.GetEditorObject(path).Get(tables.Services), ConsoleColor.Blue);
                            print();
                            break;
                        case ConsoleKey.Y:
                            if (UserType.GetUserType().IsAdmin)
                            {
                                print();
                                memento.SetNewData(path);
                                print($"----{Factory.GetEditorObject(path).Add(tables.Services, new Service().NewArray()/*Strategia*/)}----", ConsoleColor.Green);
                                goto case ConsoleKey.T;
                            }
                            break;
                        case ConsoleKey.P:
                            revertFromMemento();
                            break;
                        case ConsoleKey.Escape:
                            logout();
                            break;
                        default:
                            print("Wybrano nieobsługiwany przycisk.", ConsoleColor.Red);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    print(ex.Message, ConsoleColor.Red);
                }
            }
        }
    }
    #endregion
    //Singleton jako zawsze dostęna informacja o użytkowniku.
    #region Singleton
    class UserType
    {
        static UserType instance;
        UserType(bool? isAdmin = null, string name = null)
        {
            Name = name;
            IsAdmin = (bool)isAdmin;
        }
        public static UserType GetUserType(bool? isAdmin = null, string name = null)
        {
            if (instance == null && isAdmin != null && !String.IsNullOrEmpty(name))
                instance = new UserType(isAdmin, name);
            return instance;
        }
        public static void RemoveInstance()
        {
            instance = null;
        }
        public bool IsAdmin { get; private set; }
        public string Name { get; set; }
    }
    class userPermisions
    {
        public string Name { get; set; }
        public bool IsAdmin { get; set; }
    }
    #endregion
    //Builder tworzy odpowiednią bazę danych (SQLite lub CSV)
    #region Builder
    class BuildDirector
    {
        IDbBuildable _builder;
        public BuildDirector(IDbBuildable builder)
        {
            _builder = builder;
        }
        public void AddDbPath(string path)
        {
            if (_builder != null)
            {
                _builder.DbPath = path;
            }
        }
        public string InstancePath()
        {
            return _builder.DbPath;
        }
        public void Construct()
        {
            _builder.CreateDb();
            _builder.CreateTable();
        }
        public bool IsSuccess()
        {
            return File.Exists(_builder.DbPath);
        }
    }
    interface IDbBuildable
    {
        void CreateDb();
        void CreateTable();
        string DbPath { set; get; }
    }
    class CSVBuilder : IDbBuildable
    {
        public void CreateDb()
        {
            File.Create(DbPath).Close();
        }
        public void CreateTable()
        {
            var lines = new List<string>();
            File.AppendAllLines(DbPath, lines);
        }
        string dbPath;
        public string DbPath
        {
            get
            {
                if (String.IsNullOrEmpty(dbPath))
                    return Path.Combine(Environment.CurrentDirectory, "baza.db");
                else
                    return dbPath;

            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    dbPath = Path.Combine(Environment.CurrentDirectory, "baza.db");
                else
                    dbPath = value;
            }
        }
    }
    class SQLiteBuilder : IDbBuildable
    {
        public void CreateDb()
        {
            try
            {
                //var db = new Repository(); Stare repo.
                //db.CreateDB(DbPath); Stare repo.
                new RepositoryAdapter(DbPath).CreateDB(); // Nowe Repo.
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public void CreateTable()
        {
            try
            {
                //var db = new Repository(); //Stare repo.
                var db = new RepositoryAdapter(DbPath); //Nowe repo.
                List<string> sqlList = new List<string>();
                sqlList.Add("CREATE TABLE[Customers](" +
                        "  [ID] INTEGER PRIMARY KEY AUTOINCREMENT" +
                        ", [Name] text NULL " +
                        ", [LastName] text NULL " +
                        ", [Number] integer NULL )");
                sqlList.Add("CREATE TABLE[Services](" +
                       "  [ID] INTEGER PRIMARY KEY AUTOINCREMENT" +
                       ", [Name] text NULL " +
                       ", [Type] text NULL " +
                       ", [Price] integer NULL )");
                sqlList.Add("CREATE TABLE[Transactions](" +
                       "  [ID] INTEGER PRIMARY KEY AUTOINCREMENT" +
                       ", [CustomerID] INTEGER NULL " +
                       ", [ServiceID] INTEGER NULL " +
                       ", [Date] text NULL " +
                       ", [User] text NULL )");
                //db.ExecuteNonQuery(DbPath, sqlList); // Stara metoda
                db.ExecuteNonQuery(sqlList); // Nowa metoda
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        string dbPath;
        public string DbPath
        {
            get
            {
                if (String.IsNullOrEmpty(dbPath))
                    return Path.Combine(Environment.CurrentDirectory, "baza.db");
                else
                    return dbPath;

            }
            set
            {
                if (String.IsNullOrEmpty(value))
                    dbPath = Path.Combine(Environment.CurrentDirectory, "baza.db");
                else
                    dbPath = value;
            }
        }
    }
    #endregion
    //Fabryka obsługująca bazy danych 
    #region Factory
    interface IDbEditable
    {
        string Add(tables table, string[] one);
        List<string[]> Get(tables table, string where = "");
    }
    public enum tables
    {
        Customers,
        Services,
        Transactions
    }
    class Factory
    {
        public static IDbEditable GetEditorObject(string path)
        {
            IDbEditable ide = null;
            string[] arr = path.Split('.');
            string extention = arr[arr.Length - 1];
            switch (extention)
            {
                case "db":
                    ide = new SqliteEditor(path);
                    break;
                case "csv":
                    ide = new CsvEditor(path);
                    break;
                default:
                    throw new Exception("Nie znany typ edytora");
            }
            return ide;
        }
    }
    class SqliteEditor : IDbEditable
    {
        string _path;
        public SqliteEditor(string path)
        {
            _path = path;
        }
        public string Add(tables table, string[] one)
        {
            if (one[0] == "")
                return "Nie dodano - nieprawidłowe dane!";
            //var db = new Repository(); //Stare repo
            var db = new RepositoryAdapter(_path); //Nowe repo
            var sql = new List<string>();
            switch (table)
            {
                case tables.Customers:
                    if (Get(table, $" where Name='{one[0]}' and LastName='{one[1]}' and Number={one[2]}").Count > 0)
                    {
                        return "Klient istnieje w bazie!";
                    }
                    sql.Add($"Insert into [{table.ToString()}] (Name, LastName, Number) Values ('{one[0]}','{one[1]}',{one[2]})");
                    //db.ExecuteNonQuery(_path, sql); //Stara metoda
                    db.ExecuteNonQuery(sql); //Nowa metoda
                    return "Dodano";
                case tables.Services:
                    if (Get(table, $" where Name='{one[0]}' and Type='{one[1]}' and Price={one[2]}").Count > 0)
                    {
                        return "Usługa istnieje w bazie!";
                    }
                    sql.Add($"Insert into [{table.ToString()}] (Name, Type, Price) Values ('{one[0]}','{one[1]}',{one[2]})");
                    //db.ExecuteNonQuery(_path, sql); //Stara metoda
                    db.ExecuteNonQuery(sql); //Nowa metoda
                    return "Dodano";
                case tables.Transactions:
                    if (Get(tables.Customers, $" where ID={one[0]}").Count > 0 && Get(tables.Services, $" where ID={one[1]}").Count > 0)
                    {
                        sql.Add($"Insert into [{table.ToString()}] (CustomerID, ServiceID, Date, User) Values ({one[0]},{one[1]},'{one[2]}','{one[3]}')");
                        //db.ExecuteNonQuery(_path, sql); //Stara metoda
                        db.ExecuteNonQuery(sql); //Nowa metoda
                        return "Dodano";
                    }
                    else
                        return "Nie dodano, błędne ID klienta lub usługi!";
                default:
                    return "Nie dodano - błędny typ tabeli!";
            }
        }
        public List<string[]> Get(tables table, string where = "")
        {
            //var db = new Repository(); //Stare repo
            var db = new RepositoryAdapter(_path); //Nowe repo
            //var result = db.ExecuteReader(_path, $"SELECT * FROM [{table.ToString()}] {where}"); //Stara metoda.
            var result = db.ExecuteReader($"SELECT * FROM [{table.ToString()}] {where}"); //Nowa metoda
            switch (table)
            {
                case tables.Customers:
                    foreach (var item in result)
                    {
                        item[0] = $"ID: {item[0]}";
                        item[1] = $"Nazwa1: {item[1]}";
                        item[2] = $"Nazwa2: {item[2]}";
                        item[3] = $"Numer: {item[3]}";
                    }
                    break;
                case tables.Services:
                    foreach (var item in result)
                    {
                        item[0] = $"ID: {item[0]}";
                        item[1] = $"Nazwa: {item[1]}";
                        item[2] = $"Typ: {item[2]}";
                        item[3] = $"Cena: {item[3]}";
                    }
                    break;
                case tables.Transactions:
                    foreach (var item in result)
                    {
                        //string[] customer = db.ExecuteReader(_path, $"SELECT * FROM [{tables.Customers.ToString()}] where ID={item[1]}")[0];
                        //string[] service = db.ExecuteReader(_path, $"SELECT * FROM [{tables.Services.ToString()}] where ID={item[2]}")[0]; // Stare metody
                        string[] customer = db.ExecuteReader($"SELECT * FROM [{tables.Customers.ToString()}] where ID={item[1]}")[0];
                        string[] service = db.ExecuteReader($"SELECT * FROM [{tables.Services.ToString()}] where ID={item[2]}")[0]; // Nowe metody.
                        item[0] = $"ID: {item[0]}";
                        item[1] = $"Klient: {String.Join("*", customer)}";
                        item[2] = $"Usługa: {String.Join("*", service)}";
                        item[3] = $"Data: " + item[3];
                        item[4] = $"Kto: " + item[4];
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
    }
    class CsvEditor : IDbEditable
    {
        string _path;
        public CsvEditor(string path)
        {
            _path = path;
        }
        public string Add(tables table, string[] one)
        {
            if (one[0] == "")
                return "Nie dodano - nieprawidłowe dane!";
            var s = new StringBuilder();
            var lastIndex = File.ReadAllLines(_path).Count();
            switch (table)
            {
                case tables.Customers:
                    if (Get(table, $"{one[0]};{one[1]};{one[2]}").Count > 0)
                    {
                        return "Klient istnieje w bazie!";
                    }
                    break;
                case tables.Services:
                    if (Get(table, $"{one[0]};{one[1]};{one[2]}").Count > 0)
                    {
                        return "Usługa istnieje w bazie!";
                    }
                    break;
                case tables.Transactions:
                    if (Get(tables.Customers, $"{tables.Customers};{one[0]}").Count == 0 && Get(tables.Services, $"{tables.Services};{one[1]}").Count == 0)
                    {
                        return "Nie dodano, błędne ID klienta lub usługi!";
                    }
                    break;
                default:
                    return "Nie dodano - błędny typ tabeli!";
            }
            s.Append($"\r\n{table.ToString()};{lastIndex};");
            foreach (var item in one)
            {
                s.Append(item).Append(";");
            }
            File.AppendAllText(_path, s.ToString());
            return "Dodano";
        }
        public List<string[]> Get(tables table, string where = "")
        {
            var temp = File.ReadAllLines(_path).ToList();
            var result = new List<string[]>();
            if (where != "")
            {
                var one = temp.Where(a => a.Contains(table.ToString()) && a.Contains(where)).FirstOrDefault();
                if (one != null)
                    result.Add(one.Split(';'));
                return result;
            }
            switch (table)
            {
                case tables.Customers:
                    var customers = temp.Where(a => a.StartsWith(table.ToString())).ToList();
                    foreach (var item in customers)
                    {
                        string[] tempArr = item.Split(';');
                        if (tempArr.Length > 4)
                        {
                            result.Add(new string[]
                            {
                            $"ID: {tempArr[1]}",
                            $"Nazwa1: {tempArr[2]}",
                            $"Nazwa2: {tempArr[3]}",
                            $"Numer: {tempArr[4]}"
                            });
                        }
                    }
                    break;
                case tables.Services:
                    var services = temp.Where(a => a.StartsWith(table.ToString())).ToList();
                    foreach (var item in services)
                    {
                        string[] tempArr = item.Split(';');
                        if (tempArr.Length > 4)
                        {
                            result.Add(new string[]
                            {
                                $"ID: {tempArr[1]}",
                                $"Nazwa: {tempArr[2]}",
                                $"Typ: {tempArr[3]}",
                                $"Cena: {tempArr[4]}"
                            });
                        }
                    }
                    break;
                case tables.Transactions:
                    var transactions = temp.Where(a => a.StartsWith(table.ToString())).ToList();
                    foreach (var item in transactions)
                    {
                        string[] tempArr = item.Split(';');
                        var customer = temp.Where(a => a.StartsWith($"{tables.Customers.ToString()};{tempArr[2]}")).FirstOrDefault().Split(';');
                        var service = temp.Where(a => a.StartsWith($"{tables.Services.ToString()};{tempArr[3]}")).FirstOrDefault().Split(';');
                        customer[0] = ""; //Czyszczenie niepotrzebnych wiadomości                     
                        service[0] = "";
                        if (tempArr.Length > 5)
                        {
                            result.Add(new string[]
                            {
                                $"ID: {tempArr[1]}",
                                $"Klient: {String.Join("*", customer)}",
                                $"Usługa: {String.Join("*", service)}",
                                $"Data: {tempArr[4]}",
                                $"Kto: {tempArr[5]}"
                            });
                        }
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
    }
    #endregion
    //Pomocnik do obsługi SQLite
    #region SQLiteRepository
    class Repository
    {
        public void CreateDB(string path)
        {
            SQLiteConnection.CreateFile(path);
        }
        public void ExecuteNonQuery(string path, List<string> sqlList)
        {
            try
            {
                string connStr = "Data Source=" + path;
                if (String.IsNullOrEmpty(path))
                    throw new FileNotFoundException();
                using (var con = new SQLiteConnection(connStr))
                {
                    con.Open();
                    if (con.State == System.Data.ConnectionState.Open)
                    {
                        using (var tr = con.BeginTransaction())
                        {
                            using (var cmd = con.CreateCommand())
                            {
                                cmd.Transaction = tr;
                                foreach (var commandText in sqlList)
                                {
                                    cmd.CommandText = commandText;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            tr.Commit();
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public List<string[]> ExecuteReader(string path, string sql)
        {
            string[] row = null;
            var result = new List<string[]>();
            try
            {
                if (String.IsNullOrEmpty(path))
                    throw new FileNotFoundException();
                using (var con = new SQLiteConnection("Data Source=" + path))
                {
                    using (var cmd = new SQLiteCommand(sql, con))
                    {
                        try
                        {
                            con.Open();
                            if (con.State == System.Data.ConnectionState.Open)
                            {
                                using (var reader = cmd.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            row = new string[reader.VisibleFieldCount];
                                            for (int i = 0; i < reader.VisibleFieldCount; i++)
                                            {
                                                row[i] = reader[i].ToString();
                                            }
                                            result.Add(row);
                                        }
                                    }
                                }
                            }
                            con.Close();
                            return result;
                        }
                        catch (Exception ex)
                        {
                            con.Close();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
    #endregion
    //Proxy
    #region Proxy
    interface IPromisions
    {
        bool IsAuthenticated();
    }
    class ProxyPromisions : IPromisions
    {
        string _name = "";
        string _pwd = "";
        public ProxyPromisions(string name, string pwd)
        {
            _name = name;
            _pwd = pwd;
        }
        public bool IsAuthenticated()
        {
            // Pobieramy słownik z oryginalnej klasy
            var usersDict = new Permisions().usersDict;
            if (usersDict.Where(a => a.Key.Name == _name && a.Value == _pwd).Count() > 0)
            {
                var up = usersDict.Where(a => a.Key.Name == _name && a.Value == _pwd).Select(a => a.Key).First();
                //Singleton tworzymy instancję z uprawnieniami uzytkownika.
                UserType.GetUserType(up.IsAdmin, up.Name);
                return true;
            }
            return false;
        }
    }
    class Permisions : IPromisions
    {
        //Lista użytkowników którzy mogą się zalogować.
        public Dictionary<userPermisions, string> usersDict = new Dictionary<userPermisions, string>()
        {
           { new userPermisions { Name = "admin", IsAdmin = true}, "admin"},
           { new userPermisions { Name = "user", IsAdmin = false}, "user"}
        };
        //Oryginalny obiekt nie daje dostepu.
        public bool IsAuthenticated()
        {
            return false;
        }
    }

    #endregion
    //Decorator
    #region Decorator
    interface IPrintable
    {
        void Print(string text);
        void Print(List<string[]> list);
    }
    class Printer : IPrintable
    {
        public void Print(List<string[]> list)
        {
            var s = new StringBuilder();
            foreach (var row in list)
            {
                s.AppendLine();
                foreach (var cell in row)
                {
                    s.Append($"|{cell}|");
                }
            }
            s.AppendLine();
            Console.WriteLine(s.ToString());
        }

        public void Print(string text)
        {
            Console.WriteLine(text);
        }
    }

    class PrinterDecorated : IPrintable
    {
        ConsoleColor _color;
        Printer _printer = null;
        public PrinterDecorated(ConsoleColor color)
        {
            _printer = new Printer();
            _color = color;
        }
        void changeColor()
        {
            Console.ForegroundColor = _color;
        }
        void resetColor()
        {
            Console.ResetColor();
        }
        public void Print(List<string[]> list)
        {
            changeColor();
            _printer.Print(list);
            resetColor();
        }

        public void Print(string text)
        {
            changeColor();
            _printer.Print(text);
            resetColor();
        }
    }
    class PrinterRed : PrinterDecorated
    {
        public PrinterRed(List<string[]> list) : base(ConsoleColor.Red)
        {
            Print(list);
        }
        public PrinterRed(string text) : base(ConsoleColor.Red)
        {
            Print(text);
        }
    }
    class PrinterGreen : PrinterDecorated
    {
        public PrinterGreen(List<string[]> list) : base(ConsoleColor.Green)
        {
            Print(list);
        }
        public PrinterGreen(string text) : base(ConsoleColor.Green)
        {
            Print(text);
        }
    }
    class PrinterBlue : PrinterDecorated
    {
        public PrinterBlue(List<string[]> list) : base(ConsoleColor.Blue)
        {
            Print(list);
        }
        public PrinterBlue(string text) : base(ConsoleColor.Blue)
        {
            Print(text);
        }
    }

    #endregion
    /*Adapter opakowuje klasę Repository na potrzeby nowych wymagań w programie.
    Nowy interface w metodach nie ma możliwości podania parametru path,
    może ją podać tylko przy tworzeniu obiektu. Nowe repozytorium zostało użyte w SQLiteBuilder, SqliteEditor
    */
    #region Adapter
    interface IRepoAdapter
    {
        void CreateDB();
        void ExecuteNonQuery(List<string> sqlList);
        List<string[]> ExecuteReader(string sql);
    }
    class RepositoryAdapter : IRepoAdapter
    {
        string _path;
        Repository repo = null;
        public RepositoryAdapter(string path)
        {
            _path = path;
            repo = new Repository();
        }

        public void CreateDB()
        {
            repo.CreateDB(_path);
        }

        public void ExecuteNonQuery(List<string> sqlList)
        {
            repo.ExecuteNonQuery(_path, sqlList);
        }

        public List<string[]> ExecuteReader(string sql)
        {
            return repo.ExecuteReader(_path, sql);
        }
    }
    #endregion
    //Memento pamięta stan bazy danych
    #region Memento
    class Memento
    {
        public string Path { get; private set; }

        public byte[] Data { get; private set; }
        public Memento(string path, byte[] data)
        {
            Path = path;
            Data = data;
        }
        public bool SetNewData(string path)
        {
            if(path == Path && File.Exists(path))
            {
                Data = File.ReadAllBytes(path);
                return true;
            }
            return false;
        }
    }
    #endregion
    //State zmienia wielkosć okna
    #region State
    abstract class State
    {
        public abstract void Handle(Context context);
    }
    class Context
    {        
        public Context(State state)
        {
            State = state;
        }
        public State State { get; set; }

        public void Change()
        {
            State.Handle(this);
        }
    }
    class SmallState : State
    {
        public override void Handle(Context context)
        {
            Console.SetWindowSize(70, Console.WindowHeight);
            context.State = new BigState();
        }
    }
    class BigState : State
    {
        public override void Handle(Context context)
        {
            Console.SetWindowSize(120, Console.WindowHeight);
            context.State = new SmallState();
        }
    }
    #endregion
    //Strategy
    #region Strategy
    interface IArrayCreateable
    {
        string[] NewArray();
    }
    class Customer : IArrayCreateable
    {
        public string[] NewArray()
        {
            var printer = new Printer();            
            var result = new string[3];
            printer.Print("----Podaj Imie/Nazwę Firmy:");
            result[0] = Console.ReadLine();
            printer.Print("----Podaj Nazwisko:");
            result[1] = Console.ReadLine();
            int number = 0;
            while (number == 0)
            {
                try
                {
                    printer.Print("----Podaj PESEL/NIP:");
                    number = Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                    number = 0;
                    new PrinterRed("----Wpisana wartość nie jest numerem, spróbuj jeszcze raz");
                }
            }
            result[2] = number.ToString();
            return result;
        }
    }
    class Transaction : IArrayCreateable
    {
        public string[] NewArray()
        {
            var printer = new Printer();
            var result = new string[4];
            printer.Print("----Podaj Id klienta:");
            result[0] = Console.ReadLine();
            printer.Print("----Podaj Id usługi:");
            result[1] = Console.ReadLine();
            result[2] = DateTime.Now.ToShortDateString();
            result[3] = UserType.GetUserType().Name;
            return result;
        }
    }
    class Service : IArrayCreateable
    {
        public string[] NewArray()
        {
            var printer = new Printer();
            var result = new string[3];
            printer.Print("----Podaj Nazwę:");
            result[0] = Console.ReadLine();
            printer.Print("----Podaj Typ:");
            result[1] = Console.ReadLine();
            int number = 0;
            while (number == 0)
            {
                try
                {
                    printer.Print("----Podaj Cenę - jako liczbę pełnych złotych:");
                    number = Convert.ToInt32(Console.ReadLine());
                }
                catch
                {
                    number = 0;
                    new PrinterRed("----Wpisana wartość nie jest numerem, spróbuj jeszcze raz");
                }
            }
            result[2] = number.ToString();
            return result;
        }
    }
    #endregion

}

using CsvHelper.Configuration;
using CsvHelper;
using passwordManager.models;
using System.Globalization;


namespace passwordManager
{
    class Program
    {
        static string workDir = string.Empty;

        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.StartsWith("--workdir="))
                {
                    workDir = arg.Substring("--workdir=".Length);
                    Console.WriteLine($"Working Directory: {workDir}");
                }
                else if (arg == "register")
                {
                    Register();
                }
                else if (arg == "list")
                {
                    Login();
                }
                else
                {
                    Console.WriteLine("Invalid command");
                }
            }
        }

        static void Register()
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();

            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            Console.Write("Enter email: ");
            string email = Console.ReadLine();

            Console.Write("Enter firstname: ");
            string firstname = Console.ReadLine();

            Console.Write("Enter lastname: ");
            string lastname = Console.ReadLine();

            User user = new User();
            user.username = username;

            EncryptedType encryptedData = new EncryptedType(email, password);
            EncryptedType encryptedRes = encryptedData.Encrypt();
            user.password = encryptedRes.Secret;

            user.email = email;
            user.firstname = firstname;
            user.lastname = lastname;

            addUser(user);
        }

        private static void addUser(User user)
        {

            string userCsvPath = Path.Combine("resources", "db", "user.csv");

            // append new record at the end of our csv file
            bool mode = true;
            using (StreamWriter writer = new(userCsvPath, append: mode))
            {
                CsvConfiguration config = new(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false
                };
                using CsvWriter csv = new(writer, config);
                csv.WriteRecords(new User[] { user });
            }
        }

        /*List method functions:*/

        //Authorize the user
        private static void Login()
        {
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            User user = null;
            user = userLogin(username, password);
            if (user == null)
            {
                Console.WriteLine("siekertelen bejelentkezés");
            }
            else
            {
                Console.WriteLine($"sikeres bejelentkezés: {user.username}");
                
                Console.WriteLine("Válassz az alábbi opciók közül:");
                Console.WriteLine("-list (jelszavak kilistázása");
                Console.WriteLine("-new (új jelszó létrehozása");
                string option = Console.ReadLine();
                if (option.Equals("list"))
                {
                    showPasswords(user);
                }
                if(option.Equals("new"))
                {
                    addPassword(user);
                }
            }
        }

        // Assign the values to the user
        private static User userLogin(string username, string password)
        {
            User user = null;

            string userCsvPath = Path.Combine("resources", "db", "user.csv");

            using (StreamReader reader = new(userCsvPath))
            {
                using CsvReader csv = new(
                    reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<User>().ToList();

                foreach (var record in records)

                {
                    if (record.username == (username))
                    {


                        EncryptedType encryptedData = new EncryptedType(record.email, record.password);
                        EncryptedType decryptedResult = encryptedData.Decrypt();


                        if (password == decryptedResult.Secret)
                        {

                            user = new User();
                            user.username = record.username;
                            user.password = record.password;
                            user.email = record.email;
                            user.firstname = record.firstname;
                            user.lastname = record.lastname;

                            return user;

                        }
                    }
                }
            }
            return user;
        }

        //List passwords
        private static void showPasswords(User user)
        {
            Vault.userCsvPath = Path.Combine("resources", "db", "user.csv");
            string vaultCsvPath = Path.Combine("resources", "db", "vault.csv");

            using (StreamReader reader = new(vaultCsvPath))
            {
                using CsvReader csv = new(
                    reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<Vault>().ToList();

                foreach (var record in records)
                {
                    if (record.userid.Equals(user.username))
                    {
                        Console.WriteLine(record);
                    }
                }
            }

        }

        // adding new password

        private static void addPassword(User user)
        {
            Vault vault = new Vault();
            vault.userid = user.username;
            Console.WriteLine("Írd be a menteni kívánt felhasználónevet ");
            vault.username = Console.ReadLine();

            string password;
            Console.WriteLine("Írd be a menteni kívánt jelszót ");
            password = Console.ReadLine();

            EncryptedType encryptedData = new EncryptedType(user.email, password);
            EncryptedType encryptedRes = encryptedData.Encrypt();
            vault.password = encryptedRes.Secret;

            Console.WriteLine("Írd be a weboldalt amihez rendelni szeretnéd a jelszót ");
            vault.website = Console.ReadLine();

            string vaultCsvPath = Path.Combine("resources", "db", "vault.csv");
            // append new record at the end of our csv file
            bool mode = true;
            using (StreamWriter writer = new(vaultCsvPath, append: mode))
            {
                CsvConfiguration config = new(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false
                };
                using CsvWriter csv = new(writer, config);
                csv.WriteRecords(new Vault[] { vault });
            }



        }
    }
}

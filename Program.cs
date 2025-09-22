using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;

namespace CareerMDFirebaseUserCreator
{
    public class UserRecordModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var credentialPath = config["Firebase:ServiceAccountPath"];
            var csvPath = config["Firebase:UserCsvPath"];

            if (string.IsNullOrWhiteSpace(credentialPath) || !File.Exists(credentialPath))
            {
                Console.WriteLine("Firebase service account file not found or invalid.");
                return;
            }

            if (string.IsNullOrWhiteSpace(csvPath) || !File.Exists(csvPath))
            {
                Console.WriteLine("User CSV file not found.");
                return;
            }

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(credentialPath)
            });

            var users = ReadUsersFromCsv(csvPath);

            foreach (var user in users)
            {
                var normalizedEmail = user.Email.Trim().ToLowerInvariant();

                try
                {
                    var existingUser = await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(normalizedEmail);
                    Console.WriteLine($"User with email {normalizedEmail} already exists (UID: {existingUser.Uid}). Skipping creation.");
                }
                catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
                {
                    try
                    {
                        var userArgs = new UserRecordArgs()
                        {
                            Email = normalizedEmail,
                            Password = user.Password,
                            EmailVerified = false,
                            Disabled = false
                        };

                        UserRecord createdUser = await FirebaseAuth.DefaultInstance.CreateUserAsync(userArgs);
                        Console.WriteLine($"Created user: {createdUser.Email} (UID: {createdUser.Uid})");
                    }
                    catch (Exception createEx)
                    {
                        Console.WriteLine($"Failed to create user {normalizedEmail}: {createEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to check user {normalizedEmail}: {ex.Message}");
                }
            }
            Console.WriteLine("User creation process completed.");
        }

        static List<UserRecordModel> ReadUsersFromCsv(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            return new List<UserRecordModel>(csv.GetRecords<UserRecordModel>());
        }
    }
}

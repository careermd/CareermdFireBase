using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

            if (string.IsNullOrWhiteSpace(credentialPath) || !File.Exists(credentialPath))
            {
                Console.WriteLine("Firebase service account file not found or invalid.");
                return;
            }
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(credentialPath)
            });

            var users = new List<UserRecordModel>
            {
                new UserRecordModel { Email = "hkhokhar@careermd.com", Password = "TestPass123!" },
                new UserRecordModel { Email = "sample1@careermd.com", Password = "TestPass456!" },
                new UserRecordModel { Email = "sample2@careermd.com", Password = "TestPass789!" },
                new UserRecordModel { Email = "sample3@careermd.com", Password = "TestPass321!" },
                new UserRecordModel { Email = "sample4@careermd.com", Password = "TestPass6424!" },
                new UserRecordModel { Email = "sample5@careermd.com", Password = "TestPass768!" },
                new UserRecordModel { Email = "sample6@careermd.com", Password = "TestPass446!" },
                new UserRecordModel { Email = "sample7@careermd.com", Password = "TestPass4663!" },
                new UserRecordModel { Email = "sample8@careermd.com", Password = "TestPass132429!" },
                new UserRecordModel { Email = "sample9@careermd.com", Password = "TestPass1235432!" },
                new UserRecordModel { Email = "sample10@careermd.com", Password = "TestPass456yy5!" }
            };

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
    }
}

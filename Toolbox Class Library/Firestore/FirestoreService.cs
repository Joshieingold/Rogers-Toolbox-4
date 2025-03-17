using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using System.Text;

namespace Toolbox_Class_Library.Firestore
{
    public class FirestoreService
    {
        private static FirestoreDb _firestoreDb;
        private static readonly object _lock = new object();

        public static FirestoreDb GetFirestoreDb()
        {
            if (_firestoreDb == null)
            {
                lock (_lock) // Ensure thread safety
                {
                    if (_firestoreDb == null) // Double-check locking
                    {
                        try
                        {
                            // Debug: Output the base directory path
                            Console.WriteLine($"Base directory: {AppContext.BaseDirectory}");

                            // Set the path to your service account key
                            string filePath = Path.Combine(AppContext.BaseDirectory, "Keys", "bomwipstore-firebase-adminsdk-jhqev-acb5705838.json");
                            Console.WriteLine($"Looking for key file at: {filePath}");

                            if (!File.Exists(filePath))
                            {
                                Console.WriteLine($"ERROR: Key file not found at path: {filePath}");
                                throw new FileNotFoundException($"Firestore key file not found at: {filePath}");
                            }
                            else
                            {
                                Console.WriteLine("Key file found ✅");
                            }

                            // Read file contents for debugging
                            string jsonData = File.ReadAllText(filePath);
                            Console.WriteLine($"Key file content preview: {jsonData.Substring(0, Math.Min(jsonData.Length, 100))}..."); // Preview first 100 characters

                            // Set environment variable
                            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);
                            Console.WriteLine("GOOGLE_APPLICATION_CREDENTIALS environment variable set ✅");

                            // Load credentials directly from the JSON file
                            var credential = GoogleCredential.FromFile(filePath);
                            if (credential == null)
                            {
                                Console.WriteLine("ERROR: Failed to create GoogleCredential from file ❌");
                                throw new InvalidOperationException("Failed to create GoogleCredential");
                            }
                            else
                            {
                                Console.WriteLine("GoogleCredential created successfully ✅");
                            }

                            // Create FirestoreDb instance
                            string projectId = "bomwipstore";
                            _firestoreDb = FirestoreDb.Create(projectId);
                            Console.WriteLine("FirestoreDb created successfully ✅");
                        }
                        catch (Exception ex)
                        {
                            // Log detailed exception
                            Console.WriteLine($"Error initializing Firestore: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                            throw; // Optionally rethrow the exception
                        }
                    }
                }
            }
            return _firestoreDb;
        }
    }
}

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
                            

                            // Set the path to your service account key
                            string filePath = Path.Combine(AppContext.BaseDirectory, "Keys", "Calgary-Key.json");


                            if (!File.Exists(filePath))
                            {

                                throw new FileNotFoundException($"Firestore key file not found at: {filePath}");
                            }
                            else
                            {

                            }

                            // Read file contents for debugging
                            string jsonData = File.ReadAllText(filePath);
                            

                            // Set environment variable
                            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);


                            // Load credentials directly from the JSON file
                            var credential = GoogleCredential.FromFile(filePath);
                            if (credential == null)
                            {

                                throw new InvalidOperationException("Failed to create GoogleCredential");
                            }
                            else
                            {

                            }

                            // Create FirestoreDb instance
                            string projectId = "Calgary-Rogers-Database";
                            _firestoreDb = FirestoreDb.Create(projectId);

                        }
                        catch (Exception ex)
                        {
                            // Log detailed exception

                            throw; // Optionally rethrow the exception
                        }
                    }
                }
            }
            return _firestoreDb;
        }
    }
}

using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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
                            // Set the path to your service account key
                            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Keys", "bomwipstore-firebase-adminsdk-jhqev-acb5705838.json");
                            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", filePath);

                            // Create FirestoreDb instance
                            string projectId = "bomwipstore"; // Replace with your Firestore Project ID
                            _firestoreDb = FirestoreDb.Create(projectId);
                        }
                        catch (Exception ex)
                        {
                            // Handle initialization error (e.g., log it)
                            Console.WriteLine($"Error initializing Firestore: {ex.Message}");
                            throw; // Optionally rethrow the exception
                        }
                    }
                }
            }
            return _firestoreDb;
        }
    } // initialize firestore.
}

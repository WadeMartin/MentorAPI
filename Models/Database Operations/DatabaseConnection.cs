using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace MentorAPI.Models.Database_Operations
{
    public class DatabaseConnection
    {
        public DatabaseConnection()
        {

        }

        public IMongoDatabase DatabaseConnect(string databaseName = "MentorsLabDB")
        {
            return new MongoClient("mongodb+srv://mentors_lab:mentors_lab@mentorslabcluster-d1oqv.mongodb.net/mentorslabdb").GetDatabase(databaseName);
        }
    }
}
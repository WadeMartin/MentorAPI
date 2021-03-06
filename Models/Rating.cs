﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using  MentorAPI.Models.Database_Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MentorAPI.Models {
    public class Rating
    {
        

        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement]
        public string Rate { get; set; }
        [BsonElement]
        public string Comment { get; set; }
        [BsonElement]
        public string RaterUsername { get; set; }
        [BsonElement]
        public string For { get; set; }
        [BsonElement]
        public DateTime dateCreated { get; set; }

        #region CRUD Methods

        public bool InsertIntoDocument(string databaseName = "mentorsLabDB")
        {
            IMongoCollection<Rating> collection = new DatabaseConnection().DatabaseConnect(databaseName).GetCollection<Rating>(GetType().Name); //  ~ Consists of Point 1
            if (collection.AsQueryable().Where(p => p.Id == this.Id).LongCount() > 0) //  used to check if the current object already exists
            {
                return false; //  returns a false statement if it does
            }

            collection.InsertOne(this); // this will insert the object
            return true; // everything completed successfully
        }

        public void UpdateDocument(Rating newOne, string databaseName = "mentorsLabDB") {
            IMongoCollection<Rating> collection = new DatabaseConnection().DatabaseConnect(databaseName).GetCollection<Rating>(GetType().Name);
            List<Rating> uList = collection.AsQueryable().Where(p => p.Id == this.Id).ToList();
            BsonDocument filter = new BsonDocument();

            foreach (Rating item in uList) {
                filter.Add("_id", new ObjectId(item.Id.ToString())); // filters out the document by its unique _Id
                newOne.Id = item.Id;
                var res = collection.ReplaceOne(filter, newOne);// replaces the old document with the new document
            }


        }

        public Rating SearchFirstOrSingle(Dictionary<string, object> queryDictionary, string databaseName = "mentorsLabDB")
        {
            return SearchDocument(queryDictionary)[0];
        }

        /// <summary>
        /// This will search for a collection of objects based on a user specified list of criteria, it then returns a of List<CRUDAble> objects which the user can then manipulate
        /// Please note that this method will only do "Equals to" comparison    
        /// </summary>

        public List<Rating> SearchDocument(Dictionary<string, object> queryDictionary, string databaseName = "mentorsLabDB")
        {
            // the below statement just obtains the relevent document collection associated with the search
            var selectedCollection = new DatabaseConnection().DatabaseConnect(databaseName).GetCollection<Rating>(GetType().Name);
            BsonDocument filter = new BsonDocument(); // we create a blank filter
            foreach (string item in queryDictionary.Keys) // we now iterate through our dictionary looking for all key and value types
            {

                if (queryDictionary[item] != null) //  if the value type is not null for a key, then we add it to our filter list
                {
                    filter.Add(item, queryDictionary[item].ToString());
                }

            }
            return selectedCollection.Find(filter).ToList(); // this will use the filter to return a list of only documents that fit that specific filter
        }

        #endregion

    }
}
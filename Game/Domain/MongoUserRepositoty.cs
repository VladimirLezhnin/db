using System;
using System.IO;
using System.Net;
using Amazon.Util.Internal;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SharpCompress.Common;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id) => userCollection.Find(x => x.Id == id).FirstOrDefault();

        public UserEntity GetOrCreateByLogin(string login)
        {
            var userEntity = userCollection.Find(x => x.Login == login).FirstOrDefault();

            if (userEntity is null)
            {
                var userEntityToCreate = new UserEntity() { Login = login };
                userCollection.InsertOne(userEntityToCreate);

                return userEntityToCreate;
            }

            return userEntity;
        }

        public void Update(UserEntity user)
        {
            var filter = new BsonDocument("_id", user.Id);
            var update = new BsonDocument("$set", user.ToBsonDocument());

            userCollection.FindOneAndUpdate(filter, update);
        }

        public void Delete(Guid id)
        {
            var filter = new BsonDocument("_id", id);
            userCollection.DeleteOne(filter);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var totalCount = userCollection.CountDocuments(new BsonDocument());
            var list = userCollection
                .Find(new BsonDocument())
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .SortBy(x => x.Login)
                .ToList();

            var pageList = new PageList<UserEntity>(list, totalCount, pageNumber, pageSize);

            return pageList;
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}
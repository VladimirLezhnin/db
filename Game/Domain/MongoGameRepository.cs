using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Game.Domain
{
    // TODO Сделать по аналогии с MongoUserRepository
    public class MongoGameRepository : IGameRepository
    {
        public const string CollectionName = "games";
        private readonly IMongoCollection<GameEntity> gameCollection;

        public MongoGameRepository(IMongoDatabase db)
        {
            gameCollection = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            gameCollection.InsertOne(game);
            
            return game;
        }

        public GameEntity FindById(Guid gameId)
        {
            return gameCollection.Find(x => x.Id == gameId).FirstOrDefault();
        }

        public void Update(GameEntity game)
        {
            var filter = new BsonDocument("_id", game.Id);
            var update = new BsonDocument("$set", game.ToBsonDocument());

            gameCollection.FindOneAndUpdate(filter, update);
        }

        // Возвращает не более чем limit игр со статусом GameStatus.WaitingToStart
        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            return gameCollection
                .Find(new BsonDocument("Status", GameStatus.WaitingToStart))
                .Limit(limit)
                .ToList();
        }

        // Обновляет игру, если она находится в статусе GameStatus.WaitingToStart
        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var filter = new BsonDocument("_id", game.Id);
            filter.Set("Status", GameStatus.WaitingToStart);

            var update = new BsonDocument("$set", game.ToBsonDocument());
            var updated = gameCollection.UpdateOne(filter, update);
            
            return updated.IsAcknowledged && updated.ModifiedCount == 1;
        }
    }
}
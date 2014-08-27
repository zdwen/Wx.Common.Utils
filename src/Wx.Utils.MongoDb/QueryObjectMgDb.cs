using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;

namespace Wx.Utils.MongoDb
{
    public abstract class QueryObjectMgDb
    {
        const int DefaultLimit = -1;
        const int DefaultSkip = 0;

        protected MongoServer _dbServer;
        protected MongoDatabase _db;
        protected IMongoQuery _query;

        static readonly Dictionary<bool, WriteConcern> DicWriteConcern = new Dictionary<bool, WriteConcern>()
        {
            { true, WriteConcern.Acknowledged }, ///【闻祖东 2013-12-27-110015】其实是W1
            { false, WriteConcern.Unacknowledged }, ///【闻祖东 2013-12-27-110025】其实是W0
        };

        public QueryObjectMgDb()
        {
            CollectionName = string.Empty;
            _query = Query.Null;
            Limit = DefaultLimit;
        }

        public abstract string DbConnStr { get; }

        public string CollectionName { get; set; }
        public IMongoSortBy Sort { get; set; }
        public IMongoFields Fields { get; set; }
        public int Limit { get; set; }
        public int Skip { get; set; }
        public UpdateBuilder UpdateBuilder { get; set; }

        public void AddQuery(IMongoQuery mgQuery)
        {
            _query = _query == null
                ? mgQuery
                : Query.And(_query, mgQuery);
        }

        public QueryResult<BsonDocument> GetBsonDocs()
        {
            return GetBsonDocs<BsonDocument>();
        }

        public QueryResult<T> GetBsonDocs<T>()
        {
            using (InitConnection())
            {
                MongoCollection<BsonDocument> coll = _db.GetCollection<BsonDocument>(CollectionName, DicWriteConcern[false]);

                MongoCursor<T> cursors = _query == null
                    ? coll.FindAllAs<T>()
                    : coll.FindAs<T>(_query);

                if (Sort != null)
                    cursors.SetSortOrder(Sort);

                if (Limit != DefaultLimit)
                    cursors.SetLimit(Limit);

                if (Fields != null)
                    cursors.Fields = Fields;

                if (Skip != DefaultSkip)
                    cursors.Skip = Skip;

                return new QueryResult<T>()
                {
                    DocsMatchedSum = (int)cursors.Count(),
                    DataReturned = cursors.ToList(),
                };
            }
        }

        public WriteConcernResult InsertRecord(BsonDocument bsonDoc, bool safeMode)
        {
            using (InitConnection())
            {
                MongoCollection<BsonDocument> coll = _db.GetCollection<BsonDocument>(CollectionName, DicWriteConcern[safeMode]);
                return coll.Insert(bsonDoc);
            }
        }

        public void InsertBatch(List<BsonDocument> docs, bool safeMode)
        {
            using (InitConnection())
            {
                MongoCollection<BsonDocument> coll = _db.GetCollection<BsonDocument>(CollectionName, DicWriteConcern[safeMode]);
                coll.InsertBatch(docs);
            }
        }

        public BsonValue ExecuteCommand(string command)
        {
            using (InitConnection())
            {
                return _db.Eval(EvalFlags.NoLock, new BsonJavaScript(command));
            }
        }

        public WriteConcernResult Update(UpdateFlags flag, bool safeMode)
        {
            using (InitConnection())
            {
                MongoCollection<BsonDocument> coll = _db.GetCollection<BsonDocument>(CollectionName, DicWriteConcern[safeMode]);
                return coll.Update(_query, UpdateBuilder, flag);
            }
        }

        public WriteConcernResult RemoveRecord(bool safeMode)
        {
            using (InitConnection())
            {
                MongoCollection<BsonDocument> coll = _db.GetCollection<BsonDocument>(CollectionName, DicWriteConcern[safeMode]);
                return coll.Remove(_query);
            }
        }

        protected IDisposable InitConnection()
        {
            MongoUrl url = new MongoUrlBuilder(DbConnStr).ToMongoUrl();
            _dbServer = new MongoClient(url).GetServer();
            ////dbServer = MongoServer.Create(url); ///【闻祖东 2013-12-30-180301】还是用这个方法，默认的WriteConcern=Unacknowledged
            _db = _dbServer.GetDatabase(url.DatabaseName);

            return _dbServer.RequestStart(_db);
        }
    }
}

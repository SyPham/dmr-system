using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API.Helpers
{
    public class MongoDBDateTimeSerializer : DateTimeSerializer
    {
        //  MongoDB returns datetime as DateTimeKind.Utc, which cann't be used in our timezone conversion logic
        //  We overwrite it to be DateTimeKind.Unspecified
        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var obj = base.Deserialize(context, args);
            return new DateTime(obj.Ticks, DateTimeKind.Unspecified);
        }

        //  MongoDB stores all datetime as Utc, any datetime value DateTimeKind is not DateTimeKind.Utc, will be converted to Utc first
        //  We overwrite it to be DateTimeKind.Utc, becasue we want to preserve the raw value
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
        {
            var utcValue = new DateTime(value.Ticks, DateTimeKind.Utc);
            base.Serialize(context, args, utcValue);
        }
    }
}

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OpenHackChallenge2
{
    public static class Function3
    {
        [FunctionName("GetRatings")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            try
            {
                var mongoClient = new MongoClient(@"mongodb://openhack2018cl:zASzQucHGNyfTyAQo9sA3h6q2YGfcoG9k3Tfv72vveE9Mx4o5teQMH6WLznk8UVOFB3myWxdmgFNBftIln1VjA==@openhack2018cl.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");

                var db = mongoClient.GetDatabase("Ratings");

                var collection = db.GetCollection<Rating>("ratings");

                var cursor = await collection.FindAsync("{}");


                var found = await cursor.ToListAsync();


                return req.CreateResponse(HttpStatusCode.OK, found);
            }
            catch(Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}

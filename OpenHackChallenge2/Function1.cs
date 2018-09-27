using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace OpenHackChallenge2
{
    public static class Function1
    {
        [FunctionName("CreateRating")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

                // Get request body
            var rating = await req.Content.ReadAsAsync<Rating>();
            //var rating = new Rating()
            //{
            //    userId = new Guid(data?.userId),
            //    productId = new Guid(data?.productId),
            //    locationName = data?.locationName,
            //    rating = int.Parse(data?.rating),
            //    userNotes = data?.userNotes,
            //    timestamp = DateTime.Now,
            //    id = Guid.NewGuid()
            //};

            rating.id = Guid.NewGuid();
            rating.timestamp = DateTime.Now;
            
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new System.Uri("https://serverlessohproduct.trafficmanager.net/api/");
                var response = await client.GetAsync($"GetProduct?productId={rating.productId}");

                if (response.StatusCode != HttpStatusCode.OK)
                    return req.CreateResponse(HttpStatusCode.NotFound, "The specified product id could not be found.");

                var client2 = new HttpClient();
                client2.BaseAddress = new System.Uri("https://serverlessohuser.trafficmanager.net/api/");
                response = await client2.GetAsync($"GetUser?userId={rating.userId}");

                if (response.StatusCode != HttpStatusCode.OK)
                    return req.CreateResponse(HttpStatusCode.BadRequest, "The specified user id is invalid.");


                if (rating.rating < 0 || rating.rating > 5)
                    return req.CreateResponse(HttpStatusCode.BadRequest, "The rating value must be between 0 and 5.");


                var document = rating.ToBsonDocument();

                var mongoClient = new MongoClient(@"mongodb://openhack2018cl:zASzQucHGNyfTyAQo9sA3h6q2YGfcoG9k3Tfv72vveE9Mx4o5teQMH6WLznk8UVOFB3myWxdmgFNBftIln1VjA==@openhack2018cl.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");

                var db = mongoClient.GetDatabase("Ratings");

                var collection = db.GetCollection<BsonDocument>("ratings");
                collection.InsertOne(document);

                return req.CreateResponse(rating);
            }
            catch(Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, ex);
            }
        }
    }
}

public class Rating
{
    public Guid id { get; set; }
    public DateTime timestamp { get; set; }
    public Guid userId { get; set; }
    public Guid productId { get; set; }
    public string locationName { get; set; }
    public string userNotes { get; set; }
    public int rating { get; set; }

}
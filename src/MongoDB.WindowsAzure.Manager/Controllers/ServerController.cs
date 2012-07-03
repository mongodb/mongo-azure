using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MongoDB.WindowsAzure.Manager.Models;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    public class ServerController : ApiController
    {
        // GET api/server
        public IEnumerable<ServerStatus> Get()
        {
            return ReplicaSetStatus.GetReplicaSetStatus( ).servers;
        }

        // GET api/server/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/server
        public void Post(string value)
        {
        }

        // PUT api/server/5
        public void Put(int id, string value)
        {
        }

        // DELETE api/server/5
        public void Delete(int id)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DM;
using Microsoft.AspNetCore.Mvc;
using WF;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace webapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetResultsController : ControllerBase
    {
        // GET: /<controller>/
        [HttpPost]
        public String Post(TripleQuery query)
        {
            var dt = new QueryWF().GetResultsFromQuery(query);
            return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POSSolution.Application;
using POSSolution.Core.Models;
using POSSolution.Infrastructure;
using POSSolution.Infrastructure.ModelRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace POSSolution.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : BaseController<Country>
    {
        public CountryController(POSContext context) : base(context)
        {

        }
    }

    //[Route("api/[controller]")]
    //[ApiController]
    //public class CountryController : BaseController2<Country>
    //{
    //    public CountryController(IRepository<Country> repository) : base(repository)
    //    {

    //    }
    //}
}

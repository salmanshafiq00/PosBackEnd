﻿using Microsoft.AspNetCore.Mvc;
using POSSolution.Core.Models;
using POSSolution.Infrastructure;


namespace POSSolution.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : BaseController<Supplier>
    {
        public SupplierController(POSContext context) : base(context)
        {

        }
    }
}

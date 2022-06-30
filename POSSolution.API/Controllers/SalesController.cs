﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSSolution.API.DTO;
using POSSolution.Core.Models;
using POSSolution.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POSSolution.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : BaseController<Sales>
    {
        private POSContext _context;
        public SalesController(POSContext context) : base(context)
        {
            _context = context;
        }


        public override async Task<ActionResult<Sales>> CreateAsync([FromBody] Sales sales)
        {
            using (var transection = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (SalesDetails details in sales.SalesDetails)
                    {
                        decimal salesPrice =  _context.PurchaseDetails.OrderBy(o => o.Id).First(s => s.ItemId == details.ItemId && s.Quantity > s.SoldQty).SalesPrice;
                        details.TotalAmount = details.SalesQty * salesPrice;
                    }

                    sales.SubTotal = sales.SalesDetails.Sum(s => s.TotalAmount);

                    await _context.Sales.AddAsync(sales);
                    await _context.SaveChangesAsync();

                    List<PurchaseDetails> pdList = new List<PurchaseDetails>();
                    foreach (SalesDetails details in sales.SalesDetails)
                    {
                        PurchaseDetails pd  = _context.PurchaseDetails.OrderBy(o => o.Id).First(s => s.ItemId == details.ItemId && s.Quantity > s.SoldQty);
                        pd.SoldQty = details.SalesQty;
                        pdList.Add(pd);
                    }
                    _context.PurchaseDetails.UpdateRange(pdList);

                    List<StockCount> whList = new List<StockCount>();
                    foreach (SalesDetails details in sales.SalesDetails)
                    {

                        if (_context.StockCounts.Any(item => item.ItemId == details.ItemId))
                        {
                            StockCount wh = _context.StockCounts.Single(item => item.ItemId == details.ItemId);
                            wh.SalesQty += details.SalesQty;
                            whList.Add(wh);

                        }

                    }
                     _context.StockCounts.UpdateRange(whList);
                    await _context.SaveChangesAsync();
                    await transection.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transection.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
            //return RedirectToAction(actionName: )
            return Created("api/Sales/" + sales.Id, sales);
        }

    }
}



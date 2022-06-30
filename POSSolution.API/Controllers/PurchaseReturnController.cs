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
    public class PurchaseReturnController : BaseController<PurchaseReturn>
    {
        private POSContext _context;
        public PurchaseReturnController(POSContext context) : base(context)
        {
            _context = context;
        }

        public override async Task<ActionResult<PurchaseReturn>> CreateAsync([FromBody] PurchaseReturn purchaseReturn)
        {
            using (var transection = await _context.Database.BeginTransactionAsync())
            {
                try
                {

                    foreach (PurchaseReturnDetails details in purchaseReturn.PurchaseReturnDetails)
                    {
                        if (_context.PurchaseDetails.Any(a => a.PurchaseId == purchaseReturn.PurchaseId))
                        {
                         
                            details.UnitCost = _context.PurchaseDetails.Single(a => a.ItemId == details.ItemId).UnitCost;
                        }
                    }

                    await _context.PurchaseReturns.AddAsync(purchaseReturn);
                    await _context.SaveChangesAsync();

                    List<StockCount> whList = new List<StockCount>();
                    
                    foreach (PurchaseReturnDetails details in purchaseReturn.PurchaseReturnDetails)
                    {
                
                        if (_context.StockCounts.Any(item => item.ItemId == details.ItemId))
                        {
                            StockCount wh = _context.StockCounts.Single(item => item.ItemId == details.ItemId);
                            wh.PurchaseQty -= details.Quantity;
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
            return Created("api/PurchaseReturn/" + purchaseReturn.Id, purchaseReturn);
        }
    }
}


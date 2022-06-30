﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using POSSolution.API.DTO;
using POSSolution.Core.Common.Models;
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
    public class PurchaseController : BaseController<Purchase>
    {
        private readonly POSContext _context;
        public PurchaseController(POSContext context) : base(context)
        {
            _context = context;
        }

        public override  async Task<ActionResult<Purchase>> CreateAsync([FromBody] Purchase purchase)
        {

            using (var transection = await _context.Database.BeginTransactionAsync())
            {
                try
                {

                    foreach (PurchaseDetails details in purchase.PurchaseDetails)
                    {
                        int itemId = details.ItemId;
                        Item item = await _context.Items.FindAsync(itemId);
                        decimal discountAmount = 0;
                        decimal taxAmount = 0;
                        DiscountAndTax dt = await _context.DiscountAndTaxes.FindAsync(item.DiscountId);
                        if (dt.IsPercentage)
                        {
                            discountAmount = details.UnitCost * dt.DiscountRate;
                        }
                        else
                        {
                            discountAmount = dt.DiscountAmount;
                        }
                        details.DiscountAmount = discountAmount;
                        taxAmount = (details.UnitCost - (details.UnitCost * dt.DiscountRate)) * dt.TaxRate;
                        details.TaxAmount = taxAmount;
                        decimal profitMargin = _context.Items.Single(item => item.Id == details.ItemId).ProfitMargin;
                        details.SalesPrice = details.UnitCost - discountAmount + taxAmount + profitMargin;
                    }
                        await _context.Purchases.AddAsync(purchase);
                        await _context.SaveChangesAsync();

                       List<StockCount> whList = new List<StockCount>();
                    foreach (PurchaseDetails details in purchase.PurchaseDetails)
                    {
                       
                        if (_context.StockCounts.Any(item => item.ItemId == details.ItemId))
                        {
                            StockCount wh = _context.StockCounts.Single(item => item.ItemId == details.ItemId);
                            wh.PurchaseQty += details.Quantity;
                            whList.Add(wh);
                             _context.StockCounts.UpdateRange(whList);

                        }
                        else
                        {
                            StockCount wh = new StockCount();
                            wh.ItemId = details.ItemId;
                            wh.PurchaseQty = details.Quantity;
                            wh.Balance = wh.PurchaseQty - wh.SalesQty;
                            whList.Add(wh);
                            await _context.StockCounts.AddRangeAsync(whList);

                        }
                        
                    }
                    
                    await _context.SaveChangesAsync();
                    await transection.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transection.RollbackAsync();
                    throw new Exception(ex.Message);
                }
            }
            return Created("api/Purchase/" + purchase.Id, purchase);
        }

    }

}


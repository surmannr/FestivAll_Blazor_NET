﻿using DAL.Exceptions;
using DAL.InterfacesForRepos;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly FestivallDb db;
        public OrderItemRepository(FestivallDb _db)
        {
            db = _db;
        }
        public async Task CreateOrderItem(OrderItem newOrderItem)
        {
            if (newOrderItem == null) throw new Exception(ExceptionMessageConstants.NullObject);
            if (OrderItemRepositoryExtension.IsOrderItemParamsNull(newOrderItem)) throw new Exception(ExceptionMessageConstants.RequiredParams);
            db.OrderItems.Add(newOrderItem);
            await db.SaveChangesAsync();
        }

        public async Task DeleteOrderItem(int orderItemId)
        {
            var orderItem = await db.OrderItems.Where(o => o.Id == orderItemId).FirstOrDefaultAsync();
            if(orderItem != null)
            {
                db.OrderItems.Remove(orderItem);
                await db.SaveChangesAsync();
            }
            else throw new Exception(ExceptionMessageConstants.NullObject);
        }

        public async Task<IReadOnlyCollection<OrderItem>> GetAllOrderItems()
        {
            return await db.OrderItems.GetOrderItemsList();
        }

        public async Task<OrderItem> GetOrderItemById(int orderItemId)
        {
            return await db.OrderItems.GetOrderItemById(orderItemId);
        }

        public async Task<IReadOnlyCollection<OrderItem>> GetOrderItemsByOrderId(int orderId)
        {
            return await db.OrderItems.GetOrderItemsByOrderId(orderId);
        }

        public async Task ModifyStatus(int orderItemId, Status status)
        {
            var orderItem = await db.OrderItems.Where(o => o.Id == orderItemId).FirstOrDefaultAsync();
            if (orderItem != null)
            {
                orderItem.Status = status;
                await db.SaveChangesAsync();
            }
            else throw new Exception(ExceptionMessageConstants.NullObject);
        }

        public async Task SetOrder(int orderItemId, int orderId)
        {
            var order = await db.Orders.Where(o => o.Id == orderId).Include(o => o.OrderItems).FirstOrDefaultAsync();
            if (order != null)
            {
                var orderItem = await db.OrderItems.Where(o => o.Id == orderItemId).FirstOrDefaultAsync();
                if (orderItem != null)
                {
                    orderItem.OrderId = orderId;
                    orderItem.Order = order;
                    await db.SaveChangesAsync();
                }
                else throw new Exception(ExceptionMessageConstants.NullObject);
            }
            else throw new Exception(ExceptionMessageConstants.NullObject);
        }
    }
    internal static class OrderItemRepositoryExtension
    {
        public static bool IsOrderItemParamsNull(OrderItem orderItem)
        {
            return orderItem.TicketId == 0 && orderItem.Amount == 0 && orderItem.Price == -1;
        }

        public static async Task<IReadOnlyCollection<OrderItem>> GetOrderItemsList(this IQueryable<OrderItem> orderItems)
            => await orderItems.ToListAsync();

        public static async Task<OrderItem> GetOrderItemById(this IQueryable<OrderItem> orderItems, int orderItemId)
            => await orderItems.Where(o => o.Id == orderItemId).FirstOrDefaultAsync();

        public static async Task<IReadOnlyCollection<OrderItem>> GetOrderItemsByOrderId(this IQueryable<OrderItem> orderItems, int orderId)
            => await orderItems.Where(o => o.OrderId == orderId).ToListAsync();
    }
}
﻿using DAL.Exceptions;
using DAL.InterfacesForRepos;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using SharedLayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedLayer.Exceptions;

namespace DAL.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly FestivallDb db;
        private readonly ITicketRepository ticketRepository;
        public OrderRepository(FestivallDb _db, ITicketRepository _ticketRepository)
        {
            db = _db;
            ticketRepository = _ticketRepository;
        }
        public async Task<Order> AddItemToOrder(string orderId, int orderItemId)
        {
            var orderItem = await db.OrderItems.Where(e => e.Id == orderItemId).FirstOrDefaultAsync();
            if (orderItem != null)
            {
                var order = await db.Orders.Where(o => o.Id == orderId).Include(o => o.OrderItems).FirstOrDefaultAsync();
                if(order != null)
                {
                    order.OrderItems.ToList().Add(orderItem);
                    await db.SaveChangesAsync();
                    return order;
                }
                else throw new DbModelNullException(ExceptionMessageConstants.NullObject);
            }
            else throw new DbModelNullException(ExceptionMessageConstants.NullObject);
        }

        public async Task<Order> CreateOrder(Order newOrder)
        {
            if(newOrder == null)
                throw new DbModelNullException(ExceptionMessageConstants.NullObject);
            if(OrderRepositoryExtension.IsOrderParamsNull(newOrder))
                throw new DbModelParamsNullException(ExceptionMessageConstants.RequiredParams);
            
            foreach(var o in newOrder.OrderItems)
            {
                if(o.TicketId != null)
                {
                    var ticket = await ticketRepository.GetTicketById((int)o.TicketId);
                    if(ticket != null)
                    {
                        int total = ticket.InStock - o.Amount;
                        if (total < 0)
                        {
                            throw new DbRequirementsException("Nincs elegendő jegy a raktáron.");
                        }
                    }
                }
                
            }
            
            db.Orders.Add(newOrder);
            await db.SaveChangesAsync();
            return newOrder;
        }

        public async Task DeleteOrder(string orderId)
        {
            var order = await db.Orders.Where(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order != null)
            {
                db.Orders.Remove(order);
                await db.SaveChangesAsync();
            }
            else throw new DbModelNullException(ExceptionMessageConstants.NullObject);
        }

        public async Task<IReadOnlyCollection<Order>> GetAllOrders()
        {
            return await db.Orders.GetOrdersList();
        }

        public async Task<Order> GetOrderById(string orderId)
        {
            return await db.Orders.GetOrderById(orderId);
        }
        public async Task<Order> GetLastOrderByUser(string userId)
        {
            var list = await db.Orders.Include(o => o.OrderItems).Where(o => o.UserId == userId).ToListAsync();
            return list.LastOrDefault();
        }
        public async Task<IReadOnlyCollection<Order>> GetOrdersByUserId(string userId)
        {
            return await db.Orders.GetOrdersByUserId(userId);
        }

        public async Task<Order> ModifyStatus(string orderId, Status status)
        {
            var order = await db.Orders.Where(o => o.Id == orderId).FirstOrDefaultAsync();
            if (order != null)
            {
                order.Status = status;
                await db.SaveChangesAsync();
                return order;
            }
            else throw new DbModelNullException(ExceptionMessageConstants.NullObject);
        }
    }
    internal static class OrderRepositoryExtension
    {
        public static bool IsOrderParamsNull(Order order)
        {
            return String.IsNullOrEmpty(order.UserId);
        }

        public static async Task<IReadOnlyCollection<Order>> GetOrdersList(this IQueryable<Order> orders)
            => await orders.Include(o => o.OrderItems).ToListAsync();

        public static async Task<Order> GetOrderById(this IQueryable<Order> orders, string orderId)
            => await orders.Include(o => o.OrderItems).Where(o => o.Id == orderId).FirstOrDefaultAsync();

        public static async Task<IReadOnlyCollection<Order>> GetOrdersByUserId(this IQueryable<Order> orders, string userId)
            => await orders.Include(o => o.OrderItems).Where(o => o.UserId == userId).ToListAsync();
    }
}

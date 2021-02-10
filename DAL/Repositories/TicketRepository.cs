﻿using DAL.Exceptions;
using DAL.InterfacesForRepos;
using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly FestivallDb db;
        public TicketRepository(FestivallDb _db)
        {
            db = _db;
        }
        public async Task CreateTicket(Ticket newTicket)
        {
            if(newTicket==null) throw new Exception(ExceptionMessageConstants.NullObject);
            if(TicketRepositoryExtension.IsTicketParamsNull(newTicket)) throw new Exception(ExceptionMessageConstants.RequiredParams);
            db.Tickets.Add(newTicket);
            await db.SaveChangesAsync();
        }

        public async Task DecreaseInStockByOne(int ticketId)
        {
            var ticket = await db.Tickets.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
            if(ticket == null) throw new Exception(ExceptionMessageConstants.NullObject);
            ticket.InStock -= 1;
            await db.SaveChangesAsync();
        }

        public async Task DeleteTicket(int ticketId)
        {
            var ticket = await db.Tickets.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
            if (ticket == null) throw new Exception(ExceptionMessageConstants.NullObject);
            db.Tickets.Remove(ticket);
            await db.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<Ticket>> GetAllTickets()
        {
            return await db.Tickets.GetTicketsList();
        }

        public async Task<Ticket> GetTicketById(int ticketId)
        {
            return await db.Tickets.GetTicketById(ticketId);
        }

        public async Task<IReadOnlyCollection<Ticket>> GetTicketsByEventId(int eventId)
        {
            return await db.Tickets.GetTicketsByEventId(eventId);
        }

        public async Task ModifyCategory(int ticketId, string newCategory)
        {
            var ticket = await db.Tickets.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
            if (ticket == null) throw new Exception(ExceptionMessageConstants.NullObject);
            ticket.Category = newCategory;
            await db.SaveChangesAsync();
        }

        public async Task ModifyInStock(int ticketId, int newInStock)
        {
            var ticket = await db.Tickets.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
            if (ticket == null) throw new Exception(ExceptionMessageConstants.NullObject);
            ticket.InStock = newInStock;
            await db.SaveChangesAsync();
        }

        public async Task ModifyPrice(int ticketId, int newPrice)
        {
            var ticket = await db.Tickets.Where(t => t.Id == ticketId).FirstOrDefaultAsync();
            if (ticket == null) throw new Exception(ExceptionMessageConstants.NullObject);
            ticket.Price = newPrice;
            await db.SaveChangesAsync();
        }
    }
    internal static class TicketRepositoryExtension
    {
        public static bool IsTicketParamsNull(Ticket ticketDto)
        {
            return ticketDto.EventId == 0 && ticketDto.Category == null && ticketDto.InStock == 0;
        }

        public static async Task<IReadOnlyCollection<Ticket>> GetTicketsList(this IQueryable<Ticket> tickets)
            => await tickets.ToListAsync();

        public static async Task<Ticket> GetTicketById(this IQueryable<Ticket> tickets, int ticketId)
            => await tickets.Where(t => t.Id == ticketId).FirstOrDefaultAsync();

        public static async Task<IReadOnlyCollection<Ticket>> GetTicketsByEventId(this IQueryable<Ticket> tickets, int eventId)
            => await tickets.Where(t => t.EventId == eventId).ToListAsync();

    }
}
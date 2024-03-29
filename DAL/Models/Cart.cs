﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Models
{
    public class Cart
    {
        public int TicketId { get; set; }
        public Ticket Ticket { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int Amount { get; set; }
        public string EventLocation { get; set; }
        public DateTime EventStartDate { get; set; }
    }
}

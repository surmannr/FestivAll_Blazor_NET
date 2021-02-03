﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DAL.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Location { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public string Description { get; set; }

        public string CreatorUserId { get; set; }
        public User CreatorUser { get; set; }

        public IReadOnlyCollection<UserFollowedEvent> FollowedByUsers { get; set; }
       
        public IReadOnlyCollection<Review> Reviews { get; set; }

        public IReadOnlyCollection<Post> Posts { get; set; }

        public IReadOnlyCollection<Ticket> Tickets { get; set; }

        public Event()
        {
            FollowedByUsers = new List<UserFollowedEvent>();
            Reviews = new List<Review>();
            Posts = new List<Post>();
            Tickets = new List<Ticket>();
        }
    }
}
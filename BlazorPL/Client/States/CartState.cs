﻿using Blazored.LocalStorage;
using SharedLayer.DTOs;
using SharedLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace BlazorPL.Client.States
{
    public class CartState
    {
        public event Action OnChange;
        private ISyncLocalStorageService localStorage { get; set; }

        public CartState(ISyncLocalStorageService _localStorage)
        {
            localStorage = _localStorage;
        }
        public List<CartDto> Carts { get; set; } = new List<CartDto>();

        public bool ButtonDisabled { get; set; } = false;
        public int sumprice { get; set; } = 0;

        public void Initialize(CartDto[] carts, List<CartDto> local)
        {
            Carts.Clear();
            if(local == null)
            {
                Carts = carts.ToList();
            }
            else
            {
                Carts = carts.ToList();
                foreach (var a in carts)
                {
                    var temp = local.Where(c => c.TicketId == a.TicketId).FirstOrDefault();
                    if(temp != null)
                    {
                        a.Amount = temp.Amount;
                    }
                }
            }
            
            SumPrice();
        }

        public void Remove(CartDto cart)
        {
            Carts.Remove(cart);
            SumPrice();
            NotifyStateChanged();
        }
        public void SumPrice()
        {
            sumprice = 0;
            
            foreach (var o in Carts)
            {
                sumprice += o.TicketPrice * o.Amount;
            }
            ButtonDisabled = false;
            localStorage.SetItem("cart", Carts);
            NotifyStateChanged();

        }
        private void NotifyStateChanged() => OnChange?.Invoke();

    }
}

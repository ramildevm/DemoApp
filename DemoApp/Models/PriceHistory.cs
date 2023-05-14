//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DemoApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PriceHistory
    {
        public int PriceHistoryId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<int> AgentId { get; set; }
        public Nullable<decimal> OldPrice { get; set; }
        public Nullable<decimal> NewPrice { get; set; }
        public Nullable<System.DateTime> ChangeDate { get; set; }
    
        public virtual Agent Agent { get; set; }
        public virtual ShopProduct ShopProduct { get; set; }
    }
}
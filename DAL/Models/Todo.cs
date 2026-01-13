using System;
using System.Collections.Generic;

namespace DAL.Models;

public partial class Todo
{
    public int TodoId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public bool? Status { get; set; }
}

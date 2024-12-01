using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodReadsScraper.Models
{
    public class BookReviews
    {
        public Book Book { get; set; }
        public List<Review> Reviews { get; set; }
    }
}

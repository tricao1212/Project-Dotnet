namespace BookStore.Models
{
    public class BookViewModel
    {
        public IEnumerable<Book> Books { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}

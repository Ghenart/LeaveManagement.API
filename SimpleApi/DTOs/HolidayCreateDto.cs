namespace SimpleApi.DTOs
{
    public class HolidayCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
namespace Bend_PSA.Models.Requests
{
    public class DataRequest
    {
        public DateTime? Time { get; set; } = DateTime.Now;
        public int? ClientId { get; set; } = 0;
        public int? Result { get; set; } = 0;
        public int? ErrorCode { get; set; } = 0;
        public List<ErrorRequest> Errors { get; set; } = [];
        public List<ImageRequest> Images { get; set; } = [];
    }

    public class ErrorRequest
    {
        public int? ErrorCode { get; set; }
    }

    public class ImageRequest
    {
        public string? PathImage { get; set; }
    }
}

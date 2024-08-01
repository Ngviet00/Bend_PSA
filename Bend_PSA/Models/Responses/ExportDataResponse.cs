namespace Bend_PSA.Models.Responses
{
    public class ExportDataResponse
    {
        public string DateSelect { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public string Model { get; set; } = string.Empty;
        public string TypeModel { get; set; } = string.Empty;
        public int Ok { get; set; }
        public int Ng { get; set; }
        public int ErrorParticle { get; set; }
        public int ErrorNgTapePosition { get; set; }
        public int ErrorDeform { get; set; }
        public int ErrorScratch { get; set; }
        public int ErrorDirty { get; set; }
    }
}

namespace IdentityMail.Web.Entites
{
    public class MessageAttachment
    {
        public int Id { get; set; }
        public string OriginalName { get; set; } // Kullanıcının yüklediği isim (Örn: Proje_Raporu.pdf)
        public string SavedName { get; set; } // Sunucudaki benzersiz adı (Örn: a3f82e11...pdf)
        public string FilePath { get; set; } // /uploads/attachments/a3f82e11...pdf
        public string FileType { get; set; } // .pdf, .png vb.
        public long FileSize { get; set; } // Bayt cinsinden boyutu


        public int UserMessageId { get; set; }
        public UserMessage UserMessage { get; set; }
    }
}

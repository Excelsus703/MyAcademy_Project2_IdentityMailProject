using System.ComponentModel.DataAnnotations;

namespace IdentityMail.Web.DTOs.UserMessageDtos
{
    public class SendMailDto
    {
        [Required(ErrorMessage = "Lütfen alıcı eposta adresini giriniz.")]
        public string ReceiverMail { get; set; }

        [Required(ErrorMessage = "Lütfen konu giriniz.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Lütfen mesajı boş bırakmayınız.")]
        public string Body { get; set; }

        [Required(ErrorMessage = "Lütfen Kategori Seçiniz.")]
        [Range(1, int.MaxValue, ErrorMessage = "Lütfen Geçerli Bir Kategori Seçiniz.")]
        public int MessageCategoryId { get; set; }


        public List<IFormFile>? Attachments { get; set; }
    }
}

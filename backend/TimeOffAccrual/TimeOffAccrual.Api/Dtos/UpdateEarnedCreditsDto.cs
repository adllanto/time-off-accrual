using System.ComponentModel.DataAnnotations;

namespace TimeOffAccrual.Api.Dtos
{
    public class UpdateEarnedCreditsDto
    {
        [Range(typeof(decimal), "0", "10000")]
        public decimal EarnedHours { get; set; }
    }
}

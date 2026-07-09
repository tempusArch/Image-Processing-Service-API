using System.ComponentModel.DataAnnotations;

namespace ImageProcessingServiceApi.Application;

public class LoginUserDto {
    [Required]
    [EmailAddress]
    public string Email {get; set;}
    
    [Required]
    public string Password {get; set;}
    
}
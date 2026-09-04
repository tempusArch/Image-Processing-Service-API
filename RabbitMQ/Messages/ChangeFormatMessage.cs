using ImageProcessingServiceAPI.Domain;

namespace ImageProcessingServiceAPI.Application;

public class ChangeFormatMessage {
    public string UserId {get; set;}

    public string ImageId {get; set;}

    public TargetFormat targetFormat {get; set;}
    public int Quality {get; set;}

    public string ResultName {get; set;}

}
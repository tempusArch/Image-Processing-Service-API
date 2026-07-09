using ImageProcessingServiceApi.Domain;

namespace ImageProcessingServiceApi.Application;

public class FilterMessage {
    public string UserId {get; set;}

    public string ImageId {get; set;}

    public ImageFilter imageFilter  {get; set;}

    public string ResultName {get; set;}

}
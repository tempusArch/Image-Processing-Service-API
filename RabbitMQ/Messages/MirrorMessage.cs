using SixLabors.ImageSharp.Processing;

namespace ImageProcessingServiceApi.Application;

public class MirrorMessage {
    public string UserId {get; set;}

    public string ImageId {get; set;}

    public string ResultName {get; set;}

}
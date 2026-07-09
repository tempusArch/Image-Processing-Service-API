using SixLabors.ImageSharp.Processing;

namespace ImageProcessingServiceApi.Application;

public class FlipMessage {
    public string UserId {get; set;}

    public string ImageId {get; set;}

    public FlipMode flipMode {get; set;}

    public string ResultName {get; set;}

}
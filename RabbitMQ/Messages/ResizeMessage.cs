namespace ImageProcessingServiceAPI.Application;

public class ResizeMessage {
    public string UserId {get; set;}
    public string ImageId {get; set;}

    public int Width {get; set;}
    public int Height {get; set;}

    public string ResultName {get; set;}

}
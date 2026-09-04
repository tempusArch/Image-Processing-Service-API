namespace ImageProcessingServiceAPI.Application;

public class RotateMessage {
    public string UserId {get; set;}

    public string ImageId {get; set;}

    public float Degree {get; set;}

    public string ResultName {get; set;}

}
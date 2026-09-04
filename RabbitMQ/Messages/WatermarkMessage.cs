namespace ImageProcessingServiceAPI.Application;

public class WatermarkMessage {
    public string UserId {get; set;}

    public string ImageId {get; set;}

    public string WatermarkText {get; set;}

    public int X {get; set;}
    public int Y {get; set;}

    public float Size {get; set;}

    public string ResultName {get; set;}
}
namespace ImageProcessingServiceApi.Application;

public class CropMessage {
    public string UserId {get; set;}

    public string ImageId {get; set;}

    public int X {get; set;}
    public int Y {get; set;}

    public int Width {get; set;}
    public int Height {get; set;}

    public string ResultName {get; set;}

}